using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentModule))]
	public class DtbConsignmentModuleTest : GlowOnlyModuleThatAllowsCopyTest<DtbConsignmentModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(DtbConsignmentFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(DtbConsignmentFilterControl);

		protected override Type ExpectedCollectionType => typeof(DtbConsignmentCollection);

		protected override bool ExpectedSupportsWorkflow => true;

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.LandTransport;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.DtbConsignment;

		#region TestWorkflowType

		public void TestWorkflowType()
		{
			using (var module = new DtbConsignmentModule())
			{
				AssertEquals(WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		#endregion

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbConsignment;
		}

		public void TestOperationalActionPlugin()
		{
			using (var module = (DtbConsignmentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(DtbConsignmentOperationalActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}
	}
}
