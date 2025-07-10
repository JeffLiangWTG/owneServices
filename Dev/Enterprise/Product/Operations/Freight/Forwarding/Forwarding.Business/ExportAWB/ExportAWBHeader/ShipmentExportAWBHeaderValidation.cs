using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ShipmentExportAWBHeaderValidation : ExportAWBHeaderValidation
	{
		public ShipmentExportAWBHeaderValidation(ShipmentExportAWBHeader parent)
			: base(parent)
		{
		}

		public new ShipmentExportAWBHeader Parent
		{
			get { return (ShipmentExportAWBHeader)base.Parent; }
		}

		#region EH_ECNCRNNumber Validation

		protected override void CheckEH_ECNCRNNumber()
		{
			base.CheckEH_ECNCRNNumber();

			if (Parent.Shipment == null)
			{
				return;
			}

			if (Parent.Shipment.CusEntryNumbers.Count > 0)
			{
				if (Parent.Shipment.CusEntryNumbers.Count > 4)
				{
					Parent.EH_ECNCRNNumberInfo.AddWarning(Res.GetString("f3dff7a6-7924-4059-aa3e-5520b60a0827", "If more than four entry numbers present on a shipment, they will not be shown on this document but will be included in the FHL and FWB airline messages."));
				}

				Parent.Shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				if (Parent.Shipment.CustomsEntryNumberForBindingInfo.HasMessageErrors() || Parent.Shipment.CustomsEntryNumberForBindingInfo.HasWarnings())
				{
					Parent.Shipment.CustomsEntryNumberForBindingInfo.GetMessageErrors().ForEach(e => Parent.EH_ECNCRNNumberInfo.AddWarning(e.Message));
					Parent.Shipment.CustomsEntryNumberForBindingInfo.GetWarnings().ForEach(e => Parent.EH_ECNCRNNumberInfo.AddWarning(e.Message));
				}
			}
		}

		#endregion

		#region Shipper/Consignee Interdependent Fields

		public void ValidateAllShipperConsigneeFields()
		{
			ValidateAllShipperFields();
			ValidateAllConsigneeFields();
		}

		public void ValidateAllShipperFields()
		{
			ValidateEH_ShipperName();
			ValidateEH_ShipperAddress();
			ValidateEH_ShipperPlace();
			ValidateEH_ShipperCountryCode();
			ValidateEH_ShipperContactCode();
			ValidateEH_ShipperContactDetail();
			ValidateEH_ShipperAddress2();
			ValidateEH_ShipperAccount();
		}

		public void ValidateAllConsigneeFields()
		{
			ValidateEH_ConsigneeName();
			ValidateEH_ConsigneeAddress();
			ValidateEH_ConsigneePlace();
			ValidateEH_ConsigneeCountryCode();
			ValidateEH_ConsigneeContactCode();
			ValidateEH_ConsigneeContactDetail();
			ValidateEH_ConsigneeAddress2();
			ValidateEH_ConsigneeAccount();
		}

		bool ShipperConsigneeDetailsMandatory
		{
			get { return ShipperDetailsMandatory || ConsigneeDetailsMandatory; }
		}

		bool ShipperDetailsMandatory
		{
			get
			{
				return !Parent.EH_ShipperName.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ShipperAddress.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ShipperPlace.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ShipperCountryCode.IsCargoIMPEmpty(CIMPFieldFormats.Alpha)
					|| !Parent.EH_ShipperContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric)
					|| !Parent.EH_ShipperContactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric)
					|| !Parent.EH_ShipperAddress2.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ShipperAccount.IsCargoIMPEmpty(CIMPFieldFormats.Text);
			}
		}

		bool ConsigneeDetailsMandatory
		{
			get
			{
				return !Parent.EH_ConsigneeName.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ConsigneeAddress.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ConsigneePlace.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ConsigneeCountryCode.IsCargoIMPEmpty(CIMPFieldFormats.Alpha)
					|| !Parent.EH_ConsigneeContactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric)
					|| !Parent.EH_ConsigneeContactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric)
					|| !Parent.EH_ConsigneeAddress2.IsCargoIMPEmpty(CIMPFieldFormats.Text)
					|| !Parent.EH_ConsigneeAccount.IsCargoIMPEmpty(CIMPFieldFormats.Text);
			}
		}

		void ValidateShipperField(ZPropertyInfo info, CIMPFieldFormats format)
		{
			if (info.Value.IsCargoIMPEmpty(format) && ShipperDetailsMandatory)
			{
				info.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission,
					Res.GetString("5b88444d-8594-4b73-9cfc-7346458c0137", "{0} is required when other shipper details are supplied.", info.HumanReadableName));
			}
		}

		void ValidateConsigneeField(ZPropertyInfo info, CIMPFieldFormats format)
		{
			if (info.Value.IsCargoIMPEmpty(format) && ShipperConsigneeDetailsMandatory)
			{
				info.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission,
					Res.GetString("924fa7e4-5c6b-40a4-9271-6c614a70b98c", "{0} is required when other shipper/consignee details are supplied.", info.HumanReadableName));
			}
		}

		protected override void CheckEH_ShipperName()
		{
			base.CheckEH_ShipperName();
			ValidateShipperField(Parent.EH_ShipperNameInfo, CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ShipperAddress()
		{
			base.CheckEH_ShipperAddress();
			ValidateShipperField(Parent.EH_ShipperAddressInfo, CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ShipperPlace()
		{
			base.CheckEH_ShipperPlace();
			ValidateShipperField(Parent.EH_ShipperPlaceInfo, CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ShipperCountryCode()
		{
			base.CheckEH_ShipperCountryCode();
			ValidateShipperField(Parent.EH_ShipperCountryCodeInfo, CIMPFieldFormats.Alpha);
		}

		protected override void CheckEH_ConsigneeName()
		{
			base.CheckEH_ConsigneeName();
			ValidateConsigneeField(Parent.EH_ConsigneeNameInfo, CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ConsigneeAddress()
		{
			base.CheckEH_ConsigneeAddress();
			ValidateConsigneeField(Parent.EH_ConsigneeAddressInfo, CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ConsigneePlace()
		{
			base.CheckEH_ConsigneePlace();
			ValidateConsigneeField(Parent.EH_ConsigneePlaceInfo, CIMPFieldFormats.Text);
		}

		protected override void CheckEH_ConsigneeCountryCode()
		{
			base.CheckEH_ConsigneeCountryCode();
			ValidateConsigneeField(Parent.EH_ConsigneeCountryCodeInfo, CIMPFieldFormats.Alpha);
		}

		#endregion

		#region Charges

		public void ValidateChargeDetails()
		{
			ValidateEH_Currency();
			ValidateEH_WeightPrepaidCollect();
			ValidateEH_OtherPrepaidCollect();
		}

		bool ChargesDetailsMandatory
		{
			get { return !Parent.EH_Currency.IsEmpty || !Parent.EH_WeightPrepaidCollect.IsEmpty || !Parent.EH_OtherPrepaidCollect.IsEmpty; }
		}

		string ErrorIfChargesAreMandatory
		{
			get { return Res.GetString("707ea5bc-6e50-491c-bbf7-44a857d34d97", "If one of Currency, WT/VAL (P/C) or Other (P/C) are entered then all are required"); }
		}

		protected override void CheckEH_Currency()
		{
			base.CheckEH_Currency();
			if (ChargesDetailsMandatory)
			{
				if (Parent.EH_Currency.IsEmpty)
				{
					Parent.EH_CurrencyInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfChargesAreMandatory);
				}

				ValidateFieldLengthForElectronicTransmission(Parent.EH_CurrencyInfo, 3);
				ValidateFieldFormatForElectronicTransmission(Parent.EH_CurrencyInfo, CIMPFieldFormats.Alpha);
			}
		}

		string ErrorIfBTH
		{
			get { return Res.GetString("a0a9c5b4-5e72-4523-8646-b4bdf8199f66", "Payment type should be either Prepaid or Collect."); }
		}

		protected override void CheckEH_WeightPrepaidCollect()
		{
			base.CheckEH_WeightPrepaidCollect();
			if (!Parent.EH_WeightPrepaidCollectInfo.HasErrors() && Parent.EH_WeightBTH)
			{
				Parent.EH_WeightPrepaidCollectInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfBTH);
			}

			if (ChargesDetailsMandatory && Parent.EH_WeightPrepaidCollect.IsEmpty)
			{
				Parent.EH_WeightPrepaidCollectInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfChargesAreMandatory);
			}
		}

		protected override void CheckEH_OtherPrepaidCollect()
		{
			base.CheckEH_OtherPrepaidCollect();
			if (!Parent.EH_OtherPrepaidCollectInfo.HasErrors() && Parent.EH_OtherBTH)
			{
				Parent.EH_OtherPrepaidCollectInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfBTH);
			}

			if (ChargesDetailsMandatory && Parent.EH_OtherPrepaidCollect.IsEmpty)
			{
				Parent.EH_OtherPrepaidCollectInfo.AddNotification(NotificationLevelForFieldsRequiredForElectronicTransmission, ErrorIfChargesAreMandatory);
			}
		}

		#endregion

		#region implementation

		readonly Lazy<INotificationType> notificationLevelForFieldsRequired = new Lazy<INotificationType>(
			() => ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.Value ? CargoWise.EntityFramework.NotificationType.Warning : CargoWise.EntityFramework.NotificationType.MessageError);

		public override INotificationType NotificationLevelForFieldsRequiredForElectronicTransmission => notificationLevelForFieldsRequired.Value;

		#endregion
	}
}
