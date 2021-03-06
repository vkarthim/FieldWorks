-- Update database from version 200217 to 200218
BEGIN TRANSACTION  --( will be rolled back if wrong version#)

-------------------------------------------------------------------------------
-- LT-8239: Migration 200212To200213.sql had a constant in SetAgentEval.
-------------------------------------------------------------------------------

if object_id('SetAgentEval') is not null begin
	print 'removing proc SetAgentEval'
	drop proc SetAgentEval
end
go
print 'creating proc SetAgentEval'
go

CREATE PROC SetAgentEval
	@nAgentID INT,
	@nTargetID INT, --( A WfiAnalysis.ID or a WfiWordform.ID
	@nAccepted INT,
	@nvcDetails NVARCHAR(4000),
	@dtEval DATETIME
AS
	DECLARE
		@nIsNoCountOn INT,
		@nTranCount INT,
		@sysTranName SYSNAME,
		@nEvals INT,
		@nEvalId INT,
		@nNewObjId INT,
		@guidNewObj UNIQUEIDENTIFIER,
		@nNewObjTimeStamp INT,
		@nError INT,
		@nvcError NVARCHAR(100)

	SET @nError = 0

	SET @nIsNoCountOn = @@OPTIONS & 512
	IF @nIsNoCountOn = 0
		SET NOCOUNT ON

	--( Take care of transaction stuff
	SET @nTranCount = @@TRANCOUNT
	SET @sysTranName = 'SetAgentEval_tr' + CONVERT(VARCHAR(2), @@NESTLEVEL)
	IF @nTranCount = 0
		BEGIN TRAN @sysTranName
	ELSE
		SAVE TRAN @sysTranName

	--( See if we have an Agent Evaluation already
	SELECT TOP 1 @nEvalId = co.[Id]
	FROM CmAgentEvaluation cae
	JOIN CmObject co ON co.[Id] = cae.[Id]
		AND co.Owner$ = @nAgentID
	WHERE cae.Target = @nTargetID
	ORDER BY co.[Id]

	SET @nEvals = @@ROWCOUNT

	--== Remove Eval ==--

	--( If we don't know if the analysis is accepted or not,
	--( we don't really have an eval for it. And if we don't
	--( have an eval for it, we need to get rid of it.

	IF @nAccepted = 2 OR @nAccepted IS NULL BEGIN
		WHILE @nEvals > 0 BEGIN
			EXEC DeleteObj$ @nEvalId

			SELECT TOP 1 @nEvalId = co.[Id]
			FROM CmAgentEvaluation cae
			JOIN CmObject co ON co.[Id] = cae.[Id]
				AND co.Owner$ = @nAgentID
			WHERE cae.Target = @nTargetID
				AND co.[Id] > @nEvalId
			ORDER BY co.[Id]

			SET @nEvals = @@ROWCOUNT
		END
	END

	--== Create or Update Eval ==--

	--( Make sure the evaluation is set the way it should be.

	ELSE BEGIN

		--( Create a new Agent Evaluation
		IF @nEvals = 0 BEGIN

			EXEC @nError = MakeObj_CmAgentEvaluation
				@dtEval,
				@nAccepted,
				@nvcDetails,
				@nAgentId,					--(owner
				23006,						--(ownflid  23006
				NULL,						--(startobj
				@nNewObjId OUTPUT,
				@guidNewObj OUTPUT,
				0,			--(ReturnTimeStamp
				@nNewObjTimeStamp OUTPUT

			IF @nError != 0 BEGIN
				SET @nvcError = 'SetAgentEval: MakeObj_CmAgentEvaluation failed.'
				GOTO Fail
			END

			UPDATE CmAgentEvaluation
			SET Target = @nTargetID
			WHERE Id = @nNewObjId
		END

		--( Update the existing Agent Evaluation
		ELSE

			UPDATE CmAgentEvaluation
			SET
				DateCreated = @dtEval,
				Accepted = @nAccepted,
				Details = @nvcDetails
			FROM CmAgentEvaluation cae
			JOIN CmObject co ON co.[Id] = cae.[Id]
				AND co.Owner$ = @nAgentID
			WHERE cae.Target = @nTargetID
		--( END
	END
	GOTO Finish

Finish:

	SET @nIsNoCountOn = @@OPTIONS & 512
	IF @nIsNoCountOn = 0
		SET NOCOUNT ON

	-- determine if a transaction or savepoint was created
	IF @nTranCount = 0
		COMMIT TRAN @sysTranName

	RETURN @nError

Fail:
	RAISERROR (@nvcError, 16, 1, @nError)
	IF @nTranCount !=0
		ROLLBACK TRAN @sysTranName

	RETURN @nError

GO

---------------------------------------------------------------------------------
---- Finish or roll back transaction as applicable
---------------------------------------------------------------------------------

DECLARE @dbVersion INT
SELECT @dbVersion = DbVer FROM Version$
IF @dbVersion = 200217
BEGIN
	UPDATE Version$ SET DbVer = 200218
	COMMIT TRANSACTION
	PRINT 'database updated to version 200218'
END
ELSE
BEGIN
	ROLLBACK TRANSACTION
	PRINT 'Update aborted: this works only if DbVer = 200217 (DbVer = ' +
			CONVERT(VARCHAR, @dbVersion) + ')'
END
GO
