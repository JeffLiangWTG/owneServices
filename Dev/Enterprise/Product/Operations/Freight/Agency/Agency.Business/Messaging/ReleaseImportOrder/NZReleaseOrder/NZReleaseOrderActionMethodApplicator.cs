using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class NZReleaseOrderActionMethodApplicator : ReleaseImportOrderActionMethodApplicator
	{
		protected NZReleaseOrderActionMethodApplicator(string name, ReleaseImportOrderSettings settings)
			: base(name)
		{
			Settings = settings;
		}

		#region ReleaseImportOrderActionMethodApplicator

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;

			if (orgProxy != null)
			{
				SendMessage(log, targets.Cast<BillOfLadingContainer>().ToArray());
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("af7114a4-a5b0-11e4-a829-902b34dc814a", "The current branch does not have an org. proxy."));
			}
		}
		#endregion

		#region Implementation

		public abstract void SendMessage(INotifications log, BillOfLadingContainer container);

		protected IEnumerable<NonPersistentEDICommunicationMode> GetCommunicationModes(BillOfLadingContainer container)
		{
			return new[]
			{
				new NonPersistentEDICommunicationMode
				{
					EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
					EK_Destination = GetEHubID(container)
				}
			};
		}

		void SendMessage(IOperationalActionSectionLog log, BillOfLadingContainer[] containers)
		{
			var logger = new NotificationsLogger(log);
			var abortIsPending = false;

			log.SetSectionProgressMax(containers.Length);

			foreach (var container in containers)
			{
				if (!IsEHubIDSet(container))
				{
					log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("009127cc-a5a8-11e4-a5f5-902b34dc814a", "The Import Release Order message cannot be sent as eHub ID is not set."));
					log.BumpSectionProgress();
					continue;
				}

				if (abortIsPending)
				{
					log.BumpSectionProgress();

					continue;
				}

				if (!IsApplicable(container, log))
				{
					log.BumpSectionProgress();

					continue;
				}

				if (Validate(container, log))
				{
					if (AllowSendWhileResponsePending || !IsPendingResponse(container))
					{
						SendMessage(logger, container);
					}
					else
					{
						log.NotifyFormat(
							OperationalActionLogErrorLevel.Error,
							Res.GetString("2b80a4b0-a292-11e4-b384-902b34dc814a", "{0:G} is still waiting on a response."),
							HyperlinkHelper.Link(container));
					}
				}
				else
				{
					abortIsPending = Settings.ErrorBehaviour == OperationalActionErrorBehaviourList.Codes.Abort;
				}

				log.BumpSectionProgress();
			}
		}

		bool IsEHubIDSet(BillOfLadingContainer container)
		{
			return !string.IsNullOrEmpty(GetEHubID(container));
		}

		ZString GetEHubID(BillOfLadingContainer container)
		{
			return ShippingPortsMessagingEHubIDHelper.GetEHubID(container.Booking?.JS_NKDischargePort ?? ZString.Empty);
		}

		ReleaseImportOrderSettings Settings
		{
			[DebuggerStepThrough]
			get;
			set;
		}

		protected List<KeyValuePair<string, string>> GetMessageParameters(BillOfLadingContainer container, string messageType)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(Params.MessageType.AsKeyFor(messageType));
			ZBool registryValidation = false;
			if (container.Booking != null)
			{
				var registry = RetrievePortConfiguration(container.Booking.JS_NKDischargePort, container.Booking.Principal);
				registryValidation = registry != null ? registry.Enabled : ZBool.False;
			}

			if (registryValidation)
			{
				parameters.Add(Params.Department.AsKeyFor((NoResString)"Terminal"));
				parameters.Add(Params.Location.AsKeyFor(container.Booking.TransportsIncludingRelated.LastLeg.JW_RL_NKDiscPort));
			}
			else
			{
				parameters.Add(Params.Department.AsKeyFor((NoResString)"NZ Ports"));
			}

			return parameters;
		}

		bool IsApplicable(BillOfLadingContainer container, IOperationalActionSectionLog log)
		{
			var portConfig = RetrievePortConfiguration(container.Booking.JS_NKDischargePort, container.Booking.Principal);
			if (portConfig == null || !portConfig.Enabled)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("b307dcbb-e4a0-41f5-9bf0-e70ea0ab79b3", "Principal is not configured for sending Import Release Order to {0}.", container.Booking.JS_NKDischargePort),
					HyperlinkHelper.Link(container));

				return false;
			}

			var carrierPrincipalCode = GlbBranch.CurrentBranch.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, container.Booking.JS_NKDischargePort.SubstringSafe(0, 2)) ?? string.Empty;
			if (carrierPrincipalCode.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("da4b2ba0-9cad-4ca8-a084-f615debf6c88", "The current branch's organization proxy does not have a CAR code for the country/region of container's <{0:G}> port of discharge entered. The container is skipped"),
					HyperlinkHelper.Link(container));

				return false;
			}

			return true;
		}

		public PortMessagingPort RetrievePortConfiguration(ZString dischargePort, OrgHeader principal)
		{
			var portConfig = SearchPortConfiguration(dischargePort, principal, AgencyRegistry.Instance.ImportReleaseOrderPorts.Value);
			if (portConfig == null || !portConfig.Enabled)
			{
				var systemLevelRetriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.ImportReleaseOrderPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				portConfig = SearchPortConfiguration(dischargePort, principal, (PortMessagingPortCollection)systemLevelRetriever.GetCurrentValue().Value);
			}

			return portConfig;
		}

		PortMessagingPort SearchPortConfiguration(ZString dischargePort, OrgHeader principal, PortMessagingPortCollection ports)
		{
			var principalPK = principal != null ? principal.PK : ZGuid.Empty;

			var portConfig = ports.OfType<PortMessagingPort>().FirstOrDefault(x => x.Port == dischargePort.ToString() && x.PrincipalPK == principalPK);
			if (portConfig == null && principal != null && !principalPK.IsEmpty)
			{
				portConfig = ports.OfType<PortMessagingPort>().FirstOrDefault(x => x.Port == dischargePort.ToString() && x.PrincipalPK == ZGuid.Empty);
			}

			return portConfig;
		}

		bool Validate(BillOfLadingContainer container, IOperationalActionSectionLog log)
		{
			NZReleaseOrderMessageValidationStrategy.RegisterForFactory(container.Factory);
			container.MarkAsNeedingValidation();
			container.Booking.MarkAsNeedingValidation();
			container.Booking.RunPreSaveValidation();

			if (container.Booking.HasErrors || container.Booking.HasMessageErrors)
			{
				log.NotifyFormat(
					Settings.ErrorBehaviour == OperationalActionErrorBehaviourList.Codes.Skip ? OperationalActionLogErrorLevel.Warning : OperationalActionLogErrorLevel.Error,
					Res.GetString("92252a94-a1ed-11e4-a110-902b34dc814a", "'{0:G}' has errors and/or message errors.\r\nYou will need to correct these before an Import Release Order message can be sent for it."),
					HyperlinkHelper.Link(container.Booking));

				return false;
			}

			return true;
		}

		protected Func<IDataWritingManager, ITopLevelDataObjectWriter> GetDataWriterGetter(BillOfLadingContainer container, ZString messagePurpose)
		{
			var registry = RetrievePortConfiguration(container.Booking.JS_NKDischargePort, container.Booking.Principal);
			var senderID = registry != null ? registry.SenderID : ZString.Empty;
			var port = registry != null ? registry.Port : ZString.Empty;
			var vesselLloydsNumber = container.Booking.TransportsIncludingRelated?.LastLeg?.Vessel?.RV_LloydsNumber ?? ZString.Empty;

			return dataWritingManager => new NZReleaseOrderDataObjectWriter(dataWritingManager, senderID, port, vesselLloydsNumber, messagePurpose);
		}

		#endregion

		#region Types

		protected class NotificationsLogger : INotifications
		{
			public NotificationsLogger(IOperationalActionSectionLog log)
			{
				this.log = log;
			}

			public void Add(INotification notification)
			{
				OperationalActionLogErrorLevel errorLevel;

				if (notification.Type.IsFatal || notification.Type == CargoWise.EntityFramework.NotificationType.Error)
				{
					errorLevel = OperationalActionLogErrorLevel.Error;
				}
				else if (notification.Type == CargoWise.EntityFramework.NotificationType.Warning)
				{
					errorLevel = OperationalActionLogErrorLevel.Warning;
				}
				else
				{
					errorLevel = OperationalActionLogErrorLevel.Informational;
				}

				log.Notify(errorLevel, notification.Message);
			}

			readonly IOperationalActionSectionLog log;
		}

		#endregion
	}
}


