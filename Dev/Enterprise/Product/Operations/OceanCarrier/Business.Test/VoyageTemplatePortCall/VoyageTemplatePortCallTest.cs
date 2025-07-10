using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(VoyageTemplatePortCall))]
	sealed class VoyageTemplatePortCallTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH";

			var carrierService = Factory.New<CarrierService>();
			carrierService.CSV_Code = "CSV";
			carrierService.CSV_Name = "CSV Name";
			carrierService.CSV_OH_Carrier = orgHeader.PK;

			var voyageTemplate = Factory.New<VoyageTemplate>();
			voyageTemplate.VTV_CSV_Service = carrierService.PK;

			var portAddress = Factory.NewWithValidTestData<OrgAddress>();

			var voyageTemplatePortCall = Factory.New<VoyageTemplatePortCall>();
			voyageTemplatePortCall.VTP_VTV_VoyageTemplate = voyageTemplate.PK;
			voyageTemplatePortCall.VTP_OA_Port = portAddress.PK;
			voyageTemplatePortCall.VTP_Sequence = 1;

			return voyageTemplatePortCall;
		}
	}
}
