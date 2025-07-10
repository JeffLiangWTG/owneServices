using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ACEFDAAddressesUserControl : ZUserControl
	{
		public ACEFDAAddressesUserControl()
		{
			InitializeComponent();
		}

		public new ACEFDAJobDocAddress CurrentDataItem => (ACEFDAJobDocAddress)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var fdaDocAddress = CurrentDataItem;
			if (fdaDocAddress != null)
			{
				fdaDocAddress.E2_AddressTypeInfo.ValueChanged -= E2_AddressTypeInfo_ValueChanged;
			}

			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var fdaDocAddress = CurrentDataItem;
			if (fdaDocAddress != null)
			{
				fdaDocAddress.E2_AddressTypeInfo.ValueChanged += E2_AddressTypeInfo_ValueChanged;
			}

			RefreshWhenDocAddressItemChanged();
		}

		void E2_AddressTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshWhenDocAddressItemChanged();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AllowOutsideOfParentExtensionMethods.AllowOutsideOfParent(this.DocAddressControl);
		}

		void RefreshWhenDocAddressItemChanged()
		{
			if (DocAddressGrid.ListManager.GetCurrent() is ACEFDAJobDocAddress docAddress)
			{
				DocAddressControl.Text = docAddress.AddressDescription;

				var fda = this.DataSource as ACEFDA;
				if (fda != null)
				{
					fda.DocAddressCurrentType = docAddress.E2_AddressType;
				}
			}
		}
	}
}
