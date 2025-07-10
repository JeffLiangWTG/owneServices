using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USHFCHeader))]
	public class USHFCHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<USHFCHeader>
	{
		public void TestUSHFCDetailsShouldBeClearedWhenASHRAENumberIsNotEmptyAndViceVersa()
		{
			var header = (USHFCHeader)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("123", header.US_ASHRAENumber);
			header.USHFCDetails.AddNew();
			AssertEquals(ZString.Empty, header.US_ASHRAENumber);
			AssertEquals(1, header.USHFCDetails.Count);
			header.US_ASHRAENumber = "234";
			AssertEquals(0, header.USHFCDetails.Count);
		}

		public void TestProperties()
		{
			var header = (USHFCHeader)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("AddInfoLookups: Type", typeof(USHFCHeaderAddInfoLookups), header.AddInfoLookups.GetType());
			AssertEquals("AddInfoValidation: Type", typeof(USHFCHeaderAddInfoValidation), header.AddInfoValidation.GetType());
		}

		public void TestResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCHeader), nameof(USHFCHeader.US_LineNo), false, x => x.Caption == "Line No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCHeader), nameof(USHFCHeader.US_ASHRAENumber), false, x => x.Caption == "ASHRAE Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCHeader), nameof(USHFCHeader.US_NetWeight), false, x => x.Caption == "Net Weight(kg)");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCHeader), nameof(USHFCHeader.US_CertifyingIndividual), false, x => x.Caption == "Certifying Individual");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCHeader), nameof(USHFCHeader.US_HFCImageSent), false, x => x.Caption == "Elec. Image Submitted");
		}

		public void TestIHFCHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Importer";
			var importerAllocation = importerContact.Allocations.AddNew();
			importerAllocation.PC_Type = ContactAllocationType.USPGA;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "Consignee";
			var consigneeAllocation = consigneeContact.Allocations.AddNew();
			consigneeAllocation.PC_Type = ContactAllocationType.USPGA;
			Factory.Save();
			var header = (USHFCHeader)GetNewBusinessObjectForDeleteTest(Factory);
			header.US_LineNo = 1;
			header.US_CertifyingIndividual = EntityRoleCodeList.Codes.Importer;
			header.US_NetWeight = 2;
			var invoiceLine = header.InvoiceLine;
			var declaration = invoiceLine.Declaration;
			declaration.JE_OA_DeclarantAddress = importer.MainAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
			invoiceLine.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;

			var iHeader = (IHFCHeader)header;
			AssertEquals("ASHRAENumber", "123", iHeader.ASHRAENumber);
			AssertEquals("LineNo", 1, iHeader.LineNo);
			AssertEquals("CertifyingIndividual", EntityRoleCodeList.Codes.Importer, iHeader.CertifyingIndividual);
			AssertEquals("NetWeight", 2m, iHeader.NetWeight);
			AssertEquals("Importer", "Importer", iHeader.Importer.Name);
			AssertEquals("Consignee", "Consignee", iHeader.Consignee.Name);
			AssertEquals("CustomsBroker", invoiceLine, iHeader.CustomsBroker);
			AssertEquals("HFCDetails", 0, iHeader.HFCDetails.Count());
			AssertEquals("CusContainers", "CONTAINER1", iHeader.CusContainers.First().ContainerEquipmentID);
		}

		public void TestPGALineReadOnly()
		{
			var header = (USHFCHeader)GetNewBusinessObjectForDeleteTest(Factory);
			header.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			Factory.Save();
			header.OnLoaded();
			Assert(!header.ReadOnly);

			header.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			header.OnLoaded();
			Assert(header.ReadOnly);

			header.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			header.OnLoaded();
			Assert(!header.ReadOnly);

			header.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			header.OnLoaded();
			Assert(header.ReadOnly);

			header.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			header.OnLoaded();
			Assert(header.ReadOnly);
		}

		public void TestDelete()
		{
			var header = (USHFCHeader)GetNewBusinessObjectForDeleteTest(Factory);
			var detail = header.USHFCDetails.AddNew();
			header.Delete();
			Assert(header.IsDeleted);
			Assert(detail.IsDeleted);
		}

		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<USHFCHeader>();
			originalBO.USHFCDetails.AddNew();

			var newBO = (USHFCHeader)originalBO.Clone();

			AssertEquals(1, newBO.USHFCDetails.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (USHFCHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(USHFCHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.USHFCDetails[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.USHFCDetails[0].Factory.GetHashCode());
		}

		public void TestClone()
		{
			var hfc = Factory.New<USHFCHeader>();
			hfc.US_CertifyingIndividual = EntityRoleCodeList.Codes.Consignee;
			hfc.US_NetWeight = 50m;
			hfc.US_HFCImageSent = true;
			var detail = hfc.USHFCDetails.AddNew();
			detail.US_LPCONumber = "4000";
			detail.US_NameOfActiveIngredient = "KG";
			detail.US_ActiveIngredientPercentage = 100m;

			var clonedHFC = (USHFCHeader)hfc.Clone();
			AssertEquals("US_CertifyingIndividual", EntityRoleCodeList.Codes.Consignee, clonedHFC.US_CertifyingIndividual);
			AssertEquals("US_NetWeight", 50m, clonedHFC.US_NetWeight);
			AssertEquals("US_HFCImageSent should not be cloned", false, clonedHFC.US_HFCImageSent);

			AssertEquals(1, clonedHFC.USHFCDetails.Count);

			var clonedDetail = clonedHFC.USHFCDetails[0];
			AssertEquals("US_LPCONumber", "4000", clonedDetail.US_LPCONumber);
			AssertEquals("US_NameOfActiveIngredient", "KG", clonedDetail.US_NameOfActiveIngredient);
			AssertEquals("US_ActiveIngredientPercentage", 100m, clonedDetail.US_ActiveIngredientPercentage);
		}

		protected override IEnumerable<USHFCHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (USHFCHeader)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var head = invoiceLine.USHFCHeaders.AddNew();
			head.US_ASHRAENumber = "123";
			return head;
		}
	}
}
