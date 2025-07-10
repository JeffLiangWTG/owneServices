using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCusCodeModule))]
	sealed class OrgCusCodeModuleTest : ZModuleBasherTest
	{
		public void TestCheckpoints()
		{
			using (OrgCusCodeModule module = new OrgCusCodeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Organisation, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (OrgCusCodeModuleForTest module = new OrgCusCodeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgCusCodeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (OrgCusCodeModuleForTest module = new OrgCusCodeModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is ActiveBusinessObjectCollection<OrgCusCode>);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (OrgCusCodeModuleForTest module = new OrgCusCodeModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is OrgCusCodeFilterBusinessObject);
			}
		}

		#endregion

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgCusCode;
		}

		#region OrgCusCodeModuleForTest

		public class OrgCusCodeModuleForTest : OrgCusCodeModule
		{
			public IFilterControl NewFilterControl
			{
				get { return GetNewFilterControl(); }
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get { return GetNewGridCollection(); }
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get { return GetNewFilterBusinessObject(); }
			}
		}

		#endregion
	}
}
