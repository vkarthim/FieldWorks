/*************************************************************************
 * CreateDeleteObj
 *
 * Description:
 *	Creates a delete trigger for each object that deletes related
 *	properties to the object. used on database creation, or when the
 *	object properties get changed.
 *
 * Parameters:
 *	@nClassId = Class to create the trigger for
 *
 * Notes:
 *	Since this is a delete trigger, most of the object properties will
 *	go away with the record of this table. We still have to deal with:
 *
 *		1. References to this object
 *		2. string properties, stored in other tables
 *		3. properties in its parent class, stored in other tables
 *
 *	But before doing that, we must make sure the objects owned by this
 *	one go away.
 *
 *	The SERIALIZABLE locking hint sets these commands to the SERIALIZABLE
 *	transaction level. If we need greater concurrency, we may want to
 *	drop down to REPEATABLEREAD. See "Locking Hints" and "SET TRANSACTION
 *	LEVEL" on Books On Line (BOL).
 *************************************************************************/

IF OBJECT_ID('CreateDeleteObj') IS NOT NULL BEGIN
	PRINT 'removing procedure CreateDeleteObj'
	DROP PROC CreateDeleteObj
END
GO
PRINT 'creating procedure CreateDeleteObj'
GO

CREATE PROCEDURE CreateDeleteObj
	@nClassId INT
AS
	DECLARE
		@nvcObjClassName NVARCHAR(100),
		@nvcClassName NVARCHAR(100),
		@nFieldId INT,
		@nvcFieldName NVARCHAR(100),
		@nvcProcName NVARCHAR(120),  --( max possible size + a couple spare
		@nvcQuery1 VARCHAR(4000), --( 4000's not big enough; need more than 1 string
		@nvcQuery2 VARCHAR(4000),
		@nvcQuery3 VARCHAR(4000),
		@nvcQuery4 VARCHAR(4000),
		@fBuildProc BIT, --( Currently all tables are getting one
		@nvcDropQuery NVARCHAR(140),
		@nvcObjName NVARCHAR(100),
		@nOwnedClassId INT,
		@nDebug TINYINT

	SET @nDebug = 0

	SELECT @nvcObjClassName = c.Name FROM Class$ c WHERE c.Id = @nClassId

	SET @fBuildProc = 0
	SET @nvcProcName = N'TR_' + @nvcObjClassName + N'_ObjDel_Del'
	SET @nvcQuery1 = ''
	SET @nvcQuery2 = ''
	SET @nvcQuery3 = ''
	SET @nvcQuery4 = ''

	--( The initial part of the CREATE TRIGGER command
	IF OBJECT_ID(@nvcProcName) IS NULL
		SET @nvcQuery1 = N'CREATE'
	ELSE
		SET @nvcQuery1 = N'ALTER'

	--( This assumes only one ID (row) in deleted

	SET @nvcQuery1 = @nvcQuery1 +
		N' TRIGGER ' + @nvcProcName + N' ON ' + @nvcObjClassName + CHAR(13) +
		N'INSTEAD OF DELETE ' + CHAR(13) +
		N'AS ' + CHAR(13)
	IF @nDebug = 1
		SET @nvcQuery1 = @nvcQuery1 +
			CHAR(9) + N'PRINT ''TRIGGER ' + @nvcProcName +
				N' ON ' + @nvcObjClassName + N' INSTEAD OF DELETE ''' + CHAR(13) +
			CHAR(9) + CHAR(13)
	SET @nvcQuery1 = @nvcQuery1 +
		CHAR(9) + N'/* == This trigger generated by CreateDeleteObj == */ ' + CHAR(13) +
		CHAR(9) + CHAR(13) +
		CHAR(9) + N'DECLARE @nObjId INT ' + CHAR(13) +
		CHAR(9) + N'SELECT @nObjId = d.[Id] FROM deleted d' + CHAR(13) +
		CHAR(9) + CHAR(13)

	--==( Delete references *to* this object )==--

	--( atomic references to this object

	SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
	FROM Field$ f
	JOIN Class$ c ON c.Id = f.Class
	WHERE f.DstCls = @nClassId AND f.Type = 24
	ORDER BY f.Id

	IF @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery1 = @nvcQuery1 + CHAR(9) +
			'/* Delete atomic references *to* this object */ ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END
	WHILE @@ROWCOUNT != 0 BEGIN
		SET @nvcQuery1 = @nvcQuery1 +
			CHAR(9) + N'UPDATE ' + @nvcClassName + N' WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'SET [' + @nvcFieldName + N'] = NULL ' + CHAR(13) +
			CHAR(9) + N'WHERE [' + @nvcFieldName + N'] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)

		SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
		FROM Field$ f
		JOIN Class$ c ON c.Id = f.Class
		WHERE f.Id > @nFieldId AND f.DstCls = @nClassId AND f.Type = 24
		ORDER BY f.Id
	END

	--( collection and sequence refences

	SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
	FROM Field$ f
	JOIN Class$ c ON c.Id = f.Class
	WHERE f.DstCls = @nClassId AND f.Type IN (26, 28)
	ORDER BY f.Id

	IF @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery2 = @nvcQuery2 + CHAR(9) +
			'/* Delete collection and sequence references *to* this object */ ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END
	WHILE @@ROWCOUNT != 0 BEGIN
		SET @nvcQuery2 = @nvcQuery2 +
			CHAR(9) + N'DELETE ' + @nvcClassName + N'_' + @nvcFieldName + N' WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Dst] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)

		SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
		FROM Field$ f
		JOIN Class$ c ON c.Id = f.Class
		WHERE f.Id > @nFieldId AND f.DstCls = @nClassId AND f.Type IN (26, 28)
		ORDER BY f.Id
	END

	--==( Delete references *of* this object )==--

	--( Atomic references will get wiped out autmatically when this record
	--( goes away.

	--( Collection and Sequence refences

	SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
	FROM Field$ f
	JOIN Class$ c ON c.Id = f.Class
	WHERE f.Class = @nClassId AND f.Type IN (26, 28)
	ORDER BY f.Id

	IF @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery3 = @nvcQuery3 + CHAR(9) +
			'/* Delete references *of* this object */ ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END
	WHILE @@ROWCOUNT != 0 BEGIN
		SET @nvcQuery3 = @nvcQuery3 +
			CHAR(9) + N'DELETE ' + @nvcClassName + N'_' + @nvcFieldName + N' WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Src] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)

		SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
		FROM Field$ f
		JOIN Class$ c ON c.Id = f.Class
		WHERE f.Id > @nFieldId AND f.Class = @nClassId AND f.Type IN (26, 28)
		ORDER BY f.Id
	END

	--==( Delete strings of this object )==--

	SET @nvcQuery4 = @nvcQuery4 +
		CHAR(9) + N'/* Delete any strings of this object */ ' + CHAR(13) +
		CHAR(9) + CHAR(13)

	--( If any MultiStr$ properties, create delete code.
	SELECT TOP 1 @nFieldId = Id FROM Field$ WHERE Class = @nClassId AND Type = 14
	IF @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery4 = @nvcQuery4 +
			CHAR(9) + N'DELETE MultiStr$ WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Obj] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END

	--( If any MultiTxt$ properties, create delete code.
	SELECT TOP 1 @nFieldId = f.ID, @nvcClassName = c.Name, @nvcFieldName = f.Name
	FROM Field$ f
	JOIN Class$ c ON c.ID = f.Class
	WHERE f.Class = @nClassId AND f.Type = 16
	ORDER BY f.Id

	WHILE @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery4 = @nvcQuery4 +
			CHAR(9) + N'DELETE ' + @nvcClassName + N'_' + @nvcFieldName + N' WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Obj] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)

		SELECT TOP 1 @nFieldId = f.Id, @nvcClassName = c.Name, @nvcFieldName = f.Name
		FROM Field$ f
		JOIN Class$ c ON c.Id = f.Class
		WHERE f.ID > @nFieldId AND f.Class = @nClassId AND f.Type = 16
		ORDER BY f.Id
	END

	--( If any MultiBigStr$ properties, create delete code.

	SELECT TOP 1 @nFieldId = Id FROM Field$ WHERE Class = @nClassId AND Type = 18
	IF @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery4 = @nvcQuery4 +
			CHAR(9) + N'DELETE MultiBigStr$ WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Obj] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END

	--( If any MultiBigTxt$ properties, create delete code.

	SELECT TOP 1 @nFieldId = Id FROM Field$ WHERE Class = @nClassId AND Type = 20
	IF @@ROWCOUNT != 0 BEGIN
		SET @fBuildProc = 1
		SET @nvcQuery4 = @nvcQuery4 +
			CHAR(9) + N'DELETE MultiBigTxt$ WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Obj] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END

	--==( Delete this row, since this is an DELETE INSTEAD OF trigger )==--

	SET @fBuildProc = 1
	SET @nvcQuery4 = @nvcQuery4 +
		CHAR(9) + N'/* Delete this row (for INSTEAD OF DELETE trigger) */ ' + CHAR(13) +
		CHAR(9) + CHAR(13)
	SET @nvcQuery4 = @nvcQuery4 +
		CHAR(9) + N'DELETE ' + @nvcObjClassName + N' WITH (SERIALIZABLE) ' + CHAR(13) +
		CHAR(9) + N'WHERE [Id] = @nObjId ' + CHAR(13) +
		CHAR(9) + CHAR(13)

	--==( Delete properties in parent class )==--

	--( This will delete properties *only* in the parent class,
	--( because the parent class will have the same call to
	--( delete properties in *its* parent class. The parent
	--( class has a depth of 1.

	SELECT @nvcClassName = c.Name
	FROM ClassPar$ cp
	JOIN Class$ c ON c.Id = cp.Dst
	WHERE cp.Src = @nClassId AND cp.Depth = 1

	IF @@ROWCOUNT = 1 BEGIN	--( should only be CmObject that misses
		SET @fBuildProc = 1
		SET @nvcQuery4 = @nvcQuery4 +
			CHAR(9) + N'/* Delete properties in parent class */' + CHAR(13) +
			CHAR(9) + CHAR(13)
		SET @nvcQuery4 = @nvcQuery4 +
			CHAR(9) + N'DELETE ' + @nvcClassName + N' WITH (SERIALIZABLE) ' + CHAR(13) +
			CHAR(9) + N'WHERE [Id] = @nObjId ' + CHAR(13) +
			CHAR(9) + CHAR(13)
	END

	--==( Create the new trigger )==--

	IF @fBuildProc = 1 BEGIN
		IF @nDebug = 1 BEGIN
			PRINT '---- query1 ----'
			PRINT @nvcQuery1
			PRINT CHAR(9) + '---- query2 ----'
			PRINT @nvcQuery2
			PRINT CHAR(9) + '---- query3 ----'
			PRINT @nvcQuery3
			PRINT CHAR(9) + '---- query4 ----'
			PRINT @nvcQuery4
		END

		EXECUTE (@nvcQuery1 + @nvcQuery2 + @nvcQuery3 + @nvcQuery4)
	END
GO
