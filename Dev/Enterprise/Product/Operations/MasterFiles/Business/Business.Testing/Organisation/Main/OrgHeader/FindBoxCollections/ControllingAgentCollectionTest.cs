using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ControllingAgentCollection))]
	sealed class ControllingAgentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgDefaults = new OrganisationDefaults();
			return new ControllingAgentCollection(Factory, orgDefaults);
		}

		public void TestValidateEntityOnSaving()
		{
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader organisation = ControllingAgents.AddNew();
			organisation.OH_IsControllingAgent = false;
			ControllingAgents.ValidateEntityOnSaving(organisation);
			Assert("Error - Controlling Agent is not selected", organisation.OH_IsControllingAgentInfo.HasErrors());

			organisation.OH_IsControllingAgent = true;
			ControllingAgents.ValidateEntityOnSaving(organisation);
			Assert("No error - Controlling Agent is selected", !organisation.OH_IsControllingAgentInfo.HasErrors());

			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			organisation.OH_IsControllingAgent = false;
			ControllingAgents.ValidateEntityOnSaving(organisation);
			Assert("No error - Controlling Agent is not selected", !organisation.OH_IsControllingAgentInfo.HasErrors());
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(ControllingAgents.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property12"));
		}

		#region Implementation

		ControllingAgentCollection ControllingAgents;

		protected override void SetUp()
		{
			base.SetUp();
			ControllingAgents = new ControllingAgentCollection(Factory);
		}

		#endregion
	}
}
