using CargoWise.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class QueryInterchangeCreator
	{
		public QueryInterchangeCreator(ZGrid grid)
		{
			this.grid = Argument.NotNull(grid, nameof(grid));
		}

		readonly ZGrid grid;

		public void AddColumnAndMenuForQuery(bool columnVisible = false)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var columnStyleInfo = new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("772911DC-BEF6-4FEE-B609-EB8891445A4E", "eHub Tracking ID"),
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = "Interchange.eHubID",
					Width = 230,
					IsVisible = columnVisible
				};

				grid.ColumnStyles.Add(columnStyleInfo);

				if (GlbStaff.CurrentUser.IsSupportUser || CustomsDataRegistry.Instance.EnableQueryInterchangeByEHubPortalWebservice.Value)
				{
					var menuItem = new ZMenuItem(ResString.GetMultilingualString("f55dca43-c5aa-458c-8b00-340fb12919c6", "Query Interchange On eHub"), (s, a) =>
					{
						var message = grid.GetCurrent() as EDIMessage;
						var helper = new InterchangeQuerier(message);
						helper.Query();
					});

					grid.ContextMenu.MenuItems.Add(menuItem);
				}
			}
		}
	}
}
