using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DummyWithWorkflowAndEventDateProperty : DummyWithWorkflow
	{
		public DummyWithWorkflowAndEventDateProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[EventDateProperty(AutoEvents.IncidentClosedCode, EstimateActual.MilestoneEstimateOnly, true)]
		public override ZDateTime Z0_Date
		{
			get { return base.Z0_Date; }
			set
			{
				base.Z0_Date = value;
				Logs.CreateRecreateOrUpdateEventLog(AutoEvents.IncidentClosed, EstimateActual.Estimate, value.ToOffset());
			}
		}

		[EventDateProperty(AutoEvents.TagWasAddedOrRemovedCode, EstimateActual.Actual)]
		public override ZDateTime Z0_AnotherDate
		{
			get { return base.Z0_AnotherDate; }
			set
			{
				base.Z0_AnotherDate = value;
				Logs.CreateRecreateOrUpdateEventLog(AutoEvents.TagWasAddedOrRemoved, EstimateActual.Actual, value.ToOffset());
			}
		}

		[EventDateProperty(AutoEvents.TagWasAddedOrRemovedCode, EstimateActual.Estimate)]
		public override ZDateTime Z0_SmallDateTime
		{
			get { return base.Z0_SmallDateTime; }
			set
			{
				base.Z0_SmallDateTime = value;
				Logs.CreateRecreateOrUpdateEventLog(AutoEvents.TagWasAddedOrRemoved, EstimateActual.Estimate, value.ToOffset());
			}
		}
	}
}
