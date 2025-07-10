using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(LoadModule))]
	public class LoadModuleTest : WarehouseGlowOnlyModuleTest<LoadModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(LoadFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(LoadFilterControl);

		protected override Type ExpectedCollectionType => typeof(WhsLoadCollection);

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.WhsLoad;

		protected override bool ExpectedSupportsWorkflow => true;

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsLoad;

		#endregion

		public void TestWorkflowType()
		{
			using (var module = new LoadModule())
			{
				AssertEquals(WorkflowDescriptors.WhsLoadWorkflowDescriptorCode, module.WorkflowType);
			}
		}
	}
}
