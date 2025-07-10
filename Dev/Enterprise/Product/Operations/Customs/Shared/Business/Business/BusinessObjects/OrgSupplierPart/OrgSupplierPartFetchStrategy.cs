using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	class OrgSupplierPartFetchStrategy : MasterFiles.Business.OrgSupplierPartFetchStrategy
	{
		public OrgSupplierPartFetchStrategy(OrgSupplierPart orgSupplierPart)
			: base(orgSupplierPart)
		{
		}

		protected new OrgSupplierPart BusinessObject
		{
			get { return (OrgSupplierPart)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(OrgSupplierPart.AllOwners):
						Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.OP_OH_FormLayoutController);
						break;
					case nameof(OrgSupplierPart.AllSuppliers):
						Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.OP_OH_FormLayoutController);
						break;
					case nameof(OrgSupplierPart.OP_StockKeepingUnitForPalletProxy):
						Factory.AddFetchHint(RefPackTypeSchema.F3_Code, BusinessObject.OP_StockKeepingUnit);
						break;
					case nameof(OrgSupplierPart.OP_StockKeepingUnitPerPallet):
						Factory.AddFetchHint(RefPackTypeSchema.F3_Code, BusinessObject.OP_StockKeepingUnit);
						Factory.AddFetchHint(RefPacksSchema.RP_OH_Supplier, BusinessObject.OP_OH_FormLayoutController);
						break;
				}
			}
		}
	}
}
