using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMS))]
	internal class USDeclarationAMSTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<AMS>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<AMS>();
			originalBO.AMSLines.AddNew();

			var newBO = (AMS)originalBO.Clone();

			AssertEquals(1, newBO.AMSLines.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (AMS)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(AMS), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.AMSLines[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.AMSLines[0].Factory.GetHashCode());
		}

		public void TestPGALineReadOnly()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();

			ams.US_Program = AMSProgramList.Codes.MO1;
			ams.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			ams.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			Factory.Save();
			ams.OnLoaded();
			Assert(!ams.ReadOnly);

			ams.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			ams.OnLoaded();
			Assert(ams.ReadOnly);

			ams.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			ams.OnLoaded();
			Assert(!ams.ReadOnly);

			ams.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			ams.OnLoaded();
			Assert(ams.ReadOnly);

			ams.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			ams.OnLoaded();
			Assert(ams.ReadOnly);
		}

		public void TestUS_IntendedUseDescriptionReadOnly()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.MO2;
			ams.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._250000;
			AssertEquals("US_IntendedUseDescription_ReadOnly should be true when IntendedCode is not 980.000", true, ams.US_IntendedUseDescription_ReadOnly);
			AssertEquals("US_IntendedUseDescriptionInfo.ReadOnly should be true when IntendedCode is not 980.000", true, ams.US_IntendedUseDescriptionInfo.ReadOnly);
			AssertEquals("US_IntendedUseDescription should be empty", true, ams.US_IntendedUseDescription.IsEmpty);
			ams.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._980000;
			AssertEquals("US_IntendedUseDescription_ReadOnly should be false when IntendedCode is 980.000", false, ams.US_IntendedUseDescription_ReadOnly);
			AssertEquals("US_IntendedUseDescriptionInfo.ReadOnly should be false when IntendedCode is 980.000", false, ams.US_IntendedUseDescriptionInfo.ReadOnly);
		}

		public void TestAMSPropertyReadOnly()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.EG1;
			Assert(ams.US_USDAOrganicStandardInfo.ReadOnly);
			Assert(ams.US_EquivalentOrganicStandardInfo.ReadOnly);
			Assert(ams.US_CerNumberInfo.ReadOnly);
			Assert(ams.US_IsElecImageSubmittedInfo.ReadOnly);
			Assert(ams.US_DateInfo.ReadOnly);
			Assert(ams.US_OA_CertifyingBodyInfo.ReadOnly);
			Assert(ams.US_OA_RecipientInfo.ReadOnly);
			Assert(ams.US_NetWeightInfo.ReadOnly);
			Assert(ams.US_NetWeightUQInfo.ReadOnly);
			Assert(ams.US_RemarksInfo.ReadOnly);

			ams.US_Program = AMSProgramList.Codes.OR1;
			Assert(!ams.US_USDAOrganicStandardInfo.ReadOnly);
			Assert(!ams.US_EquivalentOrganicStandardInfo.ReadOnly);
			Assert(!ams.US_CerNumberInfo.ReadOnly);
			Assert(!ams.US_IsElecImageSubmittedInfo.ReadOnly);
			Assert(!ams.US_DateInfo.ReadOnly);
			Assert(!ams.US_OA_CertifyingBodyInfo.ReadOnly);
			Assert(!ams.US_OA_RecipientInfo.ReadOnly);
			Assert(!ams.US_NetWeightInfo.ReadOnly);
			Assert(!ams.US_NetWeightUQInfo.ReadOnly);
			Assert(!ams.US_RemarksInfo.ReadOnly);

			ams.US_Program = AMSProgramList.Codes.OR2;
			Assert(ams.US_USDAOrganicStandardInfo.ReadOnly);
			Assert(ams.US_EquivalentOrganicStandardInfo.ReadOnly);
			Assert(ams.US_CerNumberInfo.ReadOnly);
			Assert(!ams.US_IsElecImageSubmittedInfo.ReadOnly);
			Assert(ams.US_DateInfo.ReadOnly);
			Assert(ams.US_OA_CertifyingBodyInfo.ReadOnly);
			Assert(ams.US_OA_RecipientInfo.ReadOnly);
			Assert(!ams.US_NetWeightInfo.ReadOnly);
			Assert(!ams.US_NetWeightUQInfo.ReadOnly);
			Assert(ams.US_RemarksInfo.ReadOnly);
		}

		public void TestAMSDataContainers()
		{
			var containerType1 = Factory.New<RefContainer>();
			containerType1.RC_Code = "TST1";
			containerType1.RC_Length = 41m;
			containerType1.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;

			var containerType2 = Factory.New<RefContainer>();
			containerType2.RC_Code = "TST2";
			containerType2.RC_Length = 42m;
			containerType2.RC_ContainerType = Core.Constants.ContainerTypes.OpenTop;

			var containerType3 = Factory.New<RefContainer>();
			containerType3.RC_Code = "TST3";
			containerType3.RC_Length = 43m;
			containerType3.RC_ContainerType = Core.Constants.ContainerTypes.OpenTop;

			var container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_RC = containerType1.PK;

			var container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			container2.CO_RC = containerType2.PK;

			var container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";
			container3.CO_RC = containerType3.PK;

			var container4 = Declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CONT4";

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = false;
			containerPivots[2].IsForInvoiceLine = true;
			containerPivots[3].IsForInvoiceLine = true;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO7;
			var amsData = amsLine as IAMSData;
			AssertEquals(0, amsData.Containers.Count());

			foreach (string programType in new[] { AMSProgramList.Codes.MO1, AMSProgramList.Codes.MO5, AMSProgramList.Codes.EG1, AMSProgramList.Codes.PN1 })
			{
				amsLine.US_Program = programType;
				var amsData2 = amsLine as IAMSData;
				List<IContainerDetail> containers = amsData2.Containers.OrderBy(c => c.ContainerEquipmentID).ToList();

				AssertArrayEqualsByElements(new ZString[] { "CONT1", "CONT3", "CONT4" }, containers.Select(c => c.ContainerEquipmentID).ToArray());
				AssertArrayEqualsByElements(new ZShort[] { 41, 43, 0 }, containers.Select(c => c.ContainerLength).ToArray());
				AssertArrayEqualsByElements(new ZBool[] { true, false, false }, containers.Select(c => c.IsRefrigerated).ToArray());
			}
		}

		public void TestUS_ProgramChanged()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.MainAddress.OA_Address1 = "TEST MAIN ADDRESS";

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;

			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			amsLine.AMSLines.AddNew();
			AssertEquals(amsLine.AMSLines.Count, 1);
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			AssertEquals(amsLine.AMSLines.Count, 0);
			AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLine.US_IntendedUseCode);

			amsLine.US_Program = ZString.Empty;
			amsLine.US_IntendedUseCode = ZString.Empty;

			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLine.US_IntendedUseCode);

			amsLine.US_IntendedUseCode = ZString.Empty;
			amsLine.US_Program = AMSProgramList.Codes.MO2;
			AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLine.US_IntendedUseCode);

			amsLine.US_IntendedUseCode = ZString.Empty;
			amsLine.US_Program = AMSProgramList.Codes.MO5;
			AssertEquals(AMSIntendedUseCodesList.Codes._230000, amsLine.US_IntendedUseCode);

			amsLine.US_IntendedUseCode = ZString.Empty;
			amsLine.US_Program = AMSProgramList.Codes.MO6;
			AssertEquals(AMSIntendedUseCodesList.Codes._250000, amsLine.US_IntendedUseCode);

			amsLine.US_CommercialDescription = "A";
			amsLine.US_Program = AMSProgramList.Codes.MO7;
			AssertEquals(ZString.Empty, amsLine.US_IntendedUseCode);
			AssertEquals(ZString.Empty, amsLine.US_CommercialDescription);

			amsLine.US_CommercialDescription = "A";
			amsLine.US_Program = AMSProgramList.Codes.OR2;
			AssertEquals(ZString.Empty, amsLine.US_IntendedUseCode);
			AssertEquals(ZString.Empty, amsLine.US_CommercialDescription);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertEquals(orgHeader.PK, amsLine.RecipientOrgPK);
			amsLine.US_Program = AMSProgramList.Codes.EG1;
			AssertEquals(ZGuid.Empty, amsLine.RecipientOrgPK);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			amsLine.US_USDAOrganicStandard = true;
			amsLine.US_EquivalentOrganicStandard = true;
			amsLine.US_CerNumber = "Test";
			amsLine.US_IsElecImageSubmitted = true;
			amsLine.US_Date = ZDateTime.Today;
			amsLine.US_OA_CertifyingBody = orgHeader.PK;
			amsLine.CertifyingBodyOrgPK = orgHeader.MainAddress.PK;
			amsLine.US_OA_Recipient = orgHeader.PK;
			amsLine.RecipientOrgPK = orgHeader.MainAddress.PK;
			amsLine.US_NetWeight = 1m;
			amsLine.US_NetWeightUQ = "KG";
			amsLine.US_Remarks = "Test";
			amsLine.US_Program = AMSProgramList.Codes.EG1;
			AssertEquals(ZBool.False, amsLine.US_USDAOrganicStandard);
			AssertEquals(ZBool.False, amsLine.US_EquivalentOrganicStandard);
			AssertEquals(ZString.Empty, ((IAMSData)amsLine).CerType);
			AssertEquals(ZString.Empty, amsLine.US_CerNumber);
			AssertEquals(ZBool.False, amsLine.US_IsElecImageSubmitted);
			AssertEquals(ZString.Empty, ((IAMSData)amsLine).DateType);
			AssertEquals(ZDateTime.Empty, amsLine.US_Date);
			AssertEquals(ZGuid.Empty, amsLine.US_OA_CertifyingBody);
			AssertEquals(ZGuid.Empty, amsLine.US_OA_Recipient);
			AssertEquals(ZDecimal.Zero, amsLine.US_NetWeight);
			AssertEquals(ZString.Empty, amsLine.US_NetWeightUQ);
			AssertEquals(ZString.Empty, amsLine.US_Remarks);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			amsLine.US_USDAOrganicStandard = true;
			amsLine.US_EquivalentOrganicStandard = true;
			amsLine.US_CerNumber = "Test";
			amsLine.US_IsElecImageSubmitted = true;
			amsLine.US_Date = ZDateTime.Today;
			amsLine.US_OA_CertifyingBody = orgHeader.PK;
			amsLine.CertifyingBodyOrgPK = orgHeader.MainAddress.PK;
			amsLine.US_OA_Recipient = orgHeader.PK;
			amsLine.RecipientOrgPK = orgHeader.MainAddress.PK;
			amsLine.US_NetWeight = 1m;
			amsLine.US_NetWeightUQ = "KG";
			amsLine.US_Remarks = "Test";
			amsLine.US_Program = AMSProgramList.Codes.OR2;
			AssertEquals(ZBool.False, amsLine.US_USDAOrganicStandard);
			AssertEquals(ZBool.False, amsLine.US_EquivalentOrganicStandard);
			AssertEquals(ZString.Empty, ((IAMSData)amsLine).CerType);
			AssertEquals(ZString.Empty, amsLine.US_CerNumber);
			AssertEquals(ZBool.False, amsLine.US_IsElecImageSubmitted);
			AssertEquals(ZString.Empty, ((IAMSData)amsLine).DateType);
			AssertEquals(ZDateTime.Empty, amsLine.US_Date);
			AssertEquals(ZGuid.Empty, amsLine.US_OA_CertifyingBody);
			AssertEquals(ZGuid.Empty, amsLine.US_OA_Recipient);
			AssertEquals(ZDecimal.Zero, amsLine.US_NetWeight);
			AssertEquals(ZString.Empty, amsLine.US_NetWeightUQ);
			AssertEquals(ZString.Empty, amsLine.US_Remarks);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();

			ICusAddInfoTypeSupporter supporter = amsLine;
			supporter.AssertType(typeof(AMSLine), CusAddInfoTypeAttribute.Codes.USAMSLine);
			supporter.AssertType(null, "ZZ!");

			var line = amsLine.AMSLines.AddNew();
			line.US_CertNumber = "TEST";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			CusAddInfo addInfo = newFactory.Load<CusAddInfo>(line.PK);
			AssertEquals(typeof(AMSLine), addInfo.GetType());
		}

		public void TestImporter()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.MainAddress.OA_Address1 = "TEST MAIN ADDRESS";
			Declaration.IOROrgPK = orgHeader.PK;
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			OrgCusCode cusCode1 = orgHeader.CustomsCodes.AddNew("AMS", "123");
			Factory.Save();

			var importer = ((IAMSData)amsLine).Importer;
			AssertEquals("TEST MAIN ADDRESS", importer.CompanyAddress.AddressLine1);

			AssertEquals(ZString.Empty, importer.IDNumber);
			AssertEquals(ZString.Empty, importer.IDType);

			amsLine.US_Program = AMSProgramList.Codes.MO4;
			importer = ((IAMSData)amsLine).Importer;
			AssertEquals("123", importer.IDNumber);
			AssertEquals("331", importer.IDType);
		}

		public void TestConsignee()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG2";
			orgHeader.MainAddress.OA_Address1 = "TEST MAIN ADDRESS";
			var newAddress = orgHeader.Addresses.AddNew();
			newAddress.OA_Address1 = "IAN TEST ADDRESS";

			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			invoiceLine.JI_OA_ConsigneeAddress = newAddress.PK;
			OrgCusCode cusCode1 = orgHeader.CustomsCodes.AddNew("AMS", "456");

			var consignee = ((IAMSData)amsLine).Consignee;
			AssertEquals("IAN TEST ADDRESS", consignee.CompanyAddress.AddressLine1);

			invoiceLine.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
			consignee = ((IAMSData)amsLine).Consignee;
			AssertEquals("TEST MAIN ADDRESS", consignee.CompanyAddress.AddressLine1);

			AssertEquals(ZString.Empty, consignee.IDNumber);
			AssertEquals(ZString.Empty, consignee.IDType);

			amsLine.US_Program = AMSProgramList.Codes.MO4;
			consignee = ((IAMSData)amsLine).Consignee;
			AssertEquals("456", consignee.IDNumber);
			AssertEquals("331", consignee.IDType);
		}

		public void TestClone()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_USDAOrganicStandard = false;
			amsLine.US_EquivalentOrganicStandard = false;
			amsLine.US_CerNumber = "test123";
			amsLine.US_IsElecImageSubmitted = false;
			amsLine.US_Date = new ZDateTime(2016, 5, 5);
			amsLine.US_OA_CertifyingBody = ZGuid.Empty;
			amsLine.US_OA_Recipient = ZGuid.Empty;
			amsLine.US_NetWeight = 12m;
			amsLine.US_NetWeightUQ = "KG";
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			amsLine.US_CommercialDescription = "COMDESC";
			amsLine.US_Remarks = "Test";

			var amsChild = amsLine.AMSLines.AddNew();
			amsChild.US_ProductNumber = "50401704";
			amsChild.US_OA_Applicant = ZGuid.Empty;
			amsChild.US_OA_GoodsLocation = ZGuid.Empty;
			amsChild.US_Packages = 5m;
			amsChild.US_PackagesUQ = "BBL";
			amsChild.US_PackageWeight = 7m;
			amsChild.US_PackageWeightUQ = "KG";
			amsChild.US_QtyPerPackage = 2m;
			amsChild.US_QtyPerPackageUQ = "CS";
			amsChild.US_NetWeight = 8m;
			amsChild.US_NetWeightUQ = "KG";
			amsChild.US_InspecDateTime = new ZDateTime(2016, 5, 5);
			amsChild.US_InspecRemarks = "NONE";

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var amsLineNew = invoiceLine.AMSLines.AddNew();

			AssertEquals("Program must be the same", amsLine.US_Program, amsLineNew.US_Program);
			AssertEquals("USDA Organic Standard must be the same", amsLine.US_USDAOrganicStandard, amsLineNew.US_USDAOrganicStandard);
			AssertEquals("Equivalent Organic Standard must be the same", amsLine.US_EquivalentOrganicStandard, amsLineNew.US_EquivalentOrganicStandard);
			AssertEquals("Certificate Number must be the same", amsLine.US_CerNumber, amsLineNew.US_CerNumber);
			AssertEquals("Electronic Image Submitted must be the same", amsLine.US_IsElecImageSubmitted, amsLineNew.US_IsElecImageSubmitted);
			AssertEquals("Date must be the same", amsLine.US_Date, amsLineNew.US_Date);
			AssertEquals("Certifying Body must be the same", amsLine.US_OA_CertifyingBody, amsLineNew.US_OA_CertifyingBody);
			AssertEquals("Recipient must be the same", amsLine.US_OA_Recipient, amsLineNew.US_OA_Recipient);
			AssertEquals("Net Weight must be the same", amsLine.US_NetWeight, amsLineNew.US_NetWeight);
			AssertEquals("Net Weight UQ must be the same", amsLine.US_NetWeightUQ, amsLineNew.US_NetWeightUQ);
			AssertEquals("IntendedUseCode must be the same", amsLine.US_IntendedUseCode, amsLineNew.US_IntendedUseCode);
			AssertEquals("CommercialDescription must be the same", amsLine.US_CommercialDescription, amsLineNew.US_CommercialDescription);
			AssertEquals("Remarks must be the same", amsLine.US_Remarks, amsLineNew.US_Remarks);

			AssertEquals("AMSLines: AMSProductNumber must be the same", amsLine.AMSLines[0].US_ProductNumber, amsLineNew.AMSLines[0].US_ProductNumber);
			AssertEquals("AMSLines: OH_Applicant must be the same", amsLine.AMSLines[0].US_OA_Applicant, amsLineNew.AMSLines[0].US_OA_Applicant);
			AssertEquals("AMSLines: OH_GoodsLocation must be the same", amsLine.AMSLines[0].US_OA_GoodsLocation, amsLineNew.AMSLines[0].US_OA_GoodsLocation);
			AssertEquals("AMSLines: Packages must not be the copied", ZDecimal.Zero, amsLineNew.AMSLines[0].US_Packages);
			AssertEquals("AMSLines: PackagesUQ must not be the copied", ZString.Empty, amsLineNew.AMSLines[0].US_PackagesUQ);
			AssertEquals("AMSLines: PackageWeight must not be the copied", ZDecimal.Zero, amsLineNew.AMSLines[0].US_PackageWeight);
			AssertEquals("AMSLines: PackageWeightUQ must not be the copied", ZString.Empty, amsLineNew.AMSLines[0].US_PackageWeightUQ);
			AssertEquals("AMSLines: QtyPerPackage must not be the copied", ZDecimal.Zero, amsLineNew.AMSLines[0].US_QtyPerPackage);
			AssertEquals("AMSLines: QtyPerPackageUQ must not be the copied", ZString.Empty, amsLineNew.AMSLines[0].US_QtyPerPackageUQ);
			AssertEquals("AMSLines: NetWeight must not be the copied", ZDecimal.Zero, amsLineNew.AMSLines[0].US_NetWeight);
			AssertEquals("AMSLines: NetWeightUQ must not be the copied", ZString.Empty, amsLineNew.AMSLines[0].US_NetWeightUQ);
			AssertEquals("AMSLines: InspecDateTime must not be the copied", ZDateTime.Empty, amsLineNew.AMSLines[0].US_InspecDateTime);
			AssertEquals("AMSLines: InspecRemarks must not be the copied", ZString.Empty, amsLineNew.AMSLines[0].US_InspecRemarks);
		}

		public void TestCerType()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertEquals(ZString.Empty, ((IAMSData)amsLine).CerType);
			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertEquals(AMSCertTypeList.Codes.AM1, ((IAMSData)amsLine).CerType);
		}

		public void TestDateType()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertEquals(ZString.Empty, ((IAMSData)amsLine).DateType);
			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertEquals(AMSDateType.Codes.DateIssuedOrSigned, ((IAMSData)amsLine).DateType);
		}

		public void TestIntendedUseCodeAndDescription()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;

			var amsData = (IAMSData)amsLine;
			AssertEquals("amsData.IntendedUseCode", AMSIntendedUseCodesList.Codes._230000, amsData.IntendedUseCode);
			AssertEquals("amsData.IntendedUseCodeDescription", ZString.Empty, amsData.IntendedUseCodeDescription);

			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._980000;
			amsLine.US_IntendedUseDescription = "TEST DESC";
			AssertEquals("amsData.IntendedUseCode", AMSIntendedUseCodesList.Codes._980000, amsData.IntendedUseCode);
			AssertEquals("amsData.IntendedUseCodeDescription", "TEST DESC", amsData.IntendedUseCodeDescription);
		}

		public void TestDefaultNetWeightWhenOR1()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 132m;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertEquals(ZString.Empty, amsLine.US_NetWeightUQ);
			AssertEquals(0m, amsLine.US_NetWeight);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertEquals("KG", amsLine.US_NetWeightUQ);
			AssertEquals(132m, amsLine.US_NetWeight);
		}

		public void TestDefaultNetWeightWhenOR2()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 132m;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertEquals(ZString.Empty, amsLine.US_NetWeightUQ);
			AssertEquals(0m, amsLine.US_NetWeight);

			amsLine.US_Program = AMSProgramList.Codes.OR2;
			AssertEquals("KG", amsLine.US_NetWeightUQ);
			AssertEquals(132m, amsLine.US_NetWeight);
		}

		public void TestProductType()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 132m;
			var amsLine = invoiceLine.AMSLines.AddNew();

			amsLine.US_Program = AMSProgramList.Codes.EG1;
			AssertEquals("Should Be EG1", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.EG1, amsLine.ProductType);
			amsLine.US_Program = AMSProgramList.Codes.EG2;
			AssertEquals("Should Be EG1", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.EG1, amsLine.ProductType);

			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertEquals("Should Be OTH", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH, amsLine.ProductType);
			amsLine.US_Program = AMSProgramList.Codes.MO5;
			AssertEquals("Should Be OTH", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH, amsLine.ProductType);

			amsLine.US_Program = AMSProgramList.Codes.MO6;
			AssertEquals("Should Be MO6", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6, amsLine.ProductType);

			amsLine.US_Program = AMSProgramList.Codes.MO4;
			AssertEquals("Should Be ALL", USAMSLineProductNumberCollection.All, amsLine.ProductType);

			amsLine.US_Program = AMSProgramList.Codes.PN1;
			AssertEquals("Should Be PN1", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1, amsLine.ProductType);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertEquals("Should Be OR1", Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1, amsLine.ProductType);

			amsLine.US_Program = AMSProgramList.Codes.MO2;
			AssertEquals("Should Be EMPTY", "", amsLine.ProductType);
			amsLine.US_Program = AMSProgramList.Codes.MO3;
			AssertEquals("Should Be EMPTY", "", amsLine.ProductType);
			amsLine.US_Program = AMSProgramList.Codes.MO7;
			AssertEquals("Should Be EMPTY", "", amsLine.ProductType);
			amsLine.US_Program = AMSProgramList.Codes.MO8;
			AssertEquals("Should Be EMPTY", "", amsLine.ProductType);
		}

		public void TestDeleteLotCodes()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.OR2;
			var lotCode = amsLine.LotCodes.AddNew();
			Assert(!lotCode.IsDeleted);
			amsLine.US_Program = AMSProgramList.Codes.OR1;
			Assert(lotCode.IsDeleted);
			amsLine.US_Program = AMSProgramList.Codes.OR2;
			var lotCode1 = amsLine.LotCodes.AddNew();
			Assert(!lotCode1.IsDeleted);
			amsLine.Delete();
			Assert(lotCode1.IsDeleted);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();

			ICusCodeDataTypeSupporter supporter = amsLine;
			supporter.AssertType(typeof(AMSLotCode), CusCodeDataTypeList.Codes.AMSLotCode);
			supporter.AssertType(null, "ZZ!");

			var lotCode = amsLine.LotCodes.AddNew();
			lotCode.CY_Code = "TEST";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			CusCodeData cusCodeData = newFactory.Load<CusCodeData>(lotCode.PK);
			AssertEquals(typeof(AMSLotCode), cusCodeData.GetType());
		}

		public void TestIAMSDataAMSLotCodes()
		{
			var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			var lotCode = ams.LotCodes.AddNew();
			lotCode.CY_Code = "A";
			lotCode.CY_Data = "B";
			AssertEquals(1, ((IAMSData)ams).AMSLotCodes.Count());
			var amsLotCode = ((IAMSData)ams).AMSLotCodes.First();
			AssertEquals(lotCode.CY_Code, amsLotCode.Code);
			AssertEquals(lotCode.CY_Data, amsLotCode.Value);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected override IEnumerable<AMS> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (AMS)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			return invoiceLine.AMSLines.AddNew();
		}
	}
}
