using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	class WhsPickFaceViewFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsPickFaceViewFetchStrategy(WhsPickFaceView pickFaceView)
			: base(pickFaceView)
		{
		}

		// Tested in PickFacesFilterControl.cs
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case "SupplierPart+" + OrgSupplierPartSchema.Constants.OP_Desc:
						Factory.AddFetchHint(OrgSupplierPartSchema.PK, PickFaceView.WPV_OP);
						break;

					default:
						break;
				}
			}
		}

		WhsPickFaceView PickFaceView => (WhsPickFaceView)BusinessObject;
	}
}
