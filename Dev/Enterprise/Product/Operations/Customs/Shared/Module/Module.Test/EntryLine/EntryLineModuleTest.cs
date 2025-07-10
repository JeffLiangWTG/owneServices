using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(EntryLineModule))]
	public class EntryLineModuleTest : ZModuleBasherTest
	{
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			collection.Add(entryLine);
		}

		public void TestLicenseCheckPoint()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.CustomsFiles, module.SecurityCheckpoint);
			}
		}

		public void TestModuleID()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(GetModuleID(), module.ID);
			}
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.EntryLine;
		}

		protected override bool HasController() => false;
	}
}
