using System.Collections;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	internal class HAWBUserControlTest : TransactionedTestCase
	{
		public void TestConstructor()
		{
			var defaultShowChargeCodeForOtherChargesInHawbScreen = Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen;
			var defaultAllowShortGoodsDescriptionOverrideforFHL = Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL;
			try
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;
				Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = true;

				using (var userControl = new HAWBUserControlForTest())
				{
					AssertNotNull("This should not be null if InitializeComponent is called", userControl.OtherChargesGrid);
					AssertEquals("None of the column styles are removed", 6, userControl.OtherChargesGrid.ColumnStyles.Count);
					Assert("This should show the controls for EH_ManifestDescriptionOfGoods", IsEH_ManifestDescriptionOfGoodsVisible(userControl));
				}

				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = false;
				Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = false;
				using (var userControl = new HAWBUserControlForTest())
				{
					AssertEquals("Charge Code column should be removed", 5, userControl.OtherChargesGrid.ColumnStyles.Count);
					AssertColumnStyleNotExist(userControl.OtherChargesGrid.ColumnStyles, ExportAWBOtherChargesSchema.Constants.EO_ChargeCode);
					Assert("This should NOT show the controls for EH_ManifestDescriptionOfGoods", !IsEH_ManifestDescriptionOfGoodsVisible(userControl));
				}
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = defaultShowChargeCodeForOtherChargesInHawbScreen;
				Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL = defaultAllowShortGoodsDescriptionOverrideforFHL;
			}
		}

		void AssertColumnStyleNotExist(ArrayList columnStyles, ZString columnName)
		{
			foreach (ZGridColumnInfo columnInfo in columnStyles)
			{
				if (columnInfo.ColumnName == columnName)
				{
					Fail("The column \"" + columnName + "\" should have been removed.");
				}
			}
		}

		bool IsEH_ManifestDescriptionOfGoodsVisible(HAWBUserControlForTest control)
		{
			return control.EH_ManifestDescriptionOfGoodsTextBox.Visible && control.EH_ManifestDescriptionOfGoodsLabel.Visible;
		}

		#region HAWBUserControlForTest

		class HAWBUserControlForTest : HAWBUserControl
		{
			public new ZGrid OtherChargesGrid
			{
				get { return base.OtherChargesGrid; }
			}

			public new ZTextBox EH_ManifestDescriptionOfGoodsTextBox
			{
				get { return base.EH_ManifestDescriptionOfGoodsTextBox; }
			}

			public new ZLabel EH_ManifestDescriptionOfGoodsLabel
			{
				get { return base.EH_ManifestDescriptionOfGoodsLabel; }
			}
		}

		#endregion
	}
}
