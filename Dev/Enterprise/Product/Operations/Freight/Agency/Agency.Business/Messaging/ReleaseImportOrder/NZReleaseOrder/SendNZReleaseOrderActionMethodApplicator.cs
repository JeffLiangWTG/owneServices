using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business
{
	public class SendNZReleaseOrderActionMethodApplicator : NZReleaseOrderActionMethodApplicator
	{
		public SendNZReleaseOrderActionMethodApplicator(ReleaseImportOrderSettings settings)
			: base(Res.GetString("76a21c5e-9604-11e4-9454-902b34dc814a", "Send Import Release Order Message"), settings)
		{
		}

		const int pinLength = 6;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is a part of a EventReference which is not localizable, Service task logs aren't res strings")]
		public override void SendMessage(INotifications logger, BillOfLadingContainer sourceContainer)
		{
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var factory = new BusinessObjectFactory
				{
					NameForDebugging = "NZReleaseOrderMethodApplicatorFactory"
				};
				using (factory.AddDisposableService())
				{
					var container = (BillOfLadingContainer)factory.ImportFromAnotherFactory(sourceContainer);
					var pinGenerator = new ContainerPinGenerator { PinLength = pinLength, CharacterType = ContainerPinGenerator.CharacterTypes.Numeric };
					pinGenerator.PopulateEmptyPins(new[] { container });

					using (var exporter = new ManualDataExport(container.Factory, container, UniversalDataType.UniversalShipment, GetCommunicationModes(container), UniversalXmlSchema.Version_2012_11_DO_NOT_USE, dataWriterGetter: GetDataWriterGetter(container, HasContainerBeenSentAlready(container) ? MessagePurposes.Codes.Amendment : MessagePurposes.Codes.Original)))
					{
						exporter.EventCode = Events.MessageSent.Code;
						exporter.RecipientType = nameof(RecipientRoleType.PIR);
						exporter.SendData(logger);

						container.Logs.AddNew(
							Events.MessageSent,
							GetMessageParameters(container, Core.Constants.EventReferenceMessageTypes.ImportReleaseOrder).ToArray());

						container.Logs.CreateRecreateOrUpdateEventLog(
							Events.ReleaseRequested,
							EstimateActual.Actual,
							ZDateTimeOffset.Now,
							ZString.Empty,
							Params.Facility.AsKeyFor(Constants.Facilities.Code.Terminal),
							Params.Location.AsKeyFor(container.Booking.TransportsIncludingRelated.LastLeg.JW_RL_NKDiscPort),
							Params.Department.AsKeyFor((NoResString)"Carrier"));
					}

					factory.Save();
					logger.Add(new InfoNotification((NoResString)"Delivery Succeeded."));
				}
			}, () => logger.AddWarning("Error during delivery. Retrying."));
		}

		bool HasContainerBeenSentAlready(BillOfLadingContainer container)
		{
			bool messageHasBeenSend = false;

			foreach (var log in GetEventLogsInDescendingOrder(container))
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
					case Events.InterchangeSentCode:
						messageHasBeenSend = true;
						break;

					default:
						break;
				}
				break;
			}

			return messageHasBeenSend;
		}

		IEnumerable<StmALog> GetEventLogsInDescendingOrder(BillOfLadingContainer container)
		{
			foreach (var log in container.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var logMessageType = log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType);

				if (string.Compare(logMessageType, Core.Constants.EventReferenceMessageTypes.ImportReleaseOrder, System.StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}
	}
}
