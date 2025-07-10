using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public sealed class ETradeBillPartiesLayouts : IPanelLayoutProvider
	{
		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		public ETradeBillPartiesLayouts()
		{
			BillParties = CreateBillPartiesLayout();
		}

		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new ASYCUDA.GUI.BillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var etradeBill = ETradeBillPartiesControlBag.Instance;
			builder.AddControlBag(etradeBill);

			builder.AddColumn();
			builder.Add(common.ShipperSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConvertShipperToOrganizationButton, ControlWidthClass.Long);
			builder.Add(common.ShipperNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ShipperStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.ShipperRegNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConvertConsigneeToOrganizationButton, ControlWidthClass.Long);
			builder.Add(common.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeCityTextBox, ControlWidthClass.Long);
			builder.Add(common.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			builder.Add(common.ConsigneePhoneTextBox, ControlWidthClass.Long);
			builder.Add(common.ConsigneeRegoNoTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(etradeBill.DeliveryPartyDetailsSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(etradeBill.SupplementaryDeclarationNameTextBox, ControlWidthClass.Long);
			builder.Add(etradeBill.SupplementaryDeclarationRegNoIdNoTextBox, ControlWidthClass.Long);
			builder.Add(etradeBill.SupplementaryDeclarationDeliveryDateDateEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
