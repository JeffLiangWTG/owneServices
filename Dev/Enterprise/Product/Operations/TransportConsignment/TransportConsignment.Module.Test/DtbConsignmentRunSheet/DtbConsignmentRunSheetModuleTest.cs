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
	[TestedType(typeof(DtbConsignmentRunSheetModule))]
	public class DtbConsignmentRunSheetModuleTest : GlowOnlyModuleThatAllowsCopyTest<DtbConsignmentRunSheetModule>
	{
		protected override bool ExpectedAllowDelete => true;

		protected override Type ExpectedFilterBusinessObjectType => typeof(DtbConsignmentRunSheetFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(DtbConsignmentRunSheetFilterControl);

		protected override Type ExpectedCollectionType => typeof(DtbConsignmentRunSheetCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.LandTransport;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.DtbConsignmentRunSheet;

		protected override bool ExpectedSupportsWorkflow => true;

		public void TestOperationalActionPlugin()
		{
			using (var module = (DtbConsignmentRunSheetModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestWorkflowType()
		{
			using (var module = new DtbConsignmentRunSheetModule())
			{
				AssertEquals(WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(DtbConsignmentRunSheetOperationalActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DtbConsignmentRunSheet;
		}

		#endregion
	}
}
