using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(PermitCode))]
	public class PermitCodeTest : CodeDataPairTest
	{
		#region TestHumanReadableName
		public void TestHumanReadableName()
		{
			AssertEquals("Permit Code", Permit.HumanReadableName);
		}
		#endregion

		#region TestValidateZO_Code_MatchesCodeList
		public void TestValidateZO_Code_MatchesCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPER");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			Permit.ZO_Code = "XXX";
			Assert("Invalid code", Permit.ZO_CodeInfo.HasMessageErrors());

			Permit.ZO_Code = Permit.ZO_CodeList[0].Code;
			Assert("Valid code", !Permit.ZO_CodeInfo.HasMessageErrors());
		}
		#endregion

		#region TestValidateZO_Data
		public void TestValidateZO_Data()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPER");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, PermitCodeList.Codes.OfficeOfRadiationSafety, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			Permit.ZO_Code = Permit.ZO_CodeList[0].Code;
			Permit.ZO_Data = ZString.Empty;
			Assert("All permit code needs Data", Permit.ZO_DataInfo.HasNotifications());

			Permit.ZO_Data = "787878";
			Assert("All permit code needs Data", !Permit.ZO_DataInfo.HasNotifications());
		}
		#endregion

		#region TestZO_CodeList
		public void TestCorrectListForLegacyOrTSW()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPER");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, PermitCodeList.Codes.OfficeOfRadiationSafety, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "EPA", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "BIP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZPIM");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, PermitCodeList.Codes.OfficeOfRadiationSafety, new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "EPA", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "BIP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Permit.ZO_Code = PermitCodeList.Codes.CustomsDepartmentApproval;
			AssertEquals("Valid code for legacy", false, Permit.ZO_CodeInfo.HasMessageErrors());

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Permit.ZO_Code = PermitCodeList.Codes.MinistryOfAgriculture5;
			AssertEquals("'AF5' is an invalid code for TSW Export", true, Permit.ZO_CodeInfo.HasMessageErrors());

			Permit.ZO_Code = "EPA";
			AssertEquals("'EPA' is a valid code for TSW Export", false, Permit.ZO_CodeInfo.HasMessageErrors());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Permit.ZO_Code = "ZIL";
			AssertEquals("'ZIL' is an invalid code for TSW Import", true, Permit.ZO_CodeInfo.HasMessageErrors());

			Permit.ZO_Code = "BIP";
			AssertEquals("'BIP' is a valid code for TSW Import", false, Permit.ZO_CodeInfo.HasMessageErrors());
		}

		public void TestCorrectListForCusClassPartPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPEREXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "PCE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "PCB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZPERIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "PCI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "PCB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var permitCode = pivot.PermitCodes.AddNew();
			AssertCodesInOutOfList(permitCode.ZO_CodeList, new string[] { "PCE", "PCB" }, new string[] { "PCI" });

			// list reflects a change of classification type
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertCodesInOutOfList(permitCode.ZO_CodeList, new string[] { "PCI", "PCB" }, new string[] { "PCE" });

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertCodesInOutOfList(permitCode.ZO_CodeList, new string[] { "PCB", "PCI", "PCE" }, System.Array.Empty<string>());
		}

		public void TestZO_CodeList()
		{
			Assert(Permit.ZO_CodeList is CodeDescriptionPairList);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Assert(Permit.ZO_CodeList is PermitCodeList);
		}

		public void TestIsImportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "NZPERIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsImport, "MAF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			Permit.ZO_Code = "MAF";
			AssertEquals("Permit Export Code", true, Permit.IsImportCode);
			Permit.ZO_Code = "XXX";
			AssertEquals("Permit Export Code", false, Permit.IsImportCode);
		}

		public void TestIsExportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPEREXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "APA", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			Permit.ZO_Code = "APA";
			AssertEquals("Permit Export Code", true, Permit.IsExportCode);
			Permit.ZO_Code = "XXX";
			AssertEquals("Permit Export Code", false, Permit.IsExportCode);
		}
		#endregion

		#region TestAllowDuplicates
		public void TestAllowDuplicates()
		{
			AssertEquals("Permit.ShouldValidateForDuplicateCodes", false, Permit.ShouldValidateForDuplicateCodes);
		}
		#endregion

		#region Implementation
		#region Permit
		protected PermitCode Permit
		{
			get
			{
				if (fPermit == null)
				{
					fPermit = (PermitCode)GetNewBusinessObject();
				}
				return fPermit;
			}
		}
		PermitCode fPermit;
		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PermitCode(Factory, Declaration.PermitCodes);
		}

		protected override BusinessObject BOCodeInfoIsAttachedTo
		{
			get { return Declaration; }
		}

		protected override CodeDataPairCollection GetCodeInfoCollection()
		{
			return Declaration.PermitCodes;
		}

		#endregion
	}
}
