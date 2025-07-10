using System;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class InBondUserControl : ZUserControl
	{
		public InBondUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (!disposingControl)
			{
				var currentInBond = CurrentDataItem as InBond;
				if (currentInBond != inBond)
				{
					if (inBond != null)
					{
						inBond.BM_InBondEntryTypeInfo.ValueChanged -= OnInBondTypeValueChanged;
					}

					inBond = currentInBond;
					if (inBond != null)
					{
						inBond.BM_InBondEntryTypeInfo.ValueChanged += OnInBondTypeValueChanged;
					}

					OnInBondTypeValueChanged(null, EventArgs.Empty);
				}
			}
		}

		void OnInBondTypeValueChanged(object sender, EventArgs e)
		{
			ExportInBondDetailsGroupBox.Visible = inBond != null && inBond.IsExport;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				disposingControl = disposing;
				if (disposing && (components != null))
				{
					components.Dispose();
					if (inBond != null)
					{
						inBond.BM_InBondEntryTypeInfo.ValueChanged -= OnInBondTypeValueChanged;
					}
				}
				base.Dispose(disposing);
			}
			finally
			{
				disposingControl = false;
			}
		}

		bool disposingControl;
		InBond inBond;
	}
}
