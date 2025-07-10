using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class HAWBUserControl : AWBUserControl
	{
		public HAWBUserControl()
		{
			InitializeComponent();
			RemoveChargeCodeColumnIfNecessary();
			ShowShortDescriptionOverrideForFHL();

			OtherChargesGrid.AllowOverlap(AWBPanel);

			EH_HouseInsuranceValueCurrencyTextBox.AllowOverlap(AWBImagelabel);
			EH_HouseCustomsValueCurrencyTextBox.AllowOverlap(AWBImagelabel);
			EH_HouseDeclaredValueCurrencyTextBox.AllowOverlap(AWBImagelabel);
		}

		#region Column Management

		void RemoveChargeCodeColumnIfNecessary()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				foreach (ZGridColumnInfo info in OtherChargesGrid.ColumnStyles.ToArray())
				{
					if (info.ColumnName == ExportAWBOtherChargesSchema.Constants.EO_ChargeCode && !Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen)
					{
						OtherChargesGrid.ColumnStyles.Remove(info);
						break;
					}
				}
			}
		}

		#endregion

		#region Short Description Override (FHL)

		void ShowShortDescriptionOverrideForFHL()
		{
			if (!DesignModeFinder.IsDesigning && !Env.Registry.Freight.AirWaybill.AllowShortGoodsDescriptionOverrideforFHL)
			{
				this.EH_ManifestDescriptionOfGoodsTextBox.Visible = false;
				this.EH_ManifestDescriptionOfGoodsLabel.Visible = false;
			}
		}

		#endregion
	}
}
