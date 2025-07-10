using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public class ConsolidatedDeclarationFormAdaptationsProvider : IConsolidatedDeclarationFormAdaptationsProvider
	{
		public IPanelLayoutProvider HeaderDetailsLayout => new ConsolidatedDeclarationLayoutProvider();

		public IEnumerable<ZGridColumnInfo> DeclarationGridExtraColumnInfos
		{
			get
			{
				ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
				ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
				zCalcEditColumnStyleInfo1.ColumnName = "NoOfInvoices";
				zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7BF423EA-9E84-4A2D-A0C7-8FD5BFDF863E", "No of Invoices");
				zCalcEditColumnStyleInfo2.ColumnName = "CusEntryHeader+VFDWholeNZD";
				zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("98074914-2E19-49D3-86DD-F6751EFCD451", "VFD");
				yield return zCalcEditColumnStyleInfo1;
				yield return zCalcEditColumnStyleInfo2;
			}
		}

		public ZUserControl MessagesTabUserControl => new CustomsMessagesTabUserControl();

		public IConsolidatedDeclarationMenuBuilder EDIMenuBuilder => new NZEDIMenu();

		public bool EnableDocumentMenuItem => true;
	}
}
