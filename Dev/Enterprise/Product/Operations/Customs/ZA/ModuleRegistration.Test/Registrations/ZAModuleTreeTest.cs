using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.ModuleRegistration.Test
{
	[TestedType(typeof(ZAModuleListingSubset))]
	sealed class ZAModuleTreeTest : BaseModuleTreeTest
	{
		public void TestZASpecificModules()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("ZACustomsStatement", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.CustomsStatement.Name);
				var customsResponseIndex = AssertModuleAdded("CustomsResponse", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.CustomsResponse.Name);
				var za404Index = AssertModuleAdded("ZA404ProofOfPayment", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.ZA404ProofOfPayment.Name);
				var customsReportIndex = AssertModuleAdded("CustomsReport", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ModuleIDs.CustomsReport.Name);
				Assert("Customs Response should be before Customs Report", customsReportIndex > customsResponseIndex);
				Assert("Customs Report should be before ZA 404 Proof Of Payment", za404Index > customsReportIndex);
				_ = AssertModuleAdded("GenralMessage", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.GenralMessage.Name);
				_ = AssertModuleAdded("Global Manifest", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsGlobal, ModuleIDs.Customs.ASYCUDA.Manifest.Name);
			}
		}

		public void TestZAWarehouseOperatorTransactionsModule()
		{
			CombineAssertions("Only show WarehouseOperatorTransactions module if ZA and WarehouseOperatorTransactionsModuleEnabled registry item set to true", () =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
				{
					Loader.LoadModules();
					_ = AssertModuleAdded("Not ZA so don't show WarehouseOperatorTransactions module", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.WarehouseOperatorTransactions.Name);

					ObjectFactory.Get<Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					Loader.LoadModules();
					_ = AssertModuleAdded("Not ZA so don't show WarehouseOperatorTransactions module even if WarehouseOperatorTransactionsModuleEnabled registry item set to true", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.WarehouseOperatorTransactions.Name);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
				{
					ObjectFactory.Get<Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					Loader.LoadModules();
					_ = AssertModuleAdded("The default is that WarehouseOperatorTransactions module not shown for ZA as WarehouseOperatorTransactionsModuleEnabled registry item default is false", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.WarehouseOperatorTransactions.Name);

					ObjectFactory.Get<Integration.Customs.ZA.IZACustomsRegistry>().WarehouseOperatorTransactionsModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					Loader.LoadModules();
					_ = AssertModuleAdded("WarehouseOperatorTransactions module shown for ZA when WarehouseOperatorTransactionsModuleEnabled registry item set to true", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.WarehouseOperatorTransactions.Name);
				}
			});
		}

		[TestDate(2018, 5, 23)]
		public void TestZAOutturnAndGateInOutModuleZ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("OutturnAndGateInOut should be loaded", expectModuleToExist: true, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.OutturnAndGateInOut.Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				Loader.LoadModules();
				_ = AssertModuleAdded("OutturnAndGateInOut should be hidden", expectModuleToExist: false, ModuleTreeLoaderConstant.Category.Operations, ModuleTreeLoaderConstant.Section.CustomsMain, ZAModuleIDs.OutturnAndGateInOut.Name);
			}
		}
	}
}
