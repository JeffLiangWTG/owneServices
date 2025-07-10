using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class eManifestMessageSendingObjectValidation : AutoeManifestMessageSendingObjectValidation
	{
		public eManifestMessageSendingObjectValidation(AutoeManifestMessageSendingObject parent) : base(parent)
		{
		}

		public new eManifestMessageSendingObject Parent => base.Parent as eManifestMessageSendingObject;

		public void ValidateShouldSend()
		{
			((IValidationInternals)this).Validate(Parent.ShouldSendInfo, () => { CheckShouldSend(); });
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateShouldSend();
		}

		protected virtual void CheckShouldSend()
		{
			if (Parent.ShouldSend)
			{
				switch (Parent.MessageType)
				{
					case MessageTypes.Codes.UnassociatedShipments:
						if (!Parent.SendingObjectParent.AllShipmentsHaveSameReleaseStatus)
						{
							Parent.ShouldSendInfo.AddError(Res.GetString("D65C9507-55BA-4C6D-A090-1076B1154C22",
								"Unassociated Shipments message should not be sent from this menu since there are shipments with different release status, please send via 'Submit Unassociated Shipments' instead."));
						}

						break;
					case MessageTypes.Codes.PreliminaryTrip:
						if (Parent.ActionCode == MessageSubTypes.Create)
						{
							if (!Parent.SendingObjectParent.HasClearUnassociatedShipmentsMessage && !Parent.SendingObjectParent.IsExistShipmentConsistsOfEmptyIIT)
							{
								CheckPrerequisite(MessageTypes.Codes.UnassociatedShipments);
							}
						}
						else if (Parent.ActionCode == MessageSubTypes.Confirmation)
						{
							if (!Parent.SendingObjectParent.HasClearCrewAndPassengerMessage)
							{
								CheckPrerequisite(MessageTypes.Codes.CrewAndPassenger);
							}
						}
						break;
					case MessageTypes.Codes.CrewAndPassenger:
						if (!Parent.SendingObjectParent.HasClearPreliminaryTripMessage)
						{
							CheckPrerequisite(MessageTypes.Codes.PreliminaryTrip);
						}
						break;
					default:
						break;
				}
			}
		}

		void CheckPrerequisite(ZString prerequisiteMessageType)
		{
			var sendingObjectParents = Parent.SendingObjectParent.SelectedSendingObjects.Cast<eManifestMessageSendingObject>();
			if (sendingObjectParents.All(x => x.MessageType != prerequisiteMessageType))
			{
				Parent.ShouldSendInfo.AddError(Res.GetString("295C8982-893B-42F3-A9C4-2D60430D1C76", "{0} message should be sent before this message.",
						new MessageTypes().GetMultilingualDescriptionFromCode(prerequisiteMessageType)));
			}
		}
	}
}
