using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public sealed class DeliveryNotificationPartyValidation
	{
		public DeliveryNotificationPartyValidation(DeliveryNotificationParty parent)
		{
			Parent = parent;
		}

		public DeliveryNotificationParty Parent { get; }

		public void ValidateAll()
		{
			ValidateOutwardReportNotificationParty();
			ValidateOutwardReportNotificationPartyName();
			ValidateOutwardReportNotificationPartyEmail();
			ValidateOutwardReportNotificationPartyPort();
		}

		#region OutwardReportNotificationParty

		internal void ValidateOutwardReportNotificationParty()
		{
			Parent.E2_OA_DeliveryNotificationPartyInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(Parent.E2_OA_DeliveryNotificationPartyInfo);
			Parent.E2_OA_DeliveryNotificationPartyInfo.RunAdditionalValidation();

			if (!CheckOutwardReportNotificationParty())
			{
				Parent.E2_OA_DeliveryNotificationPartyInfo.AddMessageError(Res.GetString("0B6AA572-5666-44BD-A342-904F57762F93", "Delivery Notification Organization does not have a valid TSW Identification code configured.\r\n\r\nAn Approved Transitional Facility Code (ATF), Customs Client Code (CCD) or a Customs Controlled Premises Code (CCP) must be sent.\r\nPlease either select an appropriate organization with a valid ATF, CCD or CCP code or update this organization's config details to include their appropriate TSW Registered code."));
			}
			else
			{
				CheckAtLeastOnePartyIsEntered(Parent.E2_OA_DeliveryNotificationPartyInfo);
			}
		}

		internal void ValidateOutwardReportNotificationPartyName()
		{
			Parent.DeliveryNotificationPartyNameInfo.ClearAllNotifications();

			if (Parent.DeliveryNotificationPartyName.IsEmpty && !Parent.DeliveryNotificationPartyEmail.IsEmpty)
			{
				Parent.DeliveryNotificationPartyNameInfo.AddMessageError(Res.GetString("F26E46AB-377F-46DF-90AF-1D02AECE94B4", "You have entered a notification email address. You must state the name of a Delivery notification party to be notified by TSW when the OCR is accepted."));
			}
			else
			{
				CheckAtLeastOnePartyIsEntered(Parent.DeliveryNotificationPartyNameInfo);
			}
		}

		internal void ValidateOutwardReportNotificationPartyEmail()
		{
			Parent.DeliveryNotificationPartyEmailInfo.ClearAllNotifications();
			EmailAddressValidation.ValidateEmailAddress(Parent.DeliveryNotificationPartyEmailInfo);

			if (Parent.DeliveryNotificationPartyEmail.IsEmpty && !Parent.DeliveryNotificationPartyName.IsEmpty)
			{
				Parent.DeliveryNotificationPartyEmailInfo.AddMessageError(Res.GetString("89473d05-36ab-44e5-ac5c-1dc3da912c33", "Must be transmitted to state the email of the Delivery Notification Party where delivery notification is required to a non TSW registered party."));
			}
			else
			{
				CheckAtLeastOnePartyIsEntered(Parent.DeliveryNotificationPartyEmailInfo);
			}
		}

		internal void ValidateOutwardReportNotificationPartyPort()
		{
			Parent.DeliveryNotificationPartyPortInfo.ClearAllNotifications();

			ListValidation.MessageErrorIfInvalidCode(Parent.DeliveryNotificationPartyPortInfo);
			CheckAtLeastOnePartyIsEntered(Parent.DeliveryNotificationPartyPortInfo);
		}

		void CheckAtLeastOnePartyIsEntered(ZPropertyInfo info)
		{
			if (Parent.AllPartiesAreBlank)
			{
				if (Parent.Parent is OutwardReportManifestStatus outwardReportManifestStatus && outwardReportManifestStatus.Consol != null)
				{
					info.AddWarning(AtLeastOnePartyShouldBeEnteredMessage);
				}
				else if (Parent.Parent is IAsycudaManifestHeader)
				{
					info.AddMessageError(AtLeastOnePartyShouldBeEnteredMessage);
				}
			}
		}

		public string AtLeastOnePartyShouldBeEnteredMessage => Res.GetString("A008B5E9-B471-4AAD-9748-C77349883D0E", "Either Delivery Notification Organization, Delivery Notification Port or both Delivery Notification Party and email should be entered to ensure that the CCA receives delivery advices.");

		#endregion
		#region Implementation

		internal bool CheckOutwardReportNotificationParty()
		{
			var notifyParty = Parent.E2_OA_DeliveryNotificationParty_ZAddress?.OrgHeader as OrgHeader;
			var notifyPartyAddress = Parent.E2_OA_DeliveryNotificationParty_ZAddress?.OrgAddress as OrgAddress;

			return (notifyParty == null && notifyPartyAddress == null)
				|| HasCustomsRegNo(notifyParty, notifyPartyAddress, OrgCusCode.CodeTypes.CustomsClientCode)
				|| HasCustomsRegNo(notifyParty, notifyPartyAddress, OrgCusCode.CodeTypes.ControlledPremisesID)
				|| HasCustomsRegNo(notifyParty, notifyPartyAddress, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility);
		}

		bool HasCustomsRegNo(OrgHeader notifyParty, OrgAddress notificationPartyAddress, ZString code)
		{
			var result = notificationPartyAddress?.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = notifyParty?.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			}
			return !result.IsEmpty;
		}

		#endregion
	}
}
