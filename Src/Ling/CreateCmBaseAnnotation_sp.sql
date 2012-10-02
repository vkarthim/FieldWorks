SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO
if object_id('CreateCmBaseAnnotation') is not null begin
	print 'removing proc CreateCmBaseAnnotation'
	drop proc [CreateCmBaseAnnotation]
end
go


----------------------------------------------------------------
-- This stored procedure provides an efficient way to create annotations.
-- It does no checking!
----------------------------------------------------------------

CREATE  proc [CreateCmBaseAnnotation]	@Owner int = null,
	@annotationType int = null,
	@InstanceOf int = null,
	@BeginObject int = null,
	@CmBaseAnnotation_Flid int = 0,	@CmBaseAnnotation_BeginOffset int = 0,	@CmBaseAnnotation_EndOffset int = 0as
	declare @fIsNocountOn int, @Err int
	declare @ObjId int, @guid uniqueidentifier

	set @fIsNocountOn = @@options & 512
	if @fIsNocountOn = 0 set nocount on
	if @InstanceOf = 0 set @instanceOf = null

	set @guid = newid()
	-- leave ownord$ null for owning collection.
	-- 6001044 is LexDb_Annotations
	-- 37 is the class of CmBaseAnnotation.
	insert into [CmObject] with (rowlock) ([Guid$], [Class$], [Owner$], [OwnFlid$])
		values (@guid, 37, @Owner, 6001044)
	set @Err = @@error
	set @ObjId = @@identity
	if @Err <> 0 begin
		raiserror('SQL Error %d: Unable to create the new object', 16, 1, @Err)
		goto LCleanUp
	end
	insert into [CmAnnotation] ([Id], [AnnotationType], [InstanceOf]) 		values (@ObjId, @annotationType, @InstanceOf)	set @Err = @@error	if @Err <> 0 goto LCleanUp	insert into [CmBaseAnnotation] ([Id],[BeginOffset],[Flid],[EndOffset],[BeginObject],[EndObject]) 		values (@ObjId, @CmBaseAnnotation_BeginOffset, @CmBaseAnnotation_Flid, @CmBaseAnnotation_EndOffset,  @BeginObject, @BeginObject)	set @Err = @@error
LCleanUp:

	set @fIsNocountOn = @@options & 512
	if @fIsNocountOn = 0 set nocount on


	return @Err

GO
