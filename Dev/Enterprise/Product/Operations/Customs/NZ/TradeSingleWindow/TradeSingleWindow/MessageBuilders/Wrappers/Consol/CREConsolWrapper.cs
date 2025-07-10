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
	public class CREConsolWrapper : ICargoReportExport
	{
		public CREConsolWrapper(ForwardingConsol consol, IAdditionalInformation additionalInformation)
		{
			this.consol = Argument.NotNull(consol, "Consol cannot be null");
			this.additionalInformation = additionalInformation;
		}

		readonly ForwardingConsol consol;
		readonly IAdditionalInformation additionalInformation;

		#region ICargoReportExport Implementation

		ZBool ICargoReportExport.IsSea
		{
			get { return consol.IsSea; }
		}

		ZBool ICargoReportExport.IsAir
		{
			get { return consol.IsAir; }
		}

		ZBool ICargoReportExport.IsMail
		{
			get { throw new NotImplementedException(); }    //TODO: Consol used for post?
		}

		ZBool ICargoReportExport.IsContainerised
		{
			get { throw new NotImplementedException(); }    //TODO: Best method to determine this?
		}

		ZBool ICargoReportExport.HasEmptyContainersOnly
		{
			get { return false; }   //TODO: CRE not yet used at Consol level
		}

		ZString ICargoReportExport.SenderReferenceNumber
		{
			get { return consol.JK_UniqueConsignRef.GetSenderReferenceNumber(); }
		}

		ZString ICargoReportExport.TSWReferenceNumber
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
					ErrorReporter.ReportOnce("EntryNumber for CRE has more than one record", "EntryNumber for CRE has more than one record - Consol: " + consol.JK_UniqueConsignRef);
				}
				return result[0].CE_EntryNum;
			}
		}

		IOrganisationSimple ICargoReportExport.Carrier
		{
			get
			{
				var shippingLineAddress = consol.JK_OA_ShippingLineAddress_ZAddress;
				var carrier = shippingLineAddress != null ? shippingLineAddress.OrgHeader != null ? consol.Factory.Load<OrgHeader>(shippingLineAddress.OrgHeader.PK) : null : null;
				return OrgHeaderWrapper.New(carrier);
			}
		}

		IAdditionalInformation ICargoReportExport.AdditionalInformation
		{
			get { return additionalInformation; }
		}

		ZString ICargoReportExport.CraftName
		{
			get { return consol.Vessel != null ? consol.Vessel.RV_Code.ToUpper() : ZString.Empty; }
		}

		ZString ICargoReportExport.LloydsNo
		{
			get { return consol.Vessel != null ? consol.Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		ZString ICargoReportExport.VoyageNo
		{
			get { return consol.IsSea ? VoyageFlight.Left(8) : ZString.Empty; }
		}

		ZString ICargoReportExport.FlightNo
		{
			get { return consol.IsAir ? VoyageFlight : ZString.Empty; }
		}

		ZDateTime ICargoReportExport.DepartureDate
		{
			get { return Transport != null ? Transport.JW_ATD.IsEmpty ? Transport.JW_ETD : Transport.JW_ATD : ZDateTime.Empty; }
		}

		ZBool ICargoReportExport.UseInterfaceSequenceNumber => false;

		IEnumerable<ICREConsignment> ICargoReportExport.Consignments
		{
			get
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					yield return new ShipmentWrapper(shipment);
				}
			}
		}

		IDeclarant ICargoReportExport.Declarant
		{
			get { return new TSWGlbStaffWrapper(GlbStaff.CurrentUser); }
		}

		ZString ICargoReportExport.PortOfDeparture
		{
			get { return Transport != null ? Transport.JW_RL_NKLoadPort : ZString.Empty; }
		}

		IEnumerable<ITSWAttachment> ICargoReportExport.SupportingDocuments => Array.Empty<ITSWAttachment>();

		#endregion

		#region Implementation

		Transport Transport
		{
			get
			{
				Transport result = null;
				foreach (Transport transport in consol.Transports)
				{
					if (transport.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.NewZealand) && !transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.NewZealand))
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

		#endregion
	}
}
