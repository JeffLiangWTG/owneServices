using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	class ReleaseMessageStrategy
	{
		public ReleaseMessageStrategy(AgencyBooking shipment)
		{
			this.shipment = shipment;
		}

		protected readonly AgencyBooking shipment;

		public bool IncludeStrategy
		{
			get { return AgencyRegistry.Instance.ContainerExportPreAdvicePorts.FindByPortWithFallback(shipment.JS_NKLoadPort) != null; }
		}

		public ZString MessageDescription
		{
			get { return Res.GetString("37480b2f-335b-47d9-948b-722b63ff0719", "{0} Port", shipment.JS_NKLoadPort); }
		}

		public string EHubID
		{
			get
			{
				return ShippingPortsMessagingEHubIDHelper.GetEHubID(shipment.JS_NKLoadPort);
			}
		}

		public RecipientRoleType[] RecipientRoles
		{
			get { return new RecipientRoleType[] { RecipientRoleType.PER }; }
		}

		string MessageSentToDepartment
		{
			get { return Res.GetString("1a8582a8-20d4-472a-a4a9-6959dda8a44e", "{0} Ports", shipment.JS_NKLoadPort.SubstringSafe(0, 2)); }
		}

		string GetMessageType(ZString releaseType)
		{
			if (releaseType == Constants.EventReferenceReleaseTypes.Codes.Original)
			{
				return Res.GetString("2c725fc5-a767-4214-8567-578b162f741a", "Export Pre-Advice");
			}

			if (releaseType == Constants.EventReferenceReleaseTypes.Codes.Revised
				|| releaseType == Constants.EventReferenceReleaseTypes.Codes.Reprint)
			{
				return Res.GetString("b88522a3-c7ba-47c4-b7a1-8ec9b0641e88", "Export Pre-Advice Replacement");
			}

			if (releaseType == Constants.EventReferenceReleaseTypes.Codes.Cancellation)
			{
				return Res.GetString("00b4f6eb-1f95-4fa9-b5a1-a6c346863deb", "Export Pre-Advice Cancellation");
			}

			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Release Type not expected: {0}", releaseType));
		}

		public void CreateMessageEvent(ZString releaseType)
		{
			var eventType = releaseType == Constants.EventReferenceReleaseTypes.Codes.Cancellation ? Events.MessageWithdrawCancelRequest : Events.MessageSent;

			var parameters = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.MessageType, GetMessageType(releaseType))
			};

			if (AgencyRegistry.Instance.ContainerExportPreAdvicePorts.FindByPortAndPrincipalWithFallback(shipment.JS_NKLoadPort, shipment.JS_OH_DeliveryAgent)?.Enabled ?? false)
			{
				parameters.Add(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, Constants.Facilities.Desc.Terminal));
				parameters.Add(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Location, shipment.JS_NKLoadPort));
			}
			else
			{
				parameters.Add(new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, MessageSentToDepartment));
			}

			shipment.Logs.AddNew(eventType, ZString.Empty, parameters.ToArray());
		}
	}
}
