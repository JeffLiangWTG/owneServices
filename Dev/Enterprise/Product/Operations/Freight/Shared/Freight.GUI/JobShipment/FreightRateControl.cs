using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using SpotRateType = Enterprise.Freight.Business.FreightConstants.SpotRateType;

namespace Enterprise.Freight.GUI
{
	public partial class FreightRateControl : ZUserControl, IExtendedControl
	{
		public FreightRateControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
			SetBindingAndLayoutBasedOnRateAndObjectType();
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(SpotRateType.ShipmentSellRate)]
		public SpotRateType RateType
		{
			get { return rateType; }
			set
			{
				if (rateType != value)
				{
					rateType = value;
					SetBindingAndLayoutBasedOnRateAndObjectType();
				}
			}
		}
		SpotRateType rateType;

		#region SuppressResourceStringsCheckRegion

		public void SetBindingAndLayoutBasedOnRateAndObjectType()
		{
			SuspendLayout();

			try
			{
				switch (RateType)
				{
					case SpotRateType.ShipmentSellRate:
						SetBindingSource(typeof(CommonShipment));
						this.RateAndCurrencyCalcFindBox.BindToAmount = CommonShipment.Schema.JS_UnitFreightRate;
						this.RateAndCurrencyCalcFindBox.BindToUnit = CommonShipment.Schema.JS_RX_NKFrtRateCurrency;
						this.BindingSource.SetBindingMember(this.AutoratingModeDropEdit, CommonShipment.Schema.JS_FreightSpotRateAutoratingMode);
						break;

					case SpotRateType.ShipmentCostRate:
						SetBindingSource(typeof(CommonShipment));
						this.RateAndCurrencyCalcFindBox.BindToAmount = CommonShipment.Schema.JS_FreightCostRate;
						this.RateAndCurrencyCalcFindBox.BindToUnit = CommonShipment.Schema.JS_RX_NKFreightCostRateCurrency;
						this.BindingSource.SetBindingMember(this.AutoratingModeDropEdit, CommonShipment.Schema.JS_FreightCostRateAutoratingMode);
						break;

					case SpotRateType.ShipmentGatewaySellRate:
						SetBindingSource(typeof(CommonShipment));
						this.RateAndCurrencyCalcFindBox.BindToAmount = CommonShipment.Schema.JS_GatewayFreightSellRate;
						this.RateAndCurrencyCalcFindBox.BindToUnit = CommonShipment.Schema.JS_RX_NKGatewayFreightSellRateCurrency;
						this.BindingSource.SetBindingMember(this.AutoratingModeDropEdit, CommonShipment.Schema.JS_FreightGatewaySellRateAutoratingMode);
						break;

					case SpotRateType.ContainerCostRate:
						SetBindingSource(typeof(CommonContainer));
						this.RateAndCurrencyCalcFindBox.BindToAmount = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_CostSpotRate);
						this.RateAndCurrencyCalcFindBox.BindToUnit = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_RX_NKCostSpotRateCurrency);
						this.BindingSource.SetBindingMember(this.AutoratingModeDropEdit, string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_CostSpotRateMode));
						break;

					case SpotRateType.ContainerSellRate:
						SetBindingSource(typeof(CommonContainer));
						this.RateAndCurrencyCalcFindBox.BindToAmount = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_SellSpotRate);
						this.RateAndCurrencyCalcFindBox.BindToUnit = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_RX_NKSellSpotRateCurrency);
						this.BindingSource.SetBindingMember(this.AutoratingModeDropEdit, string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_SellSpotRateMode));
						break;

					case SpotRateType.ContainerGatewaySellRate:
						SetBindingSource(typeof(CommonContainer));
						this.RateAndCurrencyCalcFindBox.BindToAmount = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_GatewaySellSpotRate);
						this.RateAndCurrencyCalcFindBox.BindToUnit = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_RX_NKGatewaySellSpotRateCurrency);
						this.BindingSource.SetBindingMember(this.AutoratingModeDropEdit, string.Format(CultureInfo.InvariantCulture, "{0}.{1}", CommonContainer.Schema.TableName, CommonContainer.Schema.JC_GatewaySellSpotRateMode));
						break;

					default:
						break;
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		#endregion

		void SetBindingSource(Type bindingSourceType)
		{
			this.BindingSource.DataSourceType = bindingSourceType;
		}

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
