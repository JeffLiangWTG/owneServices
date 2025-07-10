using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.GUI
{
	partial class MessagesTabUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnEntryStatus = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnEntryStatusDesc = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnHeldUntil = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();

			// 
			// MessagesGrid
			// 
			zTextBoxColumnEntryStatus.CaptionResourceString = Res.GetData("5F71D664-84D8-432B-9384-32BDF9D67E89", "Entry Status");
			zTextBoxColumnEntryStatus.ColumnName = "EntryStatus";
			zTextBoxColumnEntryStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnEntryStatusDesc.CaptionResourceString = Res.GetData("FFC28AB8-C9A9-4B84-9D65-8DC2DD7AAC20", "Entry Status Desc.");
			zTextBoxColumnEntryStatusDesc.ColumnName = "EntryStatusDescription";
			zTextBoxColumnEntryStatusDesc.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnHeldUntil.CaptionResourceString = Res.GetData("46ABCF3B-CC84-41E3-8771-73E2C7A070D4", "Held Until Date");
			zDateEditColumnHeldUntil.ColumnName = "EM_HeldUntilDate";
			zDateEditColumnHeldUntil.IsReadOnly = true;
			zDateEditColumnHeldUntil.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnEntryStatus);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnEntryStatusDesc);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnHeldUntil);
			this.MessagesGrid.ReOrderColumns(MessagesGridSortOrder);

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		}

		string[] MessagesGridSortOrder
		{
			get
			{
				if (messagesGridSortOrder == null)
				{
					messagesGridSortOrder = new[]
					{
						"EM_MessageNum",
						"EM_MessageType",
						"EM_MessageSubType",
						"EM_MessageDateTime",
						"EM_SystemCreateTimeUtc",
						"EM_InterchangeNumber",
						"EM_DateTimeInterchangeSent",
						"EM_User",
						"EM_Status",
						"EM_ReceiveTransmit",
						nameof(NLEDIMessage.EntryStatus),
						nameof(NLEDIMessage.EntryStatusDescription),
						"EM_HeldUntilDate",
						"EM_IsTestMessage"
					};
				}
				return messagesGridSortOrder;
			}
		}
		string[] messagesGridSortOrder;

		#endregion
	}
}
