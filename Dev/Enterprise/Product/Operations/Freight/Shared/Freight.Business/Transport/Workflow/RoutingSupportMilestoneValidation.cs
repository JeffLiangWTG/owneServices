using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class RoutingSupportMilestoneValidation : MilestoneOrTriggerValidation
	{
		public RoutingSupportMilestoneValidation(RoutingSupportProcessTask parent)
			: base(parent)
		{
		}

		internal static void CheckReferenceCode(RoutingSupportProcessTask workflowItem)
		{
			if (!workflowItem.P9_ReferencedID.IsValid && !workflowItem.ReferenceCode.IsEmpty)
			{
				workflowItem.ReferenceCodeInfo.AddError(Res.GetString("869819d2-44fe-4748-9446-9969768e188d", "Enter a valid transport port pair"));
			}
			else if (workflowItem.ReferenceCode.IsEmpty)
			{
				var warning = GetEmptyLegWarning(workflowItem);

				if (!string.IsNullOrEmpty(warning))
				{
					workflowItem.ReferenceCodeInfo.AddWarning(warning);
				}
			}
		}

		static string GetEmptyLegWarning(RoutingSupportProcessTask workflowItem)
		{
			string milestoneType;

			if (workflowItem.P9_SE_NKMilestoneEvent == Events.Arrival.Code)
			{
				milestoneType = Res.GetString("ea001542-ba40-49b2-8828-99bf6776ac8f", "arrival");
			}
			else if (workflowItem.P9_SE_NKMilestoneEvent == Events.Departure.Code)
			{
				milestoneType = Res.GetString("25e69a8a-15ca-49fa-a4a7-2a93c58a0b21", "departure");
			}
			else
			{
				return string.Empty;
			}

			var warning = Res.GetString("996a13b6-670d-46fe-8b5c-020cad26edb2", "If leg is empty then the {0} milestone will not be met.", milestoneType);

			if (workflowItem.Parent is CommonConsol)
			{
				var consolDetails = Res.GetString("bdfdad0a-2c1f-4046-8aaf-834e21cf8057", "This may happen if the Consol first load port is different from the load ports of all transport legs.");
				warning += " " + consolDetails;
			}

			return warning;
		}

		protected override void CheckReferenceCode()
		{
			CheckReferenceCode(Parent);
		}

		new RoutingSupportProcessTask Parent
		{
			get { return (RoutingSupportProcessTask)base.Parent; }
		}
	}
}
