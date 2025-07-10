using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseImportNotificationBuilder
	{
		public PortbaseImportNotificationBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		readonly IContext context;

		public PortbaseImportNotification Build()
		{
			var portbase = new PortbaseImportNotification(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef,
				DataContext.NLPortbaseImportNotification);

			portbase.ConsolNumber = consol.JK_UniqueConsignRef;
			portbase.CarrierBookingRef = consol.JK_BookingReference;

			PopulateTransportMode(portbase);
			PopulateSendersCustomsNo(portbase);
			PopulateReceivingPort(portbase);
			PopulateTerminal(portbase);
			PopulateDocuments(portbase);

			portbase.CurrentUser = AddressBuilder.CreateForCurrentUser(context).AddAsciiCharactersValidation();

			portbase.ValidateAllIncludingChildren();

			return portbase;
		}

		#region TransportMode

		void PopulateTransportMode(PortbaseImportNotification portbase)
		{
			portbase.TransportMode = new CodeDescription(new PortbaseTransportModes())
			{
				Code = GetTransportModeOfTheOnForwardingLeg()
			};

			((CodeDescription)portbase.TransportMode).CodeInfo.AddMessageErrorIfEmpty((NoResString)"Transport Mode is required."); // Fixed error message
		}

		ZString GetTransportModeOfTheOnForwardingLeg()
		{
			var retrieveNext = false;
			foreach (var transport in consol.Transports.OfType<Freight.Business.Transport>().OrderBy(t => t.JW_LegOrder))
			{
				if (retrieveNext)
				{
					return transport.JW_TransportMode;
				}
				if (transport.JW_RL_NKDiscPort.StartsWith(Constants.CountryCodes.Netherlands, StringComparison.OrdinalIgnoreCase) && transport.IsSea)
				{
					retrieveNext = true;
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region SendersCustomsNo

		void PopulateSendersCustomsNo(PortbaseImportNotification portbase)
		{
			bool IsEORICode(OrgCusCode code)
			{
				return code.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			}

			var eoriCode = GlbBranch.CurrentBranch?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsEORICode)
				?? GlbCompany.CurrentCompany?.OrgProxy?.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(IsEORICode);

			portbase.SendersCustomsNo = eoriCode?.OK_CustomsRegNo ?? ZString.Empty;
			portbase.SendersCustomsNoInfo.AddMessageErrorIfEmpty((NoResString)"EORI number is mandatory, the number defaults from the logged in Organization Proxy > Config > EORI."); // Fixed error message
		}

		#endregion

		#region Receiving Port

		void PopulateReceivingPort(PortbaseImportNotification portbase)
		{
			var arrivalNLSeaLeg = SeaNLTransportsInLegOrder.LastOrDefault();
			portbase.ReceivingPort = Unloco.Create(context, arrivalNLSeaLeg?.DiscPort);

			((Unloco)portbase.ReceivingPort).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("0E6E25B4-2350-43D0-8164-B5894DDB99CA", "Receiving Port is required."));
		}

		#endregion

		#region Terminal

		void PopulateTerminal(PortbaseImportNotification portbase)
		{
			var terminalCustomsCode = consol.ArrivalCTOAddress?.Header.CustomsCodes.OfType<OrgCusCode>()
													.FirstOrDefault(c => c.OK_CodeType == OrgCusCode.NetherlandsCodeTypes.FenexLocationCode && c.OK_RN_NKCodeCountry == Constants.CountryCodes.Netherlands);
			if (terminalCustomsCode != null)
			{
				portbase.TerminalFenexRegNo = terminalCustomsCode.OK_CustomsRegNo;
				portbase.TerminalDescription = consol.ArrivalCTOAddress.Header.OH_FullName;
				portbase.CTO = AddressBuilder.Create(context, consol.ArrivalCTOAddress).AddAsciiCharactersValidation();
			}
			else
			{
				portbase.TerminalFenexRegNo = ZString.Empty;
				portbase.TerminalDescription = ZString.Empty;
				portbase.CTO = AddressBuilder.Create(context, (IAddress)null);
			}
			portbase.TerminalFenexRegNoInfo.AddMessageErrorIfEmpty((NoResString)"The FENEX location code of the CTO organization is mandatory, it is configured under Organisation > Details > Config > Registration Numbers."); // Fixed error message
			portbase.IsFerryTeminal = consol.ArrivalCTOAddress != null && consol.ArrivalCTOAddress.Header.OH_IsFerryWaterTerminal;
		}

		#endregion

		#region Documents-Containers-Shipments

		struct PackingLineWithContainer
		{
			public ForwardingPackLine PackingLine { get; set; }
			public CommonContainer Container { get; set; }
		}

		Dictionary<string, List<PackingLineWithContainer>> RetrievePackingLinesByMrn()
		{
			var packingLinesByMrn = new Dictionary<string, List<PackingLineWithContainer>>();

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
				{
					var container = packLine.GetContainer(consol);
					var mrn = GetImportRefNumberWithFallback(shipment, packLine);

					if (container == null || container.JC_ContainerNum.IsEmpty || mrn.IsEmpty)
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

		void PopulateDocuments(PortbaseImportNotification portbase)
		{
			var packingLinesByMrn = RetrievePackingLinesByMrn();

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
					EntryType = new CodeDescription(new EntryTypes())
				};

				portbaseDocument.ReferenceNumberInfo.AddWarning(() => portbaseDocument.ReferenceNumber.Length > 27, (NoResString)"Portbase message accepts only up to 27 characters, extra characters will be truncated when sending import notification message to Portbase."); // Fixed error message

				((CodeDescription)portbaseDocument.EntryType).CodeInfo.AddMessageErrorIfEmpty((NoResString)"Entry Type is mandatory."); // Fixed error message

				var portbaseContainers = new List<PortbaseContainer>();

				var packingLinesPerContainer = packingLinesWithContainer.GroupBy(p => p.Container.JC_ContainerNum);

				foreach (var packingLinePerContainer in packingLinesPerContainer)
				{
					var containerNumber = packingLinePerContainer.Key;

					var portbaseContainer = new PortbaseContainer
					{
						Number = containerNumber,
						CarrierBookingRef = consol.JK_BookingReference,
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

					if (portbase.IsFerryTeminal)
					{
						portbaseContainer.CarrierBookingRefInfo.AddMessageErrorIfEmpty(Res.GetString("C5C2BC55-6C54-4BF1-B79F-7920D1FE8B27", "Carrier Booking Reference is required."));
					}

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

		ZString GetImportRefNumberWithFallback(ForwardingShipment shipment, PackLine packLine)
		{
			var mrn = string.IsNullOrWhiteSpace(packLine.JL_ImportRefNumber)
				? shipment.CustomsEntryNumber
				: packLine.JL_ImportRefNumber;

			return mrn;
		}

		#endregion

		#region Implementation

		IReadOnlyCollection<Freight.Business.Transport> SeaNLTransportsInLegOrder => seaNLTransportsInLegOrder ?? (seaNLTransportsInLegOrder = GetSeaNLTransports());
		IReadOnlyCollection<Freight.Business.Transport> seaNLTransportsInLegOrder;

		IReadOnlyCollection<Freight.Business.Transport> GetSeaNLTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Constants.TransportModes.Sea)
				.Where(t => t.JW_RL_NKDiscPort.StartsWith(Constants.CountryCodes.Netherlands, StringComparison.OrdinalIgnoreCase))
				.ToArray();
		}
		#endregion
	}
}
