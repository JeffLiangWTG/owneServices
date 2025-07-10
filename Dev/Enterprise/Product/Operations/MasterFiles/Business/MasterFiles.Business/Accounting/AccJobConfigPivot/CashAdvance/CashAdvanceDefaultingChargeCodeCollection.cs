using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CashAdvanceDefaultingChargeCodeCollection : DependentBusinessObjectCollection<CashAdvanceDefaultingChargeCode, AccCashAdvanceDefaultingConfiguration>
	{
		public CashAdvanceDefaultingChargeCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CashAdvanceDefaultingChargeCodeCollection(AccCashAdvanceDefaultingConfiguration parent) : base(parent)
		{
			Parent = parent;
		}

		readonly AccCashAdvanceDefaultingConfiguration Parent;

		protected override string FkColumnName => AccJobConfigPivotSchema.JCT_JCF_JobConfig.Name;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CashAdvanceDefaultingChargeCode)child).JCT_JCF_JobConfig = Parent.PK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccJobConfigPivotSchema.JCT_ParentId, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			return query;
		}

		public bool MatchingChargeCodeExist(ZGuid chargeCodePk)
		{
			return this.Cast<CashAdvanceDefaultingChargeCode>().Any(x => x.JCT_ParentId == chargeCodePk);
		}
	}
}
