using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderDocumentSupporter))]
	sealed class CusISFHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var header = Factory.New<CusISFHeader>();
			AssertEquals("ISF Header or Line Details cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJob), null));
		}

		public void TestFilterValueBKRCTY()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var header = Factory.New<CusISFHeader>();
			AssertEquals("Filter BKRCTY", "US", header.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTY));
		}

		public void TestGetContactOrganisation()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertNull("GetContactOrganisation on CNE doesnt blow up on null Importer", header.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader);
			OrgHeader consignee = Factory.New<OrgHeader>();
			header.BF_OH_Importer = consignee.PK;
			AssertEquals("GetContactOrganisation(CNE) returning the Importer", consignee, header.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader);
		}

		public void TestSupportedDataContexts()
		{
			var header = Factory.New<CusISFHeader>();
			AssertEquals("DataContext.GenericFreightJob is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.GenericFreightJob)));
		}

		public void TestTransportMode()
		{
			var header = Factory.New<CusISFHeader>();
			AssertEquals("TransportMode Sea", Core.Constants.TransportModes.Sea, header.DocumentSupporter.TransportMode);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<CusISFHeader>();
	}
}
