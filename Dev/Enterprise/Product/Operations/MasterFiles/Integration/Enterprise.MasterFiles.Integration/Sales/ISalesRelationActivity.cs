using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface ISalesRelationActivity : IRelatableActivity
	{
		ISalesRelationModel SalesRelationModel { get; }

		ZString ActivityNotePropertyName { get; }
	}
}