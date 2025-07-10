using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using AdditionalReferenceNumbersCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;

namespace Enterprise.Freight.Forwarding.Business
{
	class ForwardingShipmentAdditionalRefEntryNumValidation : CommonAdditionalRefEntryNumValidation
	{
		public ForwardingShipmentAdditionalRefEntryNumValidation(AutoCusEntryNum parent) : base(parent)
		{
		}

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();

			if (Parent.CE_Category == CusEntryNumber.Categories.AdditionalReferenceNumber)
			{
				if (Parent.CE_EntryType == BrazilAdditionalReferenceNumberTypes.Codes.RUC)
				{
					ValidateRUC();
				}

				if (Parent.CE_EntryType == ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber && !Parent.CE_EntryNum.IsEmpty)
				{
					ValidateSLD();
				}

				if (Parent.CE_EntryType == AdditionalReferenceNumbersCodes.CON)
				{
					ValidateContract();
				}
			}
		}

		void ValidateRUC()
		{
			if (!ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(Parent.CE_EntryNum))
			{
				Parent.CE_EntryNumInfo.AddMessageError(Res.GetString("f301ce10-9d6d-40c3-9c1f-2b6e469bd84a", "The entered RUC code does not match the CPF format: <year, 1><country/region, 2><shipper, 11><decade, 1><reference, 1-20> or the CNPJ format: <year, 1><country/region, 2><shipper, 8><decade, 1><reference, 1-23>"));
			}

			if (ForwardingAdditionalRefEntryNumValidationHelper.GetDuplicateRUCParent(Parent) is ForwardingShipment shipment)
			{
				var errorMessageMRUC = Res.GetString("c3c2be91-3d4f-4859-b077-5c4543c20514", "RUC must be unique and cannot be reused. It is already saved against {0}. Apply a suffix if required.", shipment.HumanReadableName);
				Parent.CE_EntryNumInfo.AddMessageError(errorMessageMRUC);
			}
		}

		void ValidateSLD()
		{
			if (Parent.Parent is ForwardingShipment shipment)
			{
				var countryCodes = new ZString[] { Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Taiwan, Core.Constants.CountryCodes.HongKong };
				if (countryCodes.Contains(GlbCompany.CurrentCompany.Country.Code) && countryCodes.Contains(shipment.JS_RL_NKOrigin.SubstringSafe(0, 2)))
				{
					var otherShipmentsHaveTheSameExportRefNumber = ShippingOrderNumberValidationHelper.GetDuplicatedShipmentsByShippingOrderNumber(Parent.CE_EntryNum, shipment.PK, Parent.Factory);
					if (otherShipmentsHaveTheSameExportRefNumber.Length > 0)
					{
						Parent.CE_EntryNumInfo.AddWarning(Res.GetString("6f37b733-15d8-4328-90d6-e651b750754e", "This Shi Lian Dan/Shipping Order Number is already in use on: {0}", string.Join(", ", otherShipmentsHaveTheSameExportRefNumber.Select(otherShipment => otherShipment.JS_UniqueConsignRef))));
					}
				}
			}
		}

		void ValidateContract()
		{
			if (Parent.Parent is ForwardingShipment shipment
				&& shipment.JS_IsBooking
				&& !shipment.JS_IsForwardRegistered)
			{
				var contractNumbers = shipment
					.Numbers
					.Find(number => number.CE_EntryType == AdditionalReferenceNumbersCodes.CON)
					.ToList();

				// JS_CarrierContractNumber max length is 50, while CE_EntryNum max length is 35. We only enforce synchronisation if JS_CarrierContractNumber does not exceed 35.
				if (contractNumbers.Count > 1)
				{
					Parent.CE_EntryNumInfo.AddError(Res.GetString("2ba9e8f2-b281-74aa-4bc7-0d997c52f07c", "Only one Carrier Contract Number is allowed to exist."));
				}
				else if (contractNumbers.Count == 1
					&& !shipment.JS_CarrierContractNumber.IsEmpty
					&& shipment.JS_CarrierContractNumber != contractNumbers[0].CE_EntryNum
					&& shipment.JS_CarrierContractNumber.Length <= AutoCusEntryNum.Schema.CE_EntryNumMaxLength)
				{
					Parent.CE_EntryNumInfo.AddError(Res.GetString("97ffb07a-0ddb-4f36-8c1d-6d674ec98700", "Carrier Contract Number is different from Booking > Numbers > CON type."));
				}
			}
		}
	}
}
