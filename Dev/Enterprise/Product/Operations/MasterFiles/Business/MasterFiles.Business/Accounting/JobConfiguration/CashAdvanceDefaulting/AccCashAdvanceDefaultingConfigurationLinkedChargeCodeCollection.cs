using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection : AccChargeCodeCollection
	{
		readonly AccCashAdvanceDefaultingConfiguration Parent;

		public AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection(AccCashAdvanceDefaultingConfiguration accCashAdvanceDefaultingConfiguration)
			: base(accCashAdvanceDefaultingConfiguration.Factory)
		{
			Parent = accCashAdvanceDefaultingConfiguration;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccChargeCodeSchema.PK, Parent.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));
			return query;
		}
	}
}
