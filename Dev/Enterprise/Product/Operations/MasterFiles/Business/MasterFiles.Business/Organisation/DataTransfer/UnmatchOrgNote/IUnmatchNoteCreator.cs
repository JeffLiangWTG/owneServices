using Enterprise.MasterFiles.Integration;
namespace Enterprise.MasterFiles.Business
{
	public interface IUnmatchNoteCreator
	{
		void Create(IEntityInfo parentInfo, UnmatchOrgRecord newRecord);
	}
}
