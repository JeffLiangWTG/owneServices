using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOMCAquacultureFacility))]
	internal class USOMCAquacultureFacilityTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<USOMCAquacultureFacility>
	{
		protected override IEnumerable<USOMCAquacultureFacility> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT1";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var header = invoiceLine.OMCHeaders.AddNew();
			header.US_LineNo = 1;
			var additionalAquaculture = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "TESTAQU");
			if (additionalAquaculture == null)
			{
				additionalAquaculture = Factory.New<OrgHeader>();
				additionalAquaculture.OH_FullName = "ADDITIONAL AQUACULTURE";
				additionalAquaculture.OH_Code = "TESTAQU";
				additionalAquaculture.MainAddress.OA_Address1 = "ADDITIONAL AQUACULTURE";
				var additionalAddress = additionalAquaculture.MainAddress;
				additionalAddress.OA_Address1 = "Additional Address4";
				additionalAddress.OA_City = "NJ4";
				additionalAddress.OA_State = "J4";
				additionalAddress.OA_RL_NKRelatedPortCode = "USLAX";
				additionalAddress.OA_PostCode = "10114";
				additionalAddress.OA_CompanyNameOverride = "TEST OMC4";
			}
			var aquacultureFacility = header.AquacultureFacilities.AddNew();
			aquacultureFacility.US_OA_AquacultureFacility = additionalAquaculture.MainAddress.PK;
			yield return aquacultureFacility;
		}
	}
}
