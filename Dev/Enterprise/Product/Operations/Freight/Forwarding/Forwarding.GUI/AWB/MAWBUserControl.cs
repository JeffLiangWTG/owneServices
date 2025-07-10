using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class MAWBUserControl : AWBUserControl
	{
		public MAWBUserControl()
		{
			InitializeComponent();
			EH_KnownConsignorCodeTextBox.Visible = IsSupplyChainSecuritySG_Enabled;
		}

		#region IsSupplyChainSecuritySG_Enabled

		bool IsSupplyChainSecuritySG_Enabled
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore
					&& FreightDataRegistry.Instance.EnableSupplyChainSecurity_SG.Value;
			}
		}

		#endregion

		#region GetNewNatureAndQtyOfGoodsControl()

		protected override NatureAndQtyOfGoodsControl GetNewNatureAndQtyOfGoodsControl()
		{
			return new NatureAndQtyOfGoodsWithTypeControl();
		}

		#endregion
	}
}
