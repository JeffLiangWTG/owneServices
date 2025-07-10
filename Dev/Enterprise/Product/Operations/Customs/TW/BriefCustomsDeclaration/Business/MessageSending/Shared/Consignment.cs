using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class Consignment : IBCDConsignment, ITransportMeans
	{
		public Consignment(AsycudaManifestHeader header, AsycudaBill masterBill)
		{
			Header = Argument.NotNull(header, nameof(header));
			MasterBill = Argument.NotNull(masterBill, nameof(masterBill));
			isAirN5135 = Header.IsAir && Header.IsImport;
		}

		public AsycudaManifestHeader Header { get; }
		public AsycudaBill MasterBill { get; }
		readonly bool isAirN5135;

		ZString IBCDConsignment.AssociatedTransportDocumentId => Header.BagNumber;

		ZDecimal IBCDConsignment.BoardedQuantity => Header.HouseBills.Count();

		ZString IBCDConsignment.TransportContractDocumentId => MasterBill.ABL_BillNumber;

		ZInt IBCDConsignment.TotalPackageQuantity => ZInt.Zero;

		IEnumerable<IGovernmentProcedure> IBCDConsignment.GovernmentProcedures => Enumerable.Empty<IGovernmentProcedure>();

		IPackaging IBCDConsignment.Packaging => null;

		ZString IConsignment.ManifestSerialNumber => isAirN5135 ? ZString.Empty : Header.AMA_ManifestNumber;

		IEnumerable<IAdditionalInformation> IConsignment.AdditionalInformations => Enumerable.Empty<IAdditionalInformation>();

		ZString IConsignment.ArrivalTransportMeansTypeCode => ZString.Empty;

		ITransportMeans IConsignment.BorderTransportMeans => this;

		IPartyDetails IConsignment.Carrier => null;

		IConsignmentItem IConsignment.ConsignmentItem => null;

		ZString IConsignment.GoodsLocation => MasterBill.ABL_GoodsLocation;

		ILocation IConsignment.LoadingLocation => null;

		IEnumerable<ITransportContractDocument> IConsignment.TransportContractDocuments => Enumerable.Empty<ITransportContractDocument>();

		IEnumerable<ITransportEquipment> IConsignment.TransportEquipments => Enumerable.Empty<ITransportEquipment>();

		IBondedGoods IConsignment.BondedGoods => null;

		ZString IConsignment.ShippingOrderNumber => MasterBill.ABL_CarrierReference;

		ITransportMeans IConsignment.DepartureTransportMeans => this;

		ZString IConsignment.TransitTransportMeansTypeCode => ZString.Empty;

		IEnumerable<ZString> IConsignment.GoodsLocations => Enumerable.Empty<ZString>();

		ILocation IConsignment.UnloadingLocation => null;

		IGovernmentAgencyGoodsItem IConsignment.GovernmentAgencyGoodsItem => null;

		ILocation IConsignment.TranshipmentLocation => null;

		ILocation IConsignment.TransitDeparture => null;

		ZDecimal IBCDConsignment.InvoiceAmount => ZDecimal.Zero;

		#region BorderTransportMeans and DepartureTransportMeans
		ZDate ITransportMeans.ArrivalDateTime => ZDate.Empty;

		ZString ITransportMeans.TypeCode => ZString.Empty;

		IEnumerable<ZString> ITransportMeans.ItineraryRoutingCountryCodes => null;

		ZString ITransportMeans.ID => Header.AMA_LloydsNumber;

		ZString ITransportMeans.JourneyID => Header.AMA_Voyage;

		ZString ITransportMeans.Registration => Header.IsAir ? ZString.Empty : Header.AMA_VehicleRegistration;

		ZString ITransportMeans.Name => Header.AMA_VesselName;

		ZString ITransportMeans.CallSignID => Header.AMA_RadioCallSign;
		#endregion
	}
}
