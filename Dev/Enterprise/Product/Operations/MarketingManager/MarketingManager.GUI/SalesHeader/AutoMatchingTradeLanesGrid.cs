using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class AutoMatchingTradeLanesGrid : SalesHeaderCommonGrid
	{
		public AutoMatchingTradeLanesGrid()
			: base()
		{
			ResetCurrentRowIndexOverride();
		}

		[DpiState(DpiState.Unscaled)]
		int CurrentRowIndexOverride { get; set; }

		public void SetCurrentRowIndexOverride(ZGuid rowBusinessObjectPk)
		{
			if (List != null)
			{
				for (int i = 0; i < List.Count; i++)
				{
					if (((BusinessObject)List[i]).PK == rowBusinessObjectPk)
					{
						CurrentRowIndexOverride = i;
						break;
					}
				}
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool result = base.ProcessCmdKey(ref msg, keyData);

			if (HasCurrentRowIndexOverride && keyData == Keys.Tab)
			{
				CurrentRowIndex = CurrentRowIndexOverride;
				var firstOne = (DataSource as SalesHeader)?.FilterableEntitySalesCollection?.FirstOrDefault() as EntitySalesWrapper;

				if (firstOne != null)
				{
					var query = new ZQuery(OrgSalesSchema.OW_OH_Primary, firstOne.OW_OH_Primary);
					query.FetchOnlyFromLocalCache = true;
					var defaultEmptySaleCreatedByZGrid = firstOne.Factory.Load<OrgSales>(query).Where(x => !x.IsInDatabase && x.Origin == null && x.Destination == null).FirstOrDefault();
					defaultEmptySaleCreatedByZGrid?.Delete();
				}

				BeginEdit(TableStyles[0].GridColumnStyles[CurrentCell.ColumnNumber], CurrentRowIndex);
				ResetCurrentRowIndexOverride();
			}

			return result;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (HasCurrentRowIndexOverride)
			{
				ResetCurrentRowIndexOverride();
			}
		}

		bool HasCurrentRowIndexOverride
		{
			get { return CurrentRowIndexOverride != -1; }
		}

		void ResetCurrentRowIndexOverride()
		{
			CurrentRowIndexOverride = -1;
		}
	}
}
