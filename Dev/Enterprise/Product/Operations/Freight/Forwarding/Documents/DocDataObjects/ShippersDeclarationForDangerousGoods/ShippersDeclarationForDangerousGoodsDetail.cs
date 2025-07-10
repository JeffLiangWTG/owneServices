using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippersDeclarationForDangerousGoodsDetail : DocDataObject
	{
		public ShippersDeclarationForDangerousGoodsDetail(object id, ShippersDeclarationForDangerousGoodsHeader header, NatureAndQuantityOfDangerousGoodsLine[] natureAndQuantityOfDangerousGoods)
			: base(id)
		{
			this.header = header ?? throw new ArgumentNullException(nameof(header));
			this.natureAndQuantityOfDangerousGoods = natureAndQuantityOfDangerousGoods ?? throw new ArgumentNullException(nameof(natureAndQuantityOfDangerousGoods));
		}

		readonly ShippersDeclarationForDangerousGoodsHeader header;
		readonly NatureAndQuantityOfDangerousGoodsLine[] natureAndQuantityOfDangerousGoods;

		#region Header

		public IAddress Shipper => header.Shipper;
		public IAddress Consignee => header.Consignee;
		public IAddress Company => header.Company;
		public IUnloco AirportOfDeparture => header.AirportOfDeparture;
		public IUnloco AirportOfDestination => header.AirportOfDestination;

		public ZString AirWaybillNumber
		{
			get => header.AirWaybillNumber;
			set => header.AirWaybillNumber = value;
		}

		public ZPropertyInfo AirWaybillNumberInfo => GetWrappedZPropertyInfo(nameof(AirWaybillNumber), p => header.AirWaybillNumberInfo);

		public ZString ShippersReferenceNumber
		{
			get => header.ShippersReferenceNumber;
			set => header.ShippersReferenceNumber = value;
		}

		public ZPropertyInfo ShippersReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(ShippersReferenceNumber), p => header.ShippersReferenceNumberInfo);

		public ICodeDescription ShipmentType => header.ShipmentType;

		public ZBool IsCargoOnly
		{
			get => header.IsCargoOnly;
			set => header.IsCargoOnly = value;
		}

		public ZPropertyInfo IsCargoOnlyInfo => GetWrappedZPropertyInfo(nameof(IsCargoOnly), p => header.IsCargoOnlyInfo);

		public ZString AdditionalHandlingInformation
		{
			get => header.AdditionalHandlingInformation;
			set => header.AdditionalHandlingInformation = value;
		}

		public ZPropertyInfo AdditionalHandlingInformationInfo => GetWrappedZPropertyInfo(nameof(AdditionalHandlingInformation), p => header.AdditionalHandlingInformationInfo);

		public ZString Signatory
		{
			get => header.Signatory;
			set => header.Signatory = value;
		}

		public ZPropertyInfo SignatoryInfo => GetWrappedZPropertyInfo(nameof(Signatory), p => header.SignatoryInfo);

		public ZString PlaceOfSignature
		{
			get => header.PlaceOfSignature;
			set => header.PlaceOfSignature = value;
		}

		public ZPropertyInfo PlaceOfSignatureInfo => GetWrappedZPropertyInfo(nameof(PlaceOfSignature), p => header.PlaceOfSignatureInfo);

		public ZDateTime DateOfSignature
		{
			get => header.DateOfSignature;
			set => header.DateOfSignature = value;
		}

		public ZPropertyInfo DateOfSignatureInfo => GetWrappedZPropertyInfo(nameof(DateOfSignature), p => header.DateOfSignatureInfo);

		public IContact EmergencyContact => header.EmergencyContact;

		public ZString PermittedTransportType
		{
			get => header.PermittedTransportType;
			set => header.PermittedTransportType = value;
		}

		#endregion

		#region NatureAndQuantity

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity1 => natureAndQuantityOfDangerousGoods.Length > 0
			? natureAndQuantityOfDangerousGoods[0]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity2 => natureAndQuantityOfDangerousGoods.Length > 1
			? natureAndQuantityOfDangerousGoods[1]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity3 => natureAndQuantityOfDangerousGoods.Length > 2
			? natureAndQuantityOfDangerousGoods[2]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity4 => natureAndQuantityOfDangerousGoods.Length > 3
			? natureAndQuantityOfDangerousGoods[3]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity5 => natureAndQuantityOfDangerousGoods.Length > 4
			? natureAndQuantityOfDangerousGoods[4]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity6 => natureAndQuantityOfDangerousGoods.Length > 5
			? natureAndQuantityOfDangerousGoods[5]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity7 => natureAndQuantityOfDangerousGoods.Length > 6
			? natureAndQuantityOfDangerousGoods[6]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity8 => natureAndQuantityOfDangerousGoods.Length > 7
			? natureAndQuantityOfDangerousGoods[7]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity9 => natureAndQuantityOfDangerousGoods.Length > 8
			? natureAndQuantityOfDangerousGoods[8]
			: null;

		public NatureAndQuantityOfDangerousGoodsLine NatureAndQuantity10 => natureAndQuantityOfDangerousGoods.Length > 9
			? natureAndQuantityOfDangerousGoods[9]
			: null;

		#endregion
	}
}
