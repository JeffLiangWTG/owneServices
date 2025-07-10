using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public class CartonSizesGridTest : WhsGuiTestCaseWithFactory
	{
		#region TestNewForm

		public void TestNewForm()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "G1");
			Factory.Save();

			using (var form = new ZForm(cartonGroup))
			{
				var grid = new CartonSizesGrid();

				// sets up binding source and members
				grid.ModuleID = ModuleIDs.WhsCartonSize;
				grid.BindToFindBoxList = "Lookups.CartonSizes";

				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(WhsCartonGroup);
				bindingSourceProvider.BindingSource.SetBindingMember(grid, nameof(WhsCartonGroup.CartonGroupSizeLinks));

				// grid must have at least one column
				var calcEditColumn = new ZCalcEditColumnStyleInfo();
				calcEditColumn.ColumnName = nameof(WhsCartonGroupSizeLink.WCV_OptimizationCost);
				grid.ColumnStyles.Add(calcEditColumn);

				// exception is thrown in binding if 'Attach' or 'Detach' is visible and no BindToList has been set
				form.Controls.Add(grid);

				form.Show();

				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(grid.Controls, "toolStrip");
				var newButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).FirstOrDefault();

				newButton.PerformClick();
				using (var sizeForm = grid.LastShownZForm)
				{
					AssertNotNull(sizeForm);
					AssertType<WhsCartonSizeEntryForm>(sizeForm);

					var newSize = (WhsCartonSize)sizeForm.BusinessEntityForPersistingForm;
					AssertEquals("Should be a new carton size.", false, newSize.IsInDatabase);
					AssertNotEquals(Factory, newSize.Factory);

					var newLinks = newSize.Factory.Load<WhsCartonGroupSizeLink>(new ZQuery(WhsCartonGroupSizeLinkSchema.WCV_WCS, newSize.PK));
					var newLink = newLinks.Single();
					AssertNotNull(newLink);
					AssertEquals(cartonGroup.PK, newLink.WCV_WCG);
				}
			}
		}

		#endregion

		#region TestEditForm

		public void TestEditForm()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "G1");
			var cartonSize = Helper.CreateWhsCartonSize("1");
			cartonGroup.CartonSizes.Add(cartonSize);
			Factory.Save();

			using (var form = new ZForm(cartonGroup))
			{
				var grid = new CartonSizesGrid();

				// sets up binding source and members
				grid.ModuleID = ModuleIDs.WhsCartonSize;
				grid.BindToFindBoxList = "Lookups.CartonSizes";

				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(WhsCartonGroup);
				bindingSourceProvider.BindingSource.SetBindingMember(grid, nameof(WhsCartonGroup.CartonGroupSizeLinks));

				// grid must have at least one column
				var calcEditColumn = new ZCalcEditColumnStyleInfo();
				calcEditColumn.ColumnName = nameof(WhsCartonGroupSizeLink.WCV_OptimizationCost);
				grid.ColumnStyles.Add(calcEditColumn);

				// exception is thrown in binding if 'Attach' or 'Detach' is visible and no BindToList has been set
				form.Controls.Add(grid);

				form.Show();

				var toolStrip = GUITestHelper.FindControl<ZToolStrip>(grid.Controls, "toolStrip");
				var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).FirstOrDefault();

				editButton.PerformClick();
				using (var sizeForm = grid.LastShownZForm)
				{
					AssertNotNull(sizeForm);
					AssertType<WhsCartonSizeEntryForm>(sizeForm);

					var sizeOnForm = (WhsCartonSize)sizeForm.BusinessEntityForPersistingForm;
					AssertNotEquals(Factory, sizeOnForm.Factory);
					AssertEquals(cartonSize.PK, sizeOnForm.PK);
				}
			}
		}

		#endregion

		#region TestAttach

		public void TestAttach()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "G1");
			var cartonSize = Helper.CreateWhsCartonSize("1");
			Factory.Save();

			using (var form = new ZForm(cartonGroup))
			{
				var grid = new CartonSizesGrid();

				// sets up binding source and members
				grid.ModuleID = ModuleIDs.WhsCartonSize;
				grid.BindToFindBoxList = "Lookups.CartonSizes";

				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(WhsCartonGroup);
				bindingSourceProvider.BindingSource.SetBindingMember(grid, nameof(WhsCartonGroup.CartonGroupSizeLinks));

				// grid must have at least one column
				var calcEditColumn = new ZCalcEditColumnStyleInfo();
				calcEditColumn.ColumnName = nameof(WhsCartonGroupSizeLink.WCV_OptimizationCost);
				grid.ColumnStyles.Add(calcEditColumn);

				// exception is thrown in binding if 'Attach' or 'Detach' is visible and no BindToList has been set
				form.Controls.Add(grid);

				form.Show();
				AssertEquals("Precondition.", 0, cartonGroup.CartonSizes.Count);

				grid.AttachButtonForTest.PerformClick();
				using (var popup = grid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { cartonSize });
				}

				AssertEquals("Should have added carton size.", 1, cartonGroup.CartonSizes.Count);
				AssertEquals("Should have added carton size.", 1, cartonGroup.CartonGroupSizeLinks.Count);
				AssertEquals("Should have added carton size.", true, cartonGroup.CartonSizes.Contains(cartonSize));

				grid.AttachButtonForTest.PerformClick();
				using (var popup = grid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { cartonSize });
				}

				AssertEquals("Should *not* have added another carton size.", 1, cartonGroup.CartonSizes.Count);
				AssertEquals("Should *not* have added another carton size.", 1, cartonGroup.CartonGroupSizeLinks.Count);
				AssertEquals("Should *not* have added another carton size.", true, cartonGroup.CartonSizes.Contains(cartonSize));
				AssertContains("This record has already been selected. Please ensure you select only records that have not already been used.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestDetach

		public void TestDetach()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("G1", "G1");
			var cartonSize = Helper.CreateWhsCartonSize("1");
			cartonGroup.CartonSizes.Add(cartonSize);
			Factory.Save();

			using (var form = new ZForm(cartonGroup))
			{
				var grid = new CartonSizesGrid();

				// sets up binding source and members
				grid.ModuleID = ModuleIDs.WhsCartonSize;
				grid.BindToFindBoxList = "Lookups.CartonSizes";

				ICompositeControlBindingSourceProvider bindingSourceProvider = form;
				bindingSourceProvider.BindingSource.DataSourceType = typeof(WhsCartonGroup);
				bindingSourceProvider.BindingSource.SetBindingMember(grid, nameof(WhsCartonGroup.CartonGroupSizeLinks));

				// grid must have at least one column
				var calcEditColumn = new ZCalcEditColumnStyleInfo();
				calcEditColumn.ColumnName = nameof(WhsCartonGroupSizeLink.WCV_OptimizationCost);
				grid.ColumnStyles.Add(calcEditColumn);

				// exception is thrown in binding if 'Attach' or 'Detach' is visible and no BindToList has been set
				form.Controls.Add(grid);

				form.Show();
				AssertEquals("Precondition.", 1, cartonGroup.CartonSizes.Count);
				var link = cartonGroup.CartonGroupSizeLinks.Single();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				grid.InnerGrid.SelectAllElements();
				grid.DetachButtonForTest.PerformClick();
				AssertContains(@"Are you sure you want to detach the selected records? New records will be deleted.
All unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Should have removed carton size.", 0, cartonGroup.CartonSizes.Count);
				AssertEquals("Should have removed carton size.", false, cartonGroup.CartonSizes.Contains(cartonSize));
				AssertEquals("Should have removed carton size.", true, link.IsDeleted);
			}
		}

		#endregion
	}

	[TestedType(typeof(CartonSizesGrid))]
	class CartonSizesGridZModuleButtonGridTest : ZModuleButtonGridTestBase
	{
	}
}
