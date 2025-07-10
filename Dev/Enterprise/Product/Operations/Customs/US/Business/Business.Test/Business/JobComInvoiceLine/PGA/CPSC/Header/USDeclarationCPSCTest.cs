using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCHeader))]
	internal class USDeclarationCPSCTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CPSCHeader>
	{
		public void TestPropertiesWhenUS_ProcessingCodeChanged()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			cpsc.US_ProductIDType = "Type";
			cpsc.US_ProductID = "ProductID";
			cpsc.US_SKUProductCode = "SKUProductCode";
			cpsc.US_TradeBrandName = "TradeBrandName";
			cpsc.US_ProductName = "ProductName";
			cpsc.US_ModelNumber = "ModelNumber";
			cpsc.US_SerialNumber = "SerialNumber";
			cpsc.US_RegisteredNumber = "RegisteredNumber";
			cpsc.US_AltenateID = "AltenateID";
			cpsc.US_ModelColor = "ModelColor";
			cpsc.US_ModelDescription = "ModelDescription";
			cpsc.US_ModelStyle = "ModelStyle";
			cpsc.US_ManufacturerMonthAndYear = "123456";
			cpsc.US_ManufacturerRegistryID = "RegistryID";
			cpsc.US_NoLabTestingRequired = true;
			cpsc.US_RuleCodes = "111,222";
			cpsc.US_CertificateExists = "1";
			cpsc.Lots.AddNew();
			cpsc.RuleAndLabs.AddNew();
			AssertEquals("US_ProcessingCode", CPSCProcessingCodeList.Codes.FGC, cpsc.US_ProcessingCode);
			AssertEquals("US_ProductIDType", "Type", cpsc.US_ProductIDType);
			AssertEquals("US_ProductID", "ProductID", cpsc.US_ProductID);
			AssertEquals("US_SKUProductCode", "SKUProductCode", cpsc.US_SKUProductCode);
			AssertEquals("US_TradeBrandName", "TradeBrandName", cpsc.US_TradeBrandName);
			AssertEquals("US_ProductName", "ProductName", cpsc.US_ProductName);
			AssertEquals("US_ModelNumber", "ModelNumber", cpsc.US_ModelNumber);
			AssertEquals("US_SerialNumber", "SerialNumber", cpsc.US_SerialNumber);
			AssertEquals("US_RegisteredNumber", "RegisteredNumber", cpsc.US_RegisteredNumber);
			AssertEquals("US_AltenateID", "AltenateID", cpsc.US_AltenateID);
			AssertEquals("US_ModelColor", "ModelColor", cpsc.US_ModelColor);
			AssertEquals("US_ModelDescription", "ModelDescription", cpsc.US_ModelDescription);
			AssertEquals("US_ModelStyle", "ModelStyle", cpsc.US_ModelStyle);
			AssertEquals("US_ManufacturerMonthAndYear", "123456", cpsc.US_ManufacturerMonthAndYear);
			AssertEquals("US_ManufacturerRegistryID", "RegistryID", cpsc.US_ManufacturerRegistryID);
			AssertEquals("US_CertificateExists", "1", cpsc.US_CertificateExists);
			AssertEquals("US_NoLabTestingRequired", true, cpsc.US_NoLabTestingRequired);
			AssertEquals("US_RuleCodes", "111,222", cpsc.US_RuleCodes);
			AssertEquals("Lots.Count", 1, cpsc.Lots.Count);
			AssertEquals("RuleAndLabs.Count", 1, cpsc.RuleAndLabs.Count);

			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			cpsc.US_ReferenceNumber = "ReferenceNumber";
			cpsc.US_ProductCode = "ProductCode";
			cpsc.US_ProductCodeVersionNumber = "VersionNumber";
			AssertEquals("US_ProcessingCode", CPSCProcessingCodeList.Codes.REF, cpsc.US_ProcessingCode);
			AssertEquals("US_ReferenceNumber", "ReferenceNumber", cpsc.US_ReferenceNumber);
			AssertEquals("US_ProductCode", "ProductCode", cpsc.US_ProductCode);
			AssertEquals("US_ProductCodeVersionNumber", "VersionNumber", cpsc.US_ProductCodeVersionNumber);
			AssertEquals("US_ProductIDType", ZString.Empty, cpsc.US_ProductIDType);
			AssertEquals("US_ProductID", ZString.Empty, cpsc.US_ProductID);
			AssertEquals("US_SKUProductCode", ZString.Empty, cpsc.US_SKUProductCode);
			AssertEquals("US_TradeBrandName", ZString.Empty, cpsc.US_TradeBrandName);
			AssertEquals("US_ProductName", ZString.Empty, cpsc.US_ProductName);
			AssertEquals("US_ModelNumber", ZString.Empty, cpsc.US_ModelNumber);
			AssertEquals("US_SerialNumber", ZString.Empty, cpsc.US_SerialNumber);
			AssertEquals("US_RegisteredNumber", ZString.Empty, cpsc.US_RegisteredNumber);
			AssertEquals("US_AltenateID", ZString.Empty, cpsc.US_AltenateID);
			AssertEquals("US_ModelColor", ZString.Empty, cpsc.US_ModelColor);
			AssertEquals("US_ModelDescription", ZString.Empty, cpsc.US_ModelDescription);
			AssertEquals("US_ModelStyle", ZString.Empty, cpsc.US_ModelStyle);
			AssertEquals("US_ManufacturerMonthAndYear", ZString.Empty, cpsc.US_ManufacturerMonthAndYear);
			AssertEquals("US_ManufacturerRegistryID", ZString.Empty, cpsc.US_ManufacturerRegistryID);
			AssertEquals("US_CertificateExists", ZString.Empty, cpsc.US_CertificateExists);
			AssertEquals("US_NoLabTestingRequired", false, cpsc.US_NoLabTestingRequired);
			AssertEquals("US_RuleCodes", ZString.Empty, cpsc.US_RuleCodes);
			AssertEquals("Lots.Count", 0, cpsc.Lots.Count);
			AssertEquals("RuleAndLabs.Count", 0, cpsc.RuleAndLabs.Count);

			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			AssertEquals("US_ProcessingCode", CPSCProcessingCodeList.Codes.FCP, cpsc.US_ProcessingCode);
			AssertEquals("US_ReferenceNumber", ZString.Empty, cpsc.US_ReferenceNumber);
			AssertEquals("US_ProductCode", ZString.Empty, cpsc.US_ProductCode);
			AssertEquals("US_ProductCodeVersionNumber", ZString.Empty, cpsc.US_ProductCodeVersionNumber);
		}

		public void TestProperties()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			AssertEquals("US_ProductCode: ReadOnlyMember", "US_ProductCode_ReadOnly", cpsc.US_ProductCodeInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("US_ProductCodeVersionNumber: ReadOnlyMember", "US_ProductCodeVersionNumber_ReadOnly", cpsc.US_ProductCodeVersionNumberInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("US_ManufacturerMonthAndYear: ReadOnlyMember", "IsREF", cpsc.US_ManufacturerMonthAndYearInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("US_ManufacturerRegistryID: ReadOnlyMember", "IsREF", cpsc.US_ManufacturerRegistryIDInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("US_OA_CertifyingEntityAddress: ReadOnlyMember", "IsREF", cpsc.US_OA_CertifyingEntityAddressInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("US_OA_CertifyingEntityAddress: List", "US_OA_CertifyingEntityAddress_ZAddress.OrgAddress_List", cpsc.US_OA_CertifyingEntityAddressInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("CertifyingEntityOrgPK: ReadOnlyMember", "IsREF", cpsc.CertifyingEntityOrgPKInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("CertifyingEntityOrgPK: List", "AddInfoLookups.Organizations", cpsc.CertifyingEntityOrgPKInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("US_OA_ContactPointAddress: ReadOnlyMember", "IsREF", cpsc.US_OA_ContactPointAddressInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("US_OA_ContactPointAddress: List", "US_OA_ContactPointAddress_ZAddress.OrgAddress_List", cpsc.US_OA_ContactPointAddressInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("ContactPointOrgPK: ReadOnlyMember", "IsREF", cpsc.ContactPointOrgPKInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			AssertEquals("ContactPointOrgPK: List", "AddInfoLookups.Organizations", cpsc.ContactPointOrgPKInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("US_RuleCodes: ReadOnlyMember", "US_RuleCodes_ReadOnly", cpsc.US_RuleCodesInfo.GetAttribute<ReadOnlyMemberAttribute>().Member);
			cpsc.US_NoLabTestingRequired = false;
			Assert("US_RuleCodes: ReadOnly", cpsc.US_RuleCodesInfo.ReadOnly);
			cpsc.US_NoLabTestingRequired = true;
			Assert("US_RuleCodes: Not ReadOnly", !cpsc.US_RuleCodesInfo.ReadOnly);
		}

		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<CPSCHeader>();
			originalBO.Lots.AddNew();
			originalBO.RuleAndLabs.AddNew();

			var newBO = (CPSCHeader)originalBO.Clone();

			AssertEquals(1, newBO.Lots.Count);
			AssertEquals(1, newBO.RuleAndLabs.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (CPSCHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(CPSCHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Lots[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.RuleAndLabs[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Lots[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.RuleAndLabs[0].Factory.GetHashCode());
		}

		public void TestICPSCHeaderMembers()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			cpsc.US_IntendedUseDescription = "THIS IS VERY LONG DESC";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			var rule1 = cpsc.RuleAndLabs.AddNew();
			rule1.US_OA_SafetyTestLocationAddress = orgHeader.MainAddress.PK;
			rule1.US_CPSCAccreditedLabID = "KNZ";
			rule1.US_RuleCodes = "1,2,3,4";

			var rule2 = cpsc.RuleAndLabs.AddNew();
			rule2.US_OA_SafetyTestLocationAddress = orgHeader.MainAddress.PK;
			rule2.US_CPSCAccreditedLabID = "KNZ";
			rule2.US_RuleCodes = "5,6,7";

			var irule = (ICPSCRulesAndLabs)rule1;
			AssertEquals(irule.RuleCodes, "1,2,3,4,5,6,7");

			irule = rule2;
			AssertEquals(irule.RuleCodes, "1,2,3,4,5,6,7");

			var iheader = (ICPSCHeader)cpsc;
			AssertEquals(iheader.RulesAndLabs.Count(), 1);
			AssertEquals("iheader.IntendedUseDescription", "THIS IS VERY LONG DES", iheader.IntendedUseDescription);
		}

		public void TestPGALineReadOnly()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			cpsc.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._025000;
			cpsc.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			Factory.Save();
			cpsc.OnLoaded();
			Assert(!cpsc.ReadOnly);

			cpsc.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			cpsc.OnLoaded();
			Assert(cpsc.ReadOnly);

			cpsc.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			cpsc.OnLoaded();
			Assert(!cpsc.ReadOnly);

			cpsc.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			cpsc.OnLoaded();
			Assert(cpsc.ReadOnly);

			cpsc.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			cpsc.OnLoaded();
			Assert(cpsc.ReadOnly);
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

		protected override IEnumerable<CPSCHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (CPSCHeader)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			return cpsc;
		}
	}
}
