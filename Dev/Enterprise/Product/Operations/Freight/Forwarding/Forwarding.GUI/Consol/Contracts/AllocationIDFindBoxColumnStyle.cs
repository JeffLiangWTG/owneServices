using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class AllocationIDFindBoxColumnStyle : ZGuidFindBoxColumnStyle
	{
		public AllocationIDFindBoxColumnStyle(AllocationIDFindBoxColumnStyleInfo columnInfo)
			: base(() => new AllocationIDFindBox(), columnInfo)
		{
		}
	}
}
