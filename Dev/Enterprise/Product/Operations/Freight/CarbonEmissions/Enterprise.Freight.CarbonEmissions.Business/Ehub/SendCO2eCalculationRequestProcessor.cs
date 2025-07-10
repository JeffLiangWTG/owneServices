using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class SendCO2eCalculationRequestProcessor : ICO2eCalculationRequestProcessor
	{
		public SendCO2eCalculationRequestProcessor(BusinessObject businessObject, IUniversalActionInfo actionInfo, IEventInfo eventInfo = null, IAddressesValidationManager addressValidationManager = default, IUniversalXmlWorkflowProcessor requestSender = default, bool skipValidateInputs = false, ICO2eRecalculationChecker recalculationChecker = null)
		{
			this.businessObject = Argument.NotNull(businessObject, nameof(businessObject));
			this.actionInfo = Argument.NotNull(actionInfo, nameof(actionInfo));
			this.eventInfo = eventInfo;
			this.skipValidateInputs = skipValidateInputs;
			this.addressValidationManager = addressValidationManager;
			this.requestSender = requestSender;
			this.recalculationChecker = recalculationChecker;
		}

		readonly BusinessObject businessObject;
		readonly IUniversalActionInfo actionInfo;
		readonly IEventInfo eventInfo;
		readonly IAddressesValidationManager addressValidationManager;
		readonly bool skipValidateInputs;
		readonly IUniversalXmlWorkflowProcessor requestSender;
		readonly ICO2eRecalculationChecker recalculationChecker;

		public IMessageProcessorCommunicationModesResult GetDestinations() => new UniversalXmlCommunicationModeProvider(() => (CommunicationModes.Cast<IEDICommunicationsMode>().ToArray(), null));

		public void Process(INotifications notifications, CancellationToken token = new CancellationToken())
		{
			HasSupportersToSend = false;

			if (businessObject is ICO2eCalculationSupporter hostSupporter)
			{
				if (!skipValidateInputs)
				{
					var invalidFields = hostSupporter.ValidateInputsWithAdditionalSupporter();
					if (invalidFields.Count > 0)
					{
						notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"The greenhouse gas emissions calculation cannot be requested because the following mandatory input is missing or invalid: {string.Join(", ", invalidFields)}");
						return;
					}
				}
				var supportersBOsToSend = GetSupportersToSend(hostSupporter);

				if (supportersBOsToSend.Length == 0)
				{
					return;
				}

				HasSupportersToSend = true;

				for (var i = 0; i < supportersBOsToSend.Length; i++)
				{
					Process(supportersBOsToSend[i], notifications, token, i == 0 ? null : hostSupporter);
				}
			}
		}

		void Process(ICO2eCalculationSupporter supporter, INotifications notifications, CancellationToken token, ICO2eCalculationSupporter hostSupporter = null)
		{
			if (supporter is not BusinessObject bizo)
			{
				return;
			}

			ValidateAddressIfNeeded(supporter);
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = x =>
			{
				return hostSupporter == null ? CO2eHelper.GetCO2eRequestDataObjectWriter(bizo, x) : CO2eHelper.GetCO2eRequestDataObjectWriter(bizo, x, hostSupporter);
			};
			var cO2RequestSender = requestSender ?? UniversalXmlWorkflowProcessorBuilder.New(
				bizo.PK == businessObject.PK ? actionInfo : new ManualCO2eCalculationActionInfo(bizo),
				GetDestinations(),
				dataWriterGetter,
				bizo,
				eventInfo,
				new CO2eXmlWriter(),
				UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			cO2RequestSender.Process(notifications, token);
			supporter.OnRequested();
		}

		void ValidateAddressIfNeeded(ICO2eCalculationSupporter supporter)
		{
			if (supporter is IAddressesValidation cO2eAddressValidation
				&& cO2eAddressValidation.AddressesToValidate.Length > 0)
			{
				(addressValidationManager ?? new CO2eAddressValidationManager(cO2eAddressValidation.AddressesToValidate)).Validate();
			}
		}

		IEnumerable<NonPersistentEDICommunicationMode> CommunicationModes
		{
			get
			{
				return communicationModes ?? new[]
				{
					new NonPersistentEDICommunicationMode
					{
						EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
						EK_Destination = CO2eHelper.CO2eCalculationID,
						EK_PublishInternalMilestones = false
					}
				};
			}
		}
		readonly IEnumerable<NonPersistentEDICommunicationMode> communicationModes;

		ICO2eCalculationSupporter[] GetSupportersToSend(ICO2eCalculationSupporter hostSupporter)
		{
			if (!ShouldRecalculate(hostSupporter))
			{
				return Array.Empty<ICO2eCalculationSupporter>();
			}

			return hostSupporter.AdditionalCalculationSupporters
				.Where(x => x.Supporter.GetCO2eStatus() != CO2eStatusList.Codes.Current)
				.Select(x => x.Supporter)
				.Prepend(hostSupporter)
				.ToArray();
		}

		public bool ShouldRecalculate(ICO2eCalculationSupporter supporter)
		{
			if (supporter.GetCO2eStatus() != CO2eStatusList.Codes.Current)
			{
				return true;
			}

			if (recalculationChecker == null)
			{
				supporter.LogGHGEvent(CO2eEventType.NotRequired, Res.GetString("9dd771e3-71ac-4965-b304-c597e7549cc6", "the CO2e value is current."));
				return false;
			}

			return recalculationChecker.ShouldRecalculate(supporter);
		}

		public bool HasSupportersToSend { get; private set; }
	}
}
