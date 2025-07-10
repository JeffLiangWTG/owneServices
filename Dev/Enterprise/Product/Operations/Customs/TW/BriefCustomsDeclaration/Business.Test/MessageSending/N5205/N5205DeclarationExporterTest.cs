using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(N5205DeclarationExporter))]
	sealed class N5205DeclarationExporterTest : TestCaseWithFactory
	{
		public void TestCustomsControlID()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OrgTest1";
			var mainAddress = orgHeader.MainAddress;
			mainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "123456", Core.Constants.CountryCodes.Taiwan);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			masterBill.ABL_OA_Shipper = mainAddress.PK;
			CombineAssertions(() =>
			{
				header.AMA_TransportMode = "SEA";
				var exporter = new N5205DeclarationExporter(masterBill);
				AssertEquals("SEA CustomsControlID", "123456", ((IPartyDetails)exporter).CustomsControlID);

				header.AMA_TransportMode = "AIR";
				exporter = new N5205DeclarationExporter(masterBill);
				AssertNullOrEmpty("AIR CustomsControlID", ((IPartyDetails)exporter).CustomsControlID);
			});
		}
	}
}
