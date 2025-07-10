using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CashAdvanceDefaultingChargeGroupCollection : DependentBusinessObjectCollection<CashAdvanceDefaultingChargeGroup, AccCashAdvanceDefaultingConfiguration>
	{
		public CashAdvanceDefaultingChargeGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CashAdvanceDefaultingChargeGroupCollection(AccCashAdvanceDefaultingConfiguration parent) : base(parent)
		{
			Parent = parent;
		}

		readonly AccCashAdvanceDefaultingConfiguration Parent;

		protected override string FkColumnName => AccJobConfigPivotSchema.JCT_JCF_JobConfig.Name;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CashAdvanceDefaultingChargeGroup)child).JCT_JCF_JobConfig = Parent.PK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccJobConfigPivotSchema.JCT_ParentId, DBNull.Value);
			return query;
		}

		public bool MatchingChargeGroupExist(ZString chargeGroup)
		{
			return this.Cast<CashAdvanceDefaultingChargeGroup>().Any(x => x.JCT_Code == chargeGroup);
		}
	}
}
