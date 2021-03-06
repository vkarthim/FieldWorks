-- Update database from version 200214 to 200215
BEGIN TRANSACTION  --( will be rolled back if wrong version#)

-------------------------------------------------------------------------------
-- FWC-33: Shorten WfiWordForm_Form.Txt and MoForm_Form.Txt to 300 bytes
-------------------------------------------------------------------------------

if object_id('TR_Field$_UpdateModel_Ins') is not null begin
	print 'removing trigger TR_Field$_UpdateModel_Ins'
	drop trigger [TR_Field$_UpdateModel_Ins]
end
go
print 'creating trigger TR_Field$_UpdateModel_Ins'
go
create trigger [TR_Field$_UpdateModel_Ins] on [Field$] for insert
as
	declare @sFlid VARCHAR(20)
	declare @Type INT
	declare @Clid INT
	declare @DstCls INT
	declare @sName sysname
	declare @sClass sysname
	declare @sTargetClass sysname
	declare @Min BIGINT
	declare @Max BIGINT
	declare @Big BIT
	declare @fIsCustom bit

	declare @sql NVARCHAR(1000)
	declare @Err INT
	declare @fIsNocountOn INT

	declare @sMin VARCHAR(25)
	declare @sMax VARCHAR(25)
	declare @sTable VARCHAR(20)
	declare @sFmtArg VARCHAR(40)

	declare @sTableName sysname

	set @fIsNocountOn = @@options & 512
	if @fIsNocountOn = 0 set nocount on

	-- get the first class to process
	Select @sFlid= min([id]) from inserted

	-- loop through all of the classes in the inserted logical table
	while @sFlid is not null begin

		-- get inserted data
		select 	@Type = [Type], @Clid = [Class], @sName = [Name], @DstCls = [DstCls], @Min = [Min], @Max = [Max], @Big = [Big], @fIsCustom = [Custom]
		from	inserted i
		where	[Id] = @sFlid

		-- get class name
		select 	@sClass = [Name]  from class$  where [Id] = @Clid

		-- get target class for Reference Objects
		if @Type in (24,26,28) begin
			select 	@sTargetClass = [Name]  from class$  where [Id] = @DstCls
		end

		if @type = 2 begin

			set @sMin = coalesce(convert(varchar(25), @Min), 0)
			set @sMax = coalesce(convert(varchar(25), @Max), 0)

			-- Add Integer to table sized based on Min/Max values supplied
			set @sql = 'ALTER TABLE [' + @sClass + '] ADD [' + @sName + '] '

			if @Min >= 0 and @Max <= 255
				set @sql = @sql + 'TINYINT NOT NULL DEFAULT ' + @sMin
			else if @Min >= -32768 and @Max <= 32767
				set @sql = @sql + 'SMALLINT NOT NULL DEFAULT ' + @sMin
			else if @Min < -2147483648 or @Max > 2147483647
				set @sql = @sql + 'BIGINT NOT NULL DEFAULT ' + @sMin
			else
				set @sql = @sql + 'INT NOT NULL DEFAULT ' + @sMin
			exec (@sql)
			if @@error <> 0 goto LFail

			-- Add Check constraint
			if @Min is not null and @Max is not null begin
				-- format as text

				set @sql = 'ALTER TABLE [' + @sClass + '] ADD CONSTRAINT [' +
					'_CK_' + @sClass + '_' + @sName + '] ' + CHAR(13) + CHAR(9) +
					' check ( [' + @sName + '] is null or ([' + @sName + '] >= ' + @sMin + ' and  [' + @sName + '] <= ' + @sMax + '))'
				exec (@sql)
				set @Err = @@error
				if @Err <> 0 goto LFail
			end

		end
		else if @type IN (14,16,18,20) begin
			-- Define the view or table for this multilingual custom field

			set @sTable = case @type
				when 14 then 'MultiStr$'
				when 18 then 'MultiBigStr$'
				when 20 then 'MultiBigTxt$'
				end
			set @sFmtArg = case @type
				when 14 then '[Fmt]'
				when 18 then '[Fmt]'
				when 20 then 'cast(null as varbinary) as [Fmt]'
				end

			IF @type = 16 BEGIN
				-- TODO (SteveMiller): The Txt field really ought to be cut down
				-- to 83 or less for a number of reasons. First, indexes don't do
				-- a whole lot of good when they get beyond 83 characters. Second,
				-- a lot of these fields probably don't need more than 80, and if
				-- they do, ought to be put in a different field. Third, Firebird
				-- indexes don't go beyond 83.
				--
				-- The fields currently larger than 83 (or 40, for that matter),
				-- is flids 7001 and 20002. 6001004 does in TestLangProj, but that's
				-- "bogus data". These two might become MultiBigTxt fields. Ideally,
				-- word forms ought to be cut down to 83, but Ken indicates that
				-- idioms might be stored there.

				--( See notes under string tables about Latin1_General_BIN

				SET @sql = N'CREATE TABLE ' + @sClass + N'_' + @sName + N' ( ' + CHAR(13) + CHAR(10) +
					CHAR(9) + N'[Obj] INT NOT NULL, ' + CHAR(13) + CHAR(10) +
					CHAR(9) + N'[Ws] INT NOT NULL, ' + CHAR(13) + CHAR(10)
				IF @sName = 'Form' and (@sClass = 'MoForm' or @sClass = 'WfiWordform')
					SET @sql = @sql + CHAR(9) + N'[Txt] NVARCHAR(300) COLLATE Latin1_General_BIN NOT NULL)'
				ELSE
					SET @sql = @sql + CHAR(9) + N'[Txt] NVARCHAR(4000) COLLATE Latin1_General_BIN NOT NULL)'
				EXEC sp_executesql @sql
				SET @sql = N'CREATE INDEX pk_' + @sClass + N'_' + @sName +
					N' ON ' + @sClass + N'_' + @sName + N'(Obj, WS)'
				EXEC sp_executesql @sql
				-- We need these indexes for at least the MakeMissingAnalysesFromLexicion to
				-- perform reasonably. (This is used in parsing interlinear texts.)
				-- SQLServer allows this index even though these fields are
				-- currently too big to index if the whole length is used. In practice, the
				-- limit of 450 unicode characters is very(!) unlikely to be exceeded for
				-- a single wordform or morpheme form.
				if @sName = 'Form' and (@sClass = 'MoForm' or @sClass = 'WfiWordform') BEGIN
					SET @sql = N'CREATE INDEX Ind_' + @sClass + N'_' + @sName +
						N'_Txt ON ' + @sClass + N'_' + @sName + N'(txt)'
					EXEC sp_executesql @sql
				END
			END --( 16
			ELSE BEGIN --( 14, 18, or 20
				set @sql = 'CREATE VIEW [' + @sClass + '_' + @sName + '] AS' + CHAR(13) + CHAR(10) +
					CHAR(9) + 'select [Obj], [Flid], [Ws], [Txt], ' + @sFmtArg + CHAR(13) + CHAR(10) +
					CHAR(9) + 'FROM [' + @sTable + ']' + CHAR(13) + CHAR(10) +
					CHAR(9) + 'WHERE [Flid] = ' + @sFlid
				exec (@sql)
			END
			if @@error <> 0 goto LFail

		end
		else if @type IN (23,25,27) begin
			-- define the view for this OwningAtom/Collection/Sequence custom field.
			set @sql = 'CREATE VIEW [' + @sClass + '_' + @sName + '] AS' + CHAR(13) + CHAR(10) +
				CHAR(9) + 'select [Owner$] as [Src], [Id] as [Dst]'

			if @type = 27 set @sql = @sql + ', [OwnOrd$] as [Ord]'

			set @sql = @sql + CHAR(13) +
				CHAR(9) + 'FROM [CmObject]' + CHAR(13) + CHAR(10) +
				CHAR(9) + 'WHERE [OwnFlid$] = ' + @sFlid
			exec (@sql)
			if @@error <> 0 goto LFail

			--( Adding an owning atomic StText field requires all existing instances
			--( of the owning class to possess an empty StText and StTxtPara.

			IF @type = 23 AND @DstCls = 14 BEGIN
				SET @sql = '
				DECLARE @recId INT,
					@newId int,
					@dummyId int,
					@dummyGuid uniqueidentifier

				DECLARE curOwners CURSOR FOR SELECT [id] FROM ' + @sClass + '
				OPEN curOwners
				FETCH NEXT FROM curOwners INTO @recId
				WHILE @@FETCH_STATUS = 0
				BEGIN
					EXEC MakeObj_StText
						0, @recId, ' + @sFlid + ', null, @newId OUTPUT, @dummyGuid OUTPUT
					EXEC MakeObj_StTxtPara
						null, null, null, null, null, null, @newId, 14001, null, @dummyId, @dummyGuid OUTPUT
					FETCH NEXT FROM curOwners INTO @recId
				END
				CLOSE curOwners
				DEALLOCATE curOwners'

				EXEC (@sql)
			END

		end
		else if @type IN (26,28) begin
			-- define the table for this custom reference collection/sequence field.
			set @sql = 'CREATE TABLE [' + @sClass + '_' + @sName + '] (' + CHAR(13) +
				'[Src] INT NOT NULL,' + CHAR(13) +
				'[Dst] INT NOT NULL,' + CHAR(13)

			if @type = 28 set @sql = @sql + '[Ord] INT NOT NULL,' + CHAR(13)

			set @sql = @sql +
				'CONSTRAINT [_FK_' + @sClass + '_' + @sName + '_Src] ' +
				'FOREIGN KEY ([Src]) REFERENCES [' + @sClass + '] ([Id]),' + CHAR(13) +
				'CONSTRAINT [_FK_' + @sClass + '_' + @sName + '_Dst] ' +
				'FOREIGN KEY ([Dst]) REFERENCES [' + @sTargetClass + '] ([Id]),' + CHAR(13) +
				case @type
					when 26 then ')'
					when 28 then
						CHAR(9) + CHAR(9) + 'CONSTRAINT [_PK_' + @sClass + '_' + @sName + '] ' +
						'PRIMARY KEY CLUSTERED ([Src], [Ord])' + CHAR(13) + ')'
					end
			exec (@sql)
			if @@error <> 0 goto LFail

			if @type = 26 begin
				set @sql = 'create clustered index ' +
						@sClass + '_' + @sName + '_ind on ' +
						@sClass + '_' + @sName + ' ([Src], [Dst])'
				exec (@sql)
				if @@error <> 0 goto LFail

				set @sTableName = @sClass + '_' + @sName
				exec @Err = GenReplRCProc @sTableName
				if @Err <> 0 begin
					raiserror('TR_Field$_UpdateModel_Ins: Unable to create the procedure that handles reference collections for table %s',
							16, 1, @sName)
					goto LFail
				end

			end

			if @type = 28 begin
				set @sTableName = @sClass + '_' + @sName
				exec @Err = GenReplRSProc @sTableName, @sFlid
				if @Err <> 0 begin
					raiserror('TR_Field$_UpdateModel_Ins: Unable to create the procedure that handles reference sequences for table %s',
							16, 1, @sName)
					goto LFail
				end
			end

			--( Insert trigger
			SET @sql = 'CREATE TRIGGER [TR_' + @sClass + '_' + @sName + '_DtTmIns]' + CHAR(13) +
				CHAR(9) + 'ON [' + @sClass + '_' + @sName + '] FOR INSERT ' + CHAR(13) +
				'AS ' + CHAR(13) +
				CHAR(9) + 'UPDATE CmObject SET UpdDttm = GetDate() ' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'FROM CmObject co JOIN inserted ins ON co.[id] = ins.[src] ' + CHAR(13) +
				CHAR(9) + CHAR(13)  +
				CHAR(9) + 'IF @@error <> 0 BEGIN' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'Raiserror(''TR_' + @sClass + '_' + @sName + '_DtTmIns]: ' +
					'Unable to update CmObject'', 16, 1)' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'GOTO LFail' + CHAR(13) +
				CHAR(9) + 'END' + CHAR(13) +
				CHAR(9) + 'RETURN' + CHAR(13) +
				CHAR(9) + CHAR(13) +
				CHAR(9) + 'LFail:' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'ROLLBACK TRAN' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'RETURN' + CHAR(13)
			EXEC (@sql)
			IF @@error <> 0 GOTO LFail

			--( Delete trigger
			SET @sql = 'CREATE TRIGGER [TR_' + @sClass + '_' + @sName + '_DtTmDel]' + CHAR(13) +
				CHAR(9) + 'ON [' + @sClass + '_' + @sName + '] FOR DELETE ' + CHAR(13) +
				'AS ' + CHAR(13) +
				CHAR(9) + 'UPDATE CmObject SET UpdDttm = GetDate() ' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'FROM CmObject co JOIN deleted del ON co.[id] = del.[src] ' + CHAR(13) +
				CHAR(9) + CHAR(13)  +
				CHAR(9) + 'IF @@error <> 0 BEGIN' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'Raiserror(''TR_' + @sClass + '_' + @sName + '_DtTmDel]: ' +
					'Unable to update CmObject'', 16, 1)' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'GOTO LFail' + CHAR(13) +
				CHAR(9) + 'END' + CHAR(13) +
				CHAR(9) + 'RETURN' + CHAR(13) +
				CHAR(9) + CHAR(13) +
				CHAR(9) + 'LFail:' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'ROLLBACK TRAN' + CHAR(13) +
				CHAR(9) + CHAR(9) + 'RETURN' + CHAR(13)
			EXEC (@sql)
			IF @@error <> 0 GOTO LFail

		end
		else begin
			-- add the custom field to the appropriate table
			set @sql = 'ALTER TABLE [' + @sClass + '] ADD [' + @sName + '] ' + case
				when @type = 1 then 'BIT NOT NULL DEFAULT 0'			-- Boolean
				when @type = 3 then 'DECIMAL(28,4) NOT NULL DEFAULT 0'		-- Numeric
				when @type = 4 then 'FLOAT NOT NULL DEFAULT 0.0'		-- Float
				-- Time: default to current time except for fields in LgWritingSystem.
				when @type = 5 AND @sClass != 'LgWritingSystem' then 'DATETIME NULL DEFAULT GETDATE()'
				when @type = 5 AND @sClass = 'LgWritingSystem' then 'DATETIME NULL'
				when @type = 6 then 'UNIQUEIDENTIFIER NULL'			-- Guid
				when @type = 7 then 'IMAGE NULL'				-- Image
				when @type = 8 then 'INT NOT NULL DEFAULT 0'			-- GenDate
				when @type = 9 and @Big is null then 'VARBINARY(8000) NULL'		-- Binary
				when @type = 9 and @Big = 0 then 'VARBINARY(8000) NULL'		-- Binary
				when @type = 9 and @Big = 1 then 'IMAGE NULL'			-- Binary
				when @type = 13 then 'NVARCHAR(4000) NULL'			-- String
				when @type = 15 then 'NVARCHAR(4000) NULL'			-- Unicode
				when @type = 17 then 'NTEXT NULL'				-- BigString
				when @type = 19 then 'NTEXT NULL'				-- BigUnicode
				when @type = 24 then 'INT NULL'					-- ReferenceAtom
				end
			exec (@sql)
			if @@error <> 0 goto LFail
			if @type in (13,17)  begin
				set @sql = 'ALTER TABLE [' + @sClass + '] ADD ' + case @type
					when 13 then '[' + @sName + '_Fmt] VARBINARY(8000) NULL' -- String
					when 17 then '[' + @sName + '_Fmt] IMAGE NULL'			-- BigString
					end
				exec (@sql)
				set @Err = @@error
				if @Err <> 0 goto LFail
			end

			-- Set the 'Text In Row' option for the table if type is 7, 17 or 19.
			if @type in (7, 17, 19) exec sp_tableoption @sClass, 'text in row', '1000'

			-- don't create foreign key constraints on CmObject
			if @type = 24 and @sClass != 'CmObject' begin
				set @sql = 'ALTER TABLE [' + @sClass + '] ADD CONSTRAINT [' +		-- ReferenceAtom
					'_FK_' + @sClass + '_' + @sName + '] ' + CHAR(13) + CHAR(9) +
					' FOREIGN KEY ([' + @sName + ']) REFERENCES [' + @sTargetClass + '] ([Id])'
				exec (@sql)
				set @Err = @@error
				if @Err <> 0 goto LFail
			end

		end

		-- get the next class to process
		Select @sFlid= min([id]) from inserted  where [Id] > @sFlid

	end  -- While loop

	--( UpdateClassView$ is executed in TR_Field$_UpdateModel_InsLast, which is created
	--( in LangProjSP.sql.

	-- if nocount was turned on turn it off
	if @fIsNocountOn = 0 set nocount off
	return

LFail:
	rollback tran
	return
go

-------------------------------------------------------------------------------
-- Shorten MoForm_Form Txt Column		  									--
-------------------------------------------------------------------------------

DECLARE
		@ColumnName NVARCHAR(40),
		@TableName NVARCHAR(40),
		@Debug BIT,
		@Text NVARCHAR(4000),
		@Sql NVARCHAR(4000),
		@Parms NVARCHAR(500),
		@ShortStr NVARCHAR(300),
		@IndexName NVARCHAR(100),
		@Obj INTEGER,
		@Ws INTEGER,
		@LenTxt INTEGER

	SET @Debug = 0

	SET @ColumnName = N'Txt'
	SET @TableName = N'MoForm_Form'
	SET @IndexName = N'IND_MoForm_Form_Txt'
	--Drop the index if it exists
	IF EXISTS(SELECT * FROM sys.indexes WHERE NAME = @IndexName) BEGIN
		IF @Debug = 1
			Print N'Entering Drop Index: '
		SET @Sql = N'DROP INDEX [' + @IndexName + N'] ON [dbo].[' + @TableName +
				N'] WITH ( ONLINE = OFF );'
		EXEC sp_executesql @Sql
		IF @Debug = 1
			Print N'Sql Drop Index: ' + @Sql
	END

	DECLARE LongRows CURSOR FOR
	SELECT Obj, Ws, Len(Txt) from MoForm_Form
	WHERE LEN(Txt) > 300;

	OPEN LongRows;
	FETCH NEXT FROM LongRows INTO @Obj, @Ws, @LenTxt

	IF @Debug = 1
		Print N'Initial Read Obj: ' + CAST(@Obj AS NVARCHAR(8)) + N' Ws: ' + CAST(@Ws AS NVARCHAR(8)) +
			N' LengTxt: ' + CAST(@LenTxt AS NVARCHAR(8))
	WHILE (@@FETCH_STATUS = 0) BEGIN
	-- Truncate the data in the txt column to 300 characters

		SET @Sql = N'UPDATE ' + @TableName + N' SET  ' + @ColumnName +
			N' = SUBSTRING(' + @ColumnName + N', 1,300) WHERE Obj = ' +
			CAST(@Obj AS VARCHAR(8))+ N' AND WS = ' + CAST(@Ws AS VARCHAR(8))+ N';'
		EXEC sp_executesql @Sql
		IF @Debug = 1
			PRINT  N' UPDATE: Length ShortString: ' + CAST(Len(@ShortStr)AS CHAR(3)) + ' ' + @sql

		FETCH NEXT FROM LongRows INTO @Obj, @Ws, @Text;
		IF @Debug = 1
			Print N'Second Read Obj: ' + CAST(@Obj AS NVARCHAR(8)) + N'Ws' + CAST(@Ws AS NVARCHAR(8)) +
				N'Txt: ' + @Text + ' Length: ' +CAST(Len(@Text)AS NVARCHAR(8))
	END		-- end While

	CLOSE LongRows;
	DEALLOCATE LongRows;

	SET @Sql = N'ALTER TABLE MoForm_Form ALTER COLUMN Txt NVARCHAR(300) COLLATE Latin1_General_BIN;'
	EXEC sp_executesql @Sql
	IF @Debug = 1
		Print N'Sql Alter Column: ' + @Sql

	IF OBJECT_ID(@IndexName) IS NULL BEGIN
		SET @Sql = N'CREATE NONCLUSTERED INDEX [' + @IndexName + N'] ON [dbo].[' +
			@TableName + N'] ([' + @ColumnName + N'] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, ' +
			N'SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ' +
			N'ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY];'
		IF @Debug = 1
			Print N'Sql Create Index: ' + @Sql
		EXEC sp_executesql @Sql
	END

-------------------------------------------------------------------------------
-- Shorten WfiWordForm_Form Txt Column									     --
-------------------------------------------------------------------------------

	SET @ColumnName = N'Txt'
	SET @TableName = N'WfiWordForm_Form'
	SET @IndexName = N'IND_WfiWordForm_Form_Txt'
	--Drop the index if it exists
	IF EXISTS(SELECT * FROM sys.indexes WHERE NAME = @IndexName) BEGIN
		IF @Debug = 1
			Print N'Entering Drop Index: '
		SET @Sql = N'DROP INDEX [' + @IndexName + N'] ON [dbo].[' + @TableName +
				N'] WITH ( ONLINE = OFF );'
		EXEC sp_executesql @Sql
		IF @Debug = 1
			Print N'Sql Drop Index: ' + @Sql
	END

	DECLARE LongRows CURSOR FOR
	SELECT Obj, Ws, Len(Txt) from WfiWordForm_Form
	WHERE LEN(Txt) > 300;

	OPEN LongRows;
	FETCH NEXT FROM LongRows INTO @Obj, @Ws, @LenTxt

	IF @Debug = 1
		Print N'Initial Read Obj: ' + CAST(@Obj AS NVARCHAR(8)) + N' Ws: ' + CAST(@Ws AS NVARCHAR(8)) +
			N' LengTxt: ' + CAST(@LenTxt AS NVARCHAR(8))
	WHILE (@@FETCH_STATUS = 0) BEGIN
	-- Truncate the data in the txt column to 300 characters

		SET @Sql = N'UPDATE ' + @TableName + N' SET  ' + @ColumnName +
			N' = SUBSTRING(' + @ColumnName + N', 1,300) WHERE Obj = ' +
			CAST(@Obj AS VARCHAR(8))+ N' AND WS = ' + CAST(@Ws AS VARCHAR(8))+ N';'
		EXEC sp_executesql @Sql
		IF @Debug = 1
			PRINT  N' UPDATE: Length ShortString: ' + CAST(Len(@ShortStr)AS CHAR(3)) + ' ' + @sql

		FETCH NEXT FROM LongRows INTO @Obj, @Ws, @Text;
		IF @Debug = 1
			Print N'Second Read Obj: ' + CAST(@Obj AS NVARCHAR(8)) + N'Ws' + CAST(@Ws AS NVARCHAR(8)) +
				N'Txt: ' + @Text + ' Length: ' +CAST(Len(@Text)AS NVARCHAR(8))
	END		-- end While

	CLOSE LongRows;
	DEALLOCATE LongRows;

	SET @Sql = N'ALTER TABLE MoForm_Form ALTER COLUMN Txt NVARCHAR(300) COLLATE Latin1_General_BIN;'
	EXEC sp_executesql @Sql
	IF @Debug = 1
		Print N'Sql Alter Column: ' + @Sql

	IF OBJECT_ID(@IndexName) IS NULL BEGIN
		SET @Sql = N'CREATE NONCLUSTERED INDEX [' + @IndexName + N'] ON [dbo].[' +
			@TableName + N'] ([' + @ColumnName + N'] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, ' +
			N'SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ' +
			N'ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY];'
		IF @Debug = 1
			Print N'Sql Create Index: ' + @Sql
		EXEC sp_executesql @Sql
	END

---------------------------------------------------------------------------------
---- Finish or roll back transaction as applicable
---------------------------------------------------------------------------------

DECLARE @dbVersion INT
SELECT @dbVersion = DbVer FROM Version$
IF @dbVersion = 200214
BEGIN
	UPDATE Version$ SET DbVer = 200215
	COMMIT TRANSACTION
	PRINT 'database updated to version 200215'
END
ELSE
BEGIN
	ROLLBACK TRANSACTION
	PRINT 'Update aborted: this works only if DbVer = 200214 (DbVer = ' +
			CONVERT(VARCHAR, @dbVersion) + ')'
END
GO
