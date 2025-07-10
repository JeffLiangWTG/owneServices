using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusContainerInvoiceLinePivot : Customs.Business.CusContainerInvoiceLinePivot, Integration.Customs.US.ICusContainerInvoiceLinePivot
	{
		public CusContainerInvoiceLinePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related BO's
		public new CusContainer Container
		{
			get { return base.Container as CusContainer; }
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		#endregion

		public override void Delete()
		{
			DeleteRelatedFDAPivots();
			DeleteRelatedPGAPivots();
			base.Delete();
		}

		void DeleteRelatedFDAPivots()
		{
			var query = GetGenPivotQuery(FDARelatedContainersGenPivot.RelationType);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			Factory.Load<FDARelatedContainersGenPivot>(query).DeleteAll();
		}

		void DeleteRelatedPGAPivots()
		{
			var query = GetGenPivotQuery(PGARelatedContainersGenPivot.RelationType);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			Factory.Load<PGARelatedContainersGenPivot>(query).DeleteAll();
		}

		protected ZQuery GetGenPivotQuery(ZString relationType)
		{
			var query = new ZQuery(GenPivotSchema.XX_Relation2ID, PK);
			query.AddToFilter(GenPivotSchema.XX_RelationType, relationType);
			return query;
		}

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusContainerInvoiceLinePivot businessObject)
				: base(businessObject)
			{
				this.cusContainerInvoiceLinePivot = businessObject;
			}

			readonly CusContainerInvoiceLinePivot cusContainerInvoiceLinePivot;

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();

				Factory.AddFetchHint(GenPivotSchema.Instance, cusContainerInvoiceLinePivot.GetGenPivotQuery(FDARelatedContainersGenPivot.RelationType));
				Factory.AddFetchHint(GenPivotSchema.Instance, cusContainerInvoiceLinePivot.GetGenPivotQuery(PGARelatedContainersGenPivot.RelationType));
			}
		}

		#endregion
	}
}
