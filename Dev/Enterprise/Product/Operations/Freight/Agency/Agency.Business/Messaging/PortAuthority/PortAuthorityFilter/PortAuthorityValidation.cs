using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthorityValidation : PortMessageValidation
	{
		public PortAuthorityValidation(PortAuthority parent)
			: base(parent) { }

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo, Parent.Lookups.MessageType_List);

			ISailingEndPoint endPoint = Parent.TryGetEndPoint();
			if (endPoint != null)
			{
				string relatedVoyageName = null;

				EDIMessage message = GetLastMessage(endPoint) ?? FindMessageInSiblingPortsFromRelatedVoyages(ref relatedVoyageName);

				string warning = null;

				switch (Parent.MessageType)
				{
					case PortMessageTypeList.Codes.Cancellation:
						if (message == null)
						{
							warning = Res.GetString("64910a09-acd3-4ff6-8080-624bee3fadf0", "No messages have been sent for this port/direction, so there is nothing to cancel.");
						}
						else if (message.EM_MessageSubType == PortMessageTypeList.Codes.Cancellation)
						{
							warning = Res.GetString("71c95574-ed03-4235-8a93-716f397ef43d", "A cancellation has already been sent for this port/direction.");
						}
						break;

					case PortMessageTypeList.Codes.Replace:
						if (message == null)
						{
							warning = Res.GetString("ceade5cc-07ca-4645-8aec-e8d81e5f5554", "No messages have been sent for this port/direction, so there is nothing to replace.");
						}
						else if (message.EM_MessageSubType == PortMessageTypeList.Codes.Cancellation)
						{
							warning = Res.GetString("769cd673-3977-4b7b-af90-7cfd6180c149", "The last message sent for this port/direction was a cancellation, so there is nothing to replace.");
						}
						break;

					case PortMessageTypeList.Codes.Original:
						if (message != null && message.EM_MessageSubType != PortMessageTypeList.Codes.Cancellation)
						{
							warning = Res.GetString("bb18d4f1-b801-4e00-82da-4da61838cd10", "An uncanceled message has already been sent for this port/direction.");
						}
						break;
				}

				if (!string.IsNullOrEmpty(warning))
				{
					if (!string.IsNullOrEmpty(relatedVoyageName))
					{
						warning = relatedVoyageName + ": " + warning;
					}

					Parent.MessageTypeInfo.AddWarning(warning);
				}
			}
		}

		protected override void CheckPrincipalPK()
		{
			base.CheckPrincipalPK();

			MandatoryValidation.CheckEntered(Parent.PrincipalPKInfo);

			if (!Parent.PrincipalPKInfo.HasNotifications())
			{
				ListValidation.ErrorIfInvalidPK(Parent.PrincipalPKInfo, Parent.Lookups.LinkedPrincipals);
			}

			if (!Parent.PrincipalPKInfo.HasNotifications() && ((PortAuthorityLookups)Parent.Lookups).Settings_List.FindPortSettingWithPrincipal(Parent.Port, Parent.PrincipalPK) == null)
			{
				Parent.PrincipalPKInfo.AddError(Res.GetString("33A05837-C271-479E-8DBC-768DA4D8EE6E", "Principal is not configured for sending Port Authority message to {0}", Parent.Port));
			}
		}

		EDIMessage GetLastMessage(ISailingEndPoint endPoint)
		{
			return endPoint.Messages.GetLastMessage(EDIMessage.ApplicationCodes.PortAuthority);
		}

		EDIMessage FindMessageInSiblingPortsFromRelatedVoyages(ref string relatedVoyageName)
		{
			EDIMessage message = null;

			var anotherVoyages = Parent.Voyage.FindOtherVoyagesWithSameVesselVoyageCombination();
			foreach (var anotherVoyage in anotherVoyages)
			{
				if (Parent.Direction == Constants.PortDirection.Load)
				{
					ISailingEndPoint endPoint = anotherVoyage.Origins.GetOriginFromLoading(Parent.Port);
					if (endPoint != null)
					{
						message = GetLastMessage(endPoint);
						if (message != null)
						{
							relatedVoyageName = anotherVoyage.HumanReadableName;
							break;
						}
					}
				}
				else if (Parent.Direction == Constants.PortDirection.Discharge)
				{
					ISailingEndPoint endPoint = anotherVoyage.Destinations.GetDestinationFromDischarge(Parent.Port);
					if (endPoint != null)
					{
						message = GetLastMessage(endPoint);
						if (message != null)
						{
							relatedVoyageName = anotherVoyage.HumanReadableName;
							break;
						}
					}
				}
			}

			return message;
		}

		protected override void CheckDeliverTo3rdParty()
		{
			base.CheckDeliverTo3rdParty();
			if (Parent.DeliverTo3rdParty)
			{
				Parent.DeliverTo3rdPartyInfo.AddWarning(Res.GetString("b64932b7-7ab9-429d-88ce-481376bc8773", "This option is in the process of being removed. Please contact your account manager or support for alternative solutions"));
			}
		}

		#region Implementation

		public new PortAuthority Parent
		{
			get { return (PortAuthority)base.Parent; }
		}

		#endregion
	}
}


