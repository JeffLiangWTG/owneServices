using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class PickupDetailsUserControl : ZUserControl
	{
		public PickupDetailsUserControl()
		{
			InitializeComponent();
			LCLDatesOverrideCheckBox.AllowOverlap(JP_LCLStorageCommencesDateEdit);
		}

		protected new BaseJobDeclaration CurrentDataItem => (BaseJobDeclaration)base.CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is BaseJobDeclaration jobDeclaration)
			{
				declarationValueChangedAnnouncer = ((IInvoicesProvider)jobDeclaration).GetValueChangedAnnouncer();
				if (declarationValueChangedAnnouncer != null)
				{
					declarationValueChangedAnnouncer.OnValueChanged -= JobDeclaration_ControlVisibilityChanged;
				}
			}
			base.SetDataBinding(dataSource, dataMember);
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += JobDeclaration_ControlVisibilityChanged;
			}
			JobDeclaration_ControlVisibilityChanged(null, null);
		}

		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
		{
			var jobDeclaration = CurrentDataItem;
			var isExport = jobDeclaration?.IsExport ?? ZBool.False;
			JP_LCLAirStorageDaysOrHoursBoundCalcEdit.Visible = isExport;
			JP_LCLAirStorageChargeBoundCalcEdit.Visible = isExport;
			if (jobDeclaration != null)
			{
				JP_LCLAirStorageChargeBoundCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("9256fac9-2b5e-4fa6-9fd6-5bcfd9bef80c", "Storage {0}", jobDeclaration.DocsAndCartage.JP_StorageTimeUnits);
			}
		}
	}
}
