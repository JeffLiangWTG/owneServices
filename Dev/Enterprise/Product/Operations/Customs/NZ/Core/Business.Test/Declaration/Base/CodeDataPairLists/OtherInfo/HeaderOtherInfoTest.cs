using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(HeaderOtherInfo))]
	public sealed class HeaderOtherInfoBOTest : OtherInfoTest
	{
		public override void TestValidateZO_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "NZOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.MAFContainerDeclaration, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			CodeDataPair info1 = Declaration.OtherInfos.AddNew();
			info1.ZO_Code = HeaderOtherInfoList.Codes.ApprovedTransitionalFacility;
			info1.ZO_Data = "1234";
			CodeDataPair info2 = Declaration.OtherInfos.AddNew();
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			Assert("PreCondition: No Message Error", !info2.ZO_CodeInfo.HasMessageErrors());

			info1.ZO_Code = ZString.Empty;
			info2.ZO_Code = ZString.Empty;
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			Assert("Has error now", info2.ZO_CodeInfo.HasMessageError(HeaderOtherInfo.ATFCodeIsRequired));

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			info2.ZO_Code = ZString.Empty;
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			Assert("No error for AIR", !info2.ZO_CodeInfo.HasMessageErrors());
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			info2.ZO_Code = ZString.Empty;
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			Assert("No error for LCL", !info2.ZO_CodeInfo.HasMessageErrors());
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;

			info2.ZO_Code = ZString.Empty;
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			Assert("Should be back to error now", info2.ZO_CodeInfo.HasMessageError(HeaderOtherInfo.ATFCodeIsRequired));
		}

		public void TestImportCodeRequiresData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "TIOTH");
			var approvedFacilityList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var mafDeclarationList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.MAFContainerDeclaration, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(approvedFacilityList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(mafDeclarationList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			CodeDataPair info1 = Declaration.OtherInfos.AddNew();
			info1.ZO_Code = HeaderOtherInfoList.Codes.ApprovedTransitionalFacility;
			AssertEquals("ApprovedTransitionalFacility should require data as TSWCodeListValueRequired attribute is 'Y'.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("ApprovedTransitionalFacility should return error message if data is required", info1.ZO_DataInfo, "ATF Codes are not allowed without accompanying Data.");

			CodeDataPair info2 = Declaration.OtherInfos.AddNew();
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			AssertEquals("MAFContainerDeclaration should not require data as TSWCodeListValueRequired attribute is 'N'.", false, info2.CodeRequiresData);
			info2.RunPreSaveValidation();
			Assert("No Message Error", !info2.ZO_DataInfo.HasMessageErrors());
		}

		public void TestExportCodeRequiresData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "TEOTH");
			var approvedFacilityList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var mafDeclarationList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, HeaderOtherInfoList.Codes.MAFContainerDeclaration, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(approvedFacilityList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(mafDeclarationList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			CodeDataPair info1 = Declaration.OtherInfos.AddNew();
			info1.ZO_Code = HeaderOtherInfoList.Codes.ApprovedTransitionalFacility;
			AssertEquals("ApprovedTransitionalFacility should require data as TSWCodeListValueRequired attribute is 'Y'.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("ApprovedTransitionalFacility should return error message if data is required", info1.ZO_DataInfo, "ATF Codes are not allowed without accompanying Data.");

			CodeDataPair info2 = Declaration.OtherInfos.AddNew();
			info2.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			AssertEquals("MAFContainerDeclaration should not require data as TSWCodeListValueRequired attribute is 'N'.", false, info2.CodeRequiresData);
			info2.RunPreSaveValidation();
			Assert("No Message Error", !info2.ZO_DataInfo.HasMessageErrors());
		}

		public void TestListHasManualBACCCode()
		{
			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "MBR";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestOtherInfoExportListHasSEVandCWC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEOTH", "TEOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "CWC", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "SEV", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "CWC";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "SEV";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "CWC";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "SEV";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			info.ZO_Code = "CWC";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "SEV";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestListHasMCDCodeForBothTSWAndLegacy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEOTH", "TEOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "MCD", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "MCD";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestTSWOtherInfoImportList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TIOTH", "TIOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TIOTH", "DCP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType("TIOTL", "TIOTL");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TIOTL", "OSP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "MBR";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_Code = "DCP";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_Code = "OSP";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestTSWOtherInfoExportList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEOTH", "TEOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "EMP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType("TEOTL", "TEOTL");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTL", "CER", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "MBR";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_Code = "EMP";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_Code = "CER";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestTSWOtherInfoOnlyExportListHasPER()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEOTH", "TEOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "PER", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "PER";

			var declaration = Infos.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Assert("Declaration is TSW Import", declaration.IsTSWImportDeclaration);
			info.RunPreSaveValidation();
			AssertHasNotifications(ListValidation.InvalidCodeMessageError.ToString(), info.ZO_CodeInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Assert("Declaration is TSW Export", declaration.IsTSWExportDeclaration);
			info.RunPreSaveValidation();
			AssertNoNotifications(ListValidation.InvalidCodeMessageError.ToString(), info.ZO_CodeInfo);
		}

		public void TestListHasProvisionalValuesCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TIOTH", "TIOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TIOTH", "PVL", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "PVL";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "PVL";
			info.RunPreSaveValidation();
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);      // Code is valid for TSW Import

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "PVL";
			info.RunPreSaveValidation();
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);     // Code is NOT valid for TSW Export
		}

		protected override ZString ExpectedCodes => "ATF, AWC, MCD";

		protected override ZString CodeThatDoesntCareIfItHasDataOrNot
		{
			get { return HeaderOtherInfoList.Codes.MAFContainerDeclaration; }
		}
		protected override ZString CodeThatRequiresData
		{
			get { return HeaderOtherInfoList.Codes.ApprovedTransitionalFacility; }
		}
		protected override ZString CodeThatRequiresNoData
		{
			get { return HeaderOtherInfoList.Codes.AwareOfContents; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return Declaration.OtherInfos.AddNew();
		}

		protected override BusinessObject BOCodeInfoIsAttachedTo
		{
			get { return Declaration; }
		}

		protected override CodeDataPairCollection GetCodeInfoCollection()
		{
			return Declaration.OtherInfos;
		}

		protected override void InitialiseTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "TIOTH");
			var approvedFacilityList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var mafDeclarationList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.MAFContainerDeclaration, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var awareOfContentsList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, HeaderOtherInfoList.Codes.AwareOfContents, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(approvedFacilityList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(mafDeclarationList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			helper.CreateCusCodeListAttribute(awareOfContentsList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			Factory.Save();
		}
	}
}
