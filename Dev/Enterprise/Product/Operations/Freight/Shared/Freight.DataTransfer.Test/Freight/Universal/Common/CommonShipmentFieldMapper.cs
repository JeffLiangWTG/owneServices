using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public class CommonShipmentFieldMapper<T> where T : CommonShipment
	{
		public CommonShipmentFieldMapper(T shipment)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
			factory = shipment.Factory;
		}

		readonly T shipment;
		readonly BusinessObjectFactory factory;

		public string Map()
		{
			return string.Join(System.Environment.NewLine, GetShipmentMap());
		}

		IEnumerable<string> GetShipmentMap()
		{
			yield return shipment.GetType().FullName;
			yield return new string('-', 40);
			yield return "JS_HouseBill|" + shipment.JS_HouseBill;
			yield return "JS_TransportMode|" + shipment.JS_TransportMode;
			yield return "JS_RL_NKOrigin|" + shipment.JS_RL_NKOrigin;
			yield return "JS_RL_NKDestination|" + shipment.JS_RL_NKDestination;
			yield return "JS_INCO|" + shipment.JS_INCO;
			yield return "JS_PackingMode|" + shipment.JS_PackingMode;

			yield return "JS_GoodsDescription|" + shipment.JS_GoodsDescription;
			yield return "JS_ReleaseType|" + shipment.JS_ReleaseType;
			yield return "JS_HBLAWBChargesDisplay|" + shipment.JS_HBLAWBChargesDisplay;
			yield return "JS_ShippedOnBoard|" + shipment.JS_ShippedOnBoard;

			yield return "JS_NoCopyBills|" + shipment.JS_NoCopyBills.ToZInt();
			yield return "JS_NoOriginalBills|" + shipment.JS_NoOriginalBills.ToZInt();

			yield return "JS_ActualVolume|" + shipment.JS_ActualVolume;
			yield return "JS_UnitOfVolume|" + shipment.JS_UnitOfVolume;
			yield return "JS_ActualWeight|" + shipment.JS_ActualWeight;
			yield return "JS_UnitOfWeight|" + shipment.JS_UnitOfWeight;
			yield return "JS_ActualChargeable|" + shipment.JS_ActualChargeable;

			yield return "JS_InterimReceipt|" + shipment.JS_InterimReceipt;
			yield return "JS_ShipmentType|" + shipment.JS_ShipmentType;

			yield return "JS_ShipperCODAmount|" + shipment.JS_ShipperCODAmount;
			yield return "ShipperCODPayMethod|" + shipment.JS_ShipperCODPayMethod;

			yield return "JS_BookingReference|" + shipment.JS_BookingReference;
			yield return "JS_CFSReference|" + shipment.JS_CFSReference;

			yield return "JS_TotalPackageCount|" + shipment.JS_TotalPackageCount;
			yield return "JS_F3_NKTotalCountPackType|" + shipment.JS_F3_NKTotalCountPackType;

			yield return "JS_ManifestedVolume|" + shipment.JS_ManifestedVolume;
			yield return "JS_ManifestedWeight|" + shipment.JS_ManifestedWeight;
			yield return "JS_ManifestedChargeable|" + shipment.JS_ManifestedChargeable;

			yield return "JS_DocumentedVolume|" + shipment.JS_DocumentedVolume;
			yield return "JS_DocumentedWeight|" + shipment.JS_DocumentedWeight;
			yield return "JS_DocumentedChargeable|" + shipment.JS_DocumentedChargeable;

			yield return "JS_TranshipToOtherCFS|" + shipment.JS_TranshipToOtherCFS;

			yield return "JS_RS_NKServiceLevel|" + shipment.JS_RS_NKServiceLevel;

			yield return "JS_UnitFreightRate|" + shipment.JS_UnitFreightRate;
			yield return "JS_RX_NKFrtRateCurrency|" + shipment.JS_RX_NKFrtRateCurrency;
			yield return "JS_GoodsValue|" + shipment.JS_GoodsValue;
			yield return "JS_RX_NKGoodsValueCurr|" + shipment.JS_RX_NKGoodsValueCurr;
			yield return "JS_InsuranceValue|" + shipment.JS_InsuranceValue;
			yield return "JS_RX_NKInsuranceCurrency|" + shipment.JS_RX_NKInsuranceCurrency;

			//stanardise the ZDateTime format, align with Text File
			yield return "JS_A_BKD|" + shipment.JS_A_BKD.ToString(dateFormat, CultureInfo.InvariantCulture);
			yield return "JS_A_RCV|" + shipment.JS_A_RCV.ToString(dateFormat, CultureInfo.InvariantCulture);
			yield return "JS_E_DEP|" + shipment.JS_E_DEP.ToString(dateFormat, CultureInfo.InvariantCulture);
			yield return "JS_E_ARV|" + shipment.JS_E_ARV.ToString(dateFormat, CultureInfo.InvariantCulture);
			yield return "JS_HouseBillIssueDate|" + shipment.JS_HouseBillIssueDate.ToString(dateFormat, CultureInfo.InvariantCulture);
			yield return "JS_ShippedOnBoardDate|" + shipment.JS_ShippedOnBoardDate.ToString(dateFormat, CultureInfo.InvariantCulture);
			yield return "JS_ShipmentStatus|" + shipment.JS_ShipmentStatus;

			yield return "Organisations";
			foreach (var organisation in ProcessCollection(MapOrganisations()))
			{
				yield return indent + organisation;
			}

			yield return "Addresses";
			foreach (var shipmentAddress in ProcessCollection(MapJobDocAddresses()))
			{
				yield return indent + shipmentAddress;
			}
			foreach (var shipmentAddress in ProcessCollection(MapOtherAddresses()))
			{
				yield return indent + shipmentAddress;
			}

			yield return "Notes";
			foreach (var note in ProcessCollection(MapNotes()))
			{
				yield return indent + note;
			}

			yield return "Additional Reference Numbers";
			foreach (var number in ProcessCollection(MapReferenceNumbers()))
			{
				yield return indent + number;
			}

			yield return "Containers";
			foreach (var container in ProcessCollection(MapContainers()))
			{
				yield return indent + container;
			}

			yield return "Pack Lines";
			foreach (var packLine in ProcessCollection(MapPackLines()))
			{
				yield return indent + packLine;
			}

			yield return "Transport Legs";
			foreach (var transport in ProcessCollection(MapTransportLegs()))
			{
				yield return indent + transport;
			}
		}
		const string dateFormat = "yyyy-MM-dd hh:mm:ss tt";

		IEnumerable<string> ProcessCollection(IEnumerable<string> collection)
		{
			return collection
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.OrderBy(s => s);
		}

		protected virtual IEnumerable<string> MapOrganisations()
		{
			yield return "JS_OH_ImportBroker|" + GetOrgCodeFromOrgPK(shipment.JS_OH_ImportBroker);
			yield return "JS_OH_ExportBroker|" + GetOrgCodeFromOrgPK(shipment.JS_OH_ExportBroker);
			yield return "JS_OH_DeliveryAgent|" + GetOrgCodeFromOrgPK(shipment.JS_OH_DeliveryAgent);
		}

		protected virtual IEnumerable<string> MapJobDocAddresses()
		{
			return shipment.DocAddresses.Cast<JobDocAddress>().Select(docAddress => docAddress.DocAddressType + "|" + docAddress.AddressAsASingleLine);
		}

		protected virtual IEnumerable<string> MapOtherAddresses()
		{
			yield return "JS_OA_BookedShippingLineAddress|" + GetAddressFromPK(shipment.JS_OA_BookedShippingLineAddress);
			yield return "JS_OA_ImportReleaseDepot|" + GetAddressFromPK(shipment.JS_OA_ImportReleaseDepot);
			yield return "JS_OA_ExportReceivingDepot|" + GetAddressFromPK(shipment.JS_OA_ExportReceivingDepot);
			yield return "JH_OA_LocalChargesAddr|" +
						(
							shipment.ShipmentJobHeader != null ?
							GetAddressFromPK(shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr) :
							notSet
						);
		}

		protected virtual IEnumerable<string> MapNotes()
		{
			return shipment.Notes.GetAllNotes().Cast<StmNote>()
				.Select(NoteToString);
		}

		protected virtual IEnumerable<string> MapReferenceNumbers()
		{
			return shipment.Numbers.Cast<CusEntryNumber>()
				.Select(CusEntryNumberToString);
		}

		protected virtual IEnumerable<string> MapContainers()
		{
			return shipment.Containers.Cast<CommonContainer>()
				.Select(ContainerToString);
		}

		protected virtual IEnumerable<string> MapPackLines()
		{
			return shipment.OuterPackLines.Cast<PackLine>()
				.Select(PackLineToString);
		}

		protected virtual IEnumerable<string> MapTransportLegs()
		{
			return shipment.Transports.Cast<Transport>()
				.Select(TransportToString);
		}

		const string notSet = "not set";
		const string indent = "\t";

		string GetAddressFromPK(ZGuid addressPK)
		{
			var address = addressPK.IsValid ? factory.Load<OrgAddress>(addressPK) : null;
			return address != null ? address.AddressAsASingleLine : (ZString)notSet;
		}

		string GetOrgCodeFromOrgPK(ZGuid orgPK)
		{
			var org = orgPK.IsValid ? factory.Load<OrgHeader>(orgPK) : null;
			return org != null ? org.OH_Code : (ZString)notSet;
		}

		string NoteToString(StmNote note)
		{
			return string.Format("{0}|{1}|{2}", note.ST_Description, note.ST_NoteType, note.ST_NoteContext);
		}

		string CusEntryNumberToString(CusEntryNumber number)
		{
			return string.Format("{0}|{1}|{2}", number.CE_EntryType, number.CE_EntryNum, number.CE_Category);
		}

		protected string ContainerToString(CommonContainer container)
		{
			var containerType = container.JC_RC.IsValid ? container.Factory.Load<RefContainer>(container.JC_RC) : null;
			return string.Format("{0}|{1}", container.JC_ContainerNum, containerType != null ? containerType.RC_Code : ZString.Empty);
		}

		string PackLineToString(PackLine packLine)
		{
			var container = packLine.JL_JC.IsValid ? packLine.Factory.Load<CommonContainer>(packLine.JL_JC) : null;
			return string.Format("{0}|{1}", container != null ? container.JC_ContainerNum : ZString.Empty, packLine.JL_Description);
		}

		string TransportToString(Transport transport)
		{
			return string.Format("{0}->{1}", transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
		}
	}
}
