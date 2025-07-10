using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(RowEntryForm))]
	class RowEntryFormBasherTest : ZFormBasherTest
	{
		#region TestRowPathSequenceIsValidatedWhenFormIsShown

		public void TestRowPathSequenceIsValidatedWhenFormIsShown()
		{
			var otherFactory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctionsEnv(otherFactory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);
			row.WR_PickPathSequence = 0; // Sequence will default to 1 because it is the first unused sequence.
			otherFactory.Save();

			var reloadedRow = Factory.Load<WhsRow>(row.PK);
			using (var form = new RowEntryForm(reloadedRow))
			{
				form.Show();
				AssertHasWarning(reloadedRow.WR_PickPathSequenceInfo, "A value of Zero indicates this Row will be *Last* in the Pick Sequence.");
			}
		}

		#endregion

		#region TestLabelTipsContent

		public void TestLabelTipsContent_ProductWarehouse()
		{
			TestLabelTipsContentCore(WarehouseTypes.Codes.Product, TipsForProductWarehouse);
		}

		public void TestLabelTipsContent_ContainerYardWarehouse()
		{
			TestLabelTipsContentCore(WarehouseTypes.Codes.ContainerYard, TipsForContainerYardWarehouse);
		}

		void TestLabelTipsContentCore(string warehouseType, string expectedContent)
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);
			Factory.Save();

			using (var rowForm = new RowEntryForm(row))
			{
				rowForm.Show();

				var labelTips = rowForm.Controls.Find("labelTips", searchAllChildren: true).Single();
				AssertEquals(expectedContent, labelTips.Text);
			}
		}

		public void TestLabelTipsContent_StartFromNewRow_ChangeWarehoue()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var prwWarehouse = helper.CreateWarehouse("W1");
			AssertEquals("Pre-condition: Warehoue Type default to be Product", WarehouseTypes.Codes.Product, prwWarehouse.WW_WarehouseType);

			var cydWarehouse = helper.CreateWarehouse("W2");
			cydWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			Factory.Save();

			var row = Factory.New<WhsRow>();
			using (var rowForm = new RowEntryForm(row))
			{
				rowForm.Show();

				var labelTips = rowForm.Controls.Find("labelTips", searchAllChildren: true).Single();
				AssertEquals(TipsForProductWarehouse, labelTips.Text);

				row.WR_WW_Whs = cydWarehouse.PK;
				AssertEquals(TipsForContainerYardWarehouse, labelTips.Text);

				row.WR_WW_Whs = prwWarehouse.PK;
				AssertEquals(TipsForProductWarehouse, labelTips.Text);
			}
		}

		const string TipsForProductWarehouse = @"Tip: To define a single bulk location, enter the Location Identifier as the Row Name and specify 1 (one) for Columns, Levels and Trays.

Tip: To define a horizontally partitioned floor location, enter the Location Identifier as the Row Name and use Columns and Levels to specify the number of X,Y partitions.

Tip: To define a Rack of locations, enter the Rack (Row) Name and specify the number of vertical Columns and horizontal Levels. Use Trays to specify partitions within a location.";

		const string TipsForContainerYardWarehouse = @"Tip: To define a single bulk location, enter the Location Identifier as the Row Name and specify 1 (one) for Columns and Levels.";

		#endregion

		#region TestHandleSaveException_ShowsFriendlyMessageWhenHasDuplicateLocationString

		public void TestHandleSaveException_ShowsFriendlyMessageWhenHasDuplicateLocationString()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateFixedWidthLocationWarehouse("MWH", 1, 1, 1);
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A555", 1, 1, 1);
			Factory.Save();

			var rowWithDuplicateLocationString = helper.CreateRowAndGenerateLocations(warehouse, "A", 5, 5, 5);
			foreach (var whsLocation in rowWithDuplicateLocationString.Locations)
			{
				whsLocation.FillWithValidTestData();
				whsLocation.WLV_PickPathSequence = 1;
			}

			using (var form = new RowEntryForm(rowWithDuplicateLocationString))
			using (WarehouseDataRegistry.Instance.EnableRowNamePrefixValidationForFixedWidthWarehouses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				form.ValidatingForSave += delegate
				{
					var factory = form.BusinessEntity.Factory;
					var connection = ((IDbConnected)factory).Connection;
					throw new ZSaveException(new ZDataException(new ArgumentException(WhsRow.DuplicateLocationStringError), ((INeedRow)form.BusinessEntity).Row, connection), factory);
				};

				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();

				Assert("Excepted MessageType.Error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(WhsRow.LocationStringUniqueIndexFailureMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RowEntryForm(Factory.New<WhsRow>());
		}

		#endregion
	}
}
