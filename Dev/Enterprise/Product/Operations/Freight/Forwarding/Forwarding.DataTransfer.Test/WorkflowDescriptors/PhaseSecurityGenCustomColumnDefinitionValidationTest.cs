using System.Collections;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class PhaseSecurityGenCustomColumnDefinitionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXC_Name()
		{
			var dummyWorkflowDescriptor = new DummyWorkflowDescriptorWithPhaseValidation();
			var workflowDescriptors = new Hashtable
				{
					{ dummyWorkflowDescriptor.Code, new TestObjectHandle(dummyWorkflowDescriptor) }
				};

			using (ObjectFactory.Substitute("WorkflowDescriptors", workflowDescriptors))
			{
				string expectedWarning = string.Format(@"There is a Custom Field definition in other '{0}' Workflow Template with same Name which may create confusion with phase control.",
					dummyWorkflowDescriptor.Code);

				var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template1.P0_ProcessType = dummyWorkflowDescriptor.Code;
				var def1 = template1.GenCustomColumnDefinitions.AddNew();
				def1.XC_Name = "AAA";
				def1.XC_Type = "STR";

				Factory.Save();

				var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template2.P0_ProcessType = dummyWorkflowDescriptor.Code;
				var def2 = template2.GenCustomColumnDefinitions.AddNew();
				def2.XC_Name = "AAA";
				def2.XC_Type = "STR";

				AssertHasWarning(def2.XC_NameInfo, expectedWarning);

				def2.XC_Type = "INT";

				AssertHasWarning(def2.XC_NameInfo, expectedWarning);

				template2.P0_ProcessType = "CON";
				def2.XC_Type = "INT";

				AssertNoWarning(def2.XC_NameInfo, expectedWarning);
			}
		}

		#region Implementation

		class DummyWorkflowDescriptorWithPhaseValidation : DummyWorkflowDescriptor
		{
			public override string Code
			{
				get { return "ZZZ"; }
			}

			protected override AutoGenCustomColumnDefinitionValidation GetAdditionalCustomColumnDefinitionValidation(GenCustomColumnDefinition customColumnDefinition)
			{
				return new PhaseSecurityGenCustomColumnDefinitionValidation(customColumnDefinition);
			}
		}

		#endregion
	}
}
