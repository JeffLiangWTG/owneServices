using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class BCDGoodsShipmentConsignment : IBCDConsignment, IConsignmentItem, IPackaging
	{
		public BCDGoodsShipmentConsignment(AsycudaBill bill)
		{
			HouseBill = Argument.NotNull(bill, nameof(bill));
		}

		public AsycudaBill HouseBill { get; }

		ZDecimal IBCDConsignment.InvoiceAmount => HouseBill.ABL_CustomsValue;

		ZString IBCDConsignment.AssociatedTransportDocumentId => ZString.Empty;

		ZDecimal IBCDConsignment.BoardedQuantity => ZDecimal.Zero;

		ZString IBCDConsignment.TransportContractDocumentId => HouseBill.ABL_BillNumber;

		ZInt IBCDConsignment.TotalPackageQuantity => HouseBill.ABL_ManifestQty;

		IEnumerable<IGovernmentProcedure> IBCDConsignment.GovernmentProcedures
		{
			get
			{
				yield return new GovernmentProcedureWrapper(ZString.Empty, ZString.Empty, HouseBill.ABL_Remarks);
			}
		}

		IPackaging IBCDConsignment.Packaging => this;

		ZString IConsignment.ManifestSerialNumber => ZString.Empty;

		IEnumerable<IAdditionalInformation> IConsignment.AdditionalInformations => null;

		ZString IConsignment.ArrivalTransportMeansTypeCode => ZString.Empty;

		ITransportMeans IConsignment.BorderTransportMeans => null;

		IPartyDetails IConsignment.Carrier => null;

		IConsignmentItem IConsignment.ConsignmentItem => this;

		ZString IConsignment.GoodsLocation => ZString.Empty;

		ILocation IConsignment.LoadingLocation => new LocationWrapper(HouseBill.ABL_RL_NKPortOfLoading);

		IEnumerable<ITransportContractDocument> IConsignment.TransportContractDocuments => null;

		IEnumerable<ITransportEquipment> IConsignment.TransportEquipments => null;

		IBondedGoods IConsignment.BondedGoods => null;

		ZString IConsignment.ShippingOrderNumber => ZString.Empty;

		ITransportMeans IConsignment.DepartureTransportMeans => null;

		ZString IConsignment.TransitTransportMeansTypeCode => ZString.Empty;

		IEnumerable<ZString> IConsignment.GoodsLocations => null;

		ILocation IConsignment.UnloadingLocation => new LocationWrapper(HouseBill.ABL_RL_NKPortOfDischarge);

		IGovernmentAgencyGoodsItem IConsignment.GovernmentAgencyGoodsItem => null;

		ILocation IConsignment.TranshipmentLocation => null;

		ILocation IConsignment.TransitDeparture => null;

		#region IConsignmentItem
		IPackaging IConsignmentItem.Packaging => null;

		IEnumerable<ITransportContractDocument> IConsignmentItem.TransportContractDocuments => null;

		ZString IConsignmentItem.Split => ZString.Empty;

		ICommodity IConsignmentItem.Commodity => null;

		IGoodsMeasure IConsignmentItem.GoodsMeasure => null;

		IOrigin IConsignmentItem.Origin => null;

		ZString IConsignmentItem.AssociatedGovernmentProcedureCode => HouseBill.ABL_Procedure;
		#endregion

		#region IPacking
		ZDecimal IPackaging.QuantityQuantity => ZDecimal.Zero;

		ZString IPackaging.TypeCode => HouseBill.ABL_ManifestUQ;

		ZString IPackaging.MarksNumbers => ZString.Empty;

		ZString IPackaging.PackagingMaterialDescription => ZString.Empty;

		ZString IPackaging.Combination => ZString.Empty;

		ZDate IPackaging.PackingDateTime => ZDate.Empty;
		#endregion
	}
}
