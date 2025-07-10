using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ParentTransportTypeCodeDescriptionPairListTest : BaseFreightTest
	{
		public void TestTransportModeSea()
		{
			CodeDescriptionPairList list;

			list = new ParentTransportTypeCodeDescriptionPairList(Constants.TransportModes.Air);
			AssertMultilineASCIIEquals("Should be empty list", "", list.ElementsAsString);

			list = new ParentTransportTypeCodeDescriptionPairList(Constants.TransportModes.Road);
			AssertMultilineASCIIEquals("Should be empty list", "", list.ElementsAsString);

			list = new ParentTransportTypeCodeDescriptionPairList(Constants.TransportModes.Rail);
			AssertMultilineASCIIEquals("Should be empty list", "", list.ElementsAsString);

			const string expectedSea =
				"MAI - Main\r\n" +
				"SLT - Slot" +
				"";

			list = new ParentTransportTypeCodeDescriptionPairList(Constants.TransportModes.Sea);
			AssertMultilineASCIIEquals("Should be Sea list", expectedSea, list.ElementsAsString);
		}
	}
}
