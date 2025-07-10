using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(LineOtherInfo))]
	public class LineOtherInfoBOTest : OtherInfoTest
	{
		public override void TestValidateZO_Code()
		{
			base.TestValidateZO_Code();
			var otherInfo = Info as LineOtherInfo;
			otherInfo.ZO_Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZLowValueGoodsExclusion;
			AssertNoMessageError(otherInfo.ZO_CodeInfo, "LVX code is not applicable to the tariff chapter specified for this item.");
			InvoiceLine.JI_Tariff = "2207.01";
			otherInfo.ValidateZO_Code();
			AssertHasMessageError(otherInfo.ZO_CodeInfo, "LVX code is not applicable to the tariff chapter specified for this item.");
			InvoiceLine.JI_Tariff = "2208.01";
			otherInfo.ValidateZO_Code();
			AssertNoMessageError(otherInfo.ZO_CodeInfo, "LVX code is not applicable to the tariff chapter specified for this item.");
		}

		public void TestIsLVX()
		{
			var otherInfo = TestOtherInfo as LineOtherInfo;
			otherInfo.ZO_Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZLowValueGoodsExclusion;
			Assert(otherInfo.IsLVX);

			otherInfo.ZO_Code = "OSP";
			Assert(!otherInfo.IsLVX);
		}

		public void TestCorrectListForCusClassPartPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "NZOTHEXPHDR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "OHE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "OHB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "NZOTHIMPHDR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "OHI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "OHB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "OLE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "OLB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "OLI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "OLB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var otherInfoCode = pivot.OtherInfos.AddNew();
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLE", "OLB" }, new string[] { "OLI", "OHE", "OHI", "OHB" });

			// list reflects a change of classification type
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLI", "OLB" }, new string[] { "OLE", "OHE", "OHI", "OHB" });

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLB", "OLI", "OLE" }, new string[] { "OHE", "OHI", "OHB" });
		}

		public void TestCodeRequiresDataForCusClassPartPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");

			var bunkeringListImp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.Bunkering, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var cargoOnlyListImp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.CargoOnly, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var bunkeringListExp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, LineOtherInfoList.Codes.Bunkering, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var cargoOnlyListExp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, LineOtherInfoList.Codes.CargoOnly, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(bunkeringListImp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(cargoOnlyListImp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			helper.CreateCusCodeListAttribute(bunkeringListExp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			helper.CreateCusCodeListAttribute(cargoOnlyListExp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);

			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			var info1 = pivot.OtherInfos.AddNew();

			info1.ZO_Code = LineOtherInfoList.Codes.CargoOnly;

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("CargoOnly should require data as TSWCodeListValueRequired attribute is 'Y' for Export.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("CargoOnly should return error message if data is required", info1.ZO_DataInfo, "CGO Codes are not allowed without accompanying Data.");

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("CargoOnly should not require data as TSWCodeListValueRequired attribute is 'N' for Import.", false, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertNoMessageError("CargoOnly should not have an error message for data required", info1.ZO_DataInfo, "CGO Codes are not allowed without accompanying Data.");

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertEquals("CargoOnly should require data as TSWCodeListValueRequired attribute is 'Y' for Export.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("CargoOnly should return error message if data is required", info1.ZO_DataInfo, "CGO Codes are not allowed without accompanying Data.");

			info1.ZO_Code = LineOtherInfoList.Codes.Bunkering;

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Bunkering should not require data as TSWCodeListValueRequired attribute is 'N' for Export.", false, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertNoMessageError("Bunkering should not have an error message for data required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Bunkering should require data as TSWCodeListValueRequired attribute is 'Y' for Import.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("Bunkering should return error message if data is required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertEquals("Bunkering should require data as TSWCodeListValueRequired attribute is 'Y' for Import.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("Bunkering should return error message if data is required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");
		}

		public void TestCorrectListForCusClassification()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "NZOTHEXPHDR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "OHE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "OHB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "NZOTHIMPHDR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "OHI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportHeader, "OHB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "OLE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "OLB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "OLI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "OLB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.EXP;
			var otherInfoCode = classification.OtherInfos.AddNew();
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLE", "OLB" }, new string[] { "OLI", "OHE", "OHI", "OHB" });

			// list reflects a change of classification type
			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.IMP;
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLI", "OLB" }, new string[] { "OLE", "OHE", "OHI", "OHB" });

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.Both;
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLB", "OLI", "OLE" }, new string[] { "OHE", "OHI", "OHB" });
		}

		public void TestCodeRequiresDataForCusClassification()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");

			var bunkeringListImp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.Bunkering, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var cargoOnlyListImp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.CargoOnly, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var bunkeringListExp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, LineOtherInfoList.Codes.Bunkering, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var cargoOnlyListExp = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, LineOtherInfoList.Codes.CargoOnly, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(bunkeringListImp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(cargoOnlyListImp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			helper.CreateCusCodeListAttribute(bunkeringListExp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			helper.CreateCusCodeListAttribute(cargoOnlyListExp.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);

			Factory.Save();

			var classification = Factory.New<CusClassification>();
			var info1 = classification.OtherInfos.AddNew();

			info1.ZO_Code = LineOtherInfoList.Codes.CargoOnly;

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.EXP;
			AssertEquals("CargoOnly should require data as TSWCodeListValueRequired attribute is 'Y' for Export.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("CargoOnly should return error message if data is required", info1.ZO_DataInfo, "CGO Codes are not allowed without accompanying Data.");

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.IMP;
			AssertEquals("CargoOnly should not require data as TSWCodeListValueRequired attribute is 'N' for Import.", false, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertNoMessageError("CargoOnly should not have an error message for data required", info1.ZO_DataInfo, "CGO Codes are not allowed without accompanying Data.");

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.Both;
			AssertEquals("CargoOnly should require data as TSWCodeListValueRequired attribute is 'Y' for Export.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("CargoOnly should return error message if data is required", info1.ZO_DataInfo, "CGO Codes are not allowed without accompanying Data.");

			info1.ZO_Code = LineOtherInfoList.Codes.Bunkering;

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.EXP;
			AssertEquals("Bunkering should not require data as TSWCodeListValueRequired attribute is 'N' for Export.", false, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertNoMessageError("Bunkering should not have an error message for data required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.IMP;
			AssertEquals("Bunkering should require data as TSWCodeListValueRequired attribute is 'Y' for Import.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("Bunkering should return error message if data is required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");

			classification.CC_ClassificationType = Enterprise.Customs.Common.ClassificationType.Both;
			AssertEquals("Bunkering should require data as TSWCodeListValueRequired attribute is 'Y' for Import.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("Bunkering should return error message if data is required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");
		}

		public void TestImportCodeRequiresData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var bunkeringList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.Bunkering, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var cargoOnlyList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.CargoOnly, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(bunkeringList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(cargoOnlyList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			CodeDataPair info1 = InvoiceLine.OtherInfos.AddNew();
			info1.ZO_Code = LineOtherInfoList.Codes.Bunkering;
			AssertEquals("Bunkering should require data as TSWCodeListValueRequired attribute is 'Y'.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("Bunkering should return error message if data is required", info1.ZO_DataInfo, "BUN Codes are not allowed without accompanying Data.");

			CodeDataPair info2 = InvoiceLine.OtherInfos.AddNew();
			info2.ZO_Code = LineOtherInfoList.Codes.CargoOnly;
			AssertEquals("CargoOnly should not require data as TSWCodeListValueRequired attribute is 'N'.", false, info2.CodeRequiresData);
			info2.RunPreSaveValidation();
			Assert("No Message Error", !info2.ZO_DataInfo.HasMessageErrors());
		}

		public void TestExportCodeRequiresData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "TEOTL");
			var awareOfContentsList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, LineOtherInfoList.Codes.AwareOfContents, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			var cargoOnlyList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, LineOtherInfoList.Codes.CargoOnly, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(awareOfContentsList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			helper.CreateCusCodeListAttribute(cargoOnlyList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			CodeDataPair info1 = InvoiceLine.OtherInfos.AddNew();
			info1.ZO_Code = LineOtherInfoList.Codes.AwareOfContents;
			AssertEquals("AwareOfContents should require data as TSWCodeListValueRequired attribute is 'Y'.", true, info1.CodeRequiresData);
			info1.RunPreSaveValidation();
			AssertHasMessageError("AwareOfContents should return error message if data is required", info1.ZO_DataInfo, "AWC Codes are not allowed without accompanying Data.");

			CodeDataPair info2 = InvoiceLine.OtherInfos.AddNew();
			info2.ZO_Code = LineOtherInfoList.Codes.CargoOnly;
			AssertEquals("CargoOnly should not require data as TSWCodeListValueRequired attribute is 'N'.", false, info2.CodeRequiresData);
			info2.RunPreSaveValidation();
			Assert("No Message Error", !info2.ZO_DataInfo.HasMessageErrors());
		}

		public void TestImportListHasProvisionalValuesCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TIOTH", "TIOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TIOTH", "PVL", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType("TIOTL", "TIOTL");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TIOTL", "OSP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "PVL";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "OSP";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "PVL";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "OSP";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "OSP";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestExportListHasProvisionalValuesCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEOTH", "TEOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "PVL", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType("TEOTL", "TEOTL");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTL", "OSP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Infos.Declaration.JE_EDITransmitDate = new ZDateTime(2017, 5, 1);
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "PVL";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "OSP";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "PVL";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
			info.ZO_Code = "OSP";
			AssertNoNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);

			info.ZO_CodeInfo.ClearAllNotifications();
			Infos.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Infos.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			info.ZO_Code = "OSP";
			AssertHasNotifications("The code you have selected is not in the list.", info.ZO_CodeInfo);
		}

		public void TestIsImportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "NZOTHIMPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			TestOtherInfo.ZO_Code = "ATF";
			AssertEquals("OtherInfos Import Code", true, TestOtherInfo.IsImportCode);
			TestOtherInfo.ZO_Code = "XXX";
			AssertEquals("OtherInfos Import Code", false, TestOtherInfo.IsImportCode);
		}

		public void TestIsExportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOTHEXPLN");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "EMP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			TestOtherInfo.ZO_Code = "EMP";
			AssertEquals("OtherInfos Code", true, TestOtherInfo.IsExportCode);
			TestOtherInfo.ZO_Code = "XXX";
			AssertEquals("OtherInfos Code", false, TestOtherInfo.IsExportCode);
		}

		protected override ZString ExpectedCodes => "AWC, CZ1";

		protected override ZString CodeThatDoesntCareIfItHasDataOrNot
		{
			get { return ""; }
		}
		protected override ZString CodeThatRequiresData
		{
			get { return LineOtherInfoList.Codes.CitizenshipOfImporter1; }
		}
		protected override ZString CodeThatRequiresNoData
		{
			get { return LineOtherInfoList.Codes.AwareOfContents; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return InvoiceLine.OtherInfos.AddNew();
		}

		protected override BusinessObject BOCodeInfoIsAttachedTo
		{
			get { return InvoiceLine; }
		}

		protected override CodeDataPairCollection GetCodeInfoCollection()
		{
			return InvoiceLine.OtherInfos;
		}

		protected override void InitialiseTestData()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, "TIOTL");
			var citizenshipOfImporter1List = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.CitizenshipOfImporter1, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(citizenshipOfImporter1List.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);
			var awareOfContentsList = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosImportLine, LineOtherInfoList.Codes.AwareOfContents, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeListAttribute(awareOfContentsList.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.TSWCodeListValueRequired, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);
			Factory.Save();
		}
	}
}
