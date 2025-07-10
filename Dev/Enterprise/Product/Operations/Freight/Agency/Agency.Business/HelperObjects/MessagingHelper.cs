using System;
using System.Globalization;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class MessagingHelper
	{
		public static string GetPortAuthoritySubject(ISailingEndPoint endPoint, string senderId)
		{
			return GetText_PA_EmailSubject(senderId, endPoint.Vessel, endPoint.Voyage);
		}

		public static ISailingEndPoint GetEndPointFromMessage(EDIMessage message)
		{
			switch (message.EM_LinkTable)
			{
				case JobVoyOriginSchema.Constants.TableName:
					return message.Factory.Load<VoyageOrigin>(message.EM_LinkUniqueID);
				case JobVoyDestinationSchema.Constants.TableName:
					return message.Factory.Load<VoyageDestination>(message.EM_LinkUniqueID);

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
						"Port authority messages can only be linked to VoyageOrigin or VoyageDestination but was somehow linked to '{0}'",
						message.EM_LinkTable));
			}
		}

		public static PortAuthoritySetting GetPortAuthoritySettingFromEndPoint(ISailingEndPoint endPoint, string principal)
		{
			if (endPoint == null)
			{
				throw new ArgumentNullException(nameof(endPoint));
			}

			var result = AgencyRegistry.Instance.PortAuthoritySettings.Value.Settings.FindPortSettingWithPrincipal(endPoint.Port, endPoint.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, principal)?.PK ?? ZGuid.Empty) ?? throw new MessageProcessingException(GetText_PA_NotEnabledForPort(endPoint.Port), "", true, false);

			return result;
		}

		public static EIDOMessagingHeader GetEIDOMessagingDetail()
		{
			EIDOMessagingHeader result = AgencyRegistry.Instance.EIDOMessagingDetails.Value;

			if (result == null || result.Identities.Count == 0)
			{
				throw new MessageProcessingException(GetText_EIDO_NotEnabled(), "", true, false);
			}

			return result;
		}

		public static EIDOMessagingIdentity GetEIDOIdentityFromMessage(EIDOMessagingHeader header, EDIMessage message)
		{
			if (header == null)
			{
				throw new ArgumentNullException(nameof(header));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			BillOfLadingContainer container;
			BillOfLading bill;
			EIDOMessagingIdentity identity;

			if ((container = message.Factory.Load<BillOfLadingContainer>(message.EM_LinkUniqueID)) == null ||
				(bill = message.Factory.Load<BillOfLading>(container.JC_JS_FCLBookingOnlyLink)) == null ||
				bill.Principal == null)
			{
				throw new MessageProcessingException(GetText_CannotIdentifyPrincipal(), "", true, false);
			}
			else if ((identity = header.GetIdentity(bill.JS_OH_DeliveryAgent)) == null)
			{
				throw new MessageProcessingException(GetText_EIDO_NotEnabledForPrincipal(bill.Principal.OH_Code), "", true, false);
			}
			else
			{
				return identity;
			}
		}

		public static string Hash(string inString)
		{
			// If this argorithm changes then port authority messaging will start to use the wrong number fountains on existing clients.
			uint code = 0x5A5A5A5A;

			foreach (char c in inString)
			{
				code = (code << 5) | (code >> 27);

				if (c >= '0' && c <= '9')
				{
					code ^= (uint)(c - '0');
				}
				else if (c >= 'A' && c <= 'Z')
				{
					code ^= (uint)(c - 'A' + 10);
				}
				else if (c >= 'a' && c <= 'z')
				{
					code ^= (uint)(c - 'a' + 10);
				}
				else
				{
					code ^= 0x55;
				}
			}

			return code.ToString("X8", CultureInfo.InvariantCulture);
		}

		static string GetText_PA_EmailSubject(string senderId, string vessel, string voyage)
		{
			return Res.GetString("984d6153-a318-4862-95e8-5cc60a3098e3", "Manifest from: {0} for: {1} {2}", senderId, vessel, voyage);
		}
		static string GetText_CannotIdentifyPrincipal()
		{
			return Res.GetString(
				"42f543a5-e829-441d-b22f-7fdaa8b075f3",
				"Unable to identify the correct principal."
				);
		}
		static string GetText_EIDO_NotEnabled()
		{
			return Res.GetString(
				"9096f739-c46d-4d67-bca6-eaf022b5d091",
				"E-IDO messaging has not been enabled in the registry.\r\nTo fix this, go to the registry option Liner & Agency -> E-IDO Messaging and enter the details."
				);
		}
		static string GetText_EIDO_NotEnabledForPrincipal(string code)
		{
			return Res.GetString(
				"96d5b00c-9deb-422b-8ad2-7e1610cf3eff",
				"E-IDO messaging has not been enabled in the registry for {0}.\r\nTo fix this, go to the registry option Liner & Agency -> E-IDO Messaging and enter the details.",
				code
				);
		}
		static string GetText_PA_NotEnabledForPort(string port)
		{
			return Res.GetString(
				"cdd48070-2560-4444-8833-983f0edc2a13",
				"The port authority settings have not been configured for {0}.\r\nTo fix this, go to the registry option Freight -> Liner & Agency -> Port Authority and add a record for port {0}.",
				port
				);
		}
	}
}


