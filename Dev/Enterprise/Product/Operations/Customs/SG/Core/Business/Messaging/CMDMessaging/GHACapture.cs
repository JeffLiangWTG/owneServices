using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.SG.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class GHACapture : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GHACapture(CMDWrapperBase cMDWrapperBizO)
		{
			this.cMDWrapperBizO = cMDWrapperBizO;
		}

		#region SubmittedTo

		public const string SubmittedToName = "SubmittedTo";

		[ResourceStringData("E1159AAD-E808-43AC-9D13-B6DC2EE802F2", Caption = "Current CMD was submitted to:")]
		public ZString SubmittedTo
		{
			get
			{
				ZString result = "";

				if (RecipientsAreMixed)
				{
					result = "MIXED";
				}
				else if (cMDWrapperBizO.CMDShipments.Length > 0)
				{
					result = GetCurrentCMDRecipient(cMDWrapperBizO.CMDShipments[0]);
				}

				if (result.IsEmpty)
				{
					result = "-";
				}

				return result;
			}
		}

		public ZPropertyInfo SubmittedToInfo
		{
			get { return GetZPropertyInfo(SubmittedToName); }
		}

		bool RecipientsAreMixed
		{
			get
			{
				string lastRecipientName = null;
				foreach (CMDShipmentWrapper shipmentStatus in cMDWrapperBizO.CMDShipments)
				{
					ZString recipientName = GetCurrentCMDRecipient(shipmentStatus);
					if (lastRecipientName != recipientName && lastRecipientName != null)
					{
						return true;
					}
					lastRecipientName = recipientName;
				}

				return false;
			}
		}

		ZString GetCurrentCMDRecipient(CMDShipmentWrapper shipmentStatus)
		{
			ZString result = "";
			CMDEDIMessage message = shipmentStatus.GetLastCMDSent();
			if (message != null && message.Reply != null)
			{
				result = message.Reply.Sender;
			}
			return result;
		}

		#endregion

		#region GHA

		public const string GHAName = "GHA";

		[MaxLength(4)]
		[List(nameof(GHAList))]
		[ResourceStringData("40AE6F23-98D9-4915-8B0E-E117AA0D4F25", Caption = "Submit CMD to:")]
		public ZString GHA
		{
			get { return fGHA; }
			set
			{
				CheckMaximumLength(GHAInfo, value);
				SetNonPersistentPropertyValue(GHAInfo, ref fGHA, value);
				if (!IsValidationSuspended)
				{
					ValidateGHA();
				}
			}
		}

		public ZPropertyInfo GHAInfo
		{
			get { return GetZPropertyInfo(GHAName); }
		}

		public ReadOnlyCodeDescriptionPairList GHAList => ghaList ??= SGCustomsDataRegistry.Instance.GHAList.Value;
		ReadOnlyCodeDescriptionPairList ghaList;

		public void ValidateGHA()
		{
			GHAInfo.ClearAllNotifications();
			if (GHA.IsEmpty)
			{
				GHAInfo.AddError("Please select a GHA.");
			}
			else if (!GHAList.ContainsCode(GHA))
			{
				GHAInfo.AddError("Please enter a valid GHA from the list.");
			}
		}

		ZString fGHA;

		#endregion

		readonly CMDWrapperBase cMDWrapperBizO;
	}
}
