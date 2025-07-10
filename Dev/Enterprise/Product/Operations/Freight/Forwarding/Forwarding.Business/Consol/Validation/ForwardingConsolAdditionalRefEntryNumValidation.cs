using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	class ForwardingConsolAdditionalRefEntryNumValidation : CommonAdditionalRefEntryNumValidation
	{
		public ForwardingConsolAdditionalRefEntryNumValidation(AutoCusEntryNum parent)
			: base(parent)
		{
		}

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();

			if (Parent.CE_Category == CusEntryNumber.Categories.AdditionalReferenceNumber && Parent.CE_EntryType == BrazilAdditionalReferenceNumberTypes.Codes.RUC)
			{
				ValidateMRUC();
				return;
			}

			var consol = (ForwardingConsol)Parent.Parent;
			switch (Parent.CE_EntryType)
			{
				case CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG when consol.JK_BookingReference == Parent.CE_EntryNum:
					Parent.CE_EntryNumInfo.AddError(Res.GetString("0B0BBBF4-AFE6-4D89-B5CE-C49D1DF30624", "This Reference Number is a duplicate of this consol's Carrier Booking Reference."));
					return;

				case CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON:
					ValidateCONEntryTypeAndValue(consol, Parent);
					return;

				default:
					return;
			}
		}

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();

			var entryTypesForSystemOnly = new ZString[]
			{
				ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference,
			};

			if (entryTypesForSystemOnly.Contains(Parent.CE_EntryType)
				&& !Parent.CE_EntryIsSystemGenerated
				&& (Parent.CE_EntryTypeInfo.HasChanges || !Parent.IsInDatabase))
			{
				Parent.CE_EntryTypeInfo.AddError(Res.GetString("e2c9ce19-5059-4035-82f2-5e21c6364b04", "{0} is reserved for system use and cannot be manually entered.", Parent.CE_EntryType));
				return;
			}

			if (Parent.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
			{
				ValidateCONEntryTypeAndValue((ForwardingConsol)Parent.Parent, Parent);
			}
		}

		void ValidateCONEntryTypeAndValue(ForwardingConsol consol, CusEntryNumber entryNumberToValidate)
		{
			var rowNotificationsToClear = entryNumberToValidate.RowNotifications.ToList();
			foreach (var notification in rowNotificationsToClear)
			{
				entryNumberToValidate.RemoveRowNotification(notification);
			}

			entryNumberToValidate.AddRowWarning(Res.GetString("11192d63-df76-4ea5-8906-55ec5ec52973", @"There is a dedicated field, Carrier Contract No. available on Details > Pre-Allocation. Please use this field for rating, carrier bookings, and allocations. Reference type 'CON' is no longer used for such purposes."));
		}

		void ValidateMRUC()
		{
			if (!ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(Parent.CE_EntryNum))
			{
				Parent.CE_EntryNumInfo.AddMessageError(Res.GetString("f508ae94-e5fc-43b6-b9e7-1f58a3d88dac", "The entered MRUC code does not match the CPF format: <year, 1><country/region, 2><shipper, 11><decade, 1><reference, 1-20> or the CNPJ format: <year, 1><country/region, 2><shipper, 8><decade, 1><reference, 1-23>"));
			}

			if (ForwardingAdditionalRefEntryNumValidationHelper.GetDuplicateRUCParent(Parent) is ForwardingConsol consol)
			{
				var errorMessageMRUC = Res.GetString("7e5f0827-4dd0-4d38-a46a-5bv1de392948", "MRUC must be unique and cannot be reused. It is already saved against {0}.", consol.HumanReadableName);
				Parent.CE_EntryNumInfo.AddMessageError(errorMessageMRUC);
			}
		}
	}
}
