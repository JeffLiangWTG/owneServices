using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.Business
{
	static class ReleaseMessageHelper
	{
		internal static ReleaseMessageStrategy[] GetStrategies(AgencyBooking shipment)
		{
			return GetAllStrategies(shipment).Where(x => x.IncludeStrategy).ToArray();
		}

		static IEnumerable<ReleaseMessageStrategy> GetAllStrategies(AgencyBooking shipment)
		{
			yield return new ReleaseMessageStrategy(shipment);
		}

		internal static void SendUXml(ReleaseHeader header, ZString eventReference, INotifications notifications, ZString purposeCode, ZString documentName)
		{
			var actionInfo = new ReleaseActionInfo(header);
			var eventInfo = new ReleaseEventInfo(eventReference);
			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
			{
				var communicationsModes = header.MessageStrategies.Select(CreateCommunicationsMode).ToArray();
				if (communicationsModes.Any())
				{
					return (communicationsModes, null);
				}
				else
				{
					return (Array.Empty<IEDICommunicationsMode>(), ResString.GetMultilingualString("2ef83084-0c49-4435-a151-84cfb30d758b", "No messaging strategies for Release Header [{0}]", header));
				}
			});

			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = x =>
				new AgencyBookingExportPreAdviceDataObjectWriter(header, x, documentName, purposeCode);

			var processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo, communicationModesProvider, dataWriterGetter, header.Shipment, eventInfo, null, UniversalXmlSchema.Version_2012_11_DO_NOT_USE);
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(notifications, replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		static IEDICommunicationsMode CreateCommunicationsMode(ReleaseMessageStrategy strategy)
		{
			var communicationsMode = new NonPersistentEDICommunicationMode();
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = strategy.EHubID;

			return communicationsMode;
		}
	}
}
