using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class InvoiceLineUserControlHelperTest : TestCaseWithFactory
	{
		public void TestColumnNamesInSortOrder()
		{
			var columnNameList = new List<string>(new string[]
				{
					JobComInvoiceLine.Schema.JI_LineNo,
					JobComInvoiceLine.Schema.JI_Calc_Invoice,
					JobComInvoiceLine.Schema.JI_CEI,
					JobComInvoiceLine.Schema.JI_PartNo,
					JobComInvoiceLine.Schema.JI_FormattedTariff,
					JobComInvoiceLine.Schema.JI_Description,
					JobComInvoiceLine.Schema.JI_FormattedProcedure,
					JobComInvoiceLine.Schema.JI_InvoiceQuantity,
					JobComInvoiceLine.Schema.JI_CountryOfOrigin,
					JobComInvoiceLine.Schema.JI_CustomsQuantity,
					JobComInvoiceLine.Schema.JI_LinePrice,
					JobComInvoiceLine.Schema.JI_SupplementaryCode1,
					JobComInvoiceLine.Schema.JI_SupplementaryCode2,
					JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					JobComInvoiceLine.Schema.MergedLineNumber
				});

			AssertEquals("Should not contain JI_NDescription,", false, columnNameList.Contains(JobComInvoiceLine.Schema.JI_NDescription));
			var expectedColounList = InvoiceLineUserControlHelper.ColumnNamesInSortOrder(columnNameList);
			Assert("Should contains JI_NDescription", expectedColounList.Contains(JobComInvoiceLine.Schema.JI_NDescription));
		}

		public void TestInitializeContainerGridLayout()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			declaration.InvoiceLines.AddNew();

			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var grid = userControl.ContainersTabPage.FindSingle<ZGrid>("CusContainerInvoiceLineGrid");
					InvoiceLineUserControlHelper.InitializeContainerGridLayout(grid);
					AssertInitializeContainerGridLayout(grid);
				}
			}
		}

		public static void AssertInitializeContainerGridLayout(ZGrid containerGrid)
		{
			var columnStyles = containerGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>();
			var columnStyleInfo = containerGrid.GetColumnStyle(NonPersistentCusContainer.Schema.OwnerCountry);
			AssertEquals("Container Country", columnStyleInfo.Caption);
			AssertEquals(120, columnStyleInfo.Width);
			columnStyleInfo = containerGrid.GetColumnStyle(NonPersistentCusContainer.Schema.ContainerNumber);
			AssertEquals("Container No", columnStyleInfo.Caption);
			columnStyleInfo = containerGrid.GetColumnStyle(NonPersistentCusContainer.Schema.IsForInvoiceLine);
			AssertEquals(120, columnStyleInfo.Width);

			var visibleColumns = new[] { NonPersistentCusContainer.Schema.ContainerNumber, NonPersistentCusContainer.Schema.IsForInvoiceLine, NonPersistentCusContainer.Schema.OwnerCountry };
			foreach (var columnStyle in columnStyles)
			{
				AssertEquals(visibleColumns.Contains(columnStyle.ColumnName), columnStyle.IsVisible);
			}
		}
	}
}
