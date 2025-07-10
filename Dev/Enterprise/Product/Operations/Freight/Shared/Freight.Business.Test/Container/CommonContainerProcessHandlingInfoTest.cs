using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class CommonContainerProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestGetEventParametersToPropagate()
		{
			EnsureParametersAreNotIncludedForEvent(Events.Dehire, new[] { Params.Location, Params.Facility });
			EnsureParametersAreNotIncludedForEvent(Events.QuantityVerified, new[] { Params.Location, Params.Facility });
		}

		public void TestContainerDoesNotPropagate_ChangeOfIdentifierEvent()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var container = consol.Containers.AddNew();

			var workflowProvider = (IWorkflowProvider)consol;
			var cidEventTrigger = workflowProvider.WorkflowItems.AddNew();
			cidEventTrigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			cidEventTrigger.TriggerConditions.TriggerEventCode = Events.ChangeOfIdentifierCode;

			var logAdded = container.Logs.AddNew(Events.ChangeOfIdentifier,
				new KeyValuePair<string, string>(Params.Type, Constants.EventReferenceParameterTypes.HoldCode));
			AssertEquals("Container should not propagate change identifier event to its parent.", ZDateTime.Empty, cidEventTrigger.P9_ActualDate.ToZDateTime());
		}

		void EnsureParametersAreNotIncludedForEvent(Event evnt, string[] parameters)
		{
			var actualParameters = GetInstance().GetEventParametersToMatchDuringPropagation(evnt.Code);

			foreach (var param in parameters)
			{
				AssertCollectionNotContains(param, actualParameters);
			}
		}

		protected abstract CommonContainerProcessHandlingInfo GetInstance();
	}
}
