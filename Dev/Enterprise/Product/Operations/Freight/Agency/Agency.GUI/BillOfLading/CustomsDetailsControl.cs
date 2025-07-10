using System;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class CustomsDetailsControl : ZUserControl
	{
		public CustomsDetailsControl()
		{
			InitializeComponent();
		}

		void DetailsButton_Click(object sender, EventArgs e)
		{
			ZString customsInfo = ((BillOfLading)CurrentDataItem).UserFriendlyStatusMessage;
			string caption = Res.GetString("c7f53afb-5cd5-4b7e-9b9a-0dbf184b924d", "Customs Information");

			if (customsInfo.IsEmpty)
			{
				customsInfo = Res.GetString("0a670148-afa8-4d6f-bc19-9a365bfe9bfd", "No additional information is available.");
			}

			Globals.Message.ShowInformation(customsInfo, caption);
		}
	}
}


