using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class CustomsCusContainersUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public CustomsCusContainersUserControl()
		{
			InitializeComponent();
			InitializeContainersGridLayout();
		}

		void InitializeContainersGridLayout()
		{
			var innerGrid = CusContainersBoundGrid.InnerGrid;
			var visibleColumns = new[] { CusContainer.Schema.CO_ContainerNumber, CusContainer.Schema.CO_RN_NKOwnerCountry };
			var invisibleColumns = innerGrid.ColumnStyles.OfType<Core.Forms.ZGridColumnInfo>().Select(c => c.ColumnName).Except(visibleColumns).ToArray();

			innerGrid.SetColumnMandatory(CusContainer.Schema.CO_Seal, false);
			innerGrid.SetColumnMandatory(CusContainer.Schema.CO_RN_NKOwnerCountry, true);
			innerGrid.SetColumnVisible(false, invisibleColumns);
			innerGrid.SetColumnVisible(true, visibleColumns);

			innerGrid.SetColumnCaption(CusContainer.Schema.CO_ContainerNumber, Res.GetString("DE3F4557-4DF2-458A-A5A0-C69CD6124F1C", "Container No"));
			innerGrid.SetColumnCaption(CusContainer.Schema.CO_RN_NKOwnerCountry, Res.GetString("C1C886FE-DEEA-48E0-BA84-C9325783088A", "Container Country"));

			innerGrid.SetColumnWidth(CusContainer.Schema.CO_RN_NKOwnerCountry, 120);
		}
	}
}
