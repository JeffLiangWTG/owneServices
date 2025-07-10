using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSLine))]
	public class NMFSLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NMFSLine>
	{
		public void TestPGALineReadOnly()
		{
			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsLine.US_AMLRPermitNumber = "AMR1";
			nmfsLine.US_HMSPermitNumber = "HMS1";
			nmfsLine.US_PreApprovalIssuedNumber = "PRA1";
			Factory.Save();
			nmfsLine.OnLoaded();
			Assert(!nmfsLine.ReadOnly);

			nmfsLine.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			nmfsLine.OnLoaded();
			Assert(nmfsLine.ReadOnly);

			nmfsLine.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			nmfsLine.OnLoaded();
			Assert(!nmfsLine.ReadOnly);

			nmfsLine.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			nmfsLine.OnLoaded();
			Assert(nmfsLine.ReadOnly);

			nmfsLine.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			nmfsLine.OnLoaded();
			Assert(nmfsLine.ReadOnly);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			ICusAddInfoTypeSupporter supporter = nmfsLine;
			supporter.AssertType(typeof(NMFSHarvestingDetail), CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail);
			supporter.AssertType(null, "ZZ!");

			var harvetingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvetingDetail.US_HarvestedCountry = "ZZ";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(harvetingDetail.PK);
			AssertEquals(typeof(NMFSHarvestingDetail), addInfo.GetType());
		}

		public void TestDocumentProperties()
		{
			var requiredDocument = Declaration.DocsAndCartage.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			var eDoc = ((IDocManagerSupport)Declaration).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			var disWrapper = ObjectFactory.Get<IDISHostWrapper>("IDISHostWrapper", new object[] { Declaration });
			var disDocument = (Integration.Customs.US.DIS.IDISDocument)disWrapper.DISDocuments.AddNew();
			disDocument.IDSuffix = 1;
			disDocument.DocumentDescription = "BOB THE BUILDER";
			disDocument.EDocsDocumentPK = eDoc.UniqueKey;
			Factory.Save();

			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals("nmfsLine.US_DocumentType", NMFS370DocumentIdentifierList.Codes.NOAAForm370, nmfsLine.US_DocumentType);
			AssertEquals("nmfsLine.US_DocumentTypeDesc", NMFS370DocumentIdentifierList.Descriptions.NOAAForm370, nmfsLine.US_DocumentTypeDesc);
			AssertEquals("nmfsLine.US_DocumentTypeInfo.ReadOnly", true, nmfsLine.US_DocumentTypeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_DISDocumentIDInfo.ReadOnly", false, nmfsLine.US_DISDocumentIDInfo.ReadOnly);
			AssertEquals("nmfsLine.US_DISDocumentIDDesc", ZString.Empty, nmfsLine.US_DISDocumentIDDesc);
			AssertEquals("nmfsLine.US_DolphinSafeStatusInfo.ReadOnly", false, nmfsLine.US_DolphinSafeStatusInfo.ReadOnly);
			AssertEquals("nmfsLine.US_CaptainStatementInfo.ReadOnly", false, nmfsLine.US_CaptainStatementInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ObserverStatementInfo.ReadOnly", false, nmfsLine.US_ObserverStatementInfo.ReadOnly);
			AssertEquals("nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly", false, nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ConfidentialInfo.ReadOnly", true, nmfsLine.US_ConfidentialInfo.ReadOnly);
			AssertEquals("nmfsLine.US_SpeciesCodeInfo.ReadOnly", true, nmfsLine.US_SpeciesCodeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly", true, nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightInfo.ReadOnly", true, nmfsLine.US_NetWeightInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightUQInfo.ReadOnly", true, nmfsLine.US_NetWeightUQInfo.ReadOnly);
			AssertEquals("nmfsLine.US_EBCDNumber.ReadOnly", true, nmfsLine.US_EBCDNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.Is370ProgramType", true, nmfsLine.Is370ProgramType);
			AssertEquals("nmfsLine.IsAMRProgramType", false, nmfsLine.IsAMRProgramType);
			AssertEquals("nmfsLine.IsHMSProgramType", false, nmfsLine.IsHMSProgramType);
			AssertEquals("nmfsLine.IsSIMProgramType", false, nmfsLine.IsSIMProgramType);
			AssertEquals("nmfsLine.IsCOAProgramType", false, nmfsLine.IsCOAProgramType);
			AssertEquals("nmfsLine.Is370OrSIMOrCOAPProgramType", true, nmfsLine.Is370OrSIMOrCOAPProgramType);
			AssertEquals("nmfsLine.IsNotSIMPProgramType", true, nmfsLine.IsNotSIMPProgramType);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals("nmfsLine.US_DocumentType", ZString.Empty, nmfsLine.US_DocumentType);
			AssertEquals("nmfsLine.US_DocumentTypeDesc", ZString.Empty, nmfsLine.US_DocumentTypeDesc);
			AssertEquals("nmfsLine.US_DocumentTypeInfo.ReadOnly", false, nmfsLine.US_DocumentTypeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_DISDocumentIDInfo.ReadOnly", false, nmfsLine.US_DISDocumentIDInfo.ReadOnly);
			AssertEquals("nmfsLine.US_DISDocumentIDDesc", ZString.Empty, nmfsLine.US_DISDocumentIDDesc);
			AssertEquals("nmfsLine.US_DolphinSafeStatusInfo.ReadOnly", true, nmfsLine.US_DolphinSafeStatusInfo.ReadOnly);
			AssertEquals("nmfsLine.US_CaptainStatementInfo.ReadOnly", true, nmfsLine.US_CaptainStatementInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ObserverStatementInfo.ReadOnly", true, nmfsLine.US_ObserverStatementInfo.ReadOnly);
			AssertEquals("nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly", true, nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ConfidentialInfo.ReadOnly", true, nmfsLine.US_ConfidentialInfo.ReadOnly);
			AssertEquals("nmfsLine.US_SpeciesCodeInfo.ReadOnly", true, nmfsLine.US_SpeciesCodeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly", true, nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightInfo.ReadOnly", true, nmfsLine.US_NetWeightInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightUQInfo.ReadOnly", true, nmfsLine.US_NetWeightUQInfo.ReadOnly);
			AssertEquals("nmfsLine.US_EBCDNumber.ReadOnly", false, nmfsLine.US_EBCDNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.Is370ProgramType", false, nmfsLine.Is370ProgramType);
			AssertEquals("nmfsLine.IsAMRProgramType", false, nmfsLine.IsAMRProgramType);
			AssertEquals("nmfsLine.IsHMSProgramType", true, nmfsLine.IsHMSProgramType);
			AssertEquals("nmfsLine.IsSIMProgramType", false, nmfsLine.IsSIMProgramType);
			AssertEquals("nmfsLine.IsCOAProgramType", false, nmfsLine.IsCOAProgramType);
			AssertEquals("nmfsLine.Is370OrSIMOrCOAPProgramType", false, nmfsLine.Is370OrSIMOrCOAPProgramType);
			AssertEquals("nmfsLine.IsNotSIMPProgramType", true, nmfsLine.IsNotSIMPProgramType);

			nmfsLine.US_DISDocumentID = disDocument.DocumentID;
			AssertEquals("document.US_DISDocumentIDDesc", "BOB THE BUILDER", nmfsLine.US_DISDocumentIDDesc);

			nmfsLine.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm;
			AssertEquals("document.US_TypeDesc", NMFSHMSDocumentIdentifierList.Descriptions.CcsbtCatchMonitoringForm, nmfsLine.US_DocumentTypeDesc);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals("nmfsLine.US_DocumentType", ZString.Empty, nmfsLine.US_DocumentType);
			AssertEquals("document.US_DocumentTypeDesc", ZString.Empty, nmfsLine.US_DocumentTypeDesc);
			AssertEquals("document.US_DocumentTypeInfo.ReadOnly", false, nmfsLine.US_DocumentTypeInfo.ReadOnly);
			AssertEquals("document.US_DISDocumentIDInfo.ReadOnly", false, nmfsLine.US_DISDocumentIDInfo.ReadOnly);
			AssertEquals("document.US_DolphinSafeStatusInfo.ReadOnly", true, nmfsLine.US_DolphinSafeStatusInfo.ReadOnly);
			AssertEquals("document.US_CaptainStatementInfo.ReadOnly", true, nmfsLine.US_CaptainStatementInfo.ReadOnly);
			AssertEquals("document.US_ObserverStatementInfo.ReadOnly", true, nmfsLine.US_ObserverStatementInfo.ReadOnly);
			AssertEquals("document.US_IDCPMemberCertificationInfo.ReadOnly", true, nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ConfidentialInfo.ReadOnly", true, nmfsLine.US_ConfidentialInfo.ReadOnly);
			AssertEquals("nmfsLine.US_SpeciesCodeInfo.ReadOnly", true, nmfsLine.US_SpeciesCodeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly", true, nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightInfo.ReadOnly", true, nmfsLine.US_NetWeightInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightUQInfo.ReadOnly", true, nmfsLine.US_NetWeightUQInfo.ReadOnly);
			AssertEquals("nmfsLine.US_EBCDNumber.ReadOnly", true, nmfsLine.US_EBCDNumberInfo.ReadOnly);
			AssertEquals("document.Is370ProgramType", false, nmfsLine.Is370ProgramType);
			AssertEquals("document.IsAMRProgramType", true, nmfsLine.IsAMRProgramType);
			AssertEquals("document.IsHMSProgramType", false, nmfsLine.IsHMSProgramType);
			AssertEquals("nmfsLine.IsSIMProgramType", false, nmfsLine.IsSIMProgramType);
			AssertEquals("nmfsLine.IsCOAProgramType", false, nmfsLine.IsCOAProgramType);
			AssertEquals("nmfsLine.Is370OrSIMOrCOAPProgramType", false, nmfsLine.Is370OrSIMOrCOAPProgramType);
			AssertEquals("nmfsLine.IsNotSIMPProgramType", true, nmfsLine.IsNotSIMPProgramType);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("document.US_DocumentTypeDesc", ZString.Empty, nmfsLine.US_DocumentTypeDesc);
			AssertEquals("document.US_DocumentTypeInfo.ReadOnly", true, nmfsLine.US_DocumentTypeInfo.ReadOnly);
			AssertEquals("document.US_DISDocumentIDInfo.ReadOnly", true, nmfsLine.US_DISDocumentIDInfo.ReadOnly);
			AssertEquals("document.US_DolphinSafeStatusInfo.ReadOnly", true, nmfsLine.US_DolphinSafeStatusInfo.ReadOnly);
			AssertEquals("document.US_CaptainStatementInfo.ReadOnly", true, nmfsLine.US_CaptainStatementInfo.ReadOnly);
			AssertEquals("document.US_ObserverStatementInfo.ReadOnly", true, nmfsLine.US_ObserverStatementInfo.ReadOnly);
			AssertEquals("document.US_IDCPMemberCertificationInfo.ReadOnly", true, nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ConfidentialInfo.ReadOnly", false, nmfsLine.US_ConfidentialInfo.ReadOnly);
			AssertEquals("nmfsLine.US_SpeciesCodeInfo.ReadOnly", false, nmfsLine.US_SpeciesCodeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly", false, nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightInfo.ReadOnly", false, nmfsLine.US_NetWeightInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightUQInfo.ReadOnly", false, nmfsLine.US_NetWeightUQInfo.ReadOnly);
			AssertEquals("nmfsLine.US_EBCDNumber.ReadOnly", true, nmfsLine.US_EBCDNumberInfo.ReadOnly);
			AssertEquals("document.Is370ProgramType", false, nmfsLine.Is370ProgramType);
			AssertEquals("document.IsAMRProgramType", false, nmfsLine.IsAMRProgramType);
			AssertEquals("document.IsHMSProgramType", false, nmfsLine.IsHMSProgramType);
			AssertEquals("nmfsLine.IsSIMProgramType", true, nmfsLine.IsSIMProgramType);
			AssertEquals("nmfsLine.IsCOAProgramType", false, nmfsLine.IsCOAProgramType);
			AssertEquals("nmfsLine.Is370OrSIMOrCOAPProgramType", true, nmfsLine.Is370OrSIMOrCOAPProgramType);
			AssertEquals("nmfsLine.IsNotSIMPProgramType", false, nmfsLine.IsNotSIMPProgramType);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			AssertEquals("document.US_DocumentTypeDesc", ZString.Empty, nmfsLine.US_DocumentTypeDesc);
			AssertEquals("document.US_DocumentTypeInfo.ReadOnly", true, nmfsLine.US_DocumentTypeInfo.ReadOnly);
			AssertEquals("document.US_DISDocumentIDInfo.ReadOnly", true, nmfsLine.US_DISDocumentIDInfo.ReadOnly);
			AssertEquals("document.US_DolphinSafeStatusInfo.ReadOnly", true, nmfsLine.US_DolphinSafeStatusInfo.ReadOnly);
			AssertEquals("document.US_CaptainStatementInfo.ReadOnly", true, nmfsLine.US_CaptainStatementInfo.ReadOnly);
			AssertEquals("document.US_ObserverStatementInfo.ReadOnly", true, nmfsLine.US_ObserverStatementInfo.ReadOnly);
			AssertEquals("document.US_IDCPMemberCertificationInfo.ReadOnly", true, nmfsLine.US_IDCPMemberCertificationInfo.ReadOnly);
			AssertEquals("nmfsLine.US_ConfidentialInfo.ReadOnly", false, nmfsLine.US_ConfidentialInfo.ReadOnly);
			AssertEquals("nmfsLine.US_SpeciesCodeInfo.ReadOnly", false, nmfsLine.US_SpeciesCodeInfo.ReadOnly);
			AssertEquals("nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly", true, nmfsLine.US_OtherAuthorizationNumberInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightInfo.ReadOnly", true, nmfsLine.US_NetWeightInfo.ReadOnly);
			AssertEquals("nmfsLine.US_NetWeightUQInfo.ReadOnly", true, nmfsLine.US_NetWeightUQInfo.ReadOnly);
			AssertEquals("nmfsLine.US_EBCDNumber.ReadOnly", true, nmfsLine.US_EBCDNumberInfo.ReadOnly);
			AssertEquals("document.Is370ProgramType", false, nmfsLine.Is370ProgramType);
			AssertEquals("document.IsAMRProgramType", false, nmfsLine.IsAMRProgramType);
			AssertEquals("document.IsHMSProgramType", false, nmfsLine.IsHMSProgramType);
			AssertEquals("nmfsLine.IsSIMProgramType", false, nmfsLine.IsSIMProgramType);
			AssertEquals("nmfsLine.IsCOAProgramType", true, nmfsLine.IsCOAProgramType);
			AssertEquals("nmfsLine.Is370OrSIMOrCOAPProgramType", true, nmfsLine.Is370OrSIMOrCOAPProgramType);
			AssertEquals("nmfsLine.IsNotSIMPProgramType", true, nmfsLine.IsNotSIMPProgramType);
		}

		public void TestINMFSDocumentMembers()
		{
			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument;
			nmfsLine.US_DISDocumentID = "IDS32342";
			INMFSDocument nmfsDocument = nmfsLine;
			AssertEquals("DocumentIdentifier", NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument, nmfsDocument.DocumentIdentifier);
			AssertEquals("DocumentNumber", "IDS32342", nmfsDocument.DocumentNumber);
		}

		[TestDate(2016, 08, 15)]
		public void TestINMFSLineMembers()
		{
			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsLine.US_AMLRPermitNumber = "AMR1";
			nmfsLine.US_HMSPermitNumber = "HMS1";
			nmfsLine.US_PreApprovalIssuedNumber = "PRA1";
			nmfsLine.US_PreApprovalIssuedQuantity = 15500m;
			nmfsLine.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Grams;
			nmfsLine.US_DISDocumentID = "DIS1215";
			nmfsLine.US_CaptainStatement = true;
			nmfsLine.US_ObserverStatement = true;
			nmfsLine.US_IDCPMemberCertification = true;
			nmfsLine.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.B3;
			nmfsLine.HarvestingDetails.RemoveAndDeleteAll();

			INMFSLine line = nmfsLine;
			AssertEquals("AMLRPermitNumber", "AMR1", line.AMLRPermitNumber);
			AssertEquals("AMLRPermitNumber", "PRA1", line.PreApprovalIssuedNumber);
			AssertEquals("AMLRPermitNumber", 15.5m, line.PreApprovalIssuedQuantity);
			AssertEquals("ContainsYellowfinTuna", ZBool.False, line.ContainsYellowfinTuna);

			var documents = line.DocumentDetails.ToList();
			AssertEquals("DocumentDetails", 4, documents.Count);
			AssertDocument(documents[0], NMFS370DocumentIdentifierList.Codes.NOAAForm370, DolphinSafeStatusList.Codes.B3);
			AssertDocument(documents[1], NMFS370DocumentIdentifierList.Codes.CaptainStatement, ZString.Empty);
			AssertDocument(documents[2], NMFS370DocumentIdentifierList.Codes.ObserverStatement, ZString.Empty);
			AssertDocument(documents[3], NMFS370DocumentIdentifierList.Codes.IDCPMemberNationCertification, ZString.Empty);

			var harvestingDetail1 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = "ZZ";
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_ContainsYellowfinTuna = ZBool.False;
			harvestingDetail1.US_VesselCountry = "US";

			AssertEquals("AMLRPermitNumber", "AMR1", line.AMLRPermitNumber);
			AssertEquals("AMLRPermitNumber", "PRA1", line.PreApprovalIssuedNumber);
			AssertEquals("AMLRPermitNumber", 15.5m, line.PreApprovalIssuedQuantity);
			AssertEquals("ContainsYellowfinTuna", ZBool.False, line.ContainsYellowfinTuna);

			var harvestingDetail2 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = "ZZ";
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail2.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail2.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail2.US_VesselCountry = "US";

			AssertEquals("AMLRPermitNumber", "AMR1", line.AMLRPermitNumber);
			AssertEquals("AMLRPermitNumber", "PRA1", line.PreApprovalIssuedNumber);
			AssertEquals("AMLRPermitNumber", 15.5m, line.PreApprovalIssuedQuantity);
			AssertEquals("ContainsYellowfinTuna", ZBool.True, line.ContainsYellowfinTuna);

			var harvestingDetail3 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = "ZZ";
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail3.US_VesselCountry = "AU";

			var harvestingDetail4 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail4.US_HarvestedCountry = "ZZ";
			harvestingDetail4.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail4.US_GearType = GearTypeList.Codes.Baitboat;
			harvestingDetail4.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail4.US_VesselCountry = "US";

			var harvestingDetail5 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail5.US_HarvestedCountry = "ZZ";
			harvestingDetail5.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.A;
			harvestingDetail5.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail5.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail5.US_VesselCountry = "US";

			var harvestingDetail6 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail6.US_HarvestedCountry = "AU";
			harvestingDetail6.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail6.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail6.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail6.US_VesselCountry = "US";

			var harvestingDetails = line.HarvestingDetails.ToList();
			AssertEquals("HarvestingDetails", 5, harvestingDetails.Count);
			AssertHarvestingDetail(harvestingDetails[0], "AU", OceanGeographicAreaCodeList.Codes.ETP, GearTypeList.Codes.Longline, ZBool.True, 1);
			AssertHarvestingDetail(harvestingDetails[1], "ZZ", OceanGeographicAreaCodeList.Codes.A, GearTypeList.Codes.Longline, ZBool.True, 1);
			AssertHarvestingDetail(harvestingDetails[2], "ZZ", OceanGeographicAreaCodeList.Codes.ETP, GearTypeList.Codes.Baitboat, ZBool.True, 1);
			AssertHarvestingDetail(harvestingDetails[3], "ZZ", OceanGeographicAreaCodeList.Codes.ETP, GearTypeList.Codes.Longline, ZBool.False, 1);
			AssertHarvestingDetail(harvestingDetails[4], "ZZ", OceanGeographicAreaCodeList.Codes.ETP, GearTypeList.Codes.Longline, ZBool.True, 2);
			var harvestingVessels = harvestingDetails[4].HarvestingVessels.ToList();
			AssertEquals("HarvestingVessels", 2, harvestingVessels.Count);
			AssertEquals("AU", harvestingVessels[0]);
			AssertEquals("US", harvestingVessels[1]);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument;
			nmfsLine.US_DISDocumentID = "IDS32342";
			documents = line.DocumentDetails.ToList();
			AssertEquals(1, documents.Count);
			AssertEquals(NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument, documents[0].DocumentIdentifier);
			AssertEquals("IDS32342", documents[0].DocumentNumber);

			var documentDetail0 = nmfsLine.DocumentDetails.AddNew();
			documentDetail0.CY_Code = NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm;
			documentDetail0.CY_Data = "IDS32343";
			var documentDetail1 = nmfsLine.DocumentDetails.AddNew();
			documentDetail1.CY_Code = NMFSHMSDocumentIdentifierList.Codes.CcsbtReExportAfterLandingOfDomesticProductForm;
			documentDetail1.CY_Data = "IDS32344";
			documents = line.DocumentDetails.ToList();
			AssertEquals(2, documents.Count);
			AssertEquals(NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm, documents[0].DocumentIdentifier);
			AssertEquals("IDS32343", documents[0].DocumentNumber);
			AssertEquals(NMFSHMSDocumentIdentifierList.Codes.CcsbtReExportAfterLandingOfDomesticProductForm, documents[1].DocumentIdentifier);
			AssertEquals("IDS32344", documents[1].DocumentNumber);
		}

		public void DataIsDeletedOnSaving()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			var harvestingDetail = line.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			AssertEquals(ZString.Empty, line.US_ProgramType);
			Factory.Save();
			AssertEquals(false, line.IsDeleted);
			harvestingDetail.Delete();
			line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			Factory.Save();
			AssertEquals(false, line.IsDeleted);
			line.US_ProgramType = ZString.Empty;
			Factory.Save();
			AssertEquals(true, line.IsDeleted);
		}

		public void TestCloneSIMVesselCountry()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			line.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			line.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			var harvestingDetail = line.HarvestingDetails.AddNew();
			var vesselDetail1 = harvestingDetail.HarvestingVessles.AddNew();
			vesselDetail1.US_HarvestedCountry = "CN";
			var vesselDetail2 = harvestingDetail.HarvestingVessles.AddNew();
			vesselDetail2.US_HarvestedCountry = "FR";

			var clonedLine = (NMFSLine)line.Clone();
			AssertEquals("clonedLine.US_VesselCountry", "CN", clonedLine.US_VesselCountry);
		}

		public void TestCloneHarvestingDetailsWhenChangeToSIM()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail1 = line.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = "AU";
			var harvestingDetail2 = line.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = "NZ";
			var clonedLine = (NMFSLine)line.Clone();
			AssertEquals("2 HarvestingDetail Lines should exist", 2, line.HarvestingDetails.Count);

			line.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("1 HarvestingDetail Line should exist", 1, line.HarvestingDetails.Count);
			AssertEquals("Harvested Country", "AU", line.HarvestingDetails[0].US_HarvestedCountry);
		}

		public void TestCloneHarvestingDetailsWhenChangeToCOA()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail1 = line.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = "AU";
			var harvestingDetail2 = line.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = "NZ";
			var clonedLine = (NMFSLine)line.Clone();
			AssertEquals("2 HarvestingDetail Lines should exist", 2, line.HarvestingDetails.Count);

			line.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			AssertEquals("1 HarvestingDetail Line should exist", 1, line.HarvestingDetails.Count);
			AssertEquals("Harvested Country", "AU", line.HarvestingDetails[0].US_HarvestedCountry);
		}

		public void TestClone()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			line.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			line.US_Commodity = FishStateList.Codes.FreshToothfish;
			line.US_AMLRPermitNumber = "AMR1";
			line.US_HMSPermitNumber = "HMS1";
			line.US_LineNo = 1;
			line.US_PreApprovalIssuedNumber = "PAI1";
			line.US_PreApprovalIssuedQuantity = 10m;
			line.US_PreApprovalIssuedQuantityUQ = "KG";
			line.US_CaptainStatement = ZBool.True;
			line.US_DISDocumentID = "DIS23432";
			line.US_DolphinSafeStatus = "B1";
			line.US_IDCPMemberCertification = ZBool.True;
			line.US_ObserverStatement = ZBool.True;
			line.US_DocumentType = NMFS370DocumentIdentifierList.Codes.NOAAForm370;
			line.US_Confidential = ZBool.True;
			line.US_SpeciesCode = "YY";
			line.US_OtherAuthorizationNumber = "123";
			line.US_NetWeight = 1.38;
			line.US_NetWeightUQ = "KG";

			var harvestingDetail1 = line.HarvestingDetails.AddNew();
			harvestingDetail1.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Baitboat;
			harvestingDetail1.US_HarvestedCountry = "US";
			var harvestingDetail2 = line.HarvestingDetails.AddNew();
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.EPO;
			harvestingDetail2.US_VesselCountry = "NZ";

			line.US_CaptainStatement = ZBool.True;
			line.US_DISDocumentID = "DIS23432";
			line.US_DolphinSafeStatus = "B1";

			var clonedLine = (NMFSLine)line.Clone();
			clonedLine.B7_ParentID = line.B7_ParentID;
			clonedLine.B7_ParentTableCode = line.B7_ParentTableCode;
			AssertEquals("clonedLine.US_ProgramType", NMFSProgramCodeList.Codes.AMR, clonedLine.US_ProgramType);
			AssertEquals("clonedLine.US_AMLRPermitNumber", "AMR1", clonedLine.US_AMLRPermitNumber);
			AssertEquals("clonedLine.US_HMSPermitNumber", "HMS1", clonedLine.US_HMSPermitNumber);
			AssertEquals("clonedLine.US_LineNo should not be cloned", 0, clonedLine.US_LineNo);
			AssertEquals("clonedLine.US_PreApprovalIssuedNumber", "PAI1", clonedLine.US_PreApprovalIssuedNumber);
			AssertEquals("clonedLine.US_PreApprovalIssuedQuantity", 10m, clonedLine.US_PreApprovalIssuedQuantity);
			AssertEquals("clonedLine.US_PreApprovalIssuedQuantityUQ", "KG", clonedLine.US_PreApprovalIssuedQuantityUQ);
			AssertEquals("clonedLine.US_Commodity", FishStateList.Codes.FreshToothfish, clonedLine.US_Commodity);
			AssertEquals("clonedLine.US_CaptainStatement", ZBool.True, clonedLine.US_CaptainStatement);
			AssertEquals("clonedLine.US_DISDocumentID should not be cloned", "", clonedLine.US_DISDocumentID);
			AssertEquals("clonedLine.US_DolphinSafeStatus", "B1", clonedLine.US_DolphinSafeStatus);
			AssertEquals("clonedLine.US_Confidential", ZBool.True, clonedLine.US_Confidential);
			AssertEquals("clonedLine.US_SpeciesCode", "YY", clonedLine.US_SpeciesCode);
			AssertEquals("clonedLine.US_OtherAuthorizationNumber", "123", clonedLine.US_OtherAuthorizationNumber);
			AssertEquals("clonedLine.US_NetWeight", 1.38m, clonedLine.US_NetWeight);
			AssertEquals("clonedLine.US_NetWeightUQ", "KG", clonedLine.US_NetWeightUQ);

			AssertEquals("line.HarvestingDetails.Count", 2, clonedLine.HarvestingDetails.Count);
			var clonedHarvestingDetail = clonedLine.HarvestingDetails[0];
			AssertEquals("clonedHarvestingDetail.US_ContainsYellowfinTuna", ZBool.True, clonedHarvestingDetail.US_ContainsYellowfinTuna);
			AssertEquals("clonedHarvestingDetail.US_GearType", GearTypeList.Codes.Baitboat, clonedHarvestingDetail.US_GearType);
			AssertEquals("clonedHarvestingDetail.US_HarvestedCountry", "US", clonedHarvestingDetail.US_HarvestedCountry);
			clonedHarvestingDetail = clonedLine.HarvestingDetails[1];
			AssertEquals("clonedHarvestingDetail.US_OceanAreaOfCatch", OceanGeographicAreaCodeList.Codes.EPO, clonedHarvestingDetail.US_OceanAreaOfCatch);
			AssertEquals("clonedHarvestingDetail.US_VesselCountry", "NZ", clonedHarvestingDetail.US_VesselCountry);

			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_DocumentType = NMFS370DocumentIdentifierList.Codes.NOAAForm370;
			nmfsLine.US_DISDocumentID = "AS313";
			nmfsLine.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.B1;
			nmfsLine.US_CaptainStatement = ZBool.True;
			nmfsLine.US_ObserverStatement = ZBool.False;
			nmfsLine.US_IDCPMemberCertification = ZBool.False;
			nmfsLine.US_HMSPermitNumber = "A013";
			nmfsLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			nmfsLine.US_AMLRPermitNumber = "AS2";
			nmfsLine.US_PreApprovalIssuedNumber = "FE4";
			nmfsLine.US_PreApprovalIssuedQuantity = 12m;
			nmfsLine.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Tonnes;
			nmfsLine.US_Confidential = ZBool.True;
			nmfsLine.US_SpeciesCode = "YY";
			nmfsLine.US_OtherAuthorizationNumber = "123";
			nmfsLine.US_NetWeight = 1.38;
			nmfsLine.US_NetWeightUQ = "KG";

			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail.US_VesselCountry = "FR";
			harvestingDetail.US_GearType = GearTypeList.Codes.Longline;

			InvoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var nmfsLineNew = InvoiceLine.NMFSLines.AddNew();

			AssertEquals("ProgramType must be equal", nmfsLine.US_ProgramType, nmfsLineNew.US_ProgramType);
			AssertEquals("DocumentType must be equal", nmfsLine.US_DocumentType, nmfsLineNew.US_DocumentType);
			AssertEquals("DISDocumentID must be empty", ZString.Empty, nmfsLineNew.US_DISDocumentID);
			AssertEquals("DolphinSafeStatus must be equal", nmfsLine.US_DolphinSafeStatus, nmfsLineNew.US_DolphinSafeStatus);
			AssertEquals("CaptainStatement must be equal", nmfsLine.US_CaptainStatement, nmfsLineNew.US_CaptainStatement);
			AssertEquals("ObserverStatement must be equal", nmfsLine.US_ObserverStatement, nmfsLineNew.US_ObserverStatement);
			AssertEquals("IDCPMemberCertification must be equal", nmfsLine.US_IDCPMemberCertification, nmfsLineNew.US_IDCPMemberCertification);
			AssertEquals("HMSPermitNumber must be equal", nmfsLine.US_HMSPermitNumber, nmfsLineNew.US_HMSPermitNumber);
			AssertEquals("Toothfish must be equal", nmfsLine.US_Commodity, nmfsLineNew.US_Commodity);
			AssertEquals("AMLRPermitNumber must be equal", nmfsLine.US_AMLRPermitNumber, nmfsLineNew.US_AMLRPermitNumber);
			AssertEquals("PreApprovalIssuedNumber must be equal", nmfsLine.US_PreApprovalIssuedNumber, nmfsLineNew.US_PreApprovalIssuedNumber);
			AssertEquals("PreApprovalIssuedQuantity must be equal", nmfsLine.US_PreApprovalIssuedQuantity, nmfsLineNew.US_PreApprovalIssuedQuantity);
			AssertEquals("PreApprovalIssuedQuantityUQ must be equal", nmfsLine.US_PreApprovalIssuedQuantityUQ, nmfsLineNew.US_PreApprovalIssuedQuantityUQ);
			AssertEquals("Confidential must be equal", nmfsLine.US_Confidential, nmfsLineNew.US_Confidential);
			AssertEquals("SpeciesCode must be equal", nmfsLine.US_SpeciesCode, nmfsLineNew.US_SpeciesCode);
			AssertEquals("OtherAuthorizationNumber must be equal", nmfsLine.US_OtherAuthorizationNumber, nmfsLineNew.US_OtherAuthorizationNumber);
			AssertEquals("NetWeight must be equal", nmfsLine.US_NetWeight, nmfsLineNew.US_NetWeight);
			AssertEquals("NetWeightUQ must be equal", nmfsLine.US_NetWeightUQ, nmfsLineNew.US_NetWeightUQ);

			AssertEquals("HarvestingDetail: HarvestedCountry must be equal", nmfsLine.HarvestingDetails[0].US_HarvestedCountry, nmfsLineNew.HarvestingDetails[0].US_HarvestedCountry);
			AssertEquals("HarvestingDetail: OceanAreaOfCatch must be equal", nmfsLine.HarvestingDetails[0].US_OceanAreaOfCatch, nmfsLineNew.HarvestingDetails[0].US_OceanAreaOfCatch);
			AssertEquals("HarvestingDetail: VesselCountry must be equal", nmfsLine.HarvestingDetails[0].US_VesselCountry, nmfsLineNew.HarvestingDetails[0].US_VesselCountry);
			AssertEquals("HarvestingDetail: GearType must be equal", nmfsLine.HarvestingDetails[0].US_GearType, nmfsLineNew.HarvestingDetails[0].US_GearType);
		}

		public void TestReadOnlyProperties()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			line.US_ProgramType = ZString.Empty;
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", true, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", true, line.US_CommodityInfo.ReadOnly);

			line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", true, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", true, line.US_CommodityInfo.ReadOnly);

			line.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", true, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", true, line.US_CommodityInfo.ReadOnly);

			line.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", true, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", false, line.US_CommodityInfo.ReadOnly);

			line.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", false, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", false, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", false, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", false, line.US_CommodityInfo.ReadOnly);

			line.US_ProgramType = "@!#";
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", true, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", true, line.US_CommodityInfo.ReadOnly);

			line.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("line.US_AMLRPermitNumberInfo.ReadOnly", true, line.US_AMLRPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_HMSPermitNumberInfo.ReadOnly", true, line.US_HMSPermitNumberInfo.ReadOnly);
			AssertEquals("line.US_LineNoInfo.ReadOnly", true, line.US_LineNoInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedNumberInfo.ReadOnly", true, line.US_PreApprovalIssuedNumberInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityInfo.ReadOnly);
			AssertEquals("line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly", true, line.US_PreApprovalIssuedQuantityUQInfo.ReadOnly);
			AssertEquals("line.US_ProgramTypeInfo.ReadOnly", false, line.US_ProgramTypeInfo.ReadOnly);
			AssertEquals("line.US_CommodityInfo.ReadOnly", true, line.US_CommodityInfo.ReadOnly);
		}

		public void TestDataIsClearedOutWhenNotApplicable()
		{
			var line = InvoiceLine.NMFSLines.AddNew();
			SetupUS_FieldsData(line);
			var harvestingDetail1 = line.HarvestingDetails.AddNew();
			SetupUS_FieldsData(harvestingDetail1);
			var harvestingDetail2 = line.HarvestingDetails.AddNew();
			SetupUS_FieldsData(harvestingDetail2);
			var documentDetail1 = line.DocumentDetails.AddNew();
			documentDetail1.CY_Code = "1";
			var documentDetail2 = line.DocumentDetails.AddNew();
			documentDetail2.CY_Data = "1";

			line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals("line.DocumentDetails.Count", 0, line.DocumentDetails.Count);
			AssertUS_FieldsCleared(line, new[]
			{
				NMFSLine.Schema.US_LineNo, NMFSLine.Schema.US_ProgramType,
				NMFSLine.Schema.US_DocumentType, NMFSLine.Schema.US_DISDocumentID,
				NMFSLine.Schema.US_DolphinSafeStatus, NMFSLine.Schema.US_CaptainStatement,
				NMFSLine.Schema.US_ObserverStatement, NMFSLine.Schema.US_IDCPMemberCertification,
				NMFSLine.Schema.US_TrackingStatus, NMFSLine.Schema.US_IFTPPermitNumber
			});
			AssertUS_FieldsCleared(harvestingDetail1, new[]
			{
				NMFSHarvestingDetail.Schema.US_HarvestedCountry, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
				NMFSHarvestingDetail.Schema.US_VesselCountry, NMFSHarvestingDetail.Schema.US_GearType, NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna
			});
			AssertUS_FieldsCleared(harvestingDetail2, new[]
			{
				NMFSHarvestingDetail.Schema.US_HarvestedCountry, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
				NMFSHarvestingDetail.Schema.US_VesselCountry, NMFSHarvestingDetail.Schema.US_GearType, NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna
			});

			SetupUS_FieldsData(line);
			SetupUS_FieldsData(harvestingDetail1);
			SetupUS_FieldsData(harvestingDetail2);

			documentDetail1 = line.DocumentDetails.AddNew();
			documentDetail1.CY_Code = "1";
			documentDetail2 = line.DocumentDetails.AddNew();
			documentDetail2.CY_Data = "1";

			line.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals("line.DocumentDetails.Count", 2, line.DocumentDetails.Count);
			AssertUS_FieldsCleared(line, new[]
			{
				NMFSLine.Schema.US_LineNo, NMFSLine.Schema.US_ProgramType, NMFSLine.Schema.US_HMSPermitNumber,
				NMFSLine.Schema.US_DISDocumentID, NMFSLine.Schema.US_TrackingStatus, NMFSLine.Schema.US_IFTPPermitNumber, NMFSLine.Schema.US_EBCDNumber
			});
			AssertUS_FieldsCleared(harvestingDetail1, new[]
			{
				NMFSHarvestingDetail.Schema.US_HarvestedCountry, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
				NMFSHarvestingDetail.Schema.US_VesselCountry, NMFSHarvestingDetail.Schema.US_GearType
			});
			AssertUS_FieldsCleared(harvestingDetail2, new[]
			{
				NMFSHarvestingDetail.Schema.US_HarvestedCountry, NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch,
				NMFSHarvestingDetail.Schema.US_VesselCountry, NMFSHarvestingDetail.Schema.US_GearType
			});

			SetupUS_FieldsData(line);
			SetupUS_FieldsData(harvestingDetail1);
			SetupUS_FieldsData(harvestingDetail2);

			line.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("line.DocumentDetails.Count", 0, line.DocumentDetails.Count);
			AssertUS_FieldsCleared(line, new[]
			{
				NMFSLine.Schema.US_LineNo, NMFSLine.Schema.US_ProgramType, NMFSLine.Schema.US_IFTPPermitNumber, NMFSLine.Schema.US_Confidential,
				NMFSLine.Schema.US_NetWeight,  NMFSLine.Schema.US_NetWeightUQ, NMFSLine.Schema.US_SpeciesCode, NMFSLine.Schema.US_OtherAuthorizationNumber,
				NMFSLine.Schema.US_AuthorizationType, NMFSLine.Schema.US_SourceType
			});
			AssertUS_FieldsCleared(harvestingDetail1, new[]
			{
				NMFSHarvestingDetail.Schema.US_HarvestedCountry,
				NMFSHarvestingDetail.Schema.US_VesselCountry, NMFSHarvestingDetail.Schema.US_GearType,
				NMFSHarvestingDetail.Schema.US_GearStartDate, NMFSHarvestingDetail.Schema.US_GearDescription,
				NMFSHarvestingDetail.Schema.US_ContactPartyType,
				NMFSHarvestingDetail.Schema.US_OA_ContactParty, NMFSHarvestingDetail.Schema.ContactPartyOrgPK,
				NMFSHarvestingDetail.Schema.US_GeographicLocation, NMFSHarvestingDetail.Schema.US_NoSmallVessels,
				NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch, NMFSHarvestingDetail.Schema.US_FirstLandingCountry
			});

			SetupUS_FieldsData(line);
			SetupUS_FieldsData(harvestingDetail1);

			documentDetail1 = line.DocumentDetails.AddNew();
			documentDetail1.CY_Code = "1";
			documentDetail2 = line.DocumentDetails.AddNew();
			documentDetail2.CY_Data = "1";

			line.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			AssertEquals("line.DocumentDetails.Count", 0, line.DocumentDetails.Count);
			AssertUS_FieldsCleared(line, new[]
			{
				NMFSLine.Schema.US_LineNo, NMFSLine.Schema.US_ProgramType, NMFSLine.Schema.US_IFTPPermitNumber, NMFSLine.Schema.US_Confidential,
				NMFSLine.Schema.US_SpeciesCode, NMFSLine.Schema.US_SourceType
			});
			AssertUS_FieldsCleared(harvestingDetail1, new[]
			{
				NMFSHarvestingDetail.Schema.US_HarvestedCountry,
				NMFSHarvestingDetail.Schema.US_VesselCountry, NMFSHarvestingDetail.Schema.US_GearType,
				NMFSHarvestingDetail.Schema.US_GearStartDate, NMFSHarvestingDetail.Schema.US_GearDescription,
				NMFSHarvestingDetail.Schema.US_ContactPartyType,
				NMFSHarvestingDetail.Schema.US_OA_ContactParty, NMFSHarvestingDetail.Schema.ContactPartyOrgPK,
				NMFSHarvestingDetail.Schema.US_GeographicLocation, NMFSHarvestingDetail.Schema.US_NoSmallVessels,
				NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch
			});

			SetupUS_FieldsData(line);
			SetupUS_FieldsData(harvestingDetail1);

			documentDetail1 = line.DocumentDetails.AddNew();
			documentDetail1.CY_Code = "1";
			documentDetail2 = line.DocumentDetails.AddNew();
			documentDetail2.CY_Data = "1";

			line.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertUS_FieldsCleared(line, new[]
			{
				NMFSLine.Schema.US_LineNo, NMFSLine.Schema.US_ProgramType, NMFSLine.Schema.US_Commodity,
				NMFSLine.Schema.US_DISDocumentID, NMFSLine.Schema.US_TrackingStatus, NMFSLine.Schema.US_IFTPPermitNumber
			});

			AssertEquals("line.DocumentDetails.Count", 2, line.DocumentDetails.Count);
			AssertEquals("line.HarvestingDetails.Count", 0, line.HarvestingDetails.Count);
			AssertEquals("harvestingDetail1.IsDeleted", true, harvestingDetail1.IsDeleted);

			line.US_ProgramType = ZString.Empty;
			SetupUS_FieldsData(line);
			line.US_Commodity = FishStateList.Codes.FrozenToothfish;
			line.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertUS_FieldsCleared(line, new[]
			{
				NMFSLine.Schema.US_LineNo, NMFSLine.Schema.US_ProgramType, NMFSLine.Schema.US_Commodity,
				NMFSLine.Schema.US_PreApprovalIssuedNumber, NMFSLine.Schema.US_PreApprovalIssuedQuantity, NMFSLine.Schema.US_PreApprovalIssuedQuantityUQ,
				NMFSLine.Schema.US_DISDocumentID, NMFSLine.Schema.US_TrackingStatus, NMFSLine.Schema.US_IFTPPermitNumber
			});
		}

		public void TestIAESNMFS()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;

			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_DocumentType = "ABC";
			nmfsLine.US_ProcessingType = NMFSProductCategoryCodeList.Codes.Dressed;
			nmfsLine.US_IFTPPermitNumber = "123456";
			nmfsLine.US_DISDocumentID = "111111";
			nmfsLine.US_HarvestedCountry = "ZZ";
			nmfsLine.US_GeographicLocation = OceanGeographicAreaCodeList.Codes.CAR;
			nmfsLine.US_VesselCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(NMFSProgramCodeList.Codes.HMS, ((IAESNMFS)nmfsLine).ProgramCode);
			AssertEquals("ABC", ((IAESNMFS)nmfsLine).DocumentType);
			AssertEquals(NMFSProductCategoryCodeList.Codes.Dressed, ((IAESNMFS)nmfsLine).ProcessingTypeCode);
			AssertEquals("123456", ((IAESNMFS)nmfsLine).PermitNumber);
			AssertEquals(NMFSProgramCodeList.Codes.HMS, ((IAESNMFS)nmfsLine).ProgramCode);
			AssertEquals(YesNoDefaultList.Codes.Yes, ((IAESNMFS)nmfsLine).DocumentImageSent);
			AssertEquals("111111", ((IAESNMFS)nmfsLine).DocumentNumber);
			AssertEquals("ZZ", ((IAESNMFS)nmfsLine).SourceCountry);
			AssertEquals(OceanGeographicAreaCodeList.Codes.CAR, ((IAESNMFS)nmfsLine).GeographicLocation);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, ((IAESNMFS)nmfsLine).HarvestingCountry);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine.US_Quantity = 1000m;
			nmfsLine.US_UnitOfMeasure = Core.Constants.Weight.Tonnes;
			nmfsLine.US_CatchDocument = "AAAAAA";
			nmfsLine.US_ReExportNumber = "BBBBB";
			AssertEquals("123456", ((IAESNMFS)nmfsLine).PermitNumber);
			AssertEquals(1000000m, ((IAESNMFS)nmfsLine).Quantity);
			AssertEquals(Core.Constants.Weight.Kilograms, ((IAESNMFS)nmfsLine).UQ);
			AssertEquals("AAAAAA", ((IAESNMFS)nmfsLine).CatchDocumentNumber);
			AssertEquals("BBBBB", ((IAESNMFS)nmfsLine).ReExportNumber);
		}

		public void TestUS_SourceType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			InvoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;

			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			AssertEquals(ZString.Empty, nmfsLine.US_SourceType);
			AssertEquals("NMFSHarvestingDetails collection should be empty", 0, nmfsLine.HarvestingDetails.Count);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			AssertEquals(ZString.Empty, nmfsLine.US_SourceType);
			AssertEquals(true, nmfsLine.US_SourceTypeInfo.ReadOnly);
			AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail.US_OceanAreaOfCatch);
			AssertEquals(ZString.Empty, harvestingDetail.US_ContactPartyType);

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals(ZString.Empty, nmfsLine.US_SourceType);
			AssertEquals(false, nmfsLine.US_SourceTypeInfo.ReadOnly);

			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
			AssertEquals(SourceTypeCodesList.Codes.HatcheryBasedAquaculture, nmfsLine.US_SourceType);
			AssertEquals(ZString.Empty, harvestingDetail.US_OceanAreaOfCatch);
			AssertEquals(EntityRoleCodeList.Codes.AquacultureFacility, harvestingDetail.US_ContactPartyType);

			harvestingDetail.US_NoSmallVessels = 1;
			harvestingDetail.US_GeographicLocation = "BB";
			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			AssertEquals(ZInt.Zero, harvestingDetail.US_NoSmallVessels);
			AssertEquals(ZString.Empty, harvestingDetail.US_GeographicLocation);
		}

		public void TestIsExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoideLineNMFS = invoiceLine.NMFSLines.AddNew();
			AssertEquals(false, invoideLineNMFS.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, invoideLineNMFS.IsExport);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productNMFS = pivot.NMFSLines.AddNew();
			AssertEquals(false, productNMFS.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, productNMFS.IsExport);
		}

		public void TestDefaultIFTPPermitNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "TEST ORG";
			ZString iftpNumber = "1234567888";
			var orgCusCode = orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.IFTPPermitNumber, iftpNumber, Core.Constants.CountryCodes.UnitedStates);
			declaration.IOROrgPK = orgHeader.PK;

			var nmfsLine3 = invoiceLine.NMFSLines.AddNew();
			nmfsLine3.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			Assert("Expected: the default value of IFTP permit number is empty", nmfsLine3.US_IFTPPermitNumber.IsEmpty);

			var nmfsLine4 = invoiceLine.NMFSLines.AddNew();
			nmfsLine4.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			Assert("Expected: the default value of IFTP permit number is from org config", iftpNumber.Equals(nmfsLine4.US_IFTPPermitNumber));

			var nmfsLine5 = invoiceLine.NMFSLines.AddNew();
			nmfsLine5.US_ProgramType = NMFSProgramCodeList.Codes._370;
			Assert("Expected: the default value of IFTP permit number is from org config", iftpNumber.Equals(nmfsLine5.US_IFTPPermitNumber));

			var nmfsLine6 = invoiceLine.NMFSLines.AddNew();
			nmfsLine6.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			Assert("Expected: the default value of IFTP permit number is from org config", iftpNumber.Equals(nmfsLine6.US_IFTPPermitNumber));
		}

		public void TestIsRequiresFullData()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "USSIM");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("RequiresFullData", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, Core.Constants.CountryCodes.UnitedStates);
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew("RequiresFullData", "Yes");
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("RequiresFullData", "No");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoideLineNMFS = invoiceLine.NMFSLines.AddNew();
			invoideLineNMFS.US_SpeciesCode = "1";

			AssertEquals("RequiresFullData should be true", true, invoideLineNMFS.RequiresFullData);
		}

		public void TestChangeUS_SourceTypeHCF_ShouldClearWeight()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;

			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
			AssertEquals("nmfsLine.US_NetWeight", ZDecimal.Zero, nmfsLine.US_NetWeight);
			AssertEquals("nmfsLine.US_NetWeightUQ", ZString.Empty, nmfsLine.US_NetWeightUQ);

			nmfsLine.US_NetWeight = new ZDecimal(123);
			nmfsLine.US_NetWeightUQ = "UQ";
			AssertEquals("nmfsLine.US_NetWeight", new ZDecimal(123), nmfsLine.US_NetWeight);
			AssertEquals("nmfsLine.US_NetWeightUQ", "UQ", nmfsLine.US_NetWeightUQ);

			nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			AssertEquals("nmfsLine.US_NetWeight", ZDecimal.Zero, nmfsLine.US_NetWeight);
			AssertEquals("nmfsLine.US_NetWeightUQ", ZString.Empty, nmfsLine.US_NetWeightUQ);
		}

		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(NMFSLine).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertCollectionContains(NMFSLine.Schema.US_HarvestedCountry, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(NMFSLine.Schema.US_GeographicLocation, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(NMFSLine.Schema.US_VesselCountry, ignoreElementAttributes[0].ElementNames);

			var harvestingDetailsPropertyInfo = typeof(NMFSLine).GetProperty("HarvestingDetails");
			Assert(Attribute.IsDefined(harvestingDetailsPropertyInfo, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		public void TestUS_EBCDNumberMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productNMFS = pivot.NMFSLines.AddNew();
			AssertEquals(USNMFSLineAddInfo.Schema.US_EBCDNumberMaxLength, productNMFS.US_EBCDNumberInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(30, productNMFS.US_EBCDNumberInfo.MaxLength);
		}

		public void TestUS_IFTPPermitNumberMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productNMFS = pivot.NMFSLines.AddNew();
			AssertEquals(USNMFSLineAddInfo.Schema.US_IFTPPermitNumberMaxLength, productNMFS.US_IFTPPermitNumberInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(14, productNMFS.US_IFTPPermitNumberInfo.MaxLength);
		}

		#region Implementation

		void AssertHarvestingDetail(INMFSHarvestingDetail harvestingDetail, ZString countryCode, ZString geographicLocation, ZString processingTypeCode, ZBool containsYellowfinTuna, int harvestingVessels)
		{
			AssertEquals("CountryCode", countryCode, harvestingDetail.CountryCode);
			AssertEquals("GeographicLocation", geographicLocation, harvestingDetail.GeographicLocation);
			AssertEquals("ProcessingTypeCode", processingTypeCode, harvestingDetail.ProcessingTypeCode);
			AssertEquals("ContainsYellowfinTuna", containsYellowfinTuna, harvestingDetail.ContainsYellowfinTuna);
			AssertEquals("harvestingDetail.HarvestingVessels", harvestingVessels, harvestingDetail.HarvestingVessels.Count());
		}

		void AssertUS_FieldsCleared(BusinessObject bizObj, string[] fieldsWithData)
		{
			foreach (ZPropertyInfo info in bizObj.ZPropertyInfoHash)
			{
				var infoName = info.Name;
				if (infoName.StartsWith("US_") && info.HasSetter)
				{
					if (!IsFieldUsedForExportOnly(bizObj, infoName))
					{
						if (fieldsWithData.Contains(infoName))
						{
							AssertNotEquals(infoName + " should not be empty", info.DefaultValue, info.Value);
						}
						else
						{
							AssertEquals(infoName + " should be empty", info.DefaultValue, info.Value);
						}
					}
					else
					{
						Assert("No need to clear value for export", true);
					}
				}
			}
		}

		void SetupUS_FieldsData(BusinessObject bizObj)
		{
			foreach (ZPropertyInfo info in bizObj.ZPropertyInfoHash)
			{
				if (info.Name.StartsWith("US_") && info.HasSetter && !IsFieldUsedForExportOnly(bizObj, info.Name))
				{
					var valueType = info.Value.GetType();
					if (valueType == typeof(ZString))
					{
						info.Value = (ZString)"1";
					}
					else if (valueType == typeof(ZDecimal))
					{
						info.Value = (ZDecimal)1;
					}
					else if (valueType == typeof(ZInt))
					{
						info.Value = (ZInt)1;
					}
					else if (valueType == typeof(ZBool))
					{
						info.Value = ZBool.True;
					}
					else if (valueType == typeof(ZDateTime))
					{
						info.Value = ZDateTime.BrettsBirthday;
					}
					else if (valueType == typeof(ZGuid))
					{
						info.Value = ZGuid.BrettsGuid;
					}
					else
					{
						Fail(valueType.FullName + " is not supported");
					}
				}
			}
		}

		bool IsFieldUsedForExportOnly(BusinessObject bizObj, ZString fieldName)
		{
			return bizObj is NMFSLine
				&& (fieldName == NMFSLine.Schema.US_ProcessingType
					|| fieldName == NMFSLine.Schema.US_HarvestedCountry
					|| fieldName == NMFSLine.Schema.US_GeographicLocation
					|| fieldName == NMFSLine.Schema.US_VesselCountry
					|| fieldName == NMFSLine.Schema.US_Quantity
					|| fieldName == NMFSLine.Schema.US_UnitOfMeasure
					|| fieldName == NMFSLine.Schema.US_CatchDocument
					|| fieldName == NMFSLine.Schema.US_ReExportNumber);
		}

		void AssertDocument(INMFSDocument document, ZString documentIdentifier, ZString documentNumber)
		{
			AssertEquals("DocumentIdentifier", documentIdentifier, document.DocumentIdentifier);
			AssertEquals("ComplianceDescription", documentNumber, document.DocumentNumber);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			return nmfsLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLine.NMFSLines.AddNew();
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
					declaration.US_EnableCRL = true;
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
