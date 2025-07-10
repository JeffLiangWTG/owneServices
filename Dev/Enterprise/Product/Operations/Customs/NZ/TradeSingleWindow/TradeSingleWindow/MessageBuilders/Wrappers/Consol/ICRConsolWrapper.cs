using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class ICRConsolWrapper : IInwardCargoReport
	{
		public ICRConsolWrapper(ForwardingConsol consol, IAdditionalInformation additionalInformation)
		{
			this.consol = Argument.NotNull(consol, "Consol cannot be null");
			this.additionalInformation = additionalInformation;
		}

		readonly ForwardingConsol consol;
		readonly IAdditionalInformation additionalInformation;

		#region IInwardCargoReport Members

		ZString IInwardCargoReport.SenderReferenceNumber
		{
			get { return consol.JK_UniqueConsignRef.IsEmpty || consol.JK_UniqueConsignRef.Length > 13 ? new ZString(TSWConstants.SendersReferencePlaceHolder) : consol.JK_UniqueConsignRef.Replace("/", ""); }
		}

		ZString IInwardCargoReport.TSWReferenceNumber
		{
			get
			{
				CusEntryNumber[] result = (CusEntryNumber[])consol.Factory.Load(typeof(CusEntryNumber), GetEntryNumberFilter(consol));
				if (result.Length == 0)
				{
					return ZString.Empty;
				}
				else if (result.Length > 1)
				{
					ErrorReporter.ReportOnce("EntryNumber for write-off has more than one record", "EntryNumber for write-off has more than one record - Consol: " + consol.JK_UniqueConsignRef);
				}

				return result[0].CE_EntryNum;
			}
		}

		ZBool IInwardCargoReport.IsSea
		{
			get { return consol.IsSea; }
		}

		ZBool IInwardCargoReport.IsCarrierCargoReport
		{
			get { return false; }   // TODO: implement this if needed on consol
		}

		ZString IInwardCargoReport.CraftName
		{
			get { return consol.IsSea ? Transport != null ? Transport.JW_Vessel.ToUpper() : ZString.Empty : ZString.Empty; }
		}

		ZString IInwardCargoReport.LloydsNo
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

		ZString IInwardCargoReport.VoyageNo
		{
			get { return consol.IsSea ? VoyageFlight.Left(8) : ZString.Empty; }
		}

		ZString IInwardCargoReport.FlightNo
		{
			get { return consol.IsAir ? VoyageFlight : ZString.Empty; }
		}

		ZDateTime IInwardCargoReport.ArrivalDate
		{
			get { return Transport != null ? Transport.JW_ATA.IsEmpty ? Transport.JW_ETA : Transport.JW_ATA : ZDateTime.Empty; }
		}

		ZString IInwardCargoReport.PortOfArrival
		{
			get { return Transport != null ? Transport.JW_RL_NKDiscPort : ZString.Empty; }
		}

		IOrganisationSimple IInwardCargoReport.Carrier
		{
			get
			{
				var shippingLineAddress = consol.JK_OA_ShippingLineAddress_ZAddress;
				var carrier = shippingLineAddress != null ? shippingLineAddress.OrgHeader != null ? consol.Factory.Load<OrgHeader>(shippingLineAddress.OrgHeader.PK) : null : null;
				return OrgHeaderWrapper.New(carrier);
			}
		}

		ZBool IInwardCargoReport.UseInterfaceSequenceNumber => false;

		IEnumerable<IICRConsignment> IInwardCargoReport.Consignments
		{
			get
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					yield return new ShipmentWrapper(shipment);
				}
			}
		}

		IDeclarant IInwardCargoReport.Declarant
		{
			get { return new TSWGlbStaffWrapper(GlbStaff.CurrentUser); }
		}

		IAdditionalInformation IInwardCargoReport.AdditionalInformation
		{
			get { return additionalInformation; }
		}

		ZString IInwardCargoReport.MPIAccountDetails => ZString.Empty;

		IEnumerable<ITSWAttachment> IInwardCargoReport.SupportingDocuments => Array.Empty<ITSWAttachment>();

		#endregion

		Transport Transport
		{
			get
			{
				Transport result = null;
				foreach (Transport transport in consol.Transports)
				{
					if (transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.NewZealand) && !transport.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.NewZealand))
					{
						result = transport;
						break;
					}
				}

				return result;
			}
		}

		ZString VoyageFlight
		{
			get { return Transport != null ? Transport.JW_VoyageFlight : ZString.Empty; }
		}

		ZQuery GetEntryNumberFilter(CommonConsol consol)
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, consol.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypeList.Codes.ECIWriteOff);
			return filter;
		}
	}
}
