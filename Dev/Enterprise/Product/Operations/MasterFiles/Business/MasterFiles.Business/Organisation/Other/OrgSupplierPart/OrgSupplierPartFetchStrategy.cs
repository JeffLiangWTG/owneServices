using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IBaseCusClassPartPivot = Enterprise.Integration.Customs.IBaseCusClassPartPivot;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgSupplierPartFetchStrategy(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			AddHints();
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			AddHints();
		}

		void AddHints()
		{
			if (UseDeepFetchHintForCusClassPartPivot)
			{
				Factory.AddFetchHint(ObjectFactory.GetType<IBaseCusClassPartPivot>(), CusClassPartPivotSchema.CI_OP, BusinessObject.PK);
			}
			else
			{
				Factory.AddFetchHint(CusClassPartPivotSchema.CI_OP, BusinessObject.PK);
			}

			ZQuery bomPart = new ZQuery(OrgPartBOMSchema.OE_OP_MainProduct, BusinessObject.PK);
			bomPart.AddToFilter(JoinCondition.Or, OrgPartBOMSchema.OE_OP_Component, BusinessObject.PK);

			Factory.AddFetchHint(OrgPartBOMSchema.Instance, bomPart);
			Factory.AddFetchHint(OrgPartRelationSchema.OU_OP, BusinessObject.PK);
			Factory.AddFetchHint(OrgPartUnitSchema.OF_OP, BusinessObject.PK);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(OrgSupplierPartBarcodeSchema.PH_OP, BusinessObject.PK);
			Factory.AddFetchHint(WhsPickFaceSchema.WF_OP, BusinessObject.PK);
		}

		protected ZQuery GetInventoryQuery()
		{
			var result = new ZQuery(WhsDocketLineSchema.WE_OP, BusinessObject.PK);
			result.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
			return result;
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == "AllOwners" || column.ColumnName == "AllSuppliers")
				{
					Factory.AddFetchHint(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP, BusinessObject.PK);
				}
			}
		}

		protected virtual bool UseDeepFetchHintForCusClassPartPivot
		{
			get { return false; }
		}
	}
}
