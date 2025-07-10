using System;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class TopLevelPacksDetailsControl : ZUserControl
	{
		public TopLevelPacksDetailsControl()
		{
			InitializeComponent();
			TariffFindHelper.AddDefaultPropertyToTariffControl(harmonisedCodeFindBox, null);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var currentTopLevelPack = CurrentDataItem as AgencyShipmentContainer;
			if (currentTopLevelPack != null)
			{
				if (currentTopLevelPack.IsRollOnRollOff)
				{
					ReferenceNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("379ae964-1b36-4ae0-9809-ec682b00399b", "VIN / Serial");
				}
				else
				{
					ReferenceNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("54fb0b82-91b1-4020-95a1-9143d5ce693a", "Reference Number");
				}
			}
		}
	}
}


