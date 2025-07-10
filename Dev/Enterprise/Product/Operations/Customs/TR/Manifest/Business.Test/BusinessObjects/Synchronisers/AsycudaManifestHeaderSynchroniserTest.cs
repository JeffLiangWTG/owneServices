using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	class AsycudaManifestHeaderSynchroniserTest : SynchroniserTestCase
	{
		public void TestManifestSyncForGRUPAJAndSEA()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AU2CO";
			consol.JK_RL_NKDischargePort = "TR9AK";

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "GRUPAJ";
			manifest.SetParent(consol);
			manifest.Synchroniser.SetEnabled(true, false);
			manifest.Synchroniser.Synchronise();

			CombineAssertions("Empty Values", () =>
			{
				AssertEquals("TransportType: default value should be empty", ZString.Empty, manifest.TransportType);
				AssertEquals("AMA_VesselName: default value should be empty", ZString.Empty, manifest.AMA_VesselName);
				AssertEquals("AMA_RN_NKConveyanceNationality: default value should be empty", ZString.Empty, manifest.AMA_RN_NKConveyanceNationality);
				AssertEquals("AMA_RL_NKPortOfLoading: default value should be empty", ZString.Empty, manifest.AMA_RL_NKPortOfLoading);
				AssertEquals("AMA_CustomsLoadPort: default value should be empty", ZString.Empty, manifest.AMA_CustomsLoadPort);
				AssertEquals("AMA_RL_NKPortOfFirstArrival: default value should be empty", ZString.Empty, manifest.AMA_RL_NKPortOfFirstArrival);
			});
		}

		public void TestManifestSync()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCodeList = Factory.New<ZZRefCusCodeListCombined>();
			officeCodeList.ZZD_Code = "TR340300";
			officeCodeList.ZZD_Description = "Test Office Code";
			officeCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			officeCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Turkey;
			officeCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			officeCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);
			var officeAttribute = officeCodeList.Attributes.AddNew();
			officeAttribute.ZZE_Value = "TRIST";
			officeAttribute.ZZE_ZXE_NKName = "Test Attribute by Office Code";
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AU2CO";
			consol.JK_RL_NKDischargePort = "TRIST";
			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = DateTime.Today;

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "DENITH";
			manifest.SetParent(consol);
			manifest.Synchroniser.SetEnabled(true, false);
			manifest.Synchroniser.Synchronise();

			CombineAssertions("Sync Values", () =>
			{
				AssertEquals("AMA_RN_NKCountry", "TR", manifest.AMA_RN_NKCountry);
				AssertEquals("AMA_E_ARV", DateTime.Today, manifest.AMA_E_ARV);
				AssertEquals("AMA_RL_NKPortOfLoading", "AU2CO", manifest.AMA_RL_NKPortOfLoading);
				AssertEquals("AMA_CustomsLoadPort", "AU2CO", manifest.AMA_CustomsLoadPort);
				AssertEquals("AMA_RL_NKPortOfDischarge", "TRIST", manifest.AMA_RL_NKPortOfDischarge);
				AssertEquals("AMA_CustomsDischargePort", ZString.Empty, manifest.AMA_CustomsDischargePort);
				AssertEquals("AMA_CustomsOffice", "TR340300", manifest.AMA_CustomsOffice);
			});
		}
	}
}
