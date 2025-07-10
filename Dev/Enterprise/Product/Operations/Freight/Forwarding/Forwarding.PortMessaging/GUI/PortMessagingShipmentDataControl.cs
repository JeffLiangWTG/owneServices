using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	public partial class PortMessagingShipmentControl : ZUserControl
	{
		public PortMessagingShipmentControl()
		{
			InitializeComponent();

			Init();
		}

		protected PortMessagingManager Manager
		{
			get { return (PortMessagingManager)CurrentDataItem; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!PortMessagingHelper.IsEORIAndLRNEffectiveDate())
			{
				MRNTextBox.Visible = true;
				MRNCompleteCheckBox.Visible = true;
				LRNTextBox.Visible = false;
				LRNCompleteCheckBox.Visible = false;
			}
			else if (Manager?.PortMessaging != null)
			{
				SetPortMessagingControlsVisibility();
				Manager.PortMessaging.JSM_EntryTypeInfo.ValueChanged += JSM_EntryTypeValueChanged;
			}
		}

		void JSM_EntryTypeValueChanged(object sender, EventArgs e)
		{
			SetPortMessagingControlsVisibility();
		}

		void SetPortMessagingControlsVisibility()
		{
			var isAESType = Manager.PortMessaging.JSM_EntryType == EntryTypeList.Codes.AE1ExportDeclaration;

			MRNTextBox.Visible = !isAESType;
			MRNCompleteCheckBox.Visible = !isAESType;
			LRNTextBox.Visible = isAESType;
			LRNCompleteCheckBox.Visible = isAESType;
		}

		void Init()
		{
			new UNDGDataItemFormManager(PackLinesGrid, string.Empty, UNDGDataItemFormManagerConfig.IMOShowProperties())
			{
				ShouldHidePredicate = style => !ShouldShowColumn(style?.ColumnName)
			}.Initialize();

			var technicalNameStyle = PackLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(style => style.ColumnName == "UNDGs+UNDGTechnicalNameManager+Value");
			if (technicalNameStyle != null)
			{
				technicalNameStyle.IsReadOnly = true;
			}
		}

		static bool ShouldShowColumn(string columnName)
		{
			return columnName == "UNDGs+UNDGTechnicalNameManager+Value"
				|| columnName == "UNDGs+UNDGSubstanceManager+Value"
				|| columnName == "UNDGs+UNDGClassManager+Value";
		}
	}
}
