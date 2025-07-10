using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedure))]
	class RefCusProcedureTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetCachedList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Taiwan, "", "01", "", "", "", "IMP");
			procedure1.ZZ6_StartDate = ZDateTime.Today.AddMonths(-1);
			procedure1.ZZ6_EndDate = ZDateTime.Today.AddMonths(1);
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Taiwan, "", "02", "", "", "", "EXP");
			procedure2.ZZ6_StartDate = ZDateTime.Today.AddMonths(-1);
			procedure2.ZZ6_EndDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();
			var list1 = RefCusProcedure.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, "", "IMP");
			var list2 = RefCusProcedure.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, "", "IMP");
			AssertEquals("IsCached", true, object.ReferenceEquals(list1, list2));
			var list3 = RefCusProcedure.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, "", "EXP");
			AssertEquals("Not Cached", false, object.ReferenceEquals(list3, list1));
		}

		public void TestDelete()
		{
			var procedure = (RefCusProcedure)GetNewBusinessObjectForDeleteTest(Factory);
			var attribute1 = procedure.Attributes.AddNew("BOBAttribute", "SHORT");
			var attribute2 = procedure.Attributes.AddNew("BOBAttribute2", "SHORT");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var carrierInDiffFactory = newFactory.Load<RefCusProcedure>(procedure.PK);
			carrierInDiffFactory.Delete();
			newFactory.Save();
			AssertEquals("procedure.IsDeleted", true, procedure.IsDeleted);
			AssertEquals("attribute1.IsDeleted", true, attribute1.IsDeleted);
			AssertEquals("attribute2.IsDeleted", true, attribute2.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var bo = (RefCusProcedure)base.GetNewBusinessObjectForDeleteTest(factory);
			bo.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return bo;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		public void TestFullCodeCurrentPlusPreviousPlusConcession()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_ProcedureCode = "12";
			zz6.ZZ6_PreviousProcedureCode = "34";
			zz6.ZZ6_Concession = "567";
			AssertEquals("1234567", zz6.FullCodeCurrentPlusPreviousPlusConcession);
		}

		public void TestGetProcedureDescriptionAndConcessionDescription()
		{
			CreateRefCusCodeList(Factory);
			GlbStaff.CurrentUser.GS_WorkingLanguage = "DE";
			CombineAssertions("GetProcedureDescription & GetConcessionDescription", () =>
			{
				AssertEquals("Unmatched Country Code DE, get EU RefCusCodeList as fallback.", "EU Procedure 01", RefCusProcedure.GetProcedureDescription(Factory, Core.Constants.CountryCodes.Germany, "01"));
				AssertEquals("Unmatched Country Code DE, GetConcessionDescription returns input code.", "01", RefCusProcedure.GetConcessionDescription(Factory, Core.Constants.CountryCodes.Germany, "01"));

				AssertEquals("Load DE RefCusCodeListLanguage from RefCusCodeList CDC.", "IE-Prozedurcode 01", RefCusProcedure.GetProcedureDescription(Factory, Core.Constants.CountryCodes.Ireland, "01"));
				AssertEquals("Load DE RefCusCodeListLanguage from RefCusCodeList CPDC.", "Erweitertes IE-Verfahren 01", RefCusProcedure.GetConcessionDescription(Factory, Core.Constants.CountryCodes.Ireland, "01"));

				GlbStaff.CurrentUser.GS_WorkingLanguage = "EN";
				AssertEquals("Load description from RefCusCodeList CDC.", "IE Procedure Code 01", RefCusProcedure.GetProcedureDescription(Factory, Core.Constants.CountryCodes.Ireland, "01"));
				AssertEquals("Load description from RefCusCodeList CPDC.", "IE Advanced Procedure 01", RefCusProcedure.GetConcessionDescription(Factory, Core.Constants.CountryCodes.Ireland, "01"));
			});
		}

		static void CreateRefCusCodeList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euDataGrouping);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.ProcedureCode, "EU Procedure Code");
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "IE Advanced Procedure Code");
			const string languageCodeGermany = "DE";
			helper.CreateOrGetLanguage(languageCodeGermany, "Deutsch");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "IE Procedure 01 unmatched Grouping", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "01", "IE Procedure 01 unmatched type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "EU Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProcedureCode, "10", "IE Procedure 10 unmatched code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var ieCode01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "IE Procedure Code 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListLanguage(ieCode01, languageCodeGermany, "IE-Prozedurcode 01");

			var ieAdviceCode1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "01", "IE Advanced Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListLanguage(ieAdviceCode1, languageCodeGermany, "Erweitertes IE-Verfahren 01");
			factory.Save();
		}

		public void TestHasAttribute()
		{
			var procedure = (RefCusProcedure)GetNewBusinessObjectForDeleteTest(Factory);
			procedure.Attributes.AddNew("TESTHASATTRIBUTE", ZString.Empty);
			Assert(procedure.HasAttribute("TESTHASATTRIBUTE"));
			Assert(!procedure.HasAttribute("INVALIDATTRIBUTE"));
		}

		public void TestIsIntoRegime()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			Assert(!zz6.IsIntoRegime());
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			Assert(zz6.IsIntoRegime());
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			Assert(!zz6.IsIntoRegime());
			zz6.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Assert(zz6.IsIntoRegime());
			zz6.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.No;
			Assert(!zz6.IsIntoRegime());
			zz6.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Assert(zz6.IsIntoRegime());
			zz6.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.No;
			Assert(!zz6.IsIntoRegime());
		}

		public void TestIsOutOfRegime()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			Assert(!zz6.IsOutOfRegime());
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			Assert(zz6.IsOutOfRegime());
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			Assert(!zz6.IsOutOfRegime());
			zz6.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Assert(zz6.IsOutOfRegime());
			zz6.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.No;
			Assert(!zz6.IsOutOfRegime());
			zz6.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Assert(zz6.IsOutOfRegime());
			zz6.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.No;
			Assert(!zz6.IsOutOfRegime());
		}

		public void TestIntoWarehouse()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsIntoWarehouse());
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsIntoWarehouse());
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoWarehouse());
		}

		public void TestOutOfWarehouse()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsOutOfWarehouse());
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsOutOfWarehouse());
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfWarehouse());
		}

		public void TestIntoInwardProcessing()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsIntoInwardProcessing());
			zz6.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsIntoInwardProcessing());
			zz6.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoInwardProcessing());
		}

		public void TestIsIntoWarehouse()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			AssertEquals(ZBool.False, zz6.IsIntoWarehouse(null));
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsIntoWarehouse(null));
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsIntoWarehouse(null));
			zz6.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoWarehouse(null));
			var context = new RefCusProcedureContextForTest();
			context.IsIntoWarehouseResult = true;
			AssertEquals(ZBool.True, zz6.IsIntoWarehouse(context));
			context.IsIntoWarehouseResult = false;
			AssertEquals(ZBool.False, zz6.IsIntoWarehouse(context));
		}

		public void TestIsOutOfWarehouse()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfWarehouse(null));
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsOutOfWarehouse(null));
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsOutOfWarehouse(null));
			zz6.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfWarehouse(null));
			var context = new RefCusProcedureContextForTest();
			context.IsOutOfWarehouseResult = true;
			AssertEquals(ZBool.True, zz6.IsOutOfWarehouse(context));
			context.IsOutOfWarehouseResult = false;
			AssertEquals(ZBool.False, zz6.IsOutOfWarehouse(context));
		}

		public void TestIsIntoTemporaryImport()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_IntoTemporaryImport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryImport(null));
			zz6.ZZ6_IntoTemporaryImport = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsIntoTemporaryImport(null));
			zz6.ZZ6_IntoTemporaryImport = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryImport(null));
			zz6.ZZ6_IntoTemporaryImport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryImport(null));
			var context = new RefCusProcedureContextForTest();
			context.IsIntoTemporaryImportResult = true;
			AssertEquals(ZBool.True, zz6.IsIntoTemporaryImport(context));
			context.IsIntoTemporaryImportResult = false;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryImport(context));
		}

		public void TestIsOutOfTemporaryImport()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_OutOfTemporaryImport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryImport(null));
			zz6.ZZ6_OutOfTemporaryImport = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsOutOfTemporaryImport(null));
			zz6.ZZ6_OutOfTemporaryImport = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryImport(null));
			zz6.ZZ6_OutOfTemporaryImport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryImport(null));
			var context = new RefCusProcedureContextForTest();
			context.IsOutOfTemporaryImportResult = true;
			AssertEquals(ZBool.True, zz6.IsOutOfTemporaryImport(context));
			context.IsOutOfTemporaryImportResult = false;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryImport(context));
		}

		public void TestIsIntoTemporaryExport()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_IntoTemporaryExport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryExport(null));
			zz6.ZZ6_IntoTemporaryExport = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsIntoTemporaryExport(null));
			zz6.ZZ6_IntoTemporaryExport = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryExport(null));
			zz6.ZZ6_IntoTemporaryExport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryExport(null));
			var context = new RefCusProcedureContextForTest();
			context.IsIntoTemporaryExportResult = true;
			AssertEquals(ZBool.True, zz6.IsIntoTemporaryExport(context));
			context.IsIntoTemporaryExportResult = false;
			AssertEquals(ZBool.False, zz6.IsIntoTemporaryExport(context));
		}

		public void TestIsOutOfTemporaryExport()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_OutOfTemporaryExport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryExport(null));
			zz6.ZZ6_OutOfTemporaryExport = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsOutOfTemporaryExport(null));
			zz6.ZZ6_OutOfTemporaryExport = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryExport(null));
			zz6.ZZ6_OutOfTemporaryExport = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryExport(null));
			var context = new RefCusProcedureContextForTest();
			context.IsOutOfTemporaryExportResult = true;
			AssertEquals(ZBool.True, zz6.IsOutOfTemporaryExport(context));
			context.IsOutOfTemporaryExportResult = false;
			AssertEquals(ZBool.False, zz6.IsOutOfTemporaryExport(context));
		}

		public void TestSupportInwardAndOutwardProcessing()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_IntoInwardProcessing = "Y";
			AssertEquals("ZZ6_IntoInwardProcessing Y, SupportIntoInwardProcessing should be true.", true, zz6.IsIntoInwardProcessing());
			zz6.ZZ6_OutOfInwardProcessing = "Y";
			AssertEquals("ZZ6_OutOfInwardProcessing Y, SupportOutOfInwardProcessing should be true.", true, zz6.IsOutOfInwardProcessing());
			zz6.ZZ6_IntoInwardProcessing = "";
			AssertEquals("ZZ6_IntoInwardProcessing empty, SupportIntoInwardProcessing should be false.", false, zz6.IsIntoInwardProcessing());
			zz6.ZZ6_OutOfInwardProcessing = "";
			AssertEquals("ZZ6_OutOfInwardProcessing empty, SupportOutOfInwardProcessing should be false.", false, zz6.IsOutOfInwardProcessing());
			zz6.ZZ6_IntoOutwardProcessing = "Y";
			AssertEquals("ZZ6_IntoOutwardProcessing Y, SupportIntoOutwardProcessing should be true.", true, zz6.IsIntoOutwardProcessing());
			zz6.ZZ6_OutofOutwardProcessing = "Y";
			AssertEquals("ZZ6_OutOfOutwardProcessing Y, SupportOutOfOutwardProcessing should be true.", true, zz6.IsOutOfOutwardProcessing());
			zz6.ZZ6_IntoOutwardProcessing = "";
			AssertEquals("ZZ6_IntoOutwardProcessing empty, SupportIntoOutwardProcessing should be true.", false, zz6.IsIntoOutwardProcessing());
			zz6.ZZ6_OutofOutwardProcessing = "";
			AssertEquals("ZZ6_OutOfOutwardProcessing empty, SupportOutOfOutwardProcessing should be false.", false, zz6.IsOutOfOutwardProcessing());
			var context = new RefCusProcedureContextForTest();
			AssertEquals("ZZ6_IntoInwardProcessing empty, context returns false, SupportIntoInwardProcessing should be false.", false, zz6.IsIntoInwardProcessing(context));
			AssertEquals("ZZ6_OutOfInwardProcessing empty, context returns false, SupportOutOfInwardProcessing should be false.", false, zz6.IsOutOfInwardProcessing(context));
			AssertEquals("ZZ6_IntoOutwardProcessing empty, context returns false, SupportIntoOutwardProcessing should be false.", false, zz6.IsIntoOutwardProcessing(context));
			AssertEquals("ZZ6_OutOfOutwardProcessing empty, context returns false, SupportOutOfOutwardProcessing should be false.", false, zz6.IsOutOfOutwardProcessing(context));
			zz6.ZZ6_IntoInwardProcessing = "I";
			zz6.ZZ6_OutOfInwardProcessing = "I";
			zz6.ZZ6_IntoOutwardProcessing = "I";
			zz6.ZZ6_OutofOutwardProcessing = "I";
			AssertEquals("ZZ6_IntoInwardProcessing I, context returns false, SupportIntoInwardProcessing should be false.", false, zz6.IsIntoInwardProcessing(context));
			AssertEquals("ZZ6_OutOfInwardProcessing I, context returns false, SupportOutOfInwardProcessing should be false.", false, zz6.IsOutOfInwardProcessing(context));
			AssertEquals("ZZ6_IntoOutwardProcessing I, context returns false, SupporIntoOutwardProcessing should be false.", false, zz6.IsIntoOutwardProcessing(context));
			AssertEquals("ZZ6_OutOfOutwardProcessing I, context returns false, SupporOutOftOutwardProcessing should be false.", false, zz6.IsOutOfOutwardProcessing(context));
			context.IsIntoInwardProcessingResult = true;
			context.IsOutOfInwardProcessingResult = true;
			context.IsIntoOutwardProcessingResult = true;
			context.IsOutOfOutwardProcessingResult = true;
			AssertEquals("ZZ6_IntoInwardProcessing I, context returns true, SupportIntoInwardProcessing should be false.", true, zz6.IsIntoInwardProcessing(context));
			AssertEquals("ZZ6_OutOfInwardProcessing I, context returns true, SupportOutOfInwardProcessing should be false.", true, zz6.IsOutOfInwardProcessing(context));
			AssertEquals("ZZ6_IntoOutwardProcessing I, context returns true, SupporIntoOutwardProcessing should be false.", true, zz6.IsIntoOutwardProcessing(context));
			AssertEquals("ZZ6_OutOfOutwardProcessing I, context returns true, SupportOutOfOutwardProcessing should be false.", true, zz6.IsOutOfOutwardProcessing(context));
		}

		public void TestZZ6_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateOrGetLanguage("IT", "Italian");
			helper.CreateOrGetLanguage("EN", "English");
			Factory.Save();

			var procedure02 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Switzerland, "", "02", "", "", "Processing Trade", "");
			helper.CreateRefCusProcedureLanguage(procedure02, "FR", "Trafic de perfectionnement");
			helper.CreateRefCusProcedureLanguage(procedure02, "IT", "Traffico di perfezionamento");
			var procedure03 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Switzerland, "", "03", "", "", "Repair Trade", "");
			helper.CreateRefCusProcedureLanguage(procedure03, "FR", "Trafic de réparation");
			helper.CreateRefCusProcedureLanguage(procedure03, "IT", "Traffico di riparazione");

			CombineAssertions(() =>
			{
				var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				currentUser.GS_WorkingLanguage = "EN";
				Factory.Save();

				using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					AssertEquals("ZZI_Description in default English", "Processing Trade", procedure02.ZZ6_Description);
					AssertEquals("ZZI_Description in default English", "Repair Trade", procedure03.ZZ6_Description);
				}

				currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
				Factory.Save();
				using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					AssertEquals("ZZI_Description in French", "Trafic de perfectionnement", procedure02.ZZ6_Description);
					AssertEquals("ZZI_Description in French", "Trafic de réparation", procedure03.ZZ6_Description);
				}

				currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.Italian;
				Factory.Save();
				using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					AssertEquals("ZZI_Description in Italien", "Traffico di perfezionamento", procedure02.ZZ6_Description);
					AssertEquals("ZZI_Description in Italien", "Traffico di riparazione", procedure03.ZZ6_Description);
				}

				currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.Swedish;
				Factory.Save();
				using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					AssertEquals("ZZI_Description by default", "Processing Trade", procedure02.ZZ6_Description);
					AssertEquals("ZZI_Description by default", "Repair Trade", procedure03.ZZ6_Description);
				}
			});
		}

		public void TestIntoVATWarehouse()
		{
			var zz6 = (RefCusProcedure)GetNewBusinessObject();
			zz6.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.Yes;
			AssertEquals(ZBool.True, zz6.IsIntoVATWarehouse());
			zz6.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.No;
			AssertEquals(ZBool.False, zz6.IsIntoVATWarehouse());
			zz6.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.Indeterminate;
			AssertEquals(ZBool.False, zz6.IsIntoVATWarehouse());
		}

		class RefCusProcedureContextForTest : IRefCusProcedureContext
		{
			public ZBool IsIntoWarehouseResult;
			public ZBool IsOutOfWarehouseResult;
			public ZBool IsIntoInwardProcessingResult;
			public ZBool IsOutOfInwardProcessingResult;
			public ZBool IsIntoOutwardProcessingResult;
			public ZBool IsOutOfOutwardProcessingResult;
			public ZBool IsIntoTemporaryImportResult;
			public ZBool IsOutOfTemporaryImportResult;
			public ZBool IsIntoTemporaryExportResult;
			public ZBool IsOutOfTemporaryExportResult;
			public ZBool IsIntoVATWarehouse;
			ZBool IRefCusProcedureContext.IsIntoWarehouse => IsIntoWarehouseResult;
			ZBool IRefCusProcedureContext.IsOutOfWarehouse => IsOutOfWarehouseResult;
			ZBool IRefCusProcedureContext.IsIntoInwardProcessing => IsIntoInwardProcessingResult;
			ZBool IRefCusProcedureContext.IsOutOfInwardProcessing => IsOutOfInwardProcessingResult;
			ZBool IRefCusProcedureContext.IsIntoOutwardProcessing => IsIntoOutwardProcessingResult;
			ZBool IRefCusProcedureContext.IsOutOfOutwardProcessing => IsOutOfOutwardProcessingResult;
			ZBool IRefCusProcedureContext.IsIntoTemporaryImport => IsIntoTemporaryImportResult;
			ZBool IRefCusProcedureContext.IsOutOfTemporaryImport => IsOutOfTemporaryImportResult;
			ZBool IRefCusProcedureContext.IsIntoTemporaryExport => IsIntoTemporaryExportResult;
			ZBool IRefCusProcedureContext.IsOutOfTemporaryExport => IsOutOfTemporaryExportResult;
			ZBool IRefCusProcedureContext.IsIntoVATWarehouse => IsIntoVATWarehouse;
		}
	}
}
