using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JodAddressAdditionalInfoTransportModeCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			AssertEquals(Constants.TransportModes.Road, List[0].Code);
			AssertEquals(Constants.TransportModes.Rail, List[1].Code);
			AssertEquals(Constants.TransportModes.InlandWaterwayTransport, List[2].Code);
		}

		JobAddressAdditionalInfoTransportModeCodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new JobAddressAdditionalInfoTransportModeCodeDescriptionPairList();
				}
				return list;
			}
		}
		JobAddressAdditionalInfoTransportModeCodeDescriptionPairList list;
	}
}
