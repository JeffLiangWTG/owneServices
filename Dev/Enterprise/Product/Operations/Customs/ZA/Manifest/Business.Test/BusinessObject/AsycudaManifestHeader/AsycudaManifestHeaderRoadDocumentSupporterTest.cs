using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	[TestedType(typeof(AsycudaManifestHeaderDocumentSupporter))]
	class AsycudaManifestHeaderRoadDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "TESTBILL1";
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "SINGLE PACK FOR TEST";
			var person = header.Persons.AddNew();
			person.CPN_PER_Person = glbPerson.PK;
			var personCountry = person.Countries.AddNew();
			personCountry.CPC_Type = "DSA";
			personCountry.CPC_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			personCountry.CPC_Value = "SD";
			return header;
		}
	}
}
