using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityValueCollection : DependentBusinessObjectCollection<OrgOpportunityValue, OrgOpportunity>
	{
		public OrgOpportunityValueCollection(OrgOpportunity opportunity) : base(opportunity)
		{
		}

		#region Total Value

		internal ZDecimal TotalValue
		{
			get
			{
				ZDecimal result = 0m;
				foreach (OrgOpportunityValue valueItem in this)
				{
					result += valueItem.EstimatedValueUsedValue;
				}
				return result;
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			OrgOpportunityValue value = (OrgOpportunityValue)bizO;
			value.PV_Value = 0m;
			base.OnRemoving(value);
		}

		#endregion
	}
}
