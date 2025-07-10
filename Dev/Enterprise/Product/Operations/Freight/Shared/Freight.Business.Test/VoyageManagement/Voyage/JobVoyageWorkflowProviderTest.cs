using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobVoyage))]
	sealed class JobVoyageWorkflowProviderTest : WorkflowProviderTest<JobVoyage, JobVoyageProcessTaskCollection>
	{
		public void TestTemplateSelectionCriteria()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_OH_Line = carrier1.PK;

			IWorkflowProviderCore workflowProvider = voyage;

			Func<ColumnValueRanker> getRanker = () => (ColumnValueRanker)workflowProvider.GetTemplateSelectionCriteria();

			AssertContainsExactElementsInAnyOrder(new[] { carrier1.PK, ZGuid.Empty },
				getRanker().GetValues(ProcessTaskTemplateSchema.P0_OH_Client));

			voyage.JV_OH_Line = carrier2.PK;

			AssertContainsExactElementsInAnyOrder(new[] { carrier2.PK, ZGuid.Empty },
				getRanker().GetValues(ProcessTaskTemplateSchema.P0_OH_Client));

			AssertContainsExactElementsInAnyOrder(new[] { (ZString)Core.Constants.TransportModes.Sea, ZString.Empty },
				getRanker().GetValues(ProcessTaskTemplateSchema.P0_SubType1));

			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			AssertContainsExactElementsInAnyOrder(new[] { (ZString)Core.Constants.TransportModes.Air, ZString.Empty },
				getRanker().GetValues(ProcessTaskTemplateSchema.P0_SubType1));
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode; }
		}
	}
}
