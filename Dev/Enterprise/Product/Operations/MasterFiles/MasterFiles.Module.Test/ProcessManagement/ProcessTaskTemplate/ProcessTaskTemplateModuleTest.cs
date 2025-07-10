using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTaskTemplateModule))]
	sealed class ProcessTaskTemplateModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestFilterControl()
		{
			processTaskT = new ProcessTaskTemplateModuleForTest();
			IFilterControl controlForTest = processTaskT.GetNewFilterControlForTest();
			Assert(controlForTest is ProcessTaskTemplateFilterControl);
			controlForTest.Dispose();
			processTaskT.Dispose();
		}

		public void TestGridCollection()
		{
			processTaskT = new ProcessTaskTemplateModuleForTest();
			IBusinessObjectCollection collectionForTest = processTaskT.GetNewGridCollectionForTest();
			Assert(collectionForTest is BusinessObjectCollection);
			processTaskT.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			processTaskT = new ProcessTaskTemplateModuleForTest();
			FilterBusinessObject businessForTest = processTaskT.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			processTaskT.Dispose();
		}

		public void TestHasOperationalActions()
		{
			using (var module = new ProcessTaskTemplateModuleForTest())
			{
				var supportable = module as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertNotNull(supportable.OperationalActionSupporter);
			}
		}

		#region Implementation

		ProcessTaskTemplateModuleForTest processTaskT;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProcessTemplates;
		}

		#endregion
	}
}
