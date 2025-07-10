using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class RevenueRecognitionByChargeGroupControl : ChargeGroupSettingControl
	{
		public RevenueRecognitionByChargeGroupControl()
		{
			AddNewColumns();
		}

		void AddNewColumns()
		{
			var broker = new ZDropEditColumnStyleInfo();
			broker.CharacterCasing = CharacterCasing.Upper;
			broker.CaptionResourceString = Res.GetData("RevenueRecognitionByChargeGroupControl|f17c099d-f112-4cba-8a67-5959892d2226", "Rec Description", "Rec Date Description", "Recognition Date Description", "");
			broker.ColumnName = "BrokerCode";
			ControlDpiScalingHelper.SetWidth(ref broker, 50, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(broker);

			var recognitionDateCode = new ZDropEditColumnStyleInfo();
			recognitionDateCode.CharacterCasing = CharacterCasing.Upper;
			recognitionDateCode.CaptionResourceString = Res.GetData("RevenueRecognitionByChargeGroupControl|2cdcc617-4972-46ec-bbb7-ab9dfc258ead", "Recognition Date Option");
			recognitionDateCode.ColumnName = "RecognitionDateOptionCode";
			ControlDpiScalingHelper.SetWidth(ref recognitionDateCode, 130, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(recognitionDateCode);

			var recognitionDateDescription = new ZTextBoxColumnStyleInfo();
			recognitionDateDescription.CaptionResourceString = Res.GetData("RevenueRecognitionByChargeGroupControl|f17c099d-f112-4cba-8a67-5959892d2226", "Rec Description", "Rec Date Description", "Recognition Date Description", "");
			recognitionDateDescription.ColumnName = "RecognitionDateOptionDescription";
			ControlDpiScalingHelper.SetWidth(ref recognitionDateDescription, 170, true);
			ChargeGroupSetupGrid.ColumnStyles.Add(recognitionDateDescription);
		}
	}
}
