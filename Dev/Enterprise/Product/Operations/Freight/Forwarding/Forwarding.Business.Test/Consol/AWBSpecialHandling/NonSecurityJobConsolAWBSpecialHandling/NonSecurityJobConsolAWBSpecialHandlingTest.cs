using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(NonSecurityJobConsolAWBSpecialHandling))]
	sealed class NonSecurityJobConsolAWBSpecialHandlingTest : JobConsolAWBSpecialHandlingTest
	{
		public void TestSpecialHandlingDescription()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods;
			AssertEquals("SpecialHandlingDescription should show description for code.", AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CoolGoods, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.JKH_Code = ZString.Empty;
			AssertEquals("SpecialHandlingDescription should be empty when code is empty.", ZString.Empty, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.JKH_Code = "IVD";
			AssertEquals("SpecialHandlingDescription should be empty when code is invalid.", ZString.Empty, specialHandlingItem.SpecialHandlingDescription);
		}

		public void TestValidationType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			AssertEquals(typeof(NonSecurityJobConsolAWBSpecialHandlingValidation), specialHandlingItem.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;

			AssertEquals("Precondition: specialHandlingItem.IsSavedByFactory", true, specialHandlingItem.IsSavedByFactory);

			return specialHandlingItem;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;

			return specialHandlingItem;
		}

		#endregion
	}
}
