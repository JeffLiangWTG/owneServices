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
	[TestedType(typeof(AccGLAccountDescriptorModule))]
	public class AccGLAccountDescriptorModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccGLAccountDescriptor;
		}

		public void TestCheckpoints()
		{
			using (AccGLAccountDescriptorModule module = new AccGLAccountDescriptorModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GLAccountDescriptor, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccGLAccountDescriptorModuleForTest module = new AccGLAccountDescriptorModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccGLAccountDescriptorFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccGLAccountDescriptorModuleForTest module = new AccGLAccountDescriptorModuleForTest())
			{
				IBusinessObjectCollection glAccountDescCollection = module.NewGridCollection;
				Assert("Invalid type", glAccountDescCollection is AccGLAccountDescriptorCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccGLAccountDescriptorModuleForTest module = new AccGLAccountDescriptorModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccGLAccountDescriptorFilterBusinessObject);
			}
		}

		#endregion
	}
}
