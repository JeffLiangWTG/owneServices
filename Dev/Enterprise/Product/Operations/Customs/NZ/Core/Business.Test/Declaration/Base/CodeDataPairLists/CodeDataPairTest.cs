using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public abstract class CodeDataPairTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateZO_CodeMustHaveACode()
		{
			CodeDataPair info = Infos.AddNew();
			info.ZO_Code = "";
			AssertHasError(info.ZO_CodeInfo, CodeDataPair.MustEnterACode);
		}

		public void TestZO_CodeForcedToUpper()
		{
			Info.ZO_Code = "abc";
			AssertEquals("Info.ZO_Code", "ABC", Info.ZO_Code);
		}

		public void TestZO_DataForcedToUpper()
		{
			Info.ZO_Data = "abc";
			AssertEquals("Info.ZO_Code", "ABC", Info.ZO_Data);
		}

		public void TestDeclaration()
		{
			if (BOCodeInfoIsAttachedTo is JobDeclaration || BOCodeInfoIsAttachedTo is JobComInvoiceLine)
			{
				AssertEquals("Info.Declaration", Declaration, Info.Declaration);
			}
			else
			{
				Assert("Not Valid in this context.", condition: true);
			}
		}

		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPER");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "NZOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPRO");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "NZOIL");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportLine, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Assert("Description read only", Info.ZO_DescriptionInfo.ReadOnly);
			Info.ZO_Code = "ATF";
			AssertEquals("Description", Info.ZO_CodeList[0].Description, Info.ZO_Description);
			Info.ZO_Code = "ZZZ";
			AssertEquals("Description", ZString.Empty, Info.ZO_Description);
		}

		public void TestDeleteAffectHasChanges()
		{
			Info.ZO_Code = "XXX";
			AssertEquals("Changed", true, BOCodeInfoIsAttachedTo.HasChanges);

			Factory.Save();
			AssertEquals("Changed", false, BOCodeInfoIsAttachedTo.HasChanges);

			Infos.RemoveAndDelete(Info);
			AssertEquals("Changed", true, BOCodeInfoIsAttachedTo.HasChanges);
		}

		public void TestDeleteRevalidateDuplicateCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "NZOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.OtherInfosExportHeader, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "NZPRM");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitsExport, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPRO");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "ATF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			Info.ZO_Code = "ATF";

			CodeDataPair info2 = Infos.AddNew();
			info2.ZO_Code = Info.ZO_Code;
			AssertEquals("PreCondition: Duplicate code", Info.ShouldValidateForDuplicateCodes, info2.ZO_CodeInfo.HasErrors());

			Infos.RemoveAndDelete(Info);
			AssertEquals("No more duplicate code", false, info2.ZO_CodeInfo.HasErrors());
		}

		public void TestToString()
		{
			Info.ZO_Code = "APE";
			AssertEquals("ToString()", "APE", Info.ToString());

			Info.ZO_Code = "APD";
			Info.ZO_Data = "SOMEDATA";
			AssertEquals("ToString()", "APD=SOMEDATA", Info.ToString());
		}

		public void TestLoad()
		{
			ZString addInfoString = "APE";
			Info.LoadFromString(addInfoString);
			AssertEquals("APE", Info.ZO_Code);
			AssertEquals(ZString.Empty, Info.ZO_Data);

			addInfoString = "APD=SOMEDATA";
			Info.LoadFromString(addInfoString);
			AssertEquals("APD", Info.ZO_Code);
			AssertEquals("SOMEDATA", Info.ZO_Data);
		}

		public void TestHasChanges()
		{
			Assert("PreCondition:No change yet", !Info.HasChanges);
			Info.ZO_Code = ZString.Empty;
			Assert("No change yet", !Info.HasChanges);
			Info.ZO_Code = "ZZZ";
			Assert("Change yet", Info.HasChanges);
		}

		public void TestSettingCodeWithoutChangingDoesNotLoseHasChanges()
		{
			Assert("PreCondition:No change yet", !Info.HasChanges);
			Info.ZO_Code = "123";
			AssertEquals("PreCondition:HasChanges", true, Info.HasChanges);
			Info.ZO_Code = "123";
			AssertEquals("HasChanges", true, Info.HasChanges);
		}

		public void TestSettingDataWithoutChangingDoesNotLoseHasChanges()
		{
			Assert("PreCondition:No change yet", !Info.HasChanges);
			Info.ZO_Data = "123";
			AssertEquals("PreCondition:HasChanges", true, Info.HasChanges);
			Info.ZO_Data = "123";
			AssertEquals("HasChanges", true, Info.HasChanges);
		}

		public void TestHasChangesWithLoad()
		{
			Assert("PreCondition:No changes", !Info.HasChanges);
			ZString addInfoString = "APE";
			Info.LoadFromString(addInfoString);
			Assert("No changes", !Info.HasChanges);
		}

		public void TestDataWithoutCodeGivesMessageError()
		{
			Info.ZO_Code = "";
			Info.ZO_Data = "FREDDY";
			AssertEquals("Data without code should error", true, Info.ZO_DataInfo.HasError(CodeDataPair.MustEnterACodeForThisData));

			Info.ZO_Data = "";
			AssertEquals("ZO_Data should have no error with no data or code", false, Info.ZO_DataInfo.HasErrors());
		}

		#region Implementation

		protected void AssertCodesInOutOfList(CodeDescriptionPairList permitCodeList, string[] codesIn, string[] codesOut)
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

		protected CodeDataPair Info
		{
			get
			{
				if (fInfo == null)
				{
					fInfo = Infos.AddNew();
				}
				return fInfo;
			}
		}
		CodeDataPair fInfo;

		protected CodeDataPairCollection Infos
		{
			get
			{
				if (fInfos == null)
				{
					fInfos = GetCodeInfoCollection();
				}
				return fInfos;
			}
		}
		CodeDataPairCollection fInfos;

		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					JobComInvoiceHeader invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
					fInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		protected CusClassification Classification
		{
			get
			{
				if (fClassification == null)
				{
					fClassification = Factory.New<CusClassification>();
				}
				return fClassification;
			}
		}
		CusClassification fClassification;

		protected abstract BusinessObject BOCodeInfoIsAttachedTo { get; }
		protected abstract CodeDataPairCollection GetCodeInfoCollection();
		#endregion
	}

	public sealed class CodeDataPairNonInheritedTest : TestCaseWithFactory
	{
		public void TestChangedOverridableMethods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = declaration.OtherInfos;
			TestCodeInfo info = new TestCodeInfo(Factory, collection);
			AssertEquals("Info.ZO_Code", "", info.ZO_Code);
			AssertEquals("Info.ZO_Data", "", info.ZO_Data);
			AssertEquals("Info.ZO_Code_OnChanged_Fired", false, info.ZO_Code_OnChanged_Fired);
			AssertEquals("Info.ZO_Data_OnChanged_Fired", false, info.ZO_Data_OnChanged_Fired);

			info.ZO_Code = "AAA";
			AssertEquals("Info.ZO_Code_OnChanged_Fired", true, info.ZO_Code_OnChanged_Fired);
			AssertEquals("Info.ZO_Data_OnChanged_Fired", false, info.ZO_Data_OnChanged_Fired);
			info.ZO_Code_OnChanged_Fired = false;

			info.ZO_Data = "12345";
			AssertEquals("Info.ZO_Code_OnChanged_Fired", false, info.ZO_Code_OnChanged_Fired);
			AssertEquals("Info.ZO_Data_OnChanged_Fired", true, info.ZO_Data_OnChanged_Fired);
			info.ZO_Data_OnChanged_Fired = false;

			info.ZO_Code = "AAA";
			AssertEquals("Info.ZO_Code_OnChanged_Fired", false, info.ZO_Code_OnChanged_Fired);
			AssertEquals("Info.ZO_Data_OnChanged_Fired", false, info.ZO_Data_OnChanged_Fired);

			info.ZO_Data = "12345";
			AssertEquals("Info.ZO_Code_OnChanged_Fired", false, info.ZO_Code_OnChanged_Fired);
			AssertEquals("Info.ZO_Data_OnChanged_Fired", false, info.ZO_Data_OnChanged_Fired);

			using (collection.SuspendCodesChangedRelatedActions())
			{
				info.ZO_Code = "AAA";
				info.ZO_Data = "12345";
			}
			AssertEquals("Info.ZO_Code_OnChanged_Fired", false, info.ZO_Code_OnChanged_Fired);
			AssertEquals("Info.ZO_Data_OnChanged_Fired", false, info.ZO_Data_OnChanged_Fired);
		}

		#region Implementation
		class TestCodeInfo : CodeDataPair
		{
			public TestCodeInfo(BusinessObjectFactory factory, CodeDataPairCollection parentCollection)
				: base(factory, parentCollection)
			{
			}

			public override bool CodeRequiresData
			{
				get { return true; }
			}

			public override bool ShouldValidateForDuplicateCodes
			{
				get { return true; }
			}

			protected override CodeDescriptionPairList ExportCodes => new CodeDescriptionPairList();
			protected override CodeDescriptionPairList ImportCodes => new CodeDescriptionPairList();
			protected override CodeDescriptionPairList LegacyCodes => new CodeDescriptionPairList();
			protected override string ImportExportCodesCacheKey => "NZ|TestCodeInfo|ImportExportCodes";

			public bool ZO_Code_OnChanged_Fired;
			protected override void ZO_Code_OnChanged()
			{
				ZO_Code_OnChanged_Fired = true;
			}

			public bool ZO_Data_OnChanged_Fired;
			protected override void ZO_Data_OnChanged()
			{
				ZO_Data_OnChanged_Fired = true;
			}
		}
		#endregion
	}
}
