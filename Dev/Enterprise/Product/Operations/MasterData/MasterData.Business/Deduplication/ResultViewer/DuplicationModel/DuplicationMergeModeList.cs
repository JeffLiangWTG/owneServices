using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public class DuplicationMergeModeList : CodeDescriptionPairList
	{
		public DuplicationMergeModeList()
		{
		}

		public void SupportAdd()
		{
			AddPair(nameof(DedupeMergeMode.Add), Res.GetString("48d3f7d4-583e-4c8b-a960-28ff3caf6aad", "Add"));
		}

		public void SupportMerge()
		{
			AddPair(nameof(DedupeMergeMode.Merge), Res.GetString("c0e9e4b3-4449-4a93-82d9-603a48a032a4", "Merge"));
		}
	}

	public enum DedupeMergeMode
	{
		NotSupported,
		Add,
		Merge
	}
}
