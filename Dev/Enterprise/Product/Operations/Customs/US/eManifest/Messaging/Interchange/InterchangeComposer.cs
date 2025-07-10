using System;
using System.Globalization;
using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.Messaging.Interchange
{
	public class InterchangeComposer : BaseInterchangeSender
	{
		#region Overrides

		protected override bool SendInt(EDIInterchange interchange)
		{
			return true;
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			//NOTE: Should not sent the interchange but should prepare it for sending.
			PrepareInterchanges(new[] { EDIInterchange.ApplicationCodes.USeManifest });
		}

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			var interchanges = new InterchangeProvider(messages).Interchanges;
		}

		#endregion

		#region InterchangeProvider

		class InterchangeProvider : InterchangeProviderBase
		{
			public InterchangeProvider(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			protected override string GetCollationKey(EDIMessage message)
			{
				return DoNotCollateType;
			}

			protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
			{
				if (messages.Count > 0)
				{
					var firstMessage = messages[0];
					var sender = EDIMessage.ApplicationCodes.USeManifest;
					var receiver = Business.EDIMessage.ApplicationCodes.USCustoms;
					SetInterchangeValuesForTransmit(interchange, messages, firstMessage.EM_MessageType, receiver, sender);

					interchange.EI_HeaderText = GetInterchangeHeader(sender, receiver, firstMessage, ZString.Empty);
					interchange.EI_GB = (firstMessage.Branch ?? GlbBranch.CurrentBranch).PK;
				}
			}

			string GetInterchangeHeader(string sender, string receiver, EDIMessage message, string password)
			{
				const string qualifier = "ZZ";
				var unb = GetUNB(PreparedTime.ToZDateTime(), receiver, qualifier, sender, qualifier, ZString.Empty, "UNOA", "4", "ACE", false, false, password);
				var ung = GetUNG(PreparedTime.ToZDateTime(), GetMessageType(message), sender, qualifier, receiver, "UN", "D", GetMessageRelease(message), ZString.Empty, password);
				var interchangeHeader = unb.ToString(CurrentUNCharacterSet) + ung.ToString(CurrentUNCharacterSet).Replace(receiver + "+", string.Format("{0}:{1}+", receiver, qualifier));
				return interchangeHeader;
			}

			static ZString GetMessageType(EDIMessage message)
			{
				switch (message.EM_MessageType)
				{
					case MessageTypes.Codes.eManifest:
					case MessageTypes.Codes.UnassociatedShipments:
						return "CUSCAR";
					case MessageTypes.Codes.CrewAndPassenger:
						return "PAXLST";
					case MessageTypes.Codes.CompleteTrip:
					case MessageTypes.Codes.PreliminaryTrip:
						return "CUSREP";
					case MessageTypes.Codes.CrewOrEquipmentRegistration:
						return "MEDPID";
					default:
						throw new ApplicationException("Invalid e-Manifest message type: " + message.EM_MessageType);
				}
			}

			static string GetMessageRelease(EDIMessage message)
			{
				switch (message.EM_MessageType)
				{
					case MessageTypes.Codes.CrewOrEquipmentRegistration:
						return "02A";
					default:
						return "03B";
				}
			}

			protected override string GetDateString(ZDateTime date)
			{
				return date.ToString("yyyyMMdd");
			}

			protected override ZString GetInterchangeFooter(int messageCount)
			{
				return GetUNEString(messageCount.ToString(CultureInfo.InvariantCulture)) + GetUNZString("1");
			}

			protected override string InstructionHowToSetInterchangeSenderID
			{
				get { return string.Empty; }
			}

			protected override Type InterchangeType
			{
				get { return typeof(CBPEDIInterchange); }
			}
		}

		#endregion
	}
}
