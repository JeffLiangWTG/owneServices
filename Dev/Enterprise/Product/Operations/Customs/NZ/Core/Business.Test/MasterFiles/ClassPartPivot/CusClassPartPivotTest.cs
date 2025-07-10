using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	public class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCI_PartsOfClassification()
		{
			AssertEquals("Caption", "Parts Of Classification", DataBoundResourceStrings.GetDataForProperty(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_PartsOfClassification)).Caption);

			var pivot = Factory.New<CusClassPartPivot>();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				pivot.CI_PartsOfClassification = "1234.56.78.9";
				AssertEquals("1234.56.78.9", pivot.CI_PartsOfClassification);
				AssertEquals("1234.56.78.9", pivot.AddInfo.ZN_PartsOfClassification);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				pivot.CI_PartsOfClassification = "987654321";
				AssertEquals("987654321", pivot.CI_PartsOfClassification);
				AssertEquals("9876.54.32.1", pivot.AddInfo.ZN_PartsOfClassification);
			}
		}

		public void TestCI_ConcessionCode()
		{
			AssertEquals("Caption", "Concession", DataBoundResourceStrings.GetDataForProperty(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_ConcessionCode)).Caption);
		}

		public void TestCI_TariffNum()
		{
			AssertEquals("Caption", "Tariff", DataBoundResourceStrings.GetDataForProperty(typeof(CusClassPartPivot), nameof(CusClassPartPivot.CI_TariffNum)).Caption);

			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			var tariffQuery = new ZQuery(CusClassPartPivotSchema.CI_TariffNum, "1234.56.78.9");
			AssertNull("Precondition - no existing line with this tariff", Factory.LoadTop1<CusClassPartPivot>(tariffQuery));

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				pivot.CI_TariffNum = "1234.56.78.9";
				AssertEquals("1234.56.78.9", pivot.CI_TariffNum);
				Factory.Save();
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var pivotInNewFactory = newFactory.LoadTop1<CusClassPartPivot>(tariffQuery);
				AssertNotNull("line was stored with a dotted tariff", pivotInNewFactory);
				AssertEquals("Dots are removed in getter", "123456789", pivotInNewFactory.CI_TariffNum);

				pivotInNewFactory.CI_TariffNum = "987654321";
				pivotInNewFactory.CI_TariffNum = "123456789";  // force write back of new value
				newFactory.Save();

				var newFactory2 = new BusinessObjectFactory();
				pivotInNewFactory = newFactory2.LoadTop1<CusClassPartPivot>(tariffQuery);
				AssertNotNull("line was stored with a dotted tariff", pivotInNewFactory);
				AssertEquals("Dots are removed in getter", "123456789", pivotInNewFactory.CI_TariffNum);
			}
		}

		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZDSCEXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "PCE", "PCE DESC", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "PCB", "PCB DESC", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZDSCIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "PCI", "PCI DESC", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "PCB", "PCB DESC", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			var permitCode = pivot.PermitCodes.AddNew();

			var descriptions = string.Join(",", permitCode.ZO_CodeList.ToArray().Select(v => v.Description));

			AssertEquals("Descriptions and Order Match", $"{CodeDataPair.BothPrefix} | PCB DESC,{CodeDataPair.ExportPrefix} | PCE DESC,{CodeDataPair.ImportPrefix} | PCI DESC", descriptions);
		}

		public void TestPermitCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPEREXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "PCE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "PCB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZPERIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "PCI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "PCB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var permitCode = pivot.PermitCodes.AddNew();
			AssertCodesInOutOfList(permitCode.ZO_CodeList, new string[] { "PCE", "PCB" }, new string[] { "PCI" });

			// list reflects a change of classification type
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertCodesInOutOfList(permitCode.ZO_CodeList, new string[] { "PCI", "PCB" }, new string[] { "PCE" });

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertCodesInOutOfList(permitCode.ZO_CodeList, new string[] { "PCE", "PCI", "PCB" }, Array.Empty<string>());
		}

		public void TestProhibitedCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPROEXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "PGE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "PGB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "NZPROIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "PGI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "PGB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var prohibitedCode = pivot.ProhibitedCodes.AddNew();
			AssertCodesInOutOfList(prohibitedCode.ZO_CodeList, new string[] { "PGE", "PGB" }, new string[] { "PGI" });

			// list reflects a change of classification type
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertCodesInOutOfList(prohibitedCode.ZO_CodeList, new string[] { "PGI", "PGB" }, new string[] { "PGE" });

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertCodesInOutOfList(prohibitedCode.ZO_CodeList, new string[] { "PGB", "PGI", "PGE" }, Array.Empty<string>());
		}

		public void TestOtherInfos()
		{
			// CusClassPartPivot uses the OtherInfos Line Collection.

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

			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var otherInfoCode = pivot.OtherInfos.AddNew();
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLE", "OLB" }, new string[] { "OLI", "OHE", "OHI", "OHB" });

			// list reflects a change of classification type
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLI", "OLB" }, new string[] { "OLE", "OHE", "OHI", "OHB" });

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertCodesInOutOfList(otherInfoCode.ZO_CodeList, new string[] { "OLI", "OLE", "OLB" }, new string[] { "OHB" });
		}

		void AssertCodesInOutOfList(CodeDescriptionPairList permitCodeList, string[] codesIn, string[] codesOut)
		{
			foreach (var code in codesIn)
			{
				AssertEquals(code, true, permitCodeList.ContainsCode(code));
			}
			foreach (var code in codesOut)
			{
				AssertEquals(code, false, permitCodeList.ContainsCode(code));
			}
		}

		public void TestLastAuditPropertiesAreReadOnly()
		{
			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			Assert("CI_LastAuditedDateReadOnly", pivot.CI_LastAuditedDateReadOnly);
			Assert("CI_LastAuditedUserReadOnly", pivot.CI_LastAuditedUserReadOnly);

			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_LastAuditedDate", includesInherit: true, attrib => attrib.Member == "CI_LastAuditedDateReadOnly");
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_LastAuditedUser", includesInherit: true, attrib => attrib.Member == "CI_LastAuditedUserReadOnly");
		}

		public void TestTariffFieldsCanBeReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_TariffNum", includesInherit: true, attrib => attrib.Member == "IsTariffNumReadOnly");
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_ConcessionCode", includesInherit: true, attrib => attrib.Member == "IsTariffNumReadOnly");
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_PartsOfClassification", includesInherit: true, attrib => attrib.Member == "IsTariffNumReadOnly");
		}

		public void TestDefaultUniversalTariffProperties()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("Using Universal Tariff", false, typeof(CusClassPartPivot).GetProperty("UseUniversalTariff", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pivot));
		}

		public void TestTariffFieldsAreClearedWhenClassificationIsSet()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "ABC123";
			pivot.CI_ConcessionCode = "ABC123";
			pivot.CI_PartsOfClassification = "ABC123";

			pivot.CI_CC = ZGuid.NewZGuid();
			AssertEquals("CI_TariffNum is Empty", ZString.Empty, pivot.CI_TariffNum);
			AssertEquals("CI_ConcessionCode is Empty", ZString.Empty, pivot.CI_ConcessionCode);
			AssertEquals("CI_PartsOfClassification is Empty", ZString.Empty, pivot.CI_PartsOfClassification);
		}

		public void TestExpectedBusinessObjectTypeForList()
		{
			var additionalDataForFiltering = GetNewBusinessObject() as Common.IHaveAdditionalDataForBorderWise;
			AssertEquals("ExpectedBusinessObjectTypeForList", typeof(NZCClassification), additionalDataForFiltering.ExpectedBusinessObjectTypeForList);
		}

		public void TestGetAdditionalDataForFilteringEDITariff()
		{
			var pivot = GetNewBusinessObject() as CusClassPartPivot;

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var additionalData = pivot.GetAdditionalDataForBorderWise(ZString.Empty);
			AssertEquals("I", additionalData.ParameterForBorderWise);
			AssertEquals(ZDate.Today, additionalData.DateForDutyRate);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			additionalData = pivot.GetAdditionalDataForBorderWise(ZString.Empty);
			AssertEquals("E", additionalData.ParameterForBorderWise);
			AssertEquals(ZDate.Today, additionalData.DateForDutyRate);
		}

		[TestDate(2008, 6, 6)]
		public void TestDutyRateForCurrentCountry()
		{
			var classification = Factory.New<CusClassification>();
			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			pivot.CI_CC = classification.PK;

			classification.CC_TariffNum = "4201.00.00.01B";
			AssertEquals("classification.DutyRateForCurrentCountry", "7.00%", pivot.DutyRateForCurrentCountry);
			classification.CC_ConcessionCode = "909090A";
			AssertEquals("classification.DutyRateForCurrentCountry", "FREE", pivot.DutyRateForCurrentCountry);
			classification.CC_TariffNum = "8716.90.09.09K";
			classification.CC_ConcessionCode = "";
			AssertEquals("classification.DutyRateForCurrentCountry", "FREE", pivot.DutyRateForCurrentCountry);
			classification.CC_PartsOfClassification = "8716.80.09.10K";
			AssertEquals("classification.DutyRateForCurrentCountry", "7.00%", pivot.DutyRateForCurrentCountry);

			pivot.CI_CC = ZGuid.Empty;

			pivot.CI_TariffNum = "4201.00.00.01B";
			AssertEquals("classification.DutyRateForCurrentCountry", "7.00%", pivot.DutyRateForCurrentCountry);
			pivot.CI_ConcessionCode = "909090A";
			AssertEquals("classification.DutyRateForCurrentCountry", "FREE", pivot.DutyRateForCurrentCountry);
			pivot.CI_TariffNum = "8716.90.09.09K";
			pivot.CI_ConcessionCode = "";
			AssertEquals("classification.DutyRateForCurrentCountry", "FREE", pivot.DutyRateForCurrentCountry);
			pivot.CI_PartsOfClassification = "8716.80.09.10K";
			AssertEquals("classification.DutyRateForCurrentCountry", "7.00%", pivot.DutyRateForCurrentCountry);
		}

		public void TestITariffValidationData()
		{
			NZCClassification classificationForTariff = Factory.New<NZCClassification>();
			classificationForTariff.U0_Tariff = "0000.00.00.01A";
			classificationForTariff.U0_DateActiveFrom = ZDate.Today.AddDays(-1);
			classificationForTariff.U0_DateActiveTo = ZDate.Today.AddDays(1);
			classificationForTariff.U0_IsManual = false;

			NZCClassification classificationForPartsOfTariff = Factory.New<NZCClassification>();
			classificationForPartsOfTariff.U0_Tariff = "0000.00.00.01B";
			classificationForPartsOfTariff.U0_DateActiveFrom = ZDate.Today.AddDays(-1);
			classificationForPartsOfTariff.U0_DateActiveTo = ZDate.Today.AddDays(1);
			classificationForPartsOfTariff.U0_IsManual = true;

			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = "0000.00.00.01A";
			pivot.CI_PartsOfClassification = "0000.00.00.01B";

			PermitCode permit = pivot.PermitCodes.AddNew();
			permit.ZO_Code = "CUD";
			permit.ZO_Data = "147981H";

			var validationData = pivot as ITariffValidationData;
			AssertEquals("ITariffValidationData.TariffCode", pivot.CI_TariffNum, validationData.TariffCode);
			AssertEquals("ITariffValidationData.TariffCodeInfo", pivot.CI_TariffNumInfo, validationData.TariffCodeInfo);
			AssertEquals("ITariffValidationData.TariffBO", pivot.CI_TariffNum, (validationData.TariffBO as NZCClassification).U0_Tariff);
			AssertEquals("ITariffValidationData.DateForDutyRate", ZDateTime.Today, validationData.DateForDutyRate);
			AssertEquals("ITariffValidationData.PermitCodeCount", pivot.PermitCodes.Count, validationData.PermitCodeCount);
			AssertEquals("ITariffValidationData.EmptyTariffIsFullError", true, validationData.EmptyTariffIsFullError);
			AssertEquals("ITariffValidationData.EmptyTariffIsAllowed", true, validationData.EmptyTariffIsAllowed);
			AssertEquals("ITariffValidationData.PartsOfTariffCode", pivot.CI_PartsOfClassification, validationData.PartsOfTariffCode);
			AssertEquals("ITariffValidationData.PartsOfTariffCodeInfo", pivot.CI_PartsOfClassificationInfo, validationData.PartsOfTariffCodeInfo);
			Assert("ITariffValidationData.PartsOfTariffBO", pivot.AddInfo.PartsOfClassification == validationData.PartsOfTariffBO);

			var codeTypes = validationData.AllowableTariffCodeTypes;
			AssertEquals("ITariffValidationData.AllowableTariffCodeTypes Import", true, codeTypes.Import);
			AssertEquals("ITariffValidationData.AllowableTariffCodeTypes Export", true, codeTypes.Export);
			AssertEquals("ITariffValidationData.AllowableTariffCodeTypes Excise", false, codeTypes.Excise);
		}

		public void TestLookups()
		{
			var pivot = GetNewBusinessObject() as CusClassPartPivot;
			Assert("Lookups is CusClassPartPivotLookups", pivot.Lookups is CusClassPartPivotLookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusClassPartPivot>();
		}
	}
}
