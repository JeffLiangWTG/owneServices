using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class OCRConsolWrapper : IOutwardCargoReportHeader
	{
		public OCRConsolWrapper(ForwardingConsol consol, IAdditionalInformation additionalInformation, ZAddress notifyParty, ZString notifyPartyName, ZString notifyPartyEmail, ZString notifyPartyPort)
		{
			this.consol = Argument.NotNull(consol, "Consol cannot be null");
			this.additionalInformation = additionalInformation;
			this.notifyPartyAddress = notifyParty;
			this.notifyPartyName = notifyPartyName;
			this.notifyPartyEmail = notifyPartyEmail;
			this.notifyPartyPort = notifyPartyPort;
		}

		readonly ForwardingConsol consol;
		readonly IAdditionalInformation additionalInformation;
		readonly ZAddress notifyPartyAddress;
		readonly ZString notifyPartyName;
		readonly ZString notifyPartyEmail;
		readonly ZString notifyPartyPort;

		#region IOutwardCargoReportHeader members

		IOrganisationSimple IOutwardCargoReportHeader.Consolidator
		{
			get { return OrgHeaderWrapper.New(GlbCompany.CurrentCompany.OrgProxy); }
		}

		ZBool IOutwardCargoReportHeader.IsSea
		{
			get { return consol.IsSea; }
		}

		ZBool IOutwardCargoReportHeader.IsConsolidation
		{
			// Confirmation from Carl Hagedorn: You are correct for OCRs lodged by a Freight forwarder - effectively all would be consolidation OCRs.
			// If lodged by a Carrier however there would be no need to report the Consolidation elements hence the condition.
			get
			{
				var result = true;
				if (consol.ShippingLine != null && (consol.ShippingLine.PK == GlbCompany.CurrentCompany.OrgProxy.PK || consol.ShippingLine.PK == GlbBranch.CurrentBranch.OrgProxy.PK))
				{
					result = false;
				}

				return result;
			}
		}

		ZString IOutwardCargoReportHeader.TSWReferenceNumber => OCREntryNumber?.CE_EntryNum ?? ZString.Empty;

		ZQuery GetEntryNumberFilter(CommonConsol consol)
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, consol.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypeList.Codes.OutwardReportNumber);
			return filter;
		}

		ZString IOutwardCargoReportHeader.SenderReferenceNumber
		{
			get
			{
				var result = OCRMessageReference.IsEmpty ? consol.JK_UniqueConsignRef : OCRMessageReference;
				if (consol.JK_UniqueConsignRef.Length > 14 || OriginalSentButRejected())
				{
					result = OCRMessageReference = TSWConstants.SendersReferencePlaceHolder;
				}

				return result.Replace("/", "");
			}
		}

		CusEntryNumber OCREntryNumber
		{
			get { return fOCREntryNumber ?? (fOCREntryNumber = consol.Factory.LoadTop1<CusEntryNumber>(GetEntryNumberFilter(consol))); }
		}
		CusEntryNumber fOCREntryNumber;

		ZString OCRMessageReference
		{
			get
			{
				var ocrEntryNumber = OCREntryNumber;
				return ocrEntryNumber == null ? ZString.Empty : ocrEntryNumber.CE_EntryLineReference;
			}
			set
			{
				var ocrEntryNumber = OCREntryNumber;
				if (ocrEntryNumber == null)
				{
					ocrEntryNumber = consol.Factory.New<CusEntryNumber>();
					ocrEntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
					ocrEntryNumber.CE_RN_NKCountryCode = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					ocrEntryNumber.CE_ParentTable = ForwardingConsol.Schema.TableName;
					ocrEntryNumber.CE_ParentID = consol.PK;
				}

				ocrEntryNumber.CE_EntryLineReference = value;
			}
		}

		bool OriginalSentButRejected()
		{
			return OCREntryNumber != null && OCREntryNumber.CE_EntryStatus == "REJ" && OCREntryNumber.CE_EntryNum.IsEmpty;
		}

		IAdditionalInformation IOutwardCargoReportHeader.AdditionalInformation
		{
			get { return additionalInformation; }
		}

		ZString IOutwardCargoReportHeader.MasterBillNumber
		{
			get { return consol.JK_MasterBillNum; }
		}

		ZString IOutwardCargoReportHeader.CraftName
		{
			get { return consol.IsSea ? Transport?.JW_Vessel.ToUpper() ?? ZString.Empty : ZString.Empty; }
		}

		ZString IOutwardCargoReportHeader.LloydsNo
		{
			get
			{
				var result = ZString.Empty;
				if (consol.IsSea && Transport != null && !Transport.JW_Vessel.IsEmpty)
				{
					var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, Transport.JW_Vessel);
					var vessel = consol.Factory.LoadTop1<RefVessel>(vesselQuery);
					if (vessel != null)
					{
						result = vessel.RV_LloydsNumber;
					}
				}

				return result;
			}
		}

		ZString IOutwardCargoReportHeader.VoyageNo
		{
			get { return consol.IsSea ? VoyageFlight.Left(8) : ZString.Empty; }
		}

		ZString IOutwardCargoReportHeader.FlightNo
		{
			get { return consol.IsAir ? VoyageFlight : ZString.Empty; }
		}

		ZDateTime IOutwardCargoReportHeader.DepartureDate
		{
			get
			{
				var transport = Transport;
				return transport != null ? transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD : ZDateTime.Empty;
			}
		}

		IEnumerable<ZString> IOutwardCargoReportHeader.RoutingCountryCodes
		{
			get
			{
				var routingCountriesList = new List<ZString>();
				foreach (Transport routingLegs in consol.Transports)
				{
					var routingPort = routingLegs.JW_RL_NKDiscPort.Left(2);
					if (!routingPort.IsEmpty && routingPort != Core.Constants.CountryCodes.NewZealand)
					{
						if (!routingCountriesList.Contains(routingPort))
						{
							routingCountriesList.Add(routingPort);
						}
					}
				}

				return routingCountriesList;
			}
		}

		IOrganisationSimple IOutwardCargoReportHeader.Carrier => OrgHeaderWrapper.New(consol.ShippingLine);

		ZString IOutwardCargoReportHeader.PortOfDeparture => Transport?.JW_RL_NKLoadPort ?? ZString.Empty;

		IEnumerable<ZString> IOutwardCargoReportHeader.NotifyPartyCodes
		{
			get
			{
				if (notifyPartyAddress != null)
				{
					var notifyParty = this.notifyPartyAddress.OrgHeader as OrgHeader;
					var notifyPartyAddress = this.notifyPartyAddress.OrgAddress as OrgAddress;

					var codeTypes = new[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, OrgCusCode.CodeTypes.CustomsClientCode };
					foreach (var codeType in codeTypes)
					{
						var number = GetCustomsRegNo(notifyParty, notifyPartyAddress, codeType);
						if (!number.IsEmpty)
						{
							yield return number;
						}
					}
				}

				if (!notifyPartyPort.IsEmpty)
				{
					yield return notifyPartyPort;
				}
			}
		}

		ZString GetCustomsRegNo(OrgHeader notifyParty, OrgAddress notifyPartyAddress, ZString code)
		{
			var result = notifyPartyAddress?.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = notifyParty?.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.NewZealand, ZGuid.Empty) ?? ZString.Empty;
			}
			return result;
		}

		ZString IOutwardCargoReportHeader.NotifyPartyName => notifyPartyName;

		ZString IOutwardCargoReportHeader.NotifyPartyEmail => notifyPartyEmail;

		IEnumerable<IOCRConsignment> IOutwardCargoReportHeader.OCRLines
		{
			get
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					if (shipment.JS_JS_ColoadMasterShipment.IsEmpty)
					{
						if (!shipment.CustomsEntryNumber.IsEmpty)
						{
							yield return new ShipmentWrapper(shipment);
						}
						else
						{
							foreach (ForwardingShipment subShipment in shipment.CoLoadShipments)
							{
								yield return new ShipmentWrapper(subShipment);
							}
						}
					}
				}
			}
		}

		ZString VoyageFlight => Transport?.JW_VoyageFlight ?? ZString.Empty;

		Transport Transport => consol.Transports.Cast<Transport>().FirstOrDefault(transport => transport.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.Ordinal)
																							&& !transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.Ordinal));

		IEnumerable<ITransportEquipment> IOutwardCargoReportHeader.Containers
		{
			get
			{
				foreach (ForwardingContainer container in consol.Containers)
				{
					yield return new ContainerWrapper(container);
				}
			}
		}

		#endregion
	}
}
