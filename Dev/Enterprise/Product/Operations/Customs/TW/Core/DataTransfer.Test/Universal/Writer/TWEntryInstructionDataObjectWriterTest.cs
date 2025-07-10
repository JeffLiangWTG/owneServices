using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;
using Constants = Enterprise.Customs.TW.DataTransfer.Constants;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	[TestedType(typeof(TWEntryInstructionDataObjectWriter))]
	sealed class TWEntryInstructionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestAddInfoGroups()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "TWKEL";
			var supplierOrgAddress = testOrg.Addresses.AddNew();
			supplierOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			supplierOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			supplierOrgAddress.OA_IsActive = true;
			supplierOrgAddress.OA_CompanyNameOverride = "company name.";
			supplierOrgAddress.OA_Phone = "PHONE";
			supplierOrgAddress.OA_Email = "EMAIL";
			supplierOrgAddress.OA_Fax = "FAX";
			supplierOrgAddress.OA_Address1 = "88899 address line1.";
			supplierOrgAddress.OA_Address2 = "88899 address line2.";
			supplierOrgAddress.PrimaryOrgAddressAdditionalInfoDetail = "AdditionalAddressInformation";
			supplierOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, "FRI001", Enterprise.Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = "NX301_DN";
			controllingMessageHeader.TW1_ControllingAgency = "DN";
			controllingMessageHeader.TW1_FunctionalReferenceId = "1";
			controllingMessageHeader.PermitNumber = "A1";
			controllingMessageHeader.TW1_BusinessType = "A";
			controllingMessageHeader.TW1_ProcessingUnit = "KG";
			controllingMessageHeader.TW1_PaymentMethod = "1";
			controllingMessageHeader.TW1_ProofOfPaper = true;
			controllingMessageHeader.TW1_ElectronicReceipt = true;
			controllingMessageHeader.TW1_AppointmentDate = new ZDate(2019, 02, 12);
			controllingMessageHeader.TW1_AppointmentPeriod = "A";
			controllingMessageHeader.TW1_RequestDescription = "AAA";
			controllingMessageHeader.TW1_Purpose = "51";
			controllingMessageHeader.TW1_PrePermitNumber = "PPN";
			controllingMessageHeader.TW1_InspectionRegistrationNumber = "IRN123";
			controllingMessageHeader.TW1_PreWineInspectionStatus = "W";
			controllingMessageHeader.TW1_ApplyForSampleReturn = true;
			controllingMessageHeader.TW1_SamplingReductionReason = "SRR123";
			controllingMessageHeader.TW1_SampleReturnAddress = "SRA123";
			controllingMessageHeader.TW1_IsEstimatedLoadingDate = true;
			controllingMessageHeader.TW1_IsSpecialApplication = true;
			controllingMessageHeader.TW1_SpecialApplicationId = "TD";
			controllingMessageHeader.TW1_CopyQuantity = 1;
			controllingMessageHeader.TW1_OriginalQuantity = 2;
			controllingMessageHeader.TW1_EUSteelProductNo = "nO";
			controllingMessageHeader.TW1_EUSteelProductPhase = "P";
			controllingMessageHeader.TW1_ManufacturerPrintingCode = "C";
			controllingMessageHeader.TW1_ReturnPreviousCOO = true;
			controllingMessageHeader.TW1_PrintingCode = "PC";
			controllingMessageHeader.TW1_IsTriangularTrade = true;
			controllingMessageHeader.TW1_Remarks = "remarks";
			controllingMessageHeader.TW1_Observations = "ob";
			controllingMessageHeader.TW_Notes = "NX101 Notes";
			controllingMessageHeader.TW1_ECFAPrintedRemarks = "DESCRIPTION";
			var previousDocumentNumber1 = controllingMessageHeader.PreviousDocumentNumbers.AddNew();
			previousDocumentNumber1.CSI_ReferenceNumber = "01";
			var previousDocumentNumber2 = controllingMessageHeader.PreviousDocumentNumbers.AddNew();
			previousDocumentNumber2.CSI_ReferenceNumber = "02";
			var certificateOfOrigin1 = controllingMessageHeader.CertificateOfOrigins.AddNew();
			certificateOfOrigin1.CSI_ReferenceNumber = "03";
			var certificateOfOrigin2 = controllingMessageHeader.CertificateOfOrigins.AddNew();
			certificateOfOrigin2.CSI_ReferenceNumber = "04";
			var label = controllingMessageHeader.ProductLabelRanges.AddNew();
			label.TW0_Status = "0";
			label.TW0_EndNumber = "end0";
			label.TW0_StartNumber = "start0";
			label.TW0_RunNumber = "r0";
			label.TW0_Year = "Y77";
			label = controllingMessageHeader.ProductLabelRanges.AddNew();
			label.TW0_Status = "1";
			label.TW0_EndNumber = "end1";
			label.TW0_StartNumber = "start1";
			label.TW0_RunNumber = "r1";
			label.TW0_Year = "Y99";
			var epn = controllingMessageHeader.EthanolPermitNumbers.AddNew();
			epn.CSI_ReferenceNumber = "epn0";
			epn = controllingMessageHeader.EthanolPermitNumbers.AddNew();
			epn.CSI_ReferenceNumber = "epn1";
			var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
			localProcessorAddress.OrganisationPK = testOrg.PK;
			localProcessorAddress.E2_OA_Address = testOrg.MainAddress.PK;
			localProcessorAddress.E2_AddressOverride = true;
			var localAddress = localProcessorAddress.LocalAddress;
			localAddress.Address1 = "製造商地址 1";
			localAddress.Address2 = "製造商地址 2";
			localAddress.AdditionalAddressInformation = "製造商地址 3";
			localAddress.CompanyName = "製造商";
			localAddress.City = "台北市";
			controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			controllingMessageHeader.TW1_ControllingAgency = "FT";
			controllingMessageHeader.TW1_FunctionalReferenceId = "2";
			controllingMessageHeader.PermitNumber = "B1";
			controllingMessageHeader.TW1_BusinessType = "B";
			controllingMessageHeader.TW1_ProcessingUnit = "KG";
			controllingMessageHeader.TW1_PaymentMethod = "2";
			controllingMessageHeader.TW1_ProofOfPaper = false;
			controllingMessageHeader.TW1_ElectronicReceipt = false;
			controllingMessageHeader.TW1_AppointmentDate = new ZDate(2019, 02, 13);
			controllingMessageHeader.TW1_AppointmentPeriod = "";
			controllingMessageHeader.TW1_RequestDescription = "BBB";
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
			controllingMessageHeader.TW1_Purpose = "89";
			controllingMessageHeader.TW1_PrePermitNumber = "PPN456";
			controllingMessageHeader.TW1_InspectionRegistrationNumber = "IRN456";
			controllingMessageHeader.TW1_PreWineInspectionStatus = "X";
			controllingMessageHeader.TW1_ApplyForSampleReturn = false;
			controllingMessageHeader.TW1_SamplingReductionReason = "SRR456";
			controllingMessageHeader.TW1_SampleReturnAddress = "SRA456";
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);
			NUnit.Framework.Assert.That(result.AddInfoGroupCollection.Count, Is.EqualTo(4));

			var groupCM1 = result.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.EntryInstruction.Codes.CM
				&& x.AddInfoCollection.Any(a => a.Key.GetValueOrDefault() == Constants.AddInfoKeys.ControllingMessage.MessageNumber && a.Value.GetValueOrDefault() == "1"));
			CombineAssertions("groupCM1", () =>
			{
				NUnit.Framework.Assert.That(groupCM1, Is.Not.EqualTo(default(Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup)), "Should have CM group with message number is 1 - should not be [null]");
				NUnit.Framework.Assert.That(groupCM1.AddInfoGroupCollection.Count, Is.EqualTo(4));
				NUnit.Framework.Assert.That(groupCM1.AddInfoGroupCollection.Any(group => group.Type.Code.GetValueOrDefault() == "LB" && group.AddInfoCollection.Count == 5 && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "Status" && innerAddinfo.Value.GetValueOrDefault() == "0") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "EndNumber" && innerAddinfo.Value.GetValueOrDefault() == "end0") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "StartNumber" && innerAddinfo.Value.GetValueOrDefault() == "start0") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "RunNumber" && innerAddinfo.Value.GetValueOrDefault() == "r0") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "Year" && innerAddinfo.Value.GetValueOrDefault() == "Y77")), Is.True);
				NUnit.Framework.Assert.That(groupCM1.AddInfoGroupCollection.Any(group => group.Type.Code.GetValueOrDefault() == "LB" && group.AddInfoCollection.Count == 5 && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "Status" && innerAddinfo.Value.GetValueOrDefault() == "1") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "EndNumber" && innerAddinfo.Value.GetValueOrDefault() == "end1") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "StartNumber" && innerAddinfo.Value.GetValueOrDefault() == "start1") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "RunNumber" && innerAddinfo.Value.GetValueOrDefault() == "r1") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "Year" && innerAddinfo.Value.GetValueOrDefault() == "Y99")), Is.True);
				NUnit.Framework.Assert.That(groupCM1.AddInfoGroupCollection.Any(group => group.Type.Code.GetValueOrDefault() == "EPN" && group.AddInfoCollection.Count == 1 && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "EthanolPermitNumbers" && innerAddinfo.Value.GetValueOrDefault() == "epn0")), Is.True);
				NUnit.Framework.Assert.That(groupCM1.AddInfoGroupCollection.Any(group => group.Type.Code.GetValueOrDefault() == "EPN" && group.AddInfoCollection.Count == 1 && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "EthanolPermitNumbers" && innerAddinfo.Value.GetValueOrDefault() == "epn1")), Is.True);
				NUnit.Framework.Assert.That(groupCM1.OrganizationAddressCollection.Any(address => address.AddressType.GetValueOrDefault() == "LocalProcessorAddress" && address.CompanyName.GetValueOrDefault() == "company name." && address.Phone.GetValueOrDefault() == "PHONE" && address.Email.GetValueOrDefault() == "EMAIL" && address.Fax.GetValueOrDefault() == "FAX" && address.Address1.GetValueOrDefault() == "88899 address line1." && address.Address2.GetValueOrDefault() == "88899 address line2." && address.AdditionalAddressInformation.GetValueOrDefault() == "AdditionalAddressInformation"), Is.True);
				NUnit.Framework.Assert.That(groupCM1.OrganizationAddressCollection.Any(address => address.AddressType.GetValueOrDefault() == "LocalProcessorTranslatedDocAddress" && address.CompanyName.GetValueOrDefault() == "製造商" && address.Address1.GetValueOrDefault() == "製造商地址 1" && address.Address2.GetValueOrDefault() == "製造商地址 2" && address.AdditionalAddressInformation.GetValueOrDefault() == "製造商地址 3" && address.City.GetValueOrDefault() == "台北市"), Is.True);
			});

			var cm1AddInfoCollection = groupCM1.AddInfoCollection;
			CombineAssertions("cm1AddInfoCollection", () =>
			{
				NUnit.Framework.Assert.That(cm1AddInfoCollection.Count, Is.EqualTo(35), "Count");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PermitNumber), Is.EqualTo("A1").Using(CustomComparers.TypeComparison), "PermitNumber");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ControllingAgency), Is.EqualTo("DN").Using(CustomComparers.TypeComparison), "ControllingAgency");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.BusinessType), Is.EqualTo("A").Using(CustomComparers.TypeComparison), "BusinessType");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ProcessingUnit), Is.EqualTo("KG").Using(CustomComparers.TypeComparison), "ProcessingUnit");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PaymentMethod), Is.EqualTo("1").Using(CustomComparers.TypeComparison), "PaymentMethod");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ProofOfPaper), Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ProofOfPaper");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ElectronicReceipt), Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ElectronicReceipt");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.AppointmentDate), Is.EqualTo("2019-02-12").Using(CustomComparers.TypeComparison), "AppointmentDate");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.AppointmentPeriod), Is.EqualTo("A").Using(CustomComparers.TypeComparison), "AppointmentPeriod");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.Request), Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "Request");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ControllingMessageType), Is.EqualTo("NX301_DN").Using(CustomComparers.TypeComparison), "ControllingMessageType");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.Link), Is.EqualTo("1").Using(CustomComparers.TypeComparison), "Link");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.Purpose), Is.EqualTo("51").Using(CustomComparers.TypeComparison), "Purpose");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PreviousPermitNumber), Is.EqualTo("PPN").Using(CustomComparers.TypeComparison), "PreviousPermitNumber");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.InspectionRegistrationNumber), Is.EqualTo("IRN123").Using(CustomComparers.TypeComparison), "InspectionRegistrationNumber");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PreviousWineInspectionStatus), Is.EqualTo("W").Using(CustomComparers.TypeComparison), "PreviousWineInspectionStatus");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ApplyForSampleReturn), Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "ApplyForSampleReturn");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.SamplingReductionReason), Is.EqualTo("SRR123").Using(CustomComparers.TypeComparison), "SamplingReductionReason");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.SampleReturnAddress), Is.EqualTo("SRA123").Using(CustomComparers.TypeComparison), "SampleReturnAddress");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("IsEstimatedLoadingDate"), Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "TW1_IsEstimatedLoadingDate");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("SpecialApplicationId"), Is.EqualTo("TD").Using(CustomComparers.TypeComparison), "TW1_SpecialApplicationId");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("CopyQuantity"), Is.EqualTo("1").Using(CustomComparers.TypeComparison), "TW1_CopyQuantity");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("OriginalQuantity"), Is.EqualTo("2").Using(CustomComparers.TypeComparison), "TW1_OriginalQuantity");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("EUSteelProductNo"), Is.EqualTo("nO").Using(CustomComparers.TypeComparison), "TW1_EUSteelProductNo");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("EUSteelProductPhase"), Is.EqualTo("P").Using(CustomComparers.TypeComparison), "TW1_EUSteelProductPhase");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("ManufacturerPrintingCode"), Is.EqualTo("C").Using(CustomComparers.TypeComparison), "TW1_ManufacturerPrintingCode");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("ReturnPreviousCOO"), Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "TW1_ReturnPreviousCOO");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("PrintingCode"), Is.EqualTo("PC").Using(CustomComparers.TypeComparison), "TW1_PrintingCode");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("IsTriangularTrade"), Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "TW1_IsTriangularTrade");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("Remarks"), Is.EqualTo("remarks").Using(CustomComparers.TypeComparison), "TW1_Remarks");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue("Observations"), Is.EqualTo("ob").Using(CustomComparers.TypeComparison), "TW1_Observations");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.NX101_Notes), Is.EqualTo("NX101 Notes").Using(CustomComparers.TypeComparison), "TW_Notes");
				NUnit.Framework.Assert.That(cm1AddInfoCollection.GetZStringValue(PredefinedNoteTypes.Instance.ECFAPrintedRemarks.Description), Is.EqualTo("DESCRIPTION").Using(CustomComparers.TypeComparison), "ECFA Printed Remarks");
			});

			var groupPDN = result.AddInfoGroupCollection.Where(x => x.Type.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.PreviousDocumentNumber);
			var groupPDNAddInfoCollection = groupPDN.FirstOrDefault().AddInfoCollection;
			CombineAssertions("groupPDN", () =>
			{
				NUnit.Framework.Assert.That(groupPDN.Count(), Is.EqualTo(1), "groupPDN count");
				NUnit.Framework.Assert.That(groupPDNAddInfoCollection.Count, Is.EqualTo(2), "groupPDNAddInfoCollection count");
				AssertContainsExactElementsInAnyOrder("groupPDNAddInfoCollection should have 01, 02", new[] { "01", "02" }, groupPDNAddInfoCollection.Select(x => x.Value.GetValueOrDefault()));
			});

			var groupCON = result.AddInfoGroupCollection.Where(x => x.Type.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.CmCertificateOfOriginNumber);
			var groupCONAddInfoCollection = groupCON.FirstOrDefault().AddInfoCollection;
			CombineAssertions("groupCON", () =>
			{
				NUnit.Framework.Assert.That(groupCON.Count(), Is.EqualTo(1), "groupCON count");
				NUnit.Framework.Assert.That(groupCONAddInfoCollection.Count, Is.EqualTo(2), "groupCONAddInfoCollection count");
				AssertContainsExactElementsInAnyOrder("groupCONAddInfoCollection should have 03, 04", new[] { "03", "04" }, groupCONAddInfoCollection.Select(x => x.Value.GetValueOrDefault()));
			});

			var groupCM2 = result.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.EntryInstruction.Codes.CM
				&& x.AddInfoCollection.Any(a => a.Key.GetValueOrDefault() == Constants.AddInfoKeys.ControllingMessage.MessageNumber && a.Value.GetValueOrDefault() == "2"));
			NUnit.Framework.Assert.That(groupCM2, Is.Not.EqualTo(default(Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup)), "Should have CM group with message number is 2 - should not be [null]");

			var cm2AddInfoCollection = groupCM2.AddInfoCollection;
			CombineAssertions("cm2AddInfoCollection", () =>
			{
				NUnit.Framework.Assert.That(cm2AddInfoCollection.Count, Is.EqualTo(17));
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PermitNumber), Is.EqualTo("B1").Using(CustomComparers.TypeComparison), "PermitNumber");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ControllingAgency), Is.EqualTo("FT").Using(CustomComparers.TypeComparison), "ControllingAgency");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.BusinessType), Is.EqualTo("B").Using(CustomComparers.TypeComparison), "BusinessType");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ProcessingUnit), Is.EqualTo("KG").Using(CustomComparers.TypeComparison), "ProcessingUnit");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PaymentMethod), Is.EqualTo("2").Using(CustomComparers.TypeComparison), "PaymentMethod");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.AppointmentDate), Is.EqualTo("2019-02-13").Using(CustomComparers.TypeComparison), "AppointmentDate");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.Request), Is.EqualTo("BBB").Using(CustomComparers.TypeComparison), "Request");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.ControllingMessageType), Is.EqualTo("X101").Using(CustomComparers.TypeComparison), "ControllingMessageType");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.Link), Is.EqualTo("2").Using(CustomComparers.TypeComparison), "Link");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.CertificateType), Is.EqualTo("00").Using(CustomComparers.TypeComparison), "CertificateType");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.Purpose), Is.EqualTo("89").Using(CustomComparers.TypeComparison), "Purpose");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PreviousPermitNumber), Is.EqualTo("PPN456").Using(CustomComparers.TypeComparison), "PreviousPermitNumber");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.InspectionRegistrationNumber), Is.EqualTo("IRN456").Using(CustomComparers.TypeComparison), "InspectionRegistrationNumber");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PreviousWineInspectionStatus), Is.EqualTo("X").Using(CustomComparers.TypeComparison), "PreviousWineInspectionStatus");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.SamplingReductionReason), Is.EqualTo("SRR456").Using(CustomComparers.TypeComparison), "SamplingReductionReason");
				NUnit.Framework.Assert.That(cm2AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.SampleReturnAddress), Is.EqualTo("SRA456").Using(CustomComparers.TypeComparison), "SampleReturnAddress");
			});

			var organizationAddress = groupCM1.OrganizationAddressCollection.First(address => address.AddressType.GetValueOrDefault() == "LocalProcessorAddress");
			NUnit.Framework.Assert.That(organizationAddress.RegistrationNumberCollection.Any(registrationNumber => registrationNumber.Type.Code.GetValueOrDefault() == "FRI" && registrationNumber.Type.Description.GetValueOrDefault() == "Factory Registration Number" && registrationNumber.Value.GetValueOrDefault() == "FRI001"), Is.True);
			writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory), new List<ZGuid> { });
			result = writer.GetDataObject(entryInstruction);
			NUnit.Framework.Assert.That(result.AddInfoGroupCollection.Count, Is.EqualTo(0));
			writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory), new List<ZGuid> { controllingMessageHeader.PK });
			result = writer.GetDataObject(entryInstruction);
			NUnit.Framework.Assert.That(result.AddInfoGroupCollection.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result.AddInfoGroupCollection.Any(group => group.Type.Code.GetValueOrDefault() == "CM" && group.AddInfoCollection.Count == 17 && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "MessageNumber" && innerAddinfo.Value.GetValueOrDefault() == "2") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "PermitNumber" && innerAddinfo.Value.GetValueOrDefault() == "B1") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "ControllingAgency" && innerAddinfo.Value.GetValueOrDefault() == "FT") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "BusinessType" && innerAddinfo.Value.GetValueOrDefault() == "B") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "ProcessingUnit" && innerAddinfo.Value.GetValueOrDefault() == "KG") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "PaymentMethod" && innerAddinfo.Value.GetValueOrDefault() == "2") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "AppointmentDate" && innerAddinfo.Value.GetValueOrDefault() == "2019-02-13") && group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key.GetValueOrDefault() == "Request" && innerAddinfo.Value.GetValueOrDefault() == "BBB")), Is.True);
			localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			localProcessorAddress.IDCode = "PAS001";
			writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			result = writer.GetDataObject(entryInstruction);
			groupCM1 = result.AddInfoGroupCollection.First(group => group.Type.Code.GetValueOrDefault() == "CM" && group.AddInfoCollection.Count == 35);
			NUnit.Framework.Assert.That(groupCM1.OrganizationAddressCollection.Any(address => address.AddressType.GetValueOrDefault() == "LocalProcessorAddress" && address.CompanyName.GetValueOrDefault() == "company name." && address.Phone.GetValueOrDefault() == "PHONE" && address.Email.GetValueOrDefault() == "EMAIL" && address.Fax.GetValueOrDefault() == "FAX" && address.Address1.GetValueOrDefault() == "88899 address line1." && address.Address2.GetValueOrDefault() == "88899 address line2." && address.AdditionalAddressInformation.GetValueOrDefault() == "AdditionalAddressInformation" && address.GovRegNumType.Code.GetValueOrDefault() == "PAS" && address.GovRegNumType.Description.GetValueOrDefault() == "Passport Number" && address.GovRegNum.GetValueOrDefault() == "PAS001"), Is.True);
			localProcessorAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			localProcessorAddress.IDCode = "PID001";
			writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			result = writer.GetDataObject(entryInstruction);
			groupCM1 = result.AddInfoGroupCollection.First(group => group.Type.Code.GetValueOrDefault() == "CM" && group.AddInfoCollection.Count == 35);
			organizationAddress = groupCM1.OrganizationAddressCollection.First(address => address.AddressType.GetValueOrDefault() == "LocalProcessorAddress");
			NUnit.Framework.Assert.That(organizationAddress.RegistrationNumberCollection.Any(registrationNumber => registrationNumber.Type.Code.GetValueOrDefault() == "PID" && registrationNumber.Type.Description.GetValueOrDefault() == "Republic of China (Taiwan) National ID Card Number" && registrationNumber.Value.GetValueOrDefault() == "PID001"), Is.True);
		}

		[ExpectNoExceptions]
		public void TestOrganizationAddressCollection_Applicant()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "TWKEL";
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var documentaryAddress = controllingMessageHeader.ApplicantDocumentaryAddress;
			documentaryAddress.OrganisationPK = testOrg.PK;
			documentaryAddress.E2_OA_Address = testOrg.MainAddress.PK;
			documentaryAddress.E2_AddressOverride = true;
			var localAddress = documentaryAddress.LocalAddress;
			localAddress.Address1 = "address 1";
			localAddress.Address2 = "address 2";
			localAddress.AdditionalAddressInformation = "address 3";
			localAddress.CompanyName = "name";
			localAddress.City = "TPE";
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);
			var controllingMessageGroup = result.AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == Constants.EntryInstruction.Codes.CM);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Count, Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Any(x => x.AddressType.GetValueOrDefault() == "Applicant"), Is.True, "Applicant");
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Any(x => x.AddressType.GetValueOrDefault() == "ApplicantTranslatedDocumentaryAddress"), Is.True, "ApplicantTranslatedDocumentaryAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestOrganizationAddressCollection_Supplier()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "TWKEL";
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var documentaryAddress = controllingMessageHeader.SupplierDocumentaryAddress;
			documentaryAddress.OrganisationPK = testOrg.PK;
			documentaryAddress.E2_OA_Address = testOrg.MainAddress.PK;
			documentaryAddress.E2_AddressOverride = true;
			var localAddress = documentaryAddress.LocalAddress;
			localAddress.Address1 = "address 1";
			localAddress.Address2 = "address 2";
			localAddress.AdditionalAddressInformation = "address 3";
			localAddress.CompanyName = "name";
			localAddress.City = "TPE";
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);
			var controllingMessageGroup = result.AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == Constants.EntryInstruction.Codes.CM);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Count, Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Any(x => x.AddressType.GetValueOrDefault() == "SupplierDocumentaryAddress"), Is.True, "SupplierDocumentaryAddress");
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Any(x => x.AddressType.GetValueOrDefault() == "SupplierTranslatedDocumentaryAddress"), Is.True, "SupplierTranslatedDocumentaryAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestOrganizationAddressCollection_Importer()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "TWKEL";
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			Factory.Save();

			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var documentaryAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			documentaryAddress.OrganisationPK = testOrg.PK;
			documentaryAddress.E2_OA_Address = testOrg.MainAddress.PK;
			documentaryAddress.E2_AddressOverride = true;
			var localAddress = documentaryAddress.LocalAddress;
			localAddress.Address1 = "address 1";
			localAddress.Address2 = "address 2";
			localAddress.AdditionalAddressInformation = "address 3";
			localAddress.CompanyName = "name";
			localAddress.City = "TPE";
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);
			var controllingMessageGroup = result.AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == Constants.EntryInstruction.Codes.CM);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Count, Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Any(x => x.AddressType.GetValueOrDefault() == "ImporterDocumentaryAddress"), Is.True, "ImporterDocumentaryAddress");
				NUnit.Framework.Assert.That(controllingMessageGroup.OrganizationAddressCollection.Any(x => x.AddressType.GetValueOrDefault() == "ImporterTranslatedDocumentaryAddress"), Is.True, "ImporterTranslatedDocumentaryAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestGetEntryInstructionAddInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.TW_TradersRemarks = "A";
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(entryInstruction);
			NUnit.Framework.Assert.That(result.AddInfoCollection.Any(addInfo => addInfo.Key.GetValueOrDefault() == "TW Traders Remarks" && addInfo.Value.GetValueOrDefault() == "A"), Is.True);
		}

		[ExpectNoExceptions]
		public void TestGetEntryInstructionAddInfo_BrokerLicense()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "DEVELOPER COMPANY";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
			var newBroker = Factory.NewWithValidTestData<GlbStaff>();
			newBroker.GS_GB_HomeBranch = GlbStaff.CurrentUser.HomeBranch.PK;
			var brkCertificate = newBroker.Certificates.AddNew();
			brkCertificate.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "123456";
			brkCertificate.XZ_ExpiryOrDueDate = System.DateTime.Today.AddYears(1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var writer = new TWEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new TWDataObjectWriterHelper(Factory));
			var result = writer.GetDataObject(declaration.CusEntryInstruction);
			NUnit.Framework.Assert.That(result.AddInfoCollection.Any(addInfo => addInfo.Key.GetValueOrDefault() == "BrokerLicense" && addInfo.Value.GetValueOrDefault() == "123456"), Is.True, "AddInfo collection contains the BrokerLicense");
		}
	}
}
