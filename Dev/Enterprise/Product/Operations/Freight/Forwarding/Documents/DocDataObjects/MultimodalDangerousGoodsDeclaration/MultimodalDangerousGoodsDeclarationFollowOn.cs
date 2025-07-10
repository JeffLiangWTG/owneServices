using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class MultimodalDangerousGoodsDeclarationFollowOn : DocDataObject
	{
		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(Shipper, value);
		}
		IAddress shipper;

		#endregion

		#region TransportDocumentNumber

		public ZString TransportDocumentNumber
		{
			get => transportDocumentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TransportDocumentNumberInfo, ref transportDocumentNumber, value))
				{
				}
			}
		}
		ZString transportDocumentNumber;

		public ZPropertyInfo TransportDocumentNumberInfo => GetZPropertyInfo(nameof(TransportDocumentNumber));

		#endregion

		#region ShipperReference

		public ZString ShipperReference
		{
			get => shipperReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceInfo, ref shipperReference, value))
				{
				}
			}
		}
		ZString shipperReference;

		public ZPropertyInfo ShipperReferenceInfo => GetZPropertyInfo(nameof(ShipperReference));

		#endregion

		#region FreightForwarderReference

		public ZString FreightForwarderReference
		{
			get => freightForwarderReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceInfo, ref freightForwarderReference, value))
				{
				}
			}
		}
		ZString freightForwarderReference;

		public ZPropertyInfo FreightForwarderReferenceInfo => GetZPropertyInfo(nameof(FreightForwarderReference));

		#endregion

		#region GoodsDetails

		public ZString GoodsDetails
		{
			get => goodsDetails;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDetailsInfo, ref goodsDetails, value))
				{
					Validate(GoodsDetailsInfo);
				}
			}
		}
		ZString goodsDetails;

		public ZPropertyInfo GoodsDetailsInfo => GetZPropertyInfo(nameof(GoodsDetails));

		#endregion
	}
}
