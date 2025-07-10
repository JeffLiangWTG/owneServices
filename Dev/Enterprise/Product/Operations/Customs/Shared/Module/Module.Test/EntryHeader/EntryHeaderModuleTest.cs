using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(EntryHeaderModule))]
	public class EntryHeaderModuleTest : ZModuleBasherTest
	{
		public void TestLicenseCheckPoint()
		{
			using (var module = (EntryHeaderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = (EntryHeaderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.CustomsDeclarationEntries, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EntryHeader;
		}

		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			//Current country setting to'ER' is only done in memory, not persisted into db. However the module collection uses db only query and test branch in db is AU.
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}

			var factory = new BusinessObjectFactory();
			var declaration = GetDeclarationForTesting(factory);
			var entry = GetEntryThatMatchesGridCollectionFilter(declaration);
			factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		public void TestIOperationalActionSupportableMemebers()
		{
			using (var module = (EntryHeaderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var operationalActionSupportable = module as IOperationalActionSupportable;
				AssertNotNull("EntryHeaderModule as IOperationalActionSupportable", operationalActionSupportable);

				var operationalActionSupporter = operationalActionSupportable.OperationalActionSupporter;
				AssertNotNull(nameof(operationalActionSupportable.OperationalActionSupporter), operationalActionSupporter);
				AssertEquals("OperationalActionSupporter Type", ExpectedOperationalActionSupporterType, operationalActionSupporter.GetType());
				AssertSame("Cached OperationalActionSupporter", operationalActionSupporter, operationalActionSupportable.OperationalActionSupporter);
			}
		}

		public void TestPluginContainsOperationalActions()
		{
			using (var module = (EntryHeaderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNotNull("EntryHeaderModule.Plugins contains OperationalActions", module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		protected virtual BaseJobDeclaration GetDeclarationForTesting(BusinessObjectFactory factory)
		{
			return factory.New<BaseJobDeclaration>();
		}

		protected virtual CusEntryHeader GetEntryThatMatchesGridCollectionFilter(BaseJobDeclaration declaration)
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "TEST_REF";
			return entry;
		}

		protected virtual Type ExpectedOperationalActionSupporterType => typeof(EntryHeaderOperationalActionSupporter);
	}
}
