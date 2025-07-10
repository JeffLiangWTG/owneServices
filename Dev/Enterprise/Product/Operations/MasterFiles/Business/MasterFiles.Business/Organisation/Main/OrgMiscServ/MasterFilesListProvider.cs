using CargoWise.Integration;

using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class MasterFilesListProvider : IMasterFilesListProvider
	{
		public ICodeDescriptionPairList PackingSlipOrderByList()
		{
			return new WhsPackingSlipOrderByList();
		}

		public ICodeDescriptionPairList FailureReasons()
		{
			return new FailureReasonList();
		}
	}
}
