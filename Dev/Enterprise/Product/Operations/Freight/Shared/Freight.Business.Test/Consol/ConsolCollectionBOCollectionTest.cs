using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ConsolCollection))]
	sealed class ConsolCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonShipment parentShipment = Factory.New<CommonShipment>();
			parentShipment.Factory.Save();
			return new ConsolCollection(parentShipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonConsol result = Factory.New<CommonConsol>();
			return result;
		}

		public void TestSetDefaultsForNewChild_WhenJK_ConsolModeChanges_DepartureTransportAlsoNeedsToBeResigned()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var oldAddress = Factory.NewWithValidTestData<OrgAddress>();
			consol.JK_OA_DeparturePackCFSTransportAddress = oldAddress.PK;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var parentShipment = Factory.NewWithValidTestData<CommonShipment>();
			parentShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			parentShipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var collection = new ConsolCollection(parentShipment);
			collection.SetupNewElementButDoNotAddIt(consol, true);
			AssertEquals("consol's JK_ConsolMode not change", Core.Constants.ContainerModes.FCL, consol.JK_ConsolMode);
			AssertEquals("consol's DepartureTransport not change", oldAddress.PK, consol.JK_OA_DeparturePackCFSTransportAddress);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var sendingOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var sendingForwarderAddress = Factory.New<OrgAddress>();
			sendingForwarderAddress.OA_OH = sendingOrgHeader.PK;
			sendingForwarderAddress.OA_Address1 = "123";
			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			consol.SendingForwarder.AddRelatedParty(relatedParty.PK, Core.Constants.ContainerTypes.FlatRack, "FWD", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);
			parentShipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			collection.SetupNewElementButDoNotAddIt(consol, true);
			AssertEquals("consol's JK_ConsolMode change to LCL", Core.Constants.ContainerModes.LCL, consol.JK_ConsolMode);
			AssertEquals("but there is no corresponding Releated Party, so the consol's DepartureTransport change to empty", ZGuid.Empty, consol.JK_OA_DeparturePackCFSTransportAddress);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			sendingOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarderAddress = Factory.New<OrgAddress>();
			sendingForwarderAddress.OA_OH = sendingOrgHeader.PK;
			sendingForwarderAddress.OA_Address1 = "123";
			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;
			relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			consol.SendingForwarder.AddRelatedParty(relatedParty.PK, Core.Constants.ContainerTypes.FlatRack, "FWD", Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, GlbCompany.CurrentCompany);
			parentShipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			collection.SetupNewElementButDoNotAddIt(consol, true);
			AssertEquals("consol's JK_ConsolMode change to LCL", Core.Constants.ContainerModes.LCL, consol.JK_ConsolMode);
			AssertEquals("but there is a corresponding Releated Party, so the consol's DepartureTransport change to another", relatedParty.MainAddress.PK, consol.JK_OA_DeparturePackCFSTransportAddress);
		}
	}
}
