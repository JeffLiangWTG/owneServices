using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class CustomsCusContainersUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeContainersGridLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.ContainerTabPage;

				var userControl = form.CustomsBrokerageUserControl.ContainerUserControl;
				var columnStyles = userControl.CusContainersBoundGrid.InnerGrid.ColumnStyles.OfType<ZGridColumnInfo>();
				var columnStyleInfo = columnStyles.FirstOrDefault(x => x.ColumnName == Customs.Business.AutoCusContainer.Schema.CO_Seal);
				AssertEquals(false, columnStyleInfo.IsMandatory);

				columnStyleInfo = columnStyles.FirstOrDefault(x => x.ColumnName == Customs.Business.AutoCusContainer.Schema.CO_RN_NKOwnerCountry);
				AssertEquals(true, columnStyleInfo.IsMandatory);
				AssertEquals("Container Country", columnStyleInfo.Caption);
				AssertEquals(120, columnStyleInfo.Width);

				columnStyleInfo = columnStyles.FirstOrDefault(x => x.ColumnName == Customs.Business.AutoCusContainer.Schema.CO_ContainerNumber);
				AssertEquals("Container No", columnStyleInfo.Caption);

				var visibleColumns = new[] { Customs.Business.AutoCusContainer.Schema.CO_ContainerNumber, Customs.Business.AutoCusContainer.Schema.CO_RN_NKOwnerCountry };
				foreach (var columnStyle in columnStyles)
				{
					AssertEquals(visibleColumns.Contains(columnStyle.ColumnName), columnStyle.IsVisible);
				}
			}
		}
	}
}
