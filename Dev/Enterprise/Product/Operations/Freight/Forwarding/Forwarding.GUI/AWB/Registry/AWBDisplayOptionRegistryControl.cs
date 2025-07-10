using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class AWBDisplayOptionRegistryControl : RegistryZUserControl
	{
		public AWBDisplayOptionRegistryControl()
		{
			InitializeComponent();

			ShowAllButton.Click += delegate
			{ SetVisibility(AWBDisplayOptionEntitlement.Both, AWBDisplayOptionVisibility.Show); };
			HideAllButton.Click += delegate
			{ SetVisibility(AWBDisplayOptionEntitlement.Both, AWBDisplayOptionVisibility.Hide); };
			ShowAllCarrierButton.Click += delegate
			{ SetVisibility(AWBDisplayOptionEntitlement.C, AWBDisplayOptionVisibility.Show); };
			HideAllCarrierButton.Click += delegate
			{ SetVisibility(AWBDisplayOptionEntitlement.C, AWBDisplayOptionVisibility.Hide); };
			ShowAllAgentButton.Click += delegate
			{ SetVisibility(AWBDisplayOptionEntitlement.A, AWBDisplayOptionVisibility.Show); };
			HideAllAgentButton.Click += delegate
			{ SetVisibility(AWBDisplayOptionEntitlement.A, AWBDisplayOptionVisibility.Hide); };
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			AWBDisplayOptionsGrid.ReadOnly = readOnly;

			ShowAllButton.ReadOnly = readOnly;
			HideAllButton.ReadOnly = readOnly;
			ShowAllCarrierButton.ReadOnly = readOnly;
			HideAllCarrierButton.ReadOnly = readOnly;
			ShowAllAgentButton.ReadOnly = readOnly;
			HideAllAgentButton.ReadOnly = readOnly;
		}

		#region Implementation

		enum AWBDisplayOptionEntitlement
		{
			C = 0,
			A = 1,
			Both = 2,
		}

		void SetVisibility(AWBDisplayOptionEntitlement displayOptionEntitlement, AWBDisplayOptionVisibility displayOptionVisibility)
		{
			if (MasterCollection != null)
			{
				foreach (AWBDisplayOption option in MasterCollection)
				{
					bool changeOption = option.Entitlement == displayOptionEntitlement.ToString() || displayOptionEntitlement == AWBDisplayOptionEntitlement.Both;

					if (changeOption)
					{
						option.Visibility = displayOptionVisibility.ToString();
					}
				}
			}
		}

		AWBDisplayOptionCollection MasterCollection
		{
			get { return AWBDisplayOptionsGrid.DataSource as AWBDisplayOptionCollection; }
		}

		#endregion
	}
}
