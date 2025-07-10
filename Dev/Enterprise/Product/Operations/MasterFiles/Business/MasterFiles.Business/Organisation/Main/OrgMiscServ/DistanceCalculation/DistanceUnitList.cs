using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DistanceUnitList : CodeDescriptionPairList
	{
		public DistanceUnitList()
		{
			AddPair(ResString.GetMultilingualString("4763db94-1651-45B9-b5d3-335c5a19fd31", Constants.Length.Kilometres), ResString.GetMultilingualString("03ef02cd-50c3-4379-8193-c0dcae8fcccc", "Kilometers"));
			AddPair(ResString.GetMultilingualString("fcc13244-adc8-4fb1-b8e8-68fddf9c788c", Constants.Length.Miles), ResString.GetMultilingualString("360aa11a-02ec-4671-952d-97467ee2193e", "Miles"));
		}
	}
}
