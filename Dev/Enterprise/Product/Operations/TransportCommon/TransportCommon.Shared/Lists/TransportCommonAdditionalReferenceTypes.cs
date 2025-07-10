using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.TransportCommon.Shared
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "IDE0002:Name can be simplified", Justification = "Consistently use the same types throughout the class")]
	public class TransportCommonAdditionalReferenceTypes : CodeDescriptionPairList
	{
		#region Codes

		[CodeAlive("Provides a shared list of Additional Reference Type Codes used in Transport")]
		public abstract class Codes : TransportAdditionalReferenceTypes.Codes
		{
			public const string BookingJobId = "BJB";
		}

		#endregion

		#region Descriptions

		public abstract class Descriptions
		{
			public static MultilingualString BookingPartyReference { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|BookingPartyReference", TransportAdditionalReferenceTypes.Descriptions.BookingPartyReference); } }
			public static MultilingualString CarrierBookingReference { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|CarrierBookingReference", TransportAdditionalReferenceTypes.Descriptions.CarrierBookingReference); } }
			public static MultilingualString Client { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|Client", TransportAdditionalReferenceTypes.Descriptions.Client); } }
			public static MultilingualString ClientReferenceNumber { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|ClientReferenceNumber", "Customer Reference Number"); } }
			public static MultilingualString CommercialInvoiceNumber { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|CommercialInvoiceNumber", TransportAdditionalReferenceTypes.Descriptions.CommercialInvoiceNumber); } }
			public static MultilingualString ExternalTransportBookingNumber { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|ExternalTransportBookingNumber", TransportAdditionalReferenceTypes.Descriptions.ExternalTransportBookingNumber); } }
			public static MultilingualString ExternalUniqueConsignmentReference { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|ExternalUniqueConsignmentReference", TransportAdditionalReferenceTypes.Descriptions.ExternalUniqueConsignmentReference); } }
			public static MultilingualString HouseBill { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|HouseBill", TransportAdditionalReferenceTypes.Descriptions.HouseBill); } }
			public static MultilingualString MasterBill { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|MasterBill", TransportAdditionalReferenceTypes.Descriptions.MasterBill); } }
			public static MultilingualString OrderNumber { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|OrderNumber", TransportAdditionalReferenceTypes.Descriptions.OrderNumber); } }
			public static MultilingualString TransportReference { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|TransportReference", TransportAdditionalReferenceTypes.Descriptions.TransportReference); } }
			public static MultilingualString BookingJobID { get { return ResString.GetMultilingualString("AdditionalReferenceTypes|BookingJobID", "Booking Job ID"); } }
		}

		#endregion

		#region TransportCommonAdditionalReferenceTypes

		public TransportCommonAdditionalReferenceTypes()
		{
			AddPair(Codes.BookingPartyReference, Descriptions.BookingPartyReference);
			AddPair(Codes.CarrierBookingReference, Descriptions.CarrierBookingReference);
			AddPair(Codes.Client, Descriptions.Client);
			AddPair(Codes.ClientReferenceNumber, Descriptions.ClientReferenceNumber);
			AddPair(Codes.CommercialInvoiceNumber, Descriptions.CommercialInvoiceNumber);
			AddPair(Codes.ExternalTransportBookingNumber, Descriptions.ExternalTransportBookingNumber);
			AddPair(Codes.ExternalUniqueConsignmentReference, Descriptions.ExternalUniqueConsignmentReference);
			AddPair(Codes.HouseBill, Descriptions.HouseBill);
			AddPair(Codes.MasterBill, Descriptions.MasterBill);
			AddPair(Codes.OrderNumber, Descriptions.OrderNumber);
			AddPair(Codes.TransportReference, Descriptions.TransportReference);
			AddPair(Codes.BookingJobId, Descriptions.BookingJobID);
		}

		#endregion
	}
}
