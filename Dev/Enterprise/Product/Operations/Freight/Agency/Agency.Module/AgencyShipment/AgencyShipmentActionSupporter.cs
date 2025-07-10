using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal abstract class AgencyShipmentActionSupporter : OperationalActionSupporter
	{
		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}
	}
}
