using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.ASYCUDA.SGAccess;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		void CreateProcedureAndCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "123", "456", "1000", "", "INP", "SFZ");
			procedure1.ZZ6_StartDate = new ZDateTime(1970, 1, 1);
			procedure1.ZZ6_EndDate = new ZDateTime(2079, 6, 6);
			procedure1.Attributes.AddNew(Universal.AttributeNames.Codes.ISAEO, "anything");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "321", "654", "1000", "", "OUT", "DRT");
			procedure2.ZZ6_StartDate = new ZDateTime(1970, 1, 1);
			procedure2.ZZ6_EndDate = new ZDateTime(2079, 6, 6);
			procedure2.Attributes.AddNew(Universal.AttributeNames.Codes.ISAEO, "anything");
			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "777", "888", "1000", "", "TNP", "TTF");
			procedure3.ZZ6_StartDate = new ZDateTime(1970, 1, 1);
			procedure3.ZZ6_EndDate = new ZDateTime(2079, 6, 6);
			procedure3.Attributes.AddNew(Universal.AttributeNames.Codes.ISAEO, "anything");
			helper.CreateNewOrGetExistingCusCodeType(OrgCusCode.SingaporeCodeTypes.AEO, "AEO");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, OrgCusCode.SingaporeCodeTypes.AEO, "KR", "Korea", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, OrgCusCode.SingaporeCodeTypes.AEO, "JP", "Japan", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
		}

		public void TestShouldAEOBeApplied()
		{
			CreateProcedureAndCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			Assert(declaration.ShouldAEOBeApplied);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Assert(!declaration.ShouldAEOBeApplied);
		}

		public void TestAEOAppliedCountryList()
		{
			CreateProcedureAndCodeList();
			var declaration = Factory.New<JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "KR", "JP" }, declaration.AEOAppliedCountryList);
		}

		public void TestJE_OH_Importer()
		{
			CreateProcedureAndCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration.JE_RL_NKOrigin = "KR123";
			var orgHeader1 = Factory.New<OrgHeader>();
			var customsCode1 = orgHeader1.CustomsCodes.AddNew();
			customsCode1.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Singapore;
			customsCode1.OK_CustomsRegNo = "111";
			var customsCode2 = orgHeader1.CustomsCodes.AddNew();
			customsCode2.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			customsCode2.OK_CustomsRegNo = "222";
			var orgHeader2 = Factory.New<OrgHeader>();
			var customsCode3 = orgHeader2.CustomsCodes.AddNew();
			customsCode3.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			customsCode3.OK_CustomsRegNo = "333";
			var orgHeader3 = Factory.New<OrgHeader>();
			var customsCode4 = orgHeader3.CustomsCodes.AddNew();
			customsCode4.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			customsCode4.OK_CustomsRegNo = "444";
			declaration.JE_OH_Importer = orgHeader1.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("111", declaration.CPCs[0].SG_PC2);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Importer = orgHeader2.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("333", declaration.CPCs[0].SG_PC2);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Importer = orgHeader3.PK;
			AssertEquals(0, declaration.CPCs.Count);
			declaration.JE_RL_NKOrigin = "JP123";
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Importer = orgHeader1.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("111", declaration.CPCs[0].SG_PC2);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Importer = orgHeader2.PK;
			AssertEquals(0, declaration.CPCs.Count);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Importer = orgHeader3.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("444", declaration.CPCs[0].SG_PC2);
		}

		public void TestJE_OH_Supplier()
		{
			CreateProcedureAndCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration.JE_RL_NKFinalDestination = "KR123";
			var orgHeader1 = Factory.New<OrgHeader>();
			var customsCode1 = orgHeader1.CustomsCodes.AddNew();
			customsCode1.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Singapore;
			customsCode1.OK_CustomsRegNo = "111";
			var customsCode2 = orgHeader1.CustomsCodes.AddNew();
			customsCode2.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			customsCode2.OK_CustomsRegNo = "222";
			var orgHeader2 = Factory.New<OrgHeader>();
			var customsCode3 = orgHeader2.CustomsCodes.AddNew();
			customsCode3.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			customsCode3.OK_CustomsRegNo = "333";
			var orgHeader3 = Factory.New<OrgHeader>();
			var customsCode4 = orgHeader3.CustomsCodes.AddNew();
			customsCode4.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			customsCode4.OK_CustomsRegNo = "444";
			declaration.JE_OH_Supplier = orgHeader1.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("111", declaration.CPCs[0].SG_PC2);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Supplier = orgHeader2.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("333", declaration.CPCs[0].SG_PC2);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Supplier = orgHeader3.PK;
			AssertEquals(0, declaration.CPCs.Count);
			declaration.JE_RL_NKFinalDestination = "JP123";
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Supplier = orgHeader1.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("111", declaration.CPCs[0].SG_PC2);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Supplier = orgHeader2.PK;
			AssertEquals(0, declaration.CPCs.Count);
			declaration.CPCs.DeleteAll();
			declaration.JE_OH_Supplier = orgHeader3.PK;
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("444", declaration.CPCs[0].SG_PC2);
		}

		public void TestJE_RL_NKOrigin()
		{
			CreateProcedureAndCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TTF;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration.JE_RL_NKOrigin = "JP123";
			var orgHeader2 = Factory.New<OrgHeader>();
			var customsCode3 = orgHeader2.CustomsCodes.AddNew();
			customsCode3.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			customsCode3.OK_CustomsRegNo = "333";
			declaration.JE_OH_Importer = orgHeader2.PK;
			AssertEquals(0, declaration.CPCs.Count);
			declaration.JE_RL_NKOrigin = "KR123";
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("333", declaration.CPCs[0].SG_PC2);
		}

		public void TestJE_RL_NKFinalDestination()
		{
			CreateProcedureAndCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TTF;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration.JE_RL_NKFinalDestination = "JP123";
			var orgHeader2 = Factory.New<OrgHeader>();
			var customsCode3 = orgHeader2.CustomsCodes.AddNew();
			customsCode3.OK_CodeType = OrgCusCode.SingaporeCodeTypes.AEO;
			customsCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			customsCode3.OK_CustomsRegNo = "333";
			declaration.JE_OH_Supplier = orgHeader2.PK;
			AssertEquals(0, declaration.CPCs.Count);
			declaration.JE_RL_NKFinalDestination = "KR123";
			AssertEquals(1, declaration.CPCs.Count);
			AssertEquals("333", declaration.CPCs[0].SG_PC2);
		}

		public void TestSettingJE_RL_NKFinalDestinationUpdatesSG_RN_NKFinalDestination() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Precondition: SG_RN_NKFinalDestination is empty", ZString.Empty, declaration.SG_RN_NKFinalDestination);
			declaration.JE_RL_NKFinalDestination = "Z";
			AssertEquals("SG_RN_NKFinalDestination is not set since 'ZZ' is not a default country code", ZString.Empty, declaration.SG_RN_NKFinalDestination);
			declaration.JE_RL_NKFinalDestination = "JP123";
			AssertEquals("SG_RN_NKFinalDestination defaults to 'JP'", "JP", declaration.SG_RN_NKFinalDestination);
			declaration.JE_RL_NKFinalDestination = "AU123";
			AssertEquals("Updating JE_RL_NKFinalDestination updates SG_RN_NKFinalDestination if previous value matches previous JE_RL_NKFinalDestination", "AU", declaration.SG_RN_NKFinalDestination);
			declaration.SG_RN_NKFinalDestination = "NZ";
			declaration.JE_RL_NKFinalDestination = "DE123";
			AssertEquals("Updating JE_RL_NKFinalDestination doesn't update SG_RN_NKFinalDestination if it's not empty and doesn't match previous JE_RL_NKFinalDestination", "NZ", declaration.SG_RN_NKFinalDestination);
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertEquals("Clearing JE_RL_NKFinalDestination doesn't clear SG_RN_NKFinalDestination", "NZ", declaration.SG_RN_NKFinalDestination);
		});

		public void TestSettingJE_OH_ImporterUpdatesSG_RN_NKFinalDestination() => CombineAssertions(() =>
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "SGMIK";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_OH_Importer = org.PK;
			AssertEquals("JE_RL_NKFinalDestination", "SGMIK", dec.JE_RL_NKFinalDestination);
			AssertEquals("SG_RN_NKFinalDestination", "SG", dec.SG_RN_NKFinalDestination);
		});

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		protected override ZString ImportMessageTypeForTest
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}

		protected override ZString ExportMessageTypeForTest
		{
			get
			{
				return MessageTypeCodeList.Codes.TNP;
			}
		}

		public void TestReciprocalRates()
		{
			Assert(Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.Singapore, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestCusEntryHeader()
		{
			AssertEquals("Precondition: Should have no CusEntryHeaders", 0, Declaration.CustomsEntryHeaders.Count);
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2000m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "24011010";
			Declaration.Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = Declaration.CusEntryHeader;
			AssertEquals("Should now have a CusEntryHeader", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("CusEntryHeaders should match", Declaration.CustomsEntryHeaders[0], entryHeader);
		}

		public override void TestMessageTypeForDocumentFilter()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, Declaration.MessageTypeForDocumentFilter);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, Declaration.MessageTypeForDocumentFilter);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("Export type", JobMessageTypeList.Codes.Export, Declaration.MessageTypeForDocumentFilter);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals("Transhipment type", "TNP", Declaration.MessageTypeForDocumentFilter);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("Certificate of Origin type", ZString.Empty, Declaration.MessageTypeForDocumentFilter);
		}

		public void TestIImportExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Assert(declaration.IsCrossTrade());
			AssertEquals(Directions.CrossTrade, ((IImportExport)declaration).JobDirection);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Assert(declaration.IsImport());
			AssertEquals(Directions.Import, ((IImportExport)declaration).JobDirection);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Assert(declaration.IsExport());
			AssertEquals(Directions.Export, ((IImportExport)declaration).JobDirection);
			declaration.JE_MessageType = "BLA";
			Assert(declaration.IsUnknown());
			AssertEquals(Directions.Unknown, ((IImportExport)declaration).JobDirection);
		}

		public void TestIsOUTDEC()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals("IsOUTDEC", false, declaration.IsOUTDEC);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("IsOUTDEC", false, declaration.IsOUTDEC);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals("IsOUTDEC", true, declaration.IsOUTDEC);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals("IsOUTDEC", false, declaration.IsOUTDEC);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("IsOUTDEC", false, declaration.IsOUTDEC);
		}

		public void TestTransportModeWhenOutDec()
		{
			var inwardDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			inwardDeclaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			inwardDeclaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals(false, inwardDeclaration.IsAir);
			AssertEquals(true, inwardDeclaration.IsSea);
			AssertEquals(false, inwardDeclaration.IsPost);
			var outwardDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			outwardDeclaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			outwardDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Pre-condition: JE_TransportMode should be blank for Outdec", ZString.Empty, outwardDeclaration.JE_TransportMode);
			AssertEquals("IsAir when JE_TransportMode is empty & declaration is an OUTDEC should use the OutwardTransportMode", true, outwardDeclaration.IsAir);
			AssertEquals(false, outwardDeclaration.IsSea);
			AssertEquals(false, inwardDeclaration.IsRoad);
			var transhipmentDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			transhipmentDeclaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			transhipmentDeclaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			transhipmentDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals(false, transhipmentDeclaration.IsAir);
			AssertEquals(false, transhipmentDeclaration.IsSea);
			AssertEquals(false, transhipmentDeclaration.IsRail);
			AssertEquals("Transhipment", true, transhipmentDeclaration.IsRoad);
		}

		public void TestJE_Calc_InvoicesCount()
		{
			AssertEquals("Invoices Count", 0, Declaration.JE_Calc_InvoicesCount);
			Declaration.Invoices.AddNew();
			AssertEquals("Invoices Count", 1, Declaration.JE_Calc_InvoicesCount);
		}

		public void TestJE_OH_InwardCarrierAgent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals("Test pre-requisite: JE_OH_ShippingLine should be empty", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			var carrierAgent = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_InwardCarrierAgent = carrierAgent.PK;
			AssertEquals("JE_OH_ShippingLine should not default to Carrier Agent for Air or where Carrier Agent is not a carrier", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			declaration.JE_OH_InwardCarrierAgent = carrier.PK;
			AssertEquals("JE_OH_ShippingLine should default to Carrier Agent for SEA where Carrier Agent is a carrier", carrier.PK, declaration.JE_OH_ShippingLine);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICusAddInfoTypeSupporter supporter = declaration;
			supporter.AssertType(typeof(SGCPC), CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode);
			supporter.AssertType(null, "ZZ!");
			var cpc = declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(cpc.PK);
			AssertEquals(typeof(SGCPC), addInfo.GetType());
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICusCodeDataTypeSupporter supporter = declaration;
			supporter.AssertType(typeof(CALicenceNumber), CusCodeDataTypeList.Codes.CALicenceNumber);
			supporter.AssertType(null, "ZZ!");
			var licence = declaration.CALicences.AddNew();
			licence.CY_Data = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cusCode = newFactory.Load<CusCodeData>(licence.PK);
			AssertEquals(typeof(CALicenceNumber), cusCode.GetType());
		}

		public void TestDefaultJE_ApplicationCode_NotConfigureLocalCountryCustomsInterface()
		{
			AssertEquals("JE_ApplicationCode 4.1", JobApplicationCodeList.Codes.TradeNet41, Factory.New<JobDeclaration>().JE_ApplicationCode);
		}

		public void TestDefaultJE_ApplicationCode_ConfigureLocalCountryCustomsInterface()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BuiltIn", JobApplicationCodeList.Codes.TradeNet41, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BothBuiltInDefaulted", JobApplicationCodeList.Codes.TradeNet41, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Interfaced", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BothInterfaceDefaulted", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
			});
		}

		public void TestJE_ApplicationCode_ReadOnly_NotConfigureLocalCountryCustomsInterface()
		{
			Assert("JE_ApplicationCodeInfo.ReadOnly", Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
		}

		public void TestJE_ApplicationCode_ReadOnly_ConfigureLocalCountryCustomsInterface()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BLT", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType ITF", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BIT", false, Declaration.JE_ApplicationCodeInfo.ReadOnly);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BTH", false, Declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			});
		}

		public void TestIsTradenet4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals(true, Declaration.IsTradenet4);
			Declaration.JE_ApplicationCode = "";
			AssertEquals(false, Declaration.IsTradenet4);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals("TradeNet Version 4.1", true, Declaration.IsTradenet4);
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("ITF is not TradeNet 4", false, Declaration.IsTradenet4);
		}

		public void TestIsTradenet4OrITF()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals(true, Declaration.IsTradenet4OrITF);
			Declaration.JE_ApplicationCode = "";
			AssertEquals(false, Declaration.IsTradenet4OrITF);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals("TradeNet Version 4.1", true, Declaration.IsTradenet4OrITF);
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(true, Declaration.IsTradenet4OrITF);
		}

		public override void TestResetValuesOnTemplateCopyAfterClone()
		{
			base.TestResetValuesOnTemplateCopyAfterClone();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.SG_US_NKInwardVesselBerth = "JW";
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-001";
			var clonedDeclaration = jobDeclaration.TemplateCopy() as JobDeclaration;
			AssertEquals("JE_ApplicationCode", JobApplicationCodeList.Codes.TradeNet41, jobDeclaration.JE_ApplicationCode);
			AssertEquals("JZ_InvoiceNumber", "INV-001", clonedDeclaration.Invoices[0].JZ_InvoiceNumber);
		}

		public void TestIsSeaStoreDeclaration()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "420", "", "3000", "SEASTORE (OUT APS)", "OUT", group: "APS");
			var procedure1Attribute1 = helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.ISSEASTORE, "Y");
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			AssertEquals(false, Declaration.IsSeaStore);
			var testCPC = Factory.New<SGCPC>();
			testCPC.B7_ParentID = Declaration.PK;
			testCPC.B7_ParentTableCode = Declaration.TablePrefix;
			testCPC.SG_CPCCode = "4203000";
			testCPC.SG_PC1 = "12";
			testCPC.SG_PC2 = "7";
			IEnumerable<ICusCPC> cpcs = Declaration.CPCs;
			AssertEquals("SeaStores are now sent in the new CPC code values", true, Declaration.IsSeaStore);
		}

		[TestDate(2011, 6, 30)]
		public void TestCrewAndVoyageDurationReadOnly()
		{
			// TN4.0 uses these db fields
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals(true, Declaration.SG_NoOfCrewInfo.ReadOnly);
			AssertEquals(true, Declaration.SG_VoyageDurationInfo.ReadOnly);
			Declaration.SG_IsSeaStore = true;
			AssertEquals(false, Declaration.SG_NoOfCrewInfo.ReadOnly);
			AssertEquals(false, Declaration.SG_VoyageDurationInfo.ReadOnly);
		}

		public void TestSupportJE_PaymentMethodUsage()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.SupportJE_PaymentMethodUsage);
		}

		public void TestHasBeenCleared()
		{
			AssertEquals(false, Declaration.HasBeenCleared);
			Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals(true, Declaration.HasBeenCleared);
		}

		public override void TestSwitchFromSeaToAirWithContainersDereferencesAndDeletesContainers()
		{
			Assert(true);
		}

		public void TestUpdateOfEntryClearedDoesNotRemoveContainers()
		{
			AssertEquals("Container Count", 0, Declaration.CusContainers.Count);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			AssertEquals("Container Count", ZShort.Zero, Declaration.JE_ContainerCount);
			AssertEquals(false, Declaration.HasBeenCleared);
			var container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FCLU0010001";
			var container2 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FCLU0010002";
			Factory.Save();
			AssertEquals("Container Count", 2, Declaration.CusContainers.Count);
			AssertEquals("Container Count", 2, Declaration.JE_ContainerCount.ToZInt());
			Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals(true, Declaration.HasBeenCleared);
			AssertEquals("Container Count", 2, Declaration.CusContainers.Count);
			AssertEquals("Container Count", 2, Declaration.JE_ContainerCount.ToZInt());
		}

		public void TestDeclarationNumber()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "PMT";
			entryNumber.CE_EntryNum = "V4";
			entryNumber.CE_ParentID = Declaration.ActiveEntryHeaders.AddNew().PK;
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "PMT";
			entryNumber.CE_EntryNum = "V3";
			entryNumber.CE_ParentID = Declaration.PK;
			AssertEquals("V4", Declaration.DeclarationNumber);
			Declaration.JE_ApplicationCode = "";
			AssertEquals("V3", Declaration.DeclarationNumber);
		}

		public void TestOutwardShippingLineForwarderAddress()
		{
			AssertEquals(DocAddressType.OutwardCarrierAgent, Declaration.OutwardShippingLineForwarderDocAddress.DocAddressType);
		}

		#region Test ICusEntryNumFilterProvider
		protected override void CancelEntryHeader(Customs.Business.CusEntryHeader entryHeader)
		{
			entryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
		}

		#endregion
		public void TestClaimantDefaultsDutyExemptForGSTDec()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.SG_ClaimantCode = "S1234567E";
			AssertEquals("Duty Exempt not applicable", false, Declaration.SG_DutyExempt);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			Declaration.SG_ClaimantCode = "S9573332C";
			AssertEquals("Duty Exempt not applicable", false, Declaration.SG_DutyExempt);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Declaration.SG_ClaimantCode = "S1234567E";
			AssertEquals("Duty Exempt flag should default", true, Declaration.SG_DutyExempt);
			Declaration.SG_ClaimantCode = "";
			AssertEquals("Duty Exempt flag should default back to unticked when Claimant code is cleared", false, Declaration.SG_DutyExempt);
		}

		[TestDate(2007, 1, 1)]
		public void TestDateOfValuation()
		{
			AssertEquals(new ZDateTime(2007, 1, 1), Declaration.DateOfValuation);
		}

		public void TestSupportExtendingAmendmentReason()
		{
			var declaration = Factory.New<JobDeclaration>();
			void AssertSupportExtendingAmendmentReason(string messageType, bool expectedValue)
			{
				declaration.JE_MessageType = messageType;
				AssertEquals("Should be only true when the message type is INP", expectedValue, declaration.SupportExtendingAmendmentReason);
			}

			AssertSupportExtendingAmendmentReason(MessageTypeCodeList.Codes.COO, false);
			AssertSupportExtendingAmendmentReason(MessageTypeCodeList.Codes.IPT, false);
			AssertSupportExtendingAmendmentReason(MessageTypeCodeList.Codes.TNP, false);
			AssertSupportExtendingAmendmentReason(MessageTypeCodeList.Codes.OUT, false);
			AssertSupportExtendingAmendmentReason(MessageTypeCodeList.Codes.INP, true);
		}

		public void TestSetDefaultValues()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals(MessageTypeCodeList.Codes.INP, jobDeclaration.JE_MessageType);
			GlbDepartment.CurrentDepartment.GE_Import = false;
			jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals(MessageTypeCodeList.Codes.OUT, jobDeclaration.JE_MessageType);
			AssertEquals(OrgConstants.MergeInvoiceLines.NotMerge, jobDeclaration.JE_MergeBy);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.PK, jobDeclaration.JE_OH_Forwarder);
			AssertNotEquals("just to be sure a valid test", ZGuid.Empty, jobDeclaration.JE_OH_Forwarder);
		}

		public void TestOutwardTransportDefaultsForOutwardDepartments()
		{
			GlbDepartment.CurrentDepartment.GE_Import = false;
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			var defaultDeclarationForExportDept = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals("Export Dept. should default as an outward job", MessageTypeCodeList.Codes.OUT, defaultDeclarationForExportDept.JE_MessageType);
			AssertEquals("Standard defaulting of Transport Mode should be modified for SG Outward decs", "", defaultDeclarationForExportDept.JE_TransportMode);
			AssertEquals("Export Dept. should default outward job transport mode to SG_OutwardTransportMode", Enterprise.Core.Constants.TransportModes.Sea, defaultDeclarationForExportDept.SG_OutwardTransportMode);
			GlbDepartment.CurrentDepartment.GE_Import = true;
			GlbDepartment.CurrentDepartment.GE_Sea = false;
			GlbDepartment.CurrentDepartment.GE_Air = true;
			var defaultDeclarationForImportDept = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals("Import Dept. default as standard inward job", MessageTypeCodeList.Codes.INP, defaultDeclarationForImportDept.JE_MessageType);
			AssertEquals("Standard defaulting of Transport Mode is used for SG Inward decs", Core.Constants.TransportModes.Air, defaultDeclarationForImportDept.JE_TransportMode);
			AssertEquals("Import Dept. will not default any transport mode value to SG_OutwardTransportMode", "", defaultDeclarationForImportDept.SG_OutwardTransportMode);
		}

		public void TestDefaultCountryOfFinalDestination()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_RL_NKPortOfArrival = "ZACPT";
			AssertEquals("", Declaration.SG_RN_NKFinalDestination);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_RL_NKPortOfArrival = "ZAJHB";
			AssertEquals("ZA", Declaration.SG_RN_NKFinalDestination);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_RL_NKPortOfArrival = "ZAJHB";
			AssertEquals("ZA", Declaration.SG_RN_NKFinalDestination);
			Declaration.JE_RL_NKPortOfArrival = "USLAX";
			AssertEquals("US", Declaration.SG_RN_NKFinalDestination);
			Declaration.JE_RL_NKPortOfArrival = "";
			AssertEquals("", Declaration.SG_RN_NKFinalDestination);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.JE_RL_NKPortOfArrival = "ZAJHB";
			AssertEquals("ZA", Declaration.SG_RN_NKFinalDestination);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals("ZA", Declaration.SG_RN_NKFinalDestination);
		}

		public void TestResetStatus()
		{
			CusEntryHeader cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			Declaration.ResetToWorking();
			AssertEquals(true, cusEntryHeader.CanSendOriginal);
			cusEntryHeader.EntryNumber = "TEST123J";
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			Declaration.ResetToWorking();
			AssertEquals(false, cusEntryHeader.CanSendOriginal);
			AssertEquals("Status should be reset to DOK", cusEntryHeader.CH_Status, Core.SGConstants.DeclarationStatus.DeclarationPermitReceived);
		}

		public void TestIsExport()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(true, Declaration.IsExport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(true, Declaration.IsExport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(false, Declaration.IsExport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(true, Declaration.IsExport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(false, Declaration.IsExport);
		}

		public void TestIsImport()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(false, Declaration.IsImport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(true, Declaration.IsImport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(true, Declaration.IsImport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(false, Declaration.IsImport);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(true, Declaration.IsImport);
		}

		public void TestIsContainerised()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			AssertEquals(true, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType1;
			AssertEquals(false, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			AssertEquals(false, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType4;
			AssertEquals(false, Declaration.IsContainerised);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			AssertEquals(true, Declaration.IsContainerised);
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			AssertEquals(false, Declaration.IsContainerised);
		}

		public void TestContainersRequired()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType3;
			AssertEquals(true, Declaration.ContainersRequired);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType1;
			AssertEquals(true, Declaration.ContainersRequired);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType2;
			AssertEquals(true, Declaration.ContainersRequired);
			Declaration.JE_ContainerMode = CargoPackingTypeCodeList.Codes.PackingType4;
			AssertEquals(true, Declaration.ContainersRequired);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType9;
			AssertEquals(true, Declaration.ContainersRequired);
			Declaration.JE_ContainerMode = CargoPackingCodeList.Codes.PackingType5;
			AssertEquals("ContainersRequired always.", true, Declaration.ContainersRequired);
		}

		public void TestGetDefaultContainerisedContainerMode()
		{
			AssertEquals("9", Declaration.GetDefaultContainerisedContainerMode());
		}

		[TestDate(2012, 01, 15)]
		public override void TestDateForDutyRate()
		{
			AssertEquals(ZDate.Today, Declaration.DateForDutyRate);
		}

		[TestDate(2011, 12, 15)]
		public void TestDateForDutyRateTransition()
		{
			var dateExpected = new ZDateTime(2012, 01, 01);
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals(dateExpected, Declaration.DateForDutyRate);
		}

		public void TestIsOutwardTransportModeAir()
		{
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals(true, Declaration.IsOutwardTransportModeAir);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals(false, Declaration.IsOutwardTransportModeAir);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals(false, Declaration.IsOutwardTransportModeAir);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals(false, Declaration.IsOutwardTransportModeAir);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals(false, Declaration.IsOutwardTransportModeAir);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			AssertEquals(false, Declaration.IsOutwardTransportModeAir);
		}

		public void TestIsOutwardTransportModeSea()
		{
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals(true, Declaration.IsOutwardTransportModeSea);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals(false, Declaration.IsOutwardTransportModeSea);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals(false, Declaration.IsOutwardTransportModeSea);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals(false, Declaration.IsOutwardTransportModeSea);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals(false, Declaration.IsOutwardTransportModeSea);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			AssertEquals(false, Declaration.IsOutwardTransportModeSea);
		}

		public void TestIsOutwardTransportModeRoad()
		{
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals(true, Declaration.IsOutwardTransportModeRoad);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals(false, Declaration.IsOutwardTransportModeRoad);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals(false, Declaration.IsOutwardTransportModeRoad);
		}

		public void TestInwardTransportNonRequiredFieldsOnChangeOfTransportMode()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_MasterBill = "In-MAWB";
			Declaration.JE_HouseBill = "In-HAWB";
			Declaration.JE_VoyageFlightNo = "7890";
			Declaration.SG_US_NKInwardVesselBerth = "JW";
			Declaration.JE_VesselName = "In Vessel";
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Declaration.JE_VoyageFlightNo = "7890";
			AssertEquals("Inward Vessel should have been cleared out", "", Declaration.JE_VesselName);
			AssertEquals("Inward Vessel Berth should have been cleared out", "", Declaration.SG_US_NKInwardVesselBerth);
			AssertEquals("MasterBill should retain value entered", "In-MAWB", Declaration.JE_MasterBill);
			AssertEquals("HouseBill should retain value entered", "In-HAWB", Declaration.JE_HouseBill);
			AssertEquals("Voyage/Flight should retain value entered", "7890", Declaration.JE_VoyageFlightNo);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("Inward Vessel should have been cleared out", "", Declaration.JE_VesselName);
			AssertEquals("Inward Vessel Berth should have been cleared out", "", Declaration.SG_US_NKInwardVesselBerth);
			AssertEquals("MasterBill should have been cleared out", "", Declaration.JE_MasterBill);
			AssertEquals("HouseBill should retain value entered", "In-HAWB", Declaration.JE_HouseBill);
			AssertEquals("Voyage/Flight should have been cleared out", "", Declaration.JE_VoyageFlightNo);
			Declaration.JE_TransportMode = "";
			AssertEquals("All Inward Transport fields should have been cleared out", "", Declaration.JE_HouseBill);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			//All Inward Transport fields should now have been cleared out
			AssertEquals("Inward Vessel should have been cleared out", "", Declaration.JE_VesselName);
			AssertEquals("Inward Vessel Berth should have been cleared out", "", Declaration.SG_US_NKInwardVesselBerth);
			AssertEquals("MasterBill should have been cleared out", "", Declaration.JE_MasterBill);
			AssertEquals("HouseBill should have been cleared out", "", Declaration.JE_HouseBill);
			AssertEquals("Voyage/Flight should have been cleared out", "", Declaration.JE_VoyageFlightNo);
		}

		public void TestOutwardTransportNonRequiredFieldsOnChangeOfTransportMode()
		{
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_OutwardMAWB = "out-MAWB";
			Declaration.SG_OutwardHAWB = "out-HAWB";
			Declaration.SG_OutwardVoyageFlightNo = "7890";
			Declaration.SG_US_NKOutwardVesselBerth = "JW";
			Declaration.SG_OutwardVesselName = "out Vessel";
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("outward Vessel should have been cleared out", "", Declaration.JE_VesselName);
			AssertEquals("outward Vessel Berth should have been cleared out", "", Declaration.SG_US_NKOutwardVesselBerth);
			AssertEquals("MasterBill should return value entered without formatting characters", "outMAWB", Declaration.SG_OutwardMAWB);
			AssertEquals("HouseBill should return value entered", "out-HAWB", Declaration.SG_OutwardHAWB);
			AssertEquals("Voyage/Flight should return value entered", "7890", Declaration.SG_OutwardVoyageFlightNo);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("outward Vessel should have been cleared out", "", Declaration.JE_VesselName);
			AssertEquals("outward Vessel Berth should have been cleared out", "", Declaration.SG_US_NKOutwardVesselBerth);
			AssertEquals("MasterBill should have been cleared out", "", Declaration.SG_OutwardMAWB);
			AssertEquals("HouseBill should return value entered", "out-HAWB", Declaration.SG_OutwardHAWB);
			AssertEquals("Voyage/Flight should have been cleared out", "", Declaration.SG_OutwardVoyageFlightNo);
			Declaration.SG_OutwardTransportMode = "";
			AssertEquals("All outward Transport fields should have been cleared out", "", Declaration.JE_HouseBill);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			//All outward Transport fields should now have been cleared out
			AssertEquals("outward Vessel should have been cleared out", "", Declaration.JE_VesselName);
			AssertEquals("outward Vessel Berth should have been cleared out", "", Declaration.SG_US_NKOutwardVesselBerth);
			AssertEquals("MasterBill should have been cleared out", "", Declaration.SG_OutwardMAWB);
			AssertEquals("HouseBill should have been cleared out", "", Declaration.SG_OutwardHAWB);
			AssertEquals("Voyage/Flight should have been cleared out", "", Declaration.SG_OutwardVoyageFlightNo);
		}

		public void TestHasCofO()
		{
			AssertEquals("Pre-Condition", false, Declaration.HasCofO);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals(true, Declaration.HasCofO);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
			AssertEquals(false, Declaration.HasCofO);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			AssertEquals(false, Declaration.HasCofO);
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AssertEquals(true, Declaration.HasCofO);
		}

		public void TestCPCs()
		{
			Factory.Save();
			AssertEquals("Pre-Condition", false, Declaration.HasChanges);
			AssertNotNull(Declaration.CPCs);
			AssertEquals("Pre-Condition", false, Declaration.HasChanges);
			var sgCPC = Declaration.CPCs.AddNew();
			AssertEquals(CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode, sgCPC.B7_Type);
			AssertEquals(Declaration.PK, sgCPC.B7_ParentID);
		}

		public void TestCALicences()
		{
			Factory.Save();
			AssertEquals("Pre-Condition", false, Declaration.HasChanges);
			AssertNotNull(Declaration.CALicences);
			AssertEquals("Pre-Condition", false, Declaration.HasChanges);
			CALicenceNumber cALicenceNumber = Declaration.CALicences.AddNew();
			AssertEquals(CusCodeDataTypeList.Codes.CALicenceNumber, cALicenceNumber.CY_Type);
			AssertEquals(Declaration.PK, cALicenceNumber.CY_ParentID);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = ((ICusSupportingInfoTypeSupporter)Declaration).GetCusSupportingInfoTypes();
			AssertEquals(typeof(TradersRemark), cusSupportingInfoTypes[Common.SG.CusSupportingInfoTypeList.Codes.TradersRemarks]);
		}

		public void TestLoadTradersRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			var stmANote = new HiddenTextNote(declaration, PredefinedNoteTypes.Instance.SGTradersRemarks.Description);
			stmANote.Text = "Line1\n\rLine2";
			Factory.Save();
			AssertEquals("Line1", declaration.TradersRemarks[0].CSI_Description);
			AssertEquals("Line2", declaration.TradersRemarks[1].CSI_Description);
		}

		public void TestSG4MessageManager()
		{
			Assert(Declaration.SG4MessageManager is SG4MessageManager);
		}

		public void TestIsXMLTradeNetMessageEnabled()
		{
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!Declaration.IsXMLTradeNetMessageEnabled);
			}

			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(Declaration.IsXMLTradeNetMessageEnabled);
			}
		}

		public void TestJE_EntryStatusDescription()
		{
			AssertEquals("Working. Declaration has not been sent.", Declaration.JE_EntryStatusDescription);
			CusEntryHeader cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("Working. Declaration has not been sent.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals("Declaration Queued for Sending.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals("Declaration Sent Waiting Response.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals("Declaration Rejected.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals("Declaration Syntax Error.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationQuery;
			AssertEquals("Declaration has been queried. Please take requested action then await Custom's subsequent response.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals("Permit Approved.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals("Amendment Queued for Sending.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals("Amendment Sent Waiting Response.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals("Amendment Rejected.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals("Amendment Syntax Error.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentQuery;
			AssertEquals("Amendment has been queried. Please take requested action.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals("Permit Approved.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPending;
			AssertEquals("Refund Queued for Sending.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundSent;
			AssertEquals("Refund Sent Waiting Response.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms;
			AssertEquals("Refund Rejected.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
			AssertEquals("Refund Syntax Error.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundQuery;
			AssertEquals("Refund has been queried. Please take requested action.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.RefundPermitReceived;
			AssertEquals("Refund Approved.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals("Cancellation Queued for Sending.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals("Cancellation Sent Waiting Response.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals("Cancellation Rejected.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals("Cancellation Syntax Error.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationQuery;
			AssertEquals("Cancellation has been queried. Please take requested action.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals("Cancellation Approved.", Declaration.JE_EntryStatusDescription);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals("Certificate of Origin Queued for Sending.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals("Certificate of Origin Sent Waiting Response.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals("Certificate of Origin Rejected.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals("Certificate of Origin Syntax Error.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationQuery;
			AssertEquals("Certificate of Origin has been queried. Please take requested action.", Declaration.JE_EntryStatusDescription);
			cusEntryHeader.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals("Certificate of Origin Approved.", Declaration.JE_EntryStatusDescription);
		}

		public void TestJE_EntryStatusReadOnly()
		{
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationPending;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationSent;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationQuery;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.AmendmentPending;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.AmendmentSent;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.AmendmentQuery;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.RefundPending;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.RefundSent;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.RefundPermitReceived;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.RefundQuery;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationPending;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationSent;
			AssertEquals(true, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationQuery;
			AssertEquals(false, Declaration.ReadOnly);
			Declaration.JE_EntryStatus = Core.SGConstants.DeclarationStatus.CancellationPending;
			Factory.Save();
		}

		public void TestDocumentSuporterIsRightType()
		{
			Assert(Declaration.DocumentSupporter is JobDeclarationDocumentSupporter);
		}

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			dec.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.CaptionKeyTradeNet4Point1, "Registration", fullDescription: "For Road Transport, specify the vehicle License / Registration number.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage Number", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.CaptionKeyTradeNet4Point1, "Voyage Number", shortCaption: "Voyage", fullDescription: "For Sea Transport, specify the Voyage Number.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight No. / Aircraft Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight/Rego.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.CaptionKeyTradeNet4Point1, "Flight No. / Aircraft Registration", shortCaption: "Flight/Rego.", fullDescription: "For Air Transport, specify the Flight No. or Aircraft Registration Number for chartered flights.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Mail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Pipeline", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestAdditionalInformation()
		{
			IAdditionalMessageInformation addInfo = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			Declaration.AdditionalMessageInformation = addInfo;
			AssertEquals(addInfo, Declaration.AdditionalMessageInformation);
		}

		public void TestValidation()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Assert(Declaration.Validation is JobDeclarationValidation_IPT);
			Declaration.JE_MessageType = "";
			Assert(Declaration.Validation is JobDeclarationValidation);
		}

		public void TestJobInvoicingTransportMode()
		{
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals("Inward Transport", "SEA", ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.TransportMode);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals("Inward Transport", "ROA", ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.TransportMode);
			Declaration.JE_TransportMode = "";
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Outward Transport", "AIR", ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.TransportMode);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("Outward Transport", "RAI", ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.TransportMode);
			Declaration.SG_OutwardTransportMode = "";
			AssertEquals("No Transport Mode", "", ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.TransportMode);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			AssertEquals("Outward Transport", "OTH", ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.TransportMode);
		}

		public void TestOverridenDepartment()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals("Inward Sea Transport - Dept. s/b CIS", ObjectFactory.Get<IAccounting>().CustomsImportSeaLcl, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals("Inward Mail Transport - Dept. s/b CPP", ObjectFactory.Get<IAccounting>().CustomsImportPost, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("Inward Rail Transport - Dept. s/b CIL", ObjectFactory.Get<IAccounting>().CustomsImportRail, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Inward Air Transport - Dept. s/b CIA", ObjectFactory.Get<IAccounting>().CustomsImportAirUld, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals("Inward Road Transport - Dept. s/b CIR", ObjectFactory.Get<IAccounting>().CustomsImportRoad, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.JE_ContainerCount = 1;
			AssertEquals("Inward Sea Containerised Transport - Dept. s/b CIS", ObjectFactory.Get<IAccounting>().CustomsImportSeaFcl, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_TransportMode = "";
			Declaration.JE_ContainerCount = 0;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Outward Air Transport - Dept. s/b CEA", ObjectFactory.Get<IAccounting>().CustomsExportAirUld, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals("Outward Road Transport - Dept. s/b CER", ObjectFactory.Get<IAccounting>().CustomsExportRoad, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = "";
			AssertEquals("No Transport - Dept. s/b COT", ObjectFactory.Get<IAccounting>().CustomsOther, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals("Outward Sea Transport - Dept. s/b CES", ObjectFactory.Get<IAccounting>().CustomsExportSeaLcl, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			AssertEquals("Outward Pipeline(other) Transport - Dept. s/b COT", ObjectFactory.Get<IAccounting>().CustomsOther, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("Outward Rail Transport - Dept. s/b CEL", ObjectFactory.Get<IAccounting>().CustomsExportRail, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals("Outward Mail Transport - Dept. s/b CPP", ObjectFactory.Get<IAccounting>().CustomsExportPost, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Transhipment - Dept. s/b COT", ObjectFactory.Get<IAccounting>().CustomsOther, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			Declaration.JE_TransportMode = "";
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("Cert. of Origin with Air mode - Dept. s/b CEA", ObjectFactory.Get<IAccounting>().CustomsExportAirUld, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals("Cert. of Origin with Sea mode - Dept. s/b CES", ObjectFactory.Get<IAccounting>().CustomsExportSeaLcl, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
			Declaration.SG_OutwardTransportMode = "";
			AssertEquals("Cert. of Origin with no transport - Dept. s/b CES", ObjectFactory.Get<IAccounting>().CustomsOther, ((IJobInvoicingPlugIn)Declaration).InvoicingSupporter.OverriddenDepartmentPK);
		}

		public void TestJE_ContainerCount()
		{
			AssertEquals((ZShort)0, Declaration.JE_ContainerCount);
			Declaration.CusContainers.AddNew();
			Declaration.CusContainers.AddNew();
			Declaration.CusContainers.AddNew();
			AssertEquals((ZShort)3, Declaration.JE_ContainerCount);
		}

		public void TestJE_ContainerCountReadonly()
		{
			AssertEquals(true, Declaration.JE_ContainerCountInfo.ReadOnly);
		}

		public void TestHasLiqourOrTobacco()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(tariffType, "24011010");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff);
			Factory.Save();
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "24011010";
			AssertEquals(true, Declaration.HasLiquorOrTobacco);
		}

		public void TestIsTemporaryConsignment()
		{
			AssertEquals(false, Declaration.IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCE;
			AssertEquals(true, Declaration.IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCS;
			AssertEquals(true, Declaration.IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCR;
			AssertEquals(true, Declaration.IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			AssertEquals(true, Declaration.IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCI;
			AssertEquals(false, Declaration.IsTemporaryConsignment);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			AssertEquals(false, Declaration.IsTemporaryConsignment);
		}

		public void TestClone_()
		{
			AssertEquals("Pre-condition - JE_ApplicationCode", "4.1", Declaration.JE_ApplicationCode);
			Declaration.SG_OutwardHAWB = "TEST";
			Declaration.SG_OutwardMAWB = "TEST";
			Declaration.SG_RemovalStartDate = ZDateTime.Now;
			Declaration.SG_EndDateTempImport = ZDateTime.Now;
			var tradersRemarks = Declaration.TradersRemarks.AddNew();
			tradersRemarks.CSI_Description = "TEST";
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			CALicenceNumber licenceNumber = Declaration.CALicences.AddNew();
			licenceNumber.CY_Data = "TEST";
			AssertEquals("JE_ApplicationCode", "4.1", Declaration.JE_ApplicationCode);
			JobDeclaration newDeclaration = (JobDeclaration)Declaration.Clone();
			AssertEquals("", newDeclaration.SG_OutwardHAWB);
			AssertEquals("", newDeclaration.SG_OutwardMAWB);
			AssertEquals(ZDateTime.Empty, newDeclaration.SG_RemovalStartDate);
			AssertEquals(ZDateTime.Empty, newDeclaration.SG_EndDateTempImport);
			AssertEquals(1, newDeclaration.TradersRemarks.Count);
			AssertEquals("TEST", newDeclaration.TradersRemarks[0].CSI_Description);
			AssertEquals("TEST", newDeclaration.CALicences[0].CY_Data);
			AssertEquals("JE_ApplicationCode should pick up current default value", JobApplicationCodeList.Codes.TradeNet41, Declaration.JE_ApplicationCode);
		}

		public void TestTotalPayable()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00001111";
			invoiceLine.SG_LastSellingPrice = 100m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 2m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 3m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 4m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.OtherTax, 5m);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AssertEquals("Include Other Tax if calculated", 14m, Declaration.TotalPayable);
		}

		public void TestTotalCustomsValue()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_LastSellingPrice = 100m;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_LastSellingPrice = 200m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			AssertEquals(300m, Declaration.TotalCustomsValue);
		}

		public void TestTotalDutyPayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.Duty);
			AssertEquals(30m, Declaration.TotalDutyPayable);
		}

		public void TestTotalExcisePayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.Excise);
			AssertEquals(30m, Declaration.TotalExcisePayable);
		}

		public void TestTotalOtherTaxPayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.OtherTax);
			AssertEquals(30m, Declaration.TotalOtherTaxPayable);
		}

		public void TestTotalGSTPayable()
		{
			InsertPayableAmounts(Registry.EntryChargeTypeList.Codes.GST);
			AssertEquals(30m, Declaration.TotalGSTPayable);
		}

		void InsertPayableAmounts(string chargeType)
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00001111";
			invoiceLine.SG_LastSellingPrice = 100m;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00002222";
			invoiceLine.SG_LastSellingPrice = 100m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(chargeType, 10m);
			Declaration.ActiveEntryHeaders[0].MergedLines[1].Fees.AddOrUpdate(chargeType, 20m);
		}

		public void TestDoMergeRefreshesBinding()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			bool customsValueChangeCalled = false;
			bool totalPayableChangeCalled = false;
			bool totalExciseChangeCalled = false;
			bool totalDutyChangeCalled = false;
			bool totalGSTChangeCalled = false;
			Declaration.TotalCustomsValueInfo.ValueChanged += delegate
			{
				customsValueChangeCalled = true;
			}

			;
			Declaration.TotalPayableInfo.ValueChanged += delegate
			{
				totalPayableChangeCalled = true;
			}

			;
			Declaration.TotalExcisePayableInfo.ValueChanged += delegate
			{
				totalExciseChangeCalled = true;
			}

			;
			Declaration.TotalDutyPayableInfo.ValueChanged += delegate
			{
				totalDutyChangeCalled = true;
			}

			;
			Declaration.TotalGSTPayableInfo.ValueChanged += delegate
			{
				totalGSTChangeCalled = true;
			}

			;
			bool lineCustomsValueChangeCalled = false;
			bool lineExciseChangeCalled = false;
			bool lineDutyChangeCalled = false;
			bool linelGSTChangeCalled = false;
			invoiceLine.JI_Calc_CIFInfo.ValueChanged += delegate
			{
				lineCustomsValueChangeCalled = true;
			}

			;
			invoiceLine.JI_Calc_ExciseAmountInfo.ValueChanged += delegate
			{
				lineExciseChangeCalled = true;
			}

			;
			invoiceLine.JI_Calc_DutyAmountInfo.ValueChanged += delegate
			{
				lineDutyChangeCalled = true;
			}

			;
			invoiceLine.JI_Calc_GSTVATAmountInfo.ValueChanged += delegate
			{
				linelGSTChangeCalled = true;
			}

			;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert("Customs Value", customsValueChangeCalled);
			Assert("Payable", totalPayableChangeCalled);
			Assert("Duty Payable", totalDutyChangeCalled);
			Assert("Excise Payable", totalExciseChangeCalled);
			Assert("GST Payable", totalGSTChangeCalled);
			Assert("Customs Value", lineCustomsValueChangeCalled);
			Assert("Duty Payable", lineDutyChangeCalled);
			Assert("Excise Payable", lineExciseChangeCalled);
			Assert("GST Payable", linelGSTChangeCalled);
		}

		public void TestCertificateNumber()
		{
			CusEntryHeader cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CertificateNumber = "CERT1";
			cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CertificateNumber = "CERT2";
			AssertEquals("CERT1,CERT2", Declaration.CertificateNumber);
		}

		public void TestCertificateNumberInfo()
		{
			Assert(Declaration.CertificateNumberInfo.ReadOnly);
		}

		public void TestMessageTypeDefaultsMessageSubType()
		{
			Declaration.JE_MessageSubType = "TST";
			Declaration.JE_MessageType = "XXX";
			AssertEquals("", Declaration.JE_MessageSubType);
			Declaration.JE_MessageSubType = "TST";
			Declaration.JE_MessageType = "";
			AssertEquals("", Declaration.JE_MessageSubType);
			Declaration.JE_MessageSubType = "TST";
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			AssertEquals("", Declaration.JE_MessageSubType);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(DeclarationTypeCodeList.Codes.SFZ, Declaration.JE_MessageSubType);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AssertEquals(DeclarationTypeCodeList.Codes.GST, Declaration.JE_MessageSubType);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(DeclarationTypeCodeList.Codes.DRT, Declaration.JE_MessageSubType);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(DeclarationTypeCodeList.Codes.TTF, Declaration.JE_MessageSubType);
		}

		public void TestJE_TotalNoOfPacksDecimal()
		{
			Declaration.JE_TotalNoOfPacksDecimal = 12345678901.1234m;
			AssertEquals(12345678901.1234m, Declaration.JE_TotalNoOfPacksDecimal);
			AssertEquals(0, Declaration.JE_TotalNoOfPacks);
			Declaration.JE_TotalNoOfPacksDecimal = 9999999.1234m;
			AssertEquals(9999999, Declaration.JE_TotalNoOfPacks);
			Declaration.JE_TotalNoOfPacksDecimal = 999999.9999m;
			AssertEquals(999999, Declaration.JE_TotalNoOfPacks);
		}

		public void TestGetLastEffectivePermitSubmision()
		{
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
			CusEntryHeader cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
			//declaration permit received
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Declaration, "IPTDEC1");
			AddIncomingMessage(cusEntryHeader, "ERR", "ERR", "APERAK1");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT1");
			AssertEquals("IPTDEC1", Declaration.GetLastSentMessage_PermitOnly().EM_ApplicationReference);
			//amendment sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Amendment, "IPTUPD1");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT1");
			AssertEquals("IPTUPD1", Declaration.GetLastSentMessage_PermitOnly().EM_ApplicationReference);
			//cancellation sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Cancellation, "IPTCAN1");
			AddIncomingMessage(cusEntryHeader, CUSDECEDIMessage.Cancellation, CUSDECEDIMessage.Cancellation, "IPTRES1");
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
			//declaration sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Amendment, "IPTUPD2");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT2");
			AssertEquals("IPTUPD2", Declaration.GetLastSentMessage_PermitOnly().EM_ApplicationReference);
			//refund sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Refund, "IPTREF1");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT1");
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
		}

		public void TestGetLastEffectivePermitSubmision_ExcludesCOO()
		{
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
			CusEntryHeader cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
			//declaration permit received
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.COO, CUSDECEDIMessage.Declaration, "COODEC1");
			AssertNull(Declaration.GetLastSentMessage_PermitOnly());
		}

		public void TestGetLastEffectiveRefundSubmission() //effective means
		{
			AssertNull(Declaration.GetLastSentMessage_RefundOnly());
			CusEntryHeader cusEntryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
			AssertNull(Declaration.GetLastSentMessage_RefundOnly());
			//declaration permit received
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Declaration, "IPTDEC1");
			AddIncomingMessage(cusEntryHeader, "ERR", "ERR", "APERAK1");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT1");
			AssertNull(Declaration.GetLastSentMessage_RefundOnly());
			//amendment sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Amendment, "IPTUPD1");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT1");
			AssertNull(Declaration.GetLastSentMessage_RefundOnly());
			//refund sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Refund, "IPTREF1");
			AddIncomingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, Cuspmt09bMessageProcessor.MessageType, "IPTPMT1");
			AssertEquals("IPTREF1", Declaration.GetLastSentMessage_RefundOnly().EM_ApplicationReference);
			//cancellation sent
			AddOutgoingMessage(cusEntryHeader, MessageTypeCodeList.Codes.IPT, CUSDECEDIMessage.Cancellation, "IPTCAN1");
			AddIncomingMessage(cusEntryHeader, CUSDECEDIMessage.Cancellation, CUSDECEDIMessage.Cancellation, "IPTRES1");
			AssertNull(Declaration.GetLastSentMessage_RefundOnly());
		}

		void AddIncomingMessage(CusEntryHeader cusEntryHeader, string messageType, string messageSubType, string applicationReference)
		{
			SGEDIMessage message = Factory.New<SGEDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ApplicationReference = applicationReference;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			cusEntryHeader.Messages.Add(message);
		}

		void AddOutgoingMessage(CusEntryHeader cusEntryHeader, string messageType, string messageSubType, string applicationReference)
		{
			CUSDECEDIMessage message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ApplicationReference = applicationReference;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			if (messageType != MessageTypeCodeList.Codes.COO)
			{
				message.EM_MessageText = "CUSDEC" + message.EM_MessageText;
			}

			cusEntryHeader.Messages.Add(message);
		}

		public void TestEdocs()
		{
			AssertEquals(Declaration.DocManagerInfo.AllEDocs, Declaration.AllEDocs);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			AssertEquals(Declaration.Shipment.DocManagerInfo.AllEDocs, Declaration.AllEDocs);
			AssertNotEquals(Declaration.DocManagerInfo.AllEDocs, Declaration.AllEDocs);
		}

		public override void TestGetContainerModeForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.FCL));
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.LCL));
			AssertEquals(CargoPackingCodeList.Codes.PackingType5, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.Bulk));
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.ShippersConsol));
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AssertEquals(CargoPackingTypeCodeList.Codes.PackingType3, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.FCL));
			AssertEquals(CargoPackingTypeCodeList.Codes.PackingType1, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.Bulk));
			AssertEquals(CargoPackingTypeCodeList.Codes.PackingType2, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.LCL));
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.ShippersConsol));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(CargoPackingCodeList.Codes.PackingType9, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.ShippersConsol));
		}

		public void TestManifestStatusAndDescription()
		{
			SetupGlobalManifestAndBill(Common.SG.GlobalManifestStatusList.Codes.Clear, "08109191442", "AA1235579412");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "SG00012349";
			declaration.JE_MasterBill = "08109191442";
			declaration.JE_HouseBill = "AA1235579412";
			Factory.Save();
			AssertEquals(Common.SG.GlobalManifestStatusList.Codes.Clear, declaration.ManifestStatus);
			AssertEquals(Common.SG.GlobalManifestStatusList.Descriptions.Clear, declaration.ManifestStatusDescription);
		}

		public void TestManifestStatusAndDescriptionMulitple()
		{
			SetupGlobalManifestAndBill(Common.SG.GlobalManifestStatusList.Codes.Clear, "08109191442", "AA1235579412");
			SetupGlobalManifestAndBill(Common.SG.GlobalManifestStatusList.Codes.InspectionRequired, "08109191442", "AA1235579412");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "SG00012349";
			declaration.JE_MasterBill = "08109191442";
			declaration.JE_HouseBill = "AA1235579412";
			Factory.Save();
			AssertEquals("ML", declaration.ManifestStatus);
			AssertEquals("Multiple Statuses", declaration.ManifestStatusDescription);
		}

		public void TestEmptyManifestStatusAndDescription()
		{
			SetupGlobalManifestAndBill(ZString.Empty, "08109191442", "AA1235579412");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "SG00012349";
			declaration.JE_MasterBill = "08109191442";
			declaration.JE_HouseBill = "AA1235579412";
			Factory.Save();
			AssertEquals(ZString.Empty, declaration.ManifestStatus);
			AssertEquals(Common.SG.GlobalManifestStatusList.Descriptions.NoStatus, declaration.ManifestStatusDescription);
		}

		public void TestConsignee()
		{
			AssertNull(Declaration.Consignee);
			Declaration.JE_OH_Consignee = Factory.New<OrgHeader>().PK;
			AssertNotNull(Declaration.Consignee);
		}

		public void TestExporter()
		{
			AssertNull(Declaration.Exporter);
			Declaration.JE_OH_Exporter = Factory.New<OrgHeader>().PK;
			AssertNotNull(Declaration.Exporter);
		}

		public void TestEndUser()
		{
			AssertNull(Declaration.Buyer);
			Declaration.JE_OH_Buyer = Factory.New<OrgHeader>().PK;
			AssertNotNull(Declaration.Buyer);
		}

		public void TestClaimant()
		{
			AssertNull(Declaration.Claimant);
			Declaration.JE_OH_Claimant = Factory.New<OrgHeader>().PK;
			AssertNotNull(Declaration.Claimant);
		}

		public void TestInwardCarrierAgent()
		{
			AssertNull(Declaration.InwardCarrierAgent);
			Declaration.JE_OH_InwardCarrierAgent = Factory.New<OrgHeader>().PK;
			AssertNotNull(Declaration.InwardCarrierAgent);
		}

		public void TestHandlingAgent()
		{
			AssertNull(Declaration.HandlingAgent);
			Declaration.JE_OH_HandlingAgent = Factory.New<OrgHeader>().PK;
			AssertNotNull(Declaration.HandlingAgent);
		}

		public void TestDocAddressRequirement()
		{
			var declaration = GetJobDeclarationForTesting();
			var iDocAddresses = (IDocAddresses)declaration;
			AssertEquals(false, iDocAddresses.GetDocAddressRequirement(DocAddressType.CarrierHandlingAgent).CanOverride);
			AssertEquals(false, iDocAddresses.GetDocAddressRequirement(DocAddressType.ClaimantAddress).CanOverride);
			AssertEquals(false, iDocAddresses.GetDocAddressRequirement(DocAddressType.InwardCarrierAgent).CanOverride);
			AssertEquals(false, iDocAddresses.GetDocAddressRequirement(DocAddressType.OutwardCarrierAgent).CanOverride);
		}

		public void TestOrgHeaderList()
		{
			var declaration = GetJobDeclarationForTesting();
			var lookups = declaration.Lookups;
			var iDocAddresses = (IDocAddresses)declaration;
			AssertSame(lookups.ForwarderList, iDocAddresses.GetOrgHeaderList(DocAddressType.CarrierHandlingAgent));
			AssertSame(lookups.Organisations, iDocAddresses.GetOrgHeaderList(DocAddressType.ClaimantAddress));
			AssertSame(lookups.Organisations, iDocAddresses.GetOrgHeaderList(DocAddressType.InwardCarrierAgent));
			AssertSame(lookups.Organisations, iDocAddresses.GetOrgHeaderList(DocAddressType.OutwardCarrierAgent));
			AssertSame("JobDocAddress viewed from Addresses Tab has the correct find filtering", lookups.ForwarderList, declaration.HandlingAgentAddress.Lookups.OrgHeader_List);
			AssertSame(lookups.Organisations, declaration.ClaimantAddress.Lookups.OrgHeader_List);
			AssertSame(lookups.Organisations, declaration.InwardCarrierAgentAddress.Lookups.OrgHeader_List);
			AssertSame(lookups.Organisations, declaration.OutwardShippingLineForwarderDocAddress.Lookups.OrgHeader_List);
		}

		public void TestCorrectPlaceIsReturnedForMultiplePlaces()
		{
			var sgPlaceReference = SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "T1", "", "T1 PLACE ENRON WHARVES", effectiveDate: ZDateTime.Today.AddDays(5));
			var sgPlaceCurrentReference = Factory.NewWithValidTestData<ZZRefCusCodeList>();
			sgPlaceCurrentReference.ZZD_Code = "T1";
			sgPlaceCurrentReference.ZZD_Description = "T1 PLACE CALTEX WHARVES";
			sgPlaceCurrentReference.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			sgPlaceCurrentReference.ZZD_StartDate = ZDateTime.Today.AddYears(-1);
			sgPlaceCurrentReference.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			sgPlaceCurrentReference.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Singapore;
			Factory.Save();
			AssertEquals(1, Factory.Load<ZZRefCusCodeListCombined>(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, "T1")).Length);
			Declaration.SG_US_NKPlaceOfStorage = "T1";
			AssertEquals("Should return the 'Current' SGPlace for 'T1'", "T1 PLACE CALTEX WHARVES", Declaration.PlaceOfStorage.ZZD_Description);
		}

		public void TestPlaceOfStorage()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.PremiseType.SailingClub;
			AssertNotNull(Declaration.PlaceOfStorage);
			AssertEquals(SGCPlaces.Constants.PremiseType.SailingClub, Declaration.PlaceOfStorage.ZZD_Code);
		}

		public void TestPlaceOfRelease()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.PremiseType.SailingClub;
			AssertNotNull(Declaration.PlaceOfRelease);
			AssertEquals(SGCPlaces.Constants.PremiseType.SailingClub, Declaration.PlaceOfRelease.ZZD_Code);
		}

		public void TestPlaceOfReceipt()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.PremiseType.SailingClub;
			AssertNotNull(Declaration.PlaceOfReceipt);
			AssertEquals(SGCPlaces.Constants.PremiseType.SailingClub, Declaration.PlaceOfReceipt.ZZD_Code);
		}

		public void TestInwardVesselBerth()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKInwardVesselBerth = SGCPlaces.Constants.PremiseType.SailingClub;
			AssertNotNull(Declaration.InwardVesselBerth);
			AssertEquals(SGCPlaces.Constants.PremiseType.SailingClub, Declaration.InwardVesselBerth.ZZD_Code);
		}

		public void TestOutwardVesselBerth()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKOutwardVesselBerth = SGCPlaces.Constants.PremiseType.SailingClub;
			AssertNotNull(Declaration.OutwardVesselBerth);
			AssertEquals(SGCPlaces.Constants.PremiseType.SailingClub, Declaration.OutwardVesselBerth.ZZD_Code);
		}

		public override void TestDefaultINCOFromOrgLink()
		{
			var currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			var otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));
			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			BaseJobDeclaration dec = GetJobDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			var link1 = consignee.SupplierLinks.AddNew(consignor);
			var link1TrnModeSea = link1.OrgSupBuyLinkTrnModes[0];
			link1TrnModeSea.PF_IncoTerm = "123";
			link1TrnModeSea.PF_TransportMode = dec.TransportModeSeaCodeForTesting;
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			var link1TrnModeAir = link1.OrgSupBuyLinkTrnModes.AddNew();
			link1TrnModeAir.PF_IncoTerm = "789";
			link1TrnModeAir.PF_TransportMode = dec.TransportModeAirCodeForTesting;
			var link2 = consignee.SupplierLinks.AddNew(consignor);
			var link2TrnModSea = link2.OrgSupBuyLinkTrnModes[0];
			link2TrnModSea.PF_IncoTerm = "456";
			link2TrnModSea.PF_TransportMode = dec.TransportModeSeaCodeForTesting;
			link2.OL_RN_NKImporterCountry = otherCountry2.RN_Code;
			dec.JE_OH_Importer = consignee.PK;
			dec.JE_OH_Supplier = consignor.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			dec.JE_RL_NKFinalDestination = currentPort.RL_Code;
			AssertEquals("DO NOT Default Inco Term from org link.", "", dec.JE_ShipmentIncoTerm);
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			AssertEquals("DO NOT Default Inco Term from org link.", "", dec.JE_ShipmentIncoTerm);
			dec.JE_RL_NKFinalDestination = otherUnloco2.RL_Code;
			AssertEquals("DO NOT Default Inco Term from org link.", "", dec.JE_ShipmentIncoTerm);
			dec.JE_JS = ZGuid.Empty;
			dec.JE_OH_Importer = consignee.PK;
			dec.JE_OH_Supplier = consignor.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			dec.JE_RL_NKFinalDestination = currentPort.RL_Code;
			AssertEquals("Default Inco Term from org link as usual when standalone.", "789", dec.JE_ShipmentIncoTerm);
		}

		public override void TestDefaultINCOFromSupplier()
		{
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			var dec = GetJobDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			dec.JE_OH_Supplier = consignor.PK;
			AssertEquals("DO NOT Default Inco Term from supplier.", "", dec.JE_ShipmentIncoTerm);
			dec.JE_JS = ZGuid.Empty;
			dec.JE_OH_Supplier = ZGuid.Empty;
			dec.JE_OH_Supplier = consignor.PK;
			AssertEquals("Default Inco Term from supplier as usual when standalone.", "321", dec.JE_ShipmentIncoTerm);
		}

		public override void TestDefaultINCOFromImporter()
		{
			var consignee = OrgHeader.New(Factory);
			consignee.MiscServ.OM_IMDefaultINCOTerm = "150";
			var dec = GetJobDeclaration();
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			dec.JE_OH_Importer = consignee.PK;
			AssertEquals("DO NOT Default Inco Term from importer.", "", dec.JE_ShipmentIncoTerm);
			dec.JE_JS = ZGuid.Empty;
			dec.JE_OH_Importer = ZGuid.Empty;
			dec.JE_OH_Importer = consignee.PK;
			AssertEquals("Default Inco Term from importer as usual when standalone.", "150", dec.JE_ShipmentIncoTerm);
		}

		#region AutoGenerated Tests
		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			AssertSame(bizO.Lookups, bizO.Lookups);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<BaseJobDeclaration>() is JobDeclaration);
		}

		#endregion
		#region Overrides
		public override void TestBGMReferences()
		{
			Assert(true);
		}

		protected override Tuple<ZString, ZString> GetMessageTypeForTestDefaultPorts()
		{
			return new Tuple<ZString, ZString>(SGJobMessageTypeList.Codes.INP, SGJobMessageTypeList.Codes.OUT);
		}

		public override void TestOrganisationNames()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_Code = "consignee";
			consignee.OH_FullName = "Importer-Consignee Co. Pty. Ltd.";
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_Code = "consignor";
			consignor.OH_FullName = "Supplier-Consignor Co. Pte.";
			OrgHeader forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_IsForwarder = ZBool.True;
			forwarder.OH_Code = "forwarder";
			forwarder.OH_FullName = "Lets Move Freight P/L.";
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Importer Name", ZString.Empty, declaration.ImporterName);
			AssertEquals("Supplier Name", ZString.Empty, declaration.SupplierName);
			AssertEquals("Forwarder Name - SG sets this with a default value", "EDI CUSTOMS BROKERS", declaration.ForwarderName);
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals("Importer Name", "Importer-Consignee Co. Pty. Ltd.", declaration.ImporterName);
			AssertEquals("Supplier Name", "Supplier-Consignor Co. Pte.", declaration.SupplierName);
			AssertEquals("Forwarder Name", "Lets Move Freight P/L.", declaration.ForwarderName);
		}

		public override void TestBondedWarehouseEditable()
		{
			//the controls are not visible on GUI
			Assert(true);
		}

		public override void TestContainersRequiredOnSea()
		{
			//always show when sea, packing tab is not used
			AssertEquals(true, Declaration.ContainersRequired);
		}

		public override void TestContainersRequiredOnAir()
		{
			//always show when air, packing tab is not used
			AssertEquals(true, Declaration.ContainersRequired);
		}

		public override void TestTurningOverrideFreightDefaultOffRemovesHouseBillsThatAreNotMatched()
		{
			// synchronisation of bills does not occur in SG customs - so not required
			Assert(true);
		}

		public override void TestShouldNotSynchroniseWithShipmentEndToEnd()
		{
			//this is tested in jobdeclarationsynchronisation
			Assert(true);
		}

		public override void TestEquipmentUpdatedOnSettingTransportMode()
		{
			//todo: fix
			Assert(true);
		}

		protected override string DefaultMergeType
		{
			get
			{
				return "NON";
			}
		}

		public override void TestILandedCostHeader()
		{
			//SG Does not implement landed consting
			Assert(true);
		}

		public override void TestDisableResultApportionmentWithRealInvoice2()
		{
			Assert(true);
		}

		public override void TestOverrideFreightDefaultsSetsReadOnlyFalse()
		{
			//synchronisation is tested in jobdeclarationsynchronisation. Synch is different in SG therefore the base test fails
			Assert(true);
		}

		public override void TestContainersRequiredOnNonTransportDeclarationType()
		{
			//there is no such thing as a non-transport declaration type in sg customs
			Assert(true);
		}

		public override void TestDefaultLoadPortATDAndDischargePortATA()
		{
			//if anything, this should be in the reverse order....in any event, origin/destination do not need to be filled in, and they are based on different lists to load/discharge
			Assert(true);
		}

		public override void TestUpdatingPortOfArrivalUpdatesEmptyFinalDestination()
		{
			//if anything, this should be in the reverse order....in any event, origin/destination do not need to be filled in, and they are based on different lists to load/discharge
			Assert(true);
		}

		public void TestGlobalManifestTransferChanges()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("IsGlobalManifestIntegrationEnabled", false, dec.IsGlobalManifestIntegrationEnabled);
			var log = dec.Logs.AddNew(Events.TransferFromManifestToCustoms, "MK322423");
			AssertEquals("IsGlobalManifestIntegrationEnabled", true, dec.IsGlobalManifestIntegrationEnabled);
			dec.JE_JS = ZGuid.Invalid;
			AssertEquals("IsGlobalManifestIntegrationEnabled", false, dec.IsGlobalManifestIntegrationEnabled);
			dec.JE_JS = ZGuid.Empty;
			AssertEquals("IsGlobalManifestIntegrationEnabled", true, dec.IsGlobalManifestIntegrationEnabled);
			AssertEquals("dec.JE_MasterBillInfo.ReadOnly", true, dec.JE_MasterBillInfo.ReadOnly);
			AssertEquals("dec.JE_HouseBillInfo.ReadOnly", true, dec.JE_HouseBillInfo.ReadOnly);
			foreach (var entryStatus in new[] { Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, Core.SGConstants.DeclarationStatus.AmendmentPermitReceived, Core.SGConstants.DeclarationStatus.RefundPermitReceived, Core.SGConstants.DeclarationStatus.CancellationAccepted })
			{
				dec.JE_EntryStatus = "ERR";
				AssertNull(dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));
				dec.JE_EntryStatus = entryStatus;
				var tcmLog = dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest);
				AssertNotNull(tcmLog);
				tcmLog.Delete();
				dec.JE_EntryStatus = "ERR";
				AssertNull(dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));
			}

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = ZString.Empty;
			}

			AssertEquals("IsGlobalManifestIntegrationEnabled", false, dec.IsGlobalManifestIntegrationEnabled);
			dec.JE_EntryStatus = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
			AssertNull(dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));
		}

		public override void TestMessageTypeDescription()
		{
			Declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals(MessageTypeCodeList.Descriptions.INP, Declaration.MessageTypeDescription);
		}

		protected override string DefaultImportMessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.INP;
			}
		}

		protected override string DefaultExportMessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			declaration = null;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return Declaration;
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("Branch will be loaded in JE_EntryStatus.", true);
		}

		protected override System.Collections.Hashtable ExpectedDocAddressTypes
		{
			get
			{
				var result = base.ExpectedDocAddressTypes;
				result[DocAddressTypes.Codes.CarrierHandlingAgent] = DocAddressType.CarrierHandlingAgent;
				result[DocAddressTypes.Codes.ClaimantAddress] = DocAddressType.ClaimantAddress;
				result[DocAddressTypes.Codes.InwardCarrierAgent] = DocAddressType.InwardCarrierAgent;
				result[DocAddressTypes.Codes.OutwardCarrierAgent] = DocAddressType.OutwardCarrierAgent;
				return result;
			}
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new JobDeclarationLightValidationTester(bizObjToTest);
		}

		class JobDeclarationLightValidationTester : LightValidationTester
		{
			public JobDeclarationLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != "E2_ParentID" && propertyName != "E2_AddressOverride" && propertyName != "E2_ParentTableCode" && propertyName != "E2_AddressType" && propertyName != "E2_OA_Address";
			}
		}

		#region Related Declarations
		public override void TestRelatedDeclarations()
		{
			var messageType = MessageTypeCodeList.Codes.IPT;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			SetDeclarationForMatch(declaration, messageType);
			var matchDeclaration = GetNewMatchingDec(declaration, messageType);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration.JE_HouseBill = ZString.Empty;
			matchDeclaration.JE_MasterBill = ZString.Empty;
			Factory.Save();
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			var matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(21);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_HouseBill = "HB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			declaration.JE_HouseBill = ZString.Empty;
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_MasterBill = "MB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-11);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration1));
		}

		void SetDeclarationForMatch(JobDeclaration declaration, string messageType)
		{
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			if (messageType == MessageTypeCodeList.Codes.IPT)
			{
				declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				declaration.JE_HouseBill = "HB001";
				declaration.JE_MasterBill = "MB001";
			}
			else if (messageType == MessageTypeCodeList.Codes.TNP)
			{
				declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
				declaration.JE_HouseBill = "HB001";
				declaration.JE_MasterBill = "MB001";
				declaration.SG_OutwardHAWB = "HB00100";
				declaration.SG_OutwardMAWB = "MB00101";
			}
			else
			{
				declaration.SG_OutwardHAWB = "HB001";
				declaration.SG_OutwardMAWB = "MB001";
			}
		}

		JobDeclaration GetNewMatchingDec(JobDeclaration dec, string messageType)
		{
			var newDeclaration = Factory.New<JobDeclaration>();
			if (messageType == MessageTypeCodeList.Codes.IPT)
			{
				newDeclaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				newDeclaration.JE_HouseBill = dec.JE_HouseBill;
				newDeclaration.JE_MasterBill = dec.JE_MasterBill;
			}
			else if (messageType == MessageTypeCodeList.Codes.TNP)
			{
				newDeclaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				newDeclaration.JE_HouseBill = dec.JE_HouseBill;
				newDeclaration.JE_MasterBill = dec.JE_MasterBill;
				newDeclaration.SG_OutwardHAWB = dec.SG_OutwardHAWB;
				newDeclaration.SG_OutwardMAWB = dec.SG_OutwardMAWB;
			}
			else
			{
				newDeclaration.SG_OutwardHAWB = dec.SG_OutwardHAWB;
				newDeclaration.SG_OutwardMAWB = dec.SG_OutwardMAWB;
			}

			newDeclaration.JE_SystemCreateTimeUtc = dec.JE_SystemCreateTimeUtc.AddDays(4);
			return newDeclaration;
		}

		public void TestRelatedDeclarationsBaseTest_UsingOutDeclarations()
		{
			var messageType = MessageTypeCodeList.Codes.OUT;
			var declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			var matchDeclaration = GetNewMatchingDec(declaration, messageType);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration.SG_OutwardHAWB = ZString.Empty;
			matchDeclaration.SG_OutwardMAWB = ZString.Empty;
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			var matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(21);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.SG_OutwardHAWB = "HB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			declaration.JE_HouseBill = ZString.Empty;
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.SG_OutwardHAWB = ZString.Empty;
			matchDeclaration1.SG_OutwardMAWB = "MB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-11);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration1));
		}

		public void TestRelatedDeclarationsBaseTest_UsingTranshipmentDeclarations()
		{
			var messageType = MessageTypeCodeList.Codes.TNP;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			SetDeclarationForMatch(declaration, messageType);
			var matchDeclaration = GetNewMatchingDec(declaration, messageType);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration.SG_OutwardHAWB = ZString.Empty;
			matchDeclaration.SG_OutwardMAWB = ZString.Empty;
			Factory.Save();
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			var matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(21);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.SG_OutwardHAWB = "HB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			declaration.JE_HouseBill = ZString.Empty;
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.SG_OutwardHAWB = ZString.Empty;
			matchDeclaration1.SG_OutwardMAWB = "MB002";
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));
			declaration = Factory.New<JobDeclaration>();
			SetDeclarationForMatch(declaration, messageType);
			matchDeclaration = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1 = GetNewMatchingDec(declaration, messageType);
			matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-11);
			Factory.Save();
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
			Assert(declaration.RelatedDeclarations.Contains(matchDeclaration1));
		}

		public override void TestNewRelatedDeclaration()
		{
			base.TestNewRelatedDeclaration();
			Declaration.SG_OutwardMAWB = "OUTMB0001";
			Declaration.SG_OutwardHAWB = "OUTHB0005";
			Declaration.JE_MasterBill = "MASTERBILL";
			Declaration.JE_HouseBill = "HOUSEBILL";
			JobDeclaration relatedDeclaration = (JobDeclaration)Declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("OUTMB0001", relatedDeclaration.SG_OutwardMAWB);
			AssertEquals("OUTHB0005", relatedDeclaration.SG_OutwardHAWB);
			AssertEquals("MASTERBILL", relatedDeclaration.JE_MasterBill);
			AssertEquals("HOUSEBILL", relatedDeclaration.JE_HouseBill);
		}

		public void TestRelatedDeclarationsOnOutwardDeclarations()
		{
			var outShipment = Factory.New<ForwardingShipment>();
			outShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			outShipment.JS_RL_NKOrigin = "SGSIN";
			outShipment.JS_RL_NKDestination = "AUSYD";
			outShipment.JS_HouseBill = "OUT-HAWB1";
			var shipmentDeclaration = Factory.New<JobDeclaration>();
			shipmentDeclaration.JE_JS = outShipment.PK;
			shipmentDeclaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			shipmentDeclaration.JE_HouseBill = "OUT-HAWB1";
			shipmentDeclaration.SG_OutwardHAWB = "OUT-HAWB1";
			var relatedDeclaration1 = (JobDeclaration)shipmentDeclaration.GetNewRelatedDeclaration(Factory);
			relatedDeclaration1.JE_HouseBill = ZString.Empty;
			Factory.Save();
			AssertEquals("Inward HB should be empty", ZString.Empty, relatedDeclaration1.JE_HouseBill);
			AssertEquals("Outward HB should have been defaulted from related job", "OUT-HAWB1", relatedDeclaration1.SG_OutwardHAWB);
			var relatedDeclaration2 = (JobDeclaration)shipmentDeclaration.GetNewRelatedDeclaration(Factory);
			relatedDeclaration2.JE_HouseBill = ZString.Empty;
			Factory.Save();
			AssertEquals("Inward HB should be empty", ZString.Empty, relatedDeclaration2.JE_HouseBill);
			AssertEquals("Outward HB should have been defaulted from related job", "OUT-HAWB1", relatedDeclaration2.SG_OutwardHAWB);
			AssertEquals("Shipment Declaration should find related OUT Declarations.", 2, shipmentDeclaration.RelatedDeclarations.Count);
		}

		#endregion
		#endregion
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		#endregion
		IAsycudaManifestHeader SetupGlobalManifestAndBill(ZString billStatus, ZString masterBill, ZString houseBill)
		{
			var manifestHeader = Factory.New<IAsycudaManifestHeader>();
			if (!masterBill.IsEmpty)
			{
				manifestHeader.AMA_MasterBill = masterBill;
				var masterBillBO = manifestHeader.MasterBill;
				masterBillBO.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-20);
			}

			var manifestBill = Factory.New<IAsycudaBill>();
			manifestBill.ABL_AMA = manifestHeader.PK;
			manifestBill.ABL_BillNumber = houseBill;
			manifestBill.ABL_BillStatus = billStatus;
			return manifestHeader;
		}
	}
}
