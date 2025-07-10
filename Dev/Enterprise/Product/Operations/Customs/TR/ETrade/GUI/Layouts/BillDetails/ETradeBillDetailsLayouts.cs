using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public sealed class ETradeBillDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public ETradeBillDetailsLayouts()
		{
			BillDetails = CreateETradeBillDetailsLayouts();
		}

		public PanelLayout CreateETradeBillDetailsLayouts()
		{
			var builder = new ASYCUDA.GUI.BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var etr = ETradeBillDetailsControlBag.Instance;
			builder.AddControlBag(etr);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(etr.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(etr.DepartureCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etr.TradeCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etr.ExportCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etr.ArrivalCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(etr.PaymentMethodDropEdit, ControlWidthClass.Long);
			builder.Add(common.ProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(etr.NatureOfBusinessDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
			builder.Add(common.IncotermDropEdit, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(etr.AccountantTextBox, ControlWidthClass.Long);
			builder.Add(etr.AccountantVATTextBox, ControlWidthClass.Long);
			builder.Add(etr.ContainerNumberDropEdit, ControlWidthClass.Long);

			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.NetWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(etr.ExemptionCode1DropEdit, ControlWidthClass.Long);
			builder.Add(etr.ExemptionCode2DropEdit, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(etr.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(etr.PrecedentFreightCostLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(etr.OtherValueCalcFindBox, ControlWidthClass.Long);
			builder.Add(etr.GuaranteeTypeDropEdit, ControlWidthClass.Long);
			builder.Add(etr.GuaranteeRefNoTextBox, ControlWidthClass.Long);
			builder.Add(etr.GuaranteeAmountCalcEdit, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.CargoStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
			builder.Add(etr.SeparatedCheckBox, ControlWidthClass.Long);

			builder.SetCaption(common.GrossWeightCalcDropEdit, b => Res.GetData("937E04BF-53B9-4F43-B0B1-844AE18430D3", "Gross Weight"));
			builder.SetCaption(etr.SeparatedCheckBox, b => Res.GetData("B318A5D8-6BC2-46D7-B821-689D7E4A08F6", "Separated"));
			builder.SetCaption(common.NotifyPartyAddressControl, b => Res.GetData("BECEA07C-ACAD-485F-B261-D9662B47A0A4", "Market Place"));

			builder.AddControlBehaviour<ZAddressControl>(common.NotifyPartyAddressControl, UpdateControlBehaviourAction);

			void UpdateControlBehaviourAction(ZAddressControl control, AsycudaBill asycudaBill)
			{
				control.ShowOrganisationName = true;
				control.ShowAddressDropEdit = false;
				var addressStatusButton = control.FindSingleOrDefault<ZButton>(c => c.Name == "AddressStatusButton");
				if (addressStatusButton != null)
				{
					addressStatusButton.Visible = false;
				}
			}

			return builder.Build();
		}
	}
}
