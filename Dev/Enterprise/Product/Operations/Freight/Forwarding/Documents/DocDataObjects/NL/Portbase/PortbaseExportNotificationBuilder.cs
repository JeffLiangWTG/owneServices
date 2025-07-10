using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseExportNotificationBuilder
	{
		public PortbaseExportNotificationBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;

		public PortbaseExportNotification Build()
		{
			var portbase = new PortbaseExportNotification(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef,
				DataContext.NLPortbaseExportNotification);

			portbase.ConsolNumber = consol.JK_UniqueConsignRef;
			portbase.BookingConfirmationReference = consol.JK_BookingReference;
			var departNLSeaLeg = SeaTransportsInLegOrder.LastOrDefault(t => t.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.Netherlands, StringComparison.OrdinalIgnoreCase));
			portbase.OperationalPort = Unloco.Create(context, departNLSeaLeg?.LoadPort);

			portbase.SelectAllToSend = ZBool.True;

			PopulateSendersCustomsNo(portbase);
			PopulateTerminal(portbase);
			PopulateAddresses(portbase);
			PopulateDocuments(portbase);

			AddValidationsFields(portbase);
			portbase.ValidateAllIncludingChildren();

			return portbase;
		}

		#region SendersCustomsNo

		void PopulateSendersCustomsNo(PortbaseExportNotification portbase)
		{
			bool IsEORICode(OrgCusCode code)
			{
				return code.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			}

			var eoriCode = GlbBranch.CurrentBranch?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsEORICode)
				?? GlbCompany.CurrentCompany?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsEORICode);

			portbase.SendersCustomsNo = eoriCode?.OK_CustomsRegNo ?? ZString.Empty;
		}

		#endregion

		#region Terminal

		void PopulateTerminal(PortbaseExportNotification portbase)
		{
			var terminalCustomsCode = consol.DepartureCTOAddress?.Header.CustomsCodes.OfType<OrgCusCode>()
													.FirstOrDefault(c => c.OK_CodeType == OrgCusCode.NetherlandsCodeTypes.FenexLocationCode && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Netherlands);
			if (terminalCustomsCode != null)
			{
				portbase.TerminalFenexRegNo = terminalCustomsCode.OK_CustomsRegNo;
			}
			else
			{
				portbase.TerminalFenexRegNo = ZString.Empty;
			}

			portbase.TerminalDescription = consol.DepartureCTOAddress?.Header?.OH_FullName ?? ZString.Empty;
			portbase.IsFerryTerminal = consol.DepartureCTOAddress?.Header?.OH_IsFerryWaterTerminal ?? ZBool.False;
		}

		#endregion

		#region Addresses

		void PopulateAddresses(PortbaseExportNotification portbase)
		{
			portbase.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			portbase.DepartureCTO = AddressBuilder.Create(context, consol?.DepartureCTOAddress);
		}

		#endregion

		#region Documents-Containers-Shipments

		struct PackingLineWithContainer
		{
			public ForwardingPackLine PackingLine { get; set; }
			public CommonContainer Container { get; set; }
		}

		Dictionary<string, List<PackingLineWithContainer>> RetrievePackingLinesByMrn(ZBool isFerryTerminal)
		{
			var packingLinesByMrn = new Dictionary<string, List<PackingLineWithContainer>>();

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
				{
					var container = packLine.GetContainer(consol);
					var mrn = GetExportRefNumberWithFallback(shipment, packLine);

					if (container == null || !isFerryTerminal && (container?.JC_ContainerNum.IsEmpty ?? false) || mrn.IsEmpty)
					{
						continue;
					}

					if (!packingLinesByMrn.TryGetValue(mrn, out var packingLines))
					{
						packingLines = new List<PackingLineWithContainer>();
						packingLinesByMrn.Add(mrn, packingLines);
					}

					packingLines.Add(new PackingLineWithContainer
					{
						Container = container,
						PackingLine = packLine
					});
				}
			}

			return packingLinesByMrn;
		}

		void PopulateDocuments(PortbaseExportNotification portbase)
		{
			var packingLinesByMrn = RetrievePackingLinesByMrn(portbase.IsFerryTerminal);

			var portbaseDocuments = new List<PortbaseDocument>();

			foreach (var packingLinesWithMrn in packingLinesByMrn)
			{
				var mrn = packingLinesWithMrn.Key;

				if (!packingLinesByMrn.TryGetValue(mrn, out var packingLinesWithContainer))
				{
					continue;
				}

				var portbaseDocument = new PortbaseDocument(mrn)
				{
					ReferenceNumber = mrn,
					EntryType = new CodeDescription(new EntryTypesExport()),
					Status = GetLatestEventLogDetails(mrn),
					IsSelectedToSend = portbase.SelectAllToSend,
					CanWithdraw = CanWithdraw(mrn)
				};

				portbaseDocument.OnValueChanged(nameof(portbaseDocument.IsSelectedToSend)).Do(() =>
				{
					portbase.SelectAllToSend = portbase.Documents.All(d => d.IsSelectedToSend);
					portbase.Validate(nameof(portbase.SelectAllToSend));
					portbaseDocument.ValidateAllIncludingChildren();
				});

				var portbaseContainers = new List<PortbaseContainer>();

				var packingLinesPerContainer = packingLinesWithContainer.GroupBy(p => p.Container.PK);

				foreach (var packingLinePerContainer in packingLinesPerContainer)
				{
					var containerNumber = packingLinePerContainer.First().Container.JC_ContainerNum;

					var portbaseContainer = new PortbaseContainer(string.Format("{0}_{1}", mrn, packingLinePerContainer.Key))
					{
						Type = (containerNumber.IsEmpty && portbase.IsFerryTerminal ? DutchPortsConstants.Types.Equipment : DutchPortsConstants.Types.Container),
						Number = containerNumber,
						GrossWeight = new Measurement
						{
							Value = packingLinePerContainer.First().Container.JC_GrossWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = packingLinePerContainer.First().Container.JC_GrossWeightUQ
							}
						},
						IsNonOperativeReefer = packingLinePerContainer.First().Container.JC_IsNonOperativeReefer
					};

					var shipments = packingLinePerContainer.Select(p => new PortbaseShipment
					{
						Number = p.PackingLine.Shipment.JS_UniqueConsignRef,
						Quantity = p.PackingLine.JL_PackageCount,
						PackageType = new CodeDescription(p.PackingLine.Lookups.PackTypes)
						{
							Code = p.PackingLine.JL_F3_NKPackType
						},
						Weight = new Measurement
						{
							Value = p.PackingLine.JL_ActualWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = p.PackingLine.JL_ActualWeightUQ
							}
						}
					});

					portbaseContainer.Shipments = shipments.ToArray();

					portbaseContainers.Add(portbaseContainer);
				}

				portbaseDocument.Containers = portbaseContainers.ToArray();

				portbaseDocuments.Add(portbaseDocument);
			}

			portbase.Documents = portbaseDocuments;
		}

		ZString GetExportRefNumberWithFallback(ForwardingShipment shipment, PackLine packLine)
		{
			var mrn = string.IsNullOrWhiteSpace(packLine.JL_ExportRefNumber)
				? shipment.CustomsEntryNumber
				: packLine.JL_ExportRefNumber;

			return mrn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		String GetLatestEventLogDetails(string mrn)
		{
			foreach (var log in GetEventLogsForMRNInDescendingOrder(mrn))
			{
				return String.Format("{0:dd-MMM-yyyy HH:mm} - {1}", log.SL_EventTime, log.SL_EventDescription);
			}
			return Res.GetString("f2ccbec6-3ddd-45ac-a72a-825cb92d158c", "No responses received for this MRN");
		}

		Boolean CanWithdraw(string mrn)
		{
			var lastEventCode = GetEventLogsForMRNInDescendingOrder(mrn)
				.Where(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode || log.SL_SE_NKEvent == Events.MessageWithdrawCancelAcceptedCode)
				.Select(log => log.SL_SE_NKEvent)
				.FirstOrDefault();
			return (lastEventCode == Events.MessageAcceptedCode);
		}

		IEnumerable<StmALog> GetEventLogsForMRNInDescendingOrder(string mrn)
		{
			return consol
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.Where(log => !log.SL_IsCancelled && IsPortbasExportNotificationLog(log) && log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var mrnlog) && mrnlog.ToUpper() == mrn);
		}

		bool IsPortbasExportNotificationLog(StmALog log)
		{
			switch (log.SL_SE_NKEvent)
			{
				case Events.MessageSentCode:
				case Events.MessageWithdrawCancelRequestCode:
				case Events.InterchangeReceiptAcknowledgedCode:
				case Events.InterchangeRejectedCode:
				case Events.MessageAcceptedCode:
				case Events.MessageReceivedCode:
				case Events.MessageRejectedCode:
				case Events.MessageWithdrawCancelAcceptedCode:
				case Events.StatusUpdatedCode:
					return (string.Compare(log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.Department), DutchPortsConstants.Departments.Portbase, StringComparison.OrdinalIgnoreCase) == 0 &&
						string.Compare(log.Parameters.GetValueSafe(Constants.EventReferenceParameters.Codes.MessageType), DutchPortsConstants.MessageTypes.ExportNotification, StringComparison.OrdinalIgnoreCase) == 0);
				default:
					return false;
			}
		}

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInLegOrder => seaTransportsInLegOrder ?? (seaTransportsInLegOrder = GetSeaTransports());
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInLegOrder;

		IReadOnlyCollection<Freight.Business.Transport> GetSeaTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)
				.ToArray();
		}

		void AddValidationsFields(PortbaseExportNotification portbase)
		{
			portbase.BookingConfirmationReferenceInfo.AddErrorIfEmpty(Res.GetString("cd32ef7f-07c8-4dda-b75e-22a50adc0b61", "Carrier Booking Reference is mandatory. Refer to Consol > Details > Organizations > Carrier Booking Reference."));
			portbase.SendersCustomsNoInfo.AddMessageErrorIfEmpty(Res.GetString("d426bcaa-7133-44d9-875e-621c4c419215", "EORI number is mandatory, the number defaults from the logged in Organization Proxy > Config > EORI."));
			portbase.TerminalFenexRegNoInfo.AddMessageErrorIfEmpty(Res.GetString("f8b74d08-b78e-4df2-aee9-75ea90dfed26", "The FENEX location code of the CTO organization is mandatory, it is configured under Organization > Details > Config > Registration Numbers."));
			portbase.SelectAllToSendInfo.AddMessageError(() => portbase.Documents.Any() && !portbase.Documents.Any(document => document.IsSelectedToSend), Res.GetString("77c537bb-2f26-447d-80d1-e3738bc4e2ea", "You must select at least one document to send the message."));

			void AddMissingContactDetailsValidation(Address address)
			{
				var errorMessage = Res.GetString("06ea1339-8a05-4aeb-8618-3f163ac1406e", "Your staff login profile does not specify contact details. At least one communication number (email or phone) is required.");

				address.EmailInfo.AddMessageError(() => address.Email.IsEmpty && address.Phone.IsEmpty, errorMessage);
				address.PhoneInfo.AddMessageError(() => address.Email.IsEmpty && address.Phone.IsEmpty, errorMessage);
			}

			AddMissingContactDetailsValidation(portbase.CurrentUser);

			foreach (var document in portbase.Documents)
			{
				document.ReferenceNumberInfo.AddWarning(() => document.ReferenceNumber.Length > 27, Res.GetString("2c1003a0-ca1f-44d3-8030-24848a9a329c", "Portbase message accepts only up to 27 characters, extra characters will be truncated when sending export notification message to Portbase."));
				((CodeDescription)document.EntryType).CodeInfo.AddMessageError(() => document.IsSelectedToSend && document.EntryType.Code.IsEmpty, Res.GetString("21314229-700d-4ebc-804f-52568240736a", "Entry Type is mandatory."));

				foreach (var container in document.Containers)
				{
					container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("f19b6900-60fe-4992-8534-346e715579eb", "Container/Equipment Number is mandatory. Refer to Consol > Containers"));
					container.GrossWeight.ValueInfo.AddMessageError(() => container.GrossWeight.Value == 0, Res.GetString("7a3a1fef-053f-4cfb-8924-75dcb2c14100", "Container/Equipment Gross Weight is required."));

					foreach (var shipment in container.Shipments)
					{
						shipment.Weight.ValueInfo.AddMessageError(() => shipment.Weight.Value == 0, Res.GetString("06da7a31-fea8-462e-a3b3-9daf780d97fe", "Weight is required."));
					}
				}
			}
		}

		#endregion
	}
}
