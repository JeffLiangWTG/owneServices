using System.ComponentModel;

using CargoWise.ComponentModel;
using CargoWise.Types;

using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class OneOffQuoteDetailsControl : ZUserControl, IBindTo
	{
		public OneOffQuoteDetailsControl()
			: base()
		{
			InitializeComponent();
		}

		public RateOneOffShipment OneOffQuote
		{
			get
			{
				RateOneOffShipment result = null;
				Quote quote = CurrentDataItem as Quote;
				if (quote != null)
				{
					result = quote.CurrentOneOffQuote;
				}
				return result;
			}
		}

		#region Binding

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set
			{
				fBindTo = value;
				ActualVolumeDropEdit.BindToAmount = GetBindTo(ActualVolumeDropEdit.BindToAmount);
				ActualVolumeDropEdit.BindToUnit = GetBindTo(ActualVolumeDropEdit.BindToUnit);
				ActualVolumeDropEdit.BindToList = GetBindTo(ActualVolumeDropEdit.BindToList);
				ActualWeightDropEdit.BindToAmount = GetBindTo(ActualWeightDropEdit.BindToAmount);
				ActualWeightDropEdit.BindToUnit = GetBindTo(ActualWeightDropEdit.BindToUnit);
				ActualWeightDropEdit.BindToList = GetBindTo(ActualWeightDropEdit.BindToList);
				EntryInvoiceLinesCalcEdit.BindTo = GetBindTo(EntryInvoiceLinesCalcEdit.BindTo);
				EntriesCalcEdit.BindTo = GetBindTo(EntriesCalcEdit.BindTo);
				ServiceLevelFindBox.BindTo = GetBindTo(ServiceLevelFindBox.BindTo);
				ServiceLevelFindBox.BindToList = GetBindTo(ServiceLevelFindBox.BindToList);
				CommodityFindBox.BindTo = GetBindTo(CommodityFindBox.BindTo);
				CommodityFindBox.BindToList = GetBindTo(CommodityFindBox.BindToList);
				ContainersGrid.BindTo = GetBindTo(ContainersGrid.BindTo);
				LooseCargoGrid.BindTo = GetBindTo(LooseCargoGrid.BindTo);
				ChargeableDropEdit.BindTo = GetBindTo(ChargeableDropEdit.BindTo);
				ChargeableUnitLabel.BindTo = GetBindTo(ChargeableUnitLabel.BindTo);
				ValueOfGoodsCalcEdit.BindTo = GetBindTo(ValueOfGoodsCalcEdit.BindTo);
				ValueOfGoodsCurrencyFindbox.BindTo = GetBindTo(ValueOfGoodsCurrencyFindbox.BindTo);
				ValueOfGoodsCurrencyFindbox.BindToList = GetBindTo(ValueOfGoodsCurrencyFindbox.BindToList);
				ValueOfInsuranceCalcEdit.BindTo = GetBindTo(ValueOfInsuranceCalcEdit.BindTo);
				ValueOfInsuranceCurrencyFindbox.BindTo = GetBindTo(ValueOfInsuranceCurrencyFindbox.BindTo);
				ValueOfInsuranceCurrencyFindbox.BindToList = GetBindTo(ValueOfInsuranceCurrencyFindbox.BindToList);
				PickupEquipmentDropEdit.BindTo = GetBindTo(PickupEquipmentDropEdit.BindTo);
				PickupEquipmentDropEdit.BindToList = GetBindTo(PickupEquipmentDropEdit.BindToList);
				DeliveryEquipmentDropEdit.BindTo = GetBindTo(DeliveryEquipmentDropEdit.BindTo);
				DeliveryEquipmentDropEdit.BindToList = GetBindTo(DeliveryEquipmentDropEdit.BindToList);
			}
		}

		string fBindTo;

		ZString GetBindTo(ZString existingBindTo)
		{
			return !existingBindTo.IsEmpty ? (ZString)(BindTo + "." + existingBindTo) : ZString.Empty;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
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
