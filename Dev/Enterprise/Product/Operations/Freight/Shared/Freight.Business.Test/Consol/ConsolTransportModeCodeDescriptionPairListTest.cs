using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolTransportModeCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			AssertEquals(Constants.TransportModes.Air, List[0].Code);
			AssertEquals(Constants.TransportModes.Sea, List[1].Code);
			AssertEquals(Constants.TransportModes.Road, List[2].Code);
			AssertEquals(Constants.TransportModes.Rail, List[3].Code);
		}

		ConsolTransportModeCodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new ConsolTransportModeCodeDescriptionPairList();
				}
				return list;
			}
		}
		ConsolTransportModeCodeDescriptionPairList list;
	}
}
