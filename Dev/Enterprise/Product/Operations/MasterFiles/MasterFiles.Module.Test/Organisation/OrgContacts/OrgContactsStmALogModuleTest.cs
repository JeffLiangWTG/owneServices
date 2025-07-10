using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsStmALogModule))]
	sealed class OrgContactsStmALogModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgContactsStmALog;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestCheckpoints()
		{
			using (OrgContactsModule module = new OrgContactsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.OrgContact, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new OrgContactsStmALogModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgContactStmALogFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new OrgContactsStmALogModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is StmALogCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new OrgContactsStmALogModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is OrgContactStmALogFilterBusinessObject);
			}
		}

		#endregion

		#region OrgContactsModuleForTest

		public class OrgContactsStmALogModuleForTest : OrgContactsStmALogModule
		{
			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return new StmALogCollection(Factory); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}
		}

		#endregion
	}
}
