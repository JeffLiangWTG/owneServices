using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FWSHeader))]
	public class FWSHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<FWSHeader>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<FWSHeader>();
			originalBO.Licenses.AddNew();

			var newBO = (FWSHeader)originalBO.Clone();
			AssertEquals(1, newBO.Licenses.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (FWSHeader)originalBO.Clone(new BusinessObjectCloneArgs(fac, Array.Empty<string>(), typeof(FWSHeader), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Licenses[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Licenses[0].Factory.GetHashCode());
		}

		public void TestPGALineReadOnly()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			fwsHeader.US_IsDocSubmitted = ZBool.True;
			fwsHeader.US_ProductType = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			Factory.Save();
			fwsHeader.OnLoaded();
			Assert(!fwsHeader.ReadOnly);

			fwsHeader.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			fwsHeader.OnLoaded();
			Assert(fwsHeader.ReadOnly);

			fwsHeader.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			fwsHeader.OnLoaded();
			Assert(!fwsHeader.ReadOnly);

			fwsHeader.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			fwsHeader.OnLoaded();
			Assert(fwsHeader.ReadOnly);

			fwsHeader.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			fwsHeader.OnLoaded();
			Assert(fwsHeader.ReadOnly);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			ICusAddInfoTypeSupporter supporter = fwsHeader;
			supporter.AssertType(typeof(FWSLicense), CusAddInfoTypeAttribute.Codes.USFWSLicense);
			supporter.AssertType(null, "ZZ!");

			var license = fwsHeader.Licenses.AddNew();
			license.US_Number = "ABC";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(license.PK);
			AssertEquals(typeof(FWSLicense), addInfo.GetType());
		}

		public void TestIFWSHeaderMembers()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			IFWSHeader iHeader = fwsHeader;
			fwsHeader.US_LineNo = 10;
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			fwsHeader.US_IsDocSubmitted = ZBool.True;
			fwsHeader.US_ProductType = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			fwsHeader.US_ProductNumber = "AB#@#";
			fwsHeader.US_ScientificGenusName = "SGN";
			fwsHeader.US_ScientificSpeciesName = "SSN";
			fwsHeader.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Doves;
			fwsHeader.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.BAL;
			fwsHeader.US_Scientific2GenusName = "S2GN";
			fwsHeader.US_Scientific2SpeciesName = "S2SN";
			fwsHeader.US_SpeciesOrigin = Core.Constants.CountryCodes.NewZealand;
			fwsHeader.US_WildlifeSource = FWSWildlifeSourceList.Codes.C;
			fwsHeader.US_Hybrid = FWSHybridTypeList.Codes.Intergeneric;
			fwsHeader.Licenses.AddNew();
			fwsHeader.Licenses.AddNew();
			fwsHeader.US_CommoditySpecificName = "CSN";
			fwsHeader.US_CommodityGeneralName = "CGN";
			fwsHeader.US_IsLiveVenomous = true;
			fwsHeader.US_CartonQty = 10;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "BOB THE BUILDER";
			fwsHeader.US_OA_FWSImporterAddress = org1.MainAddress.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "WENDY THE DESTROYER";
			fwsHeader.US_OA_FWSExporterAddress = org2.MainAddress.PK;
			org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DN34", Core.Constants.CountryCodes.UnitedStates);
			fwsHeader.US_RemarksText = "BOD LIKES TO BUILD";
			fwsHeader.US_Value = 32342m;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";
			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = false;
			containerPivots[2].IsForInvoiceLine = true;
			fwsHeader.US_NetCommodityUQ = FWSUnitOfMeasureList.Codes.Centimeters;
			fwsHeader.US_NetCommodity = 324.59m;
			fwsHeader.US_FIRMS = "H435";
			fwsHeader.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Doves;
			AssertEquals("iHeader.LineNo", 10, iHeader.LineNo);
			AssertEquals("iHeader.ProcessingCode", FWSProcessingCodeList.Codes.EDS, iHeader.ProcessingCode);
			AssertEquals("iHeader.DeclarationCode", DeclarationCodeList.Codes.FW3, iHeader.DeclarationCode);
			AssertEquals("iHeader.IsDocSubmitted", ZBool.True, iHeader.IsDocSubmitted);
			AssertEquals("iHeader.ProductType", GlobalUniqueProductCodeQualifierList.Codes.SRV, iHeader.ProductType);
			AssertEquals("iHeader.ProductNumber", "AB#@#", iHeader.ProductNumber);
			AssertEquals("iHeader.ScientificGenusName", "SGN", iHeader.ScientificGenusName);
			AssertEquals("iHeader.ScientificSpeciesName", "SSN", iHeader.ScientificSpeciesName);
			AssertEquals("iHeader.ScientificSpeciesCode", "DOV", iHeader.ScientificSpeciesCode);
			AssertEquals("iHeader.FWSDescriptionCode", FWSWildlifeDescriptionCodesList.Codes.BAL, iHeader.FWSDescriptionCode);
			AssertEquals("iHeader.Scientific2GenusName", "S2GN", iHeader.Scientific2GenusName);
			AssertEquals("iHeader.Scientific2SpeciesName", "S2SN", iHeader.Scientific2SpeciesName);
			AssertEquals("iHeader.SourceCountryCode", Core.Constants.CountryCodes.NewZealand, iHeader.SourceCountryCode);
			AssertEquals("iHeader.CommodityQualifierCode", FWSWildlifeSourceList.Codes.C, iHeader.CommodityQualifierCode);
			AssertEquals("iHeader.Hybrid", FWSHybridTypeList.Codes.Intergeneric, iHeader.Hybrid);
			var licenses = iHeader.Licenses.ToArray();
			AssertEquals("iHeader.Licenses", 2, licenses.Length);
			AssertEquals("iHeader.CommoditySpecificName", "CSN", iHeader.CommoditySpecificName);
			AssertEquals("iHeader.CommodityGeneralName", "CGN", iHeader.CommodityGeneralName);
			AssertEquals("iHeader.IsLiveVenomous", YesNoDefaultList.Codes.Yes, iHeader.IsLiveVenomous);
			AssertEquals("iHeader.CartonQty", (short)10, iHeader.CartonQty);
			var importer = iHeader.FWSImporter;
			AssertEquals("importer.Name", "BOB THE BUILDER", importer.CompanyAddress.CompanyName);
			Declaration.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE10", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("iHeader.FilerAccountNumber from company org proxy", "FWE10", iHeader.FilerAccountNumber);
			Declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE11", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("iHeader.FilerAccountNumber from branch org proxy", "FWE11", iHeader.FilerAccountNumber);
			org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE12", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("iHeader.FWSImporterFWE from Importer", "FWE12", iHeader.FWSImporterFWE);

			var exporter = iHeader.FWSForeignExporter;
			AssertEquals("exporter.Name", "WENDY THE DESTROYER", exporter.CompanyAddress.CompanyName);
			AssertEquals("iHeader.FWSForeignExporterDUNS", "DN34", iHeader.FWSForeignExporterDUNS);
			AssertEquals("iHeader.RemarksText", "BOD LIKES TO BUILD", iHeader.RemarksText);
			AssertEquals("iHeader.PGALineValue", 32342m, iHeader.PGALineValue);
			var containerNumbers = iHeader.ContainerNumbers.ToArray();
			AssertEquals("containerNumbers.Length", 2, containerNumbers.Length);
			AssertCollectionContains("CONT1", containerNumbers);
			AssertCollectionContains("CONT3", containerNumbers);
			AssertEquals("iHeader.NetCommodityUQ", FWSUnitOfMeasureList.Codes.Centimeters, iHeader.NetCommodityUQ);
			AssertEquals("iHeader.NetCommodityQty", 324.59m, iHeader.NetCommodityQty);
			AssertEquals("iHeader.FIRMS", "H435", iHeader.FIRMS);

			fwsHeader.US_SpeciesOrigin = FWSConstants.HighSeas;
			AssertEquals("iHeader.SourceCountryCode", FWSConstants.HighSeas, iHeader.SourceCountryCode);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			AssertEquals("iHeader.DeclarationCode", ZString.Empty, iHeader.DeclarationCode);

			fwsHeader.US_ConfirmationNum = "HXU20230228";
			AssertEquals("iHeader.Licenses", 2, iHeader.Licenses.Count());
		}

		public void TestProperties()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			fwsHeader.US_IsDocSubmitted = ZBool.True;
			fwsHeader.US_ProductType = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			fwsHeader.US_ProductNumber = "AB#@#";
			fwsHeader.US_Hybrid = FWSHybridTypeList.Codes.Intergeneric;
			fwsHeader.US_ScientificGenusName = "SGN";
			fwsHeader.US_ScientificSpeciesName = "SSN";
			fwsHeader.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.Doves;
			fwsHeader.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.BAL;
			fwsHeader.US_Scientific2GenusName = "S2GN";
			fwsHeader.US_Scientific2SpeciesName = "S2SN";
			fwsHeader.US_SpeciesOrigin = Core.Constants.CountryCodes.NewZealand;
			fwsHeader.US_WildlifeSource = FWSWildlifeSourceList.Codes.C;
			fwsHeader.Licenses.AddNew();
			fwsHeader.Licenses.AddNew();
			fwsHeader.US_CommoditySpecificName = "CSN";
			fwsHeader.US_CommodityGeneralName = "CGN";
			fwsHeader.US_IsLiveVenomous = true;
			fwsHeader.US_CartonQty = 10;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "BOB THE BUILDER";
			fwsHeader.US_OA_FWSImporterAddress = org1.MainAddress.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "WENDY THE DESTROYER";
			fwsHeader.US_OA_FWSExporterAddress = org2.MainAddress.PK;
			org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DN34", Core.Constants.CountryCodes.UnitedStates);
			fwsHeader.US_RemarksText = "BOD LIKES TO BUILD";
			fwsHeader.US_Value = 32342m;
			fwsHeader.US_NetCommodityUQ = FWSUnitOfMeasureList.Codes.Centimeters;
			fwsHeader.US_NetCommodity = 324.59m;
			fwsHeader.US_FIRMS = "H435";

			AssertEquals("fwsHeader.US_ProcessingCode", FWSProcessingCodeList.Codes.EDS, fwsHeader.US_ProcessingCode);
			AssertEquals("fwsHeader.US_IsDocSubmitted", ZBool.True, fwsHeader.US_IsDocSubmitted);
			AssertEquals("fwsHeader.US_ProductType", GlobalUniqueProductCodeQualifierList.Codes.SRV, fwsHeader.US_ProductType);
			AssertEquals("fwsHeader.US_ProductNumber", "AB#@#", fwsHeader.US_ProductNumber);
			AssertEquals("fwsHeader.US_Hybrid", FWSHybridTypeList.Codes.Intergeneric, fwsHeader.US_Hybrid);
			AssertEquals("fwsHeader.US_ScientificGenusName", "SGN", fwsHeader.US_ScientificGenusName);
			AssertEquals("fwsHeader.US_ScientificSpeciesName", "SSN", fwsHeader.US_ScientificSpeciesName);
			AssertEquals("fwsHeader.US_WildlifeCategoryCode", "DOV", fwsHeader.US_WildlifeCategoryCode);
			AssertEquals("fwsHeader.US_WildlifeDescriptionCode", FWSWildlifeDescriptionCodesList.Codes.BAL, fwsHeader.US_WildlifeDescriptionCode);
			AssertEquals("fwsHeader.US_Scientific2GenusName", "S2GN", fwsHeader.US_Scientific2GenusName);
			AssertEquals("fwsHeader.US_Scientific2SpeciesName", "S2SN", fwsHeader.US_Scientific2SpeciesName);
			AssertEquals("fwsHeader.US_SpeciesOrigin", Core.Constants.CountryCodes.NewZealand, fwsHeader.US_SpeciesOrigin);

			AssertEquals("fwsHeader.US_WildlifeSource", FWSWildlifeSourceList.Codes.C, fwsHeader.US_WildlifeSource);
			AssertEquals("fwsHeader.Licenses.Count", 2, fwsHeader.Licenses.Count);
			AssertEquals("fwsHeader.US_CommoditySpecificName", "CSN", fwsHeader.US_CommoditySpecificName);
			AssertEquals("fwsHeader.US_CommodityGeneralName", "CGN", fwsHeader.US_CommodityGeneralName);
			AssertEquals("fwsHeader.US_IsLiveVenomous", true, fwsHeader.US_IsLiveVenomous);
			AssertEquals("fwsHeader.US_CartonQty", (short)10, fwsHeader.US_CartonQty);
			AssertEquals("fwsHeader.US_OA_FWSImporterAddress", org1.MainAddress.PK, fwsHeader.US_OA_FWSImporterAddress);
			AssertEquals("fwsHeader.US_OA_FWSExporterAddress", org2.MainAddress.PK, fwsHeader.US_OA_FWSExporterAddress);
			AssertEquals("fwsHeader.US_RemarksText", "BOD LIKES TO BUILD", fwsHeader.US_RemarksText);
			AssertEquals("fwsHeader.US_Value", 32342m, fwsHeader.US_Value);
			AssertEquals("fwsHeader.US_NetCommodityUQ", FWSUnitOfMeasureList.Codes.Centimeters, fwsHeader.US_NetCommodityUQ);
			AssertEquals("fwsHeader.US_NetCommodity", 324.59m, fwsHeader.US_NetCommodity);
			AssertEquals("fwsHeader.US_FIRMS", "H435", fwsHeader.US_FIRMS);
		}

		public void DataIsDeletedOnSaving()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			Factory.Save();
			AssertEquals(false, fwsHeader.IsDeleted);
			Factory.Save();
			AssertEquals(true, fwsHeader.IsDeleted);

			fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			var license = fwsHeader.Licenses.AddNew();
			Factory.Save();
			AssertEquals(false, fwsHeader.IsDeleted);
			license.Delete();
			Factory.Save();
			AssertEquals(true, fwsHeader.IsDeleted);
		}
		public void TestCertifySignatureDate()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			fwsHeader.InvoiceHeader.US_FWSSignDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, ((IFWSHeader)fwsHeader).CertifySignatureDate);

			var expectedDate = new ZDate(2017, 01, 01);
			fwsHeader.InvoiceHeader.US_FWSSignDate = expectedDate;
			AssertEquals(expectedDate, ((IFWSHeader)fwsHeader).CertifySignatureDate);
		}

		public void TestClone()
		{
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = "AB";
			var license1 = fwsHeader.Licenses.AddNew();
			license1.US_Number = "111";
			var license2 = fwsHeader.Licenses.AddNew();
			license2.US_Number = "222";

			var clonedHeader = (FWSHeader)fwsHeader.Clone();
			AssertEquals("clonedHeader.US_ProcessingCode", "AB", clonedHeader.US_ProcessingCode);
			AssertEquals("clonedHeader.Licenses.Count", 2, clonedHeader.Licenses.Count);
			var clonedLicense = clonedHeader.Licenses[0];
			AssertEquals("clonedLicense.US_Number", "111", clonedLicense.US_Number);
			clonedLicense = clonedHeader.Licenses[1];
			AssertEquals("clonedLicense.US_Number", "222", clonedLicense.US_Number);
		}

		public void TestIAESFWSMembers()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			InvoiceLine.ExportFWS.US_ConfirmationNum = "AAAAAAAAAA";
			InvoiceLine.ExportFWS.US_TaxonomicSerialNumber = "BBBBBBBB";
			InvoiceLine.ExportFWS.US_PurposeCode = "M";
			InvoiceLine.ExportFWS.US_WildlifeDescriptionCode = "W";
			InvoiceLine.ExportFWS.US_SpeciesOrigin = "US";
			InvoiceLine.ExportFWS.US_WildlifeSource = "DOM";
			InvoiceLine.ExportFWS.US_CertificationCode = "B";
			InvoiceLine.ExportFWS.US_WildlifeCategoryCode = "D";
			InvoiceLine.ExportFWS.US_USState = "CA";
			InvoiceLine.JI_Description = "TEST EXPORT FWS";

			var aesFWS = (IAESFWS)InvoiceLine.ExportFWS;
			AssertEquals("AAAAAAAAAA", aesFWS.EDecsConfirmation);
			AssertEquals("BBBBBBBB", aesFWS.TaxonomicSerialNumber);
			AssertEquals("M", aesFWS.PurposeCode);
			AssertEquals("W", aesFWS.DescriptionCode);
			AssertEquals("US", aesFWS.SpeciesOrigin);
			AssertEquals("J", aesFWS.SourceCode);
			AssertEquals("B", aesFWS.ExemptionCertification);
			AssertEquals("D", aesFWS.WildlifeCategoryCode);
			AssertEquals("CA", aesFWS.StateCode);
			AssertEquals("TEST EXPORT FWS", aesFWS.CommercialDescription);
		}

		public void TestBrokerDetails()
		{
			Invoice.US_FDAContactName = "Tom";
			Invoice.US_FDAContactPhoneNo = "8293845001";
			Invoice.US_FDAContactEmail = "thomas.anderson@brokery.com";

			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			AssertNull("Broker Details should be null without an EDS on the header", ((IFWSHeader)fwsHeader).BrokerDetails);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			AssertNull("Broker Details should be null without an EDS on the header", ((IFWSHeader)fwsHeader).BrokerDetails);
			AssertBrokerDetailsWithProcessingCode(fwsHeader, FWSProcessingCodeList.Codes.EDS);
		}

		void AssertBrokerDetailsWithProcessingCode(FWSHeader fwsHeader, ZString processingCode)
		{
			fwsHeader.US_ProcessingCode = processingCode;
			var brokerDetails = ((IFWSHeader)fwsHeader).BrokerDetails;
			AssertNotNull("Broker Details should not be null when the header has a " + processingCode + " processing code", brokerDetails);
			AssertEquals("brokerDetails.ContactName should return Invoice.US_FDAContactName", "Tom", brokerDetails.ContactName);
			AssertEquals("brokerDetails.ContactPhone should return Invoice.US_FDAContactPhoneNo", "8293845001", brokerDetails.ContactPhone);
			AssertEquals("brokerDetails.ContactEmail should return Invoice.US_FDAContactEmail", "thomas.anderson@brokery.com", brokerDetails.ContactEmail);
		}

		public void TestExportFWS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineFWS = invoiceLine.FWSHeaders.AddNew();
			AssertEquals(false, invoiceLineFWS.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLineFWS.US_CertificationCode = "A";
			AssertEquals(true, invoiceLineFWS.IsExport);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productFWS = pivot.FWSLines.AddNew();
			AssertEquals(false, productFWS.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, productFWS.IsExport);
		}

		public void TestUS_ProcessingCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 200m;
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			AssertEquals(200m, fwsHeader.US_InvCurrPGAValue);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			AssertEquals(200m, fwsHeader.US_InvCurrPGAValue);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			AssertEquals(0m, fwsHeader.US_InvCurrPGAValue);
		}

		#region Implementation

		protected override IEnumerable<FWSHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (FWSHeader)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			return fwsHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.FWSHeaders.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			fWSFuncs = ZZCustomsFunctionality.TemporarilySetupFWSEffective();
		}

		IDisposable fWSFuncs;

		protected override void TearDown()
		{
			fWSFuncs.Dispose();
			base.TearDown();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
