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
	[TestedType(typeof(StmTemplateModule))]
	sealed class StmTemplateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DocumentTemplate;
		}

		public void TestCheckpoints()
		{
			using (StmTemplateModule module = new StmTemplateModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.QuotationDocuments, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (StmTemplateModuleForTest module = new StmTemplateModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is StmTemplateFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (StmTemplateModuleForTest module = new StmTemplateModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is StmTemplateCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (StmTemplateModuleForTest module = new StmTemplateModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is StmTemplateFilterBusinessObject);
			}
		}

		sealed class StmTemplateModuleForTest : StmTemplateModule
		{
			public StmTemplateModuleForTest()
			{
			}

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
	}
}
