using System;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportCommon.Shared
{
	/// <summary>
	/// Parent Types made up of JobInvoicingConsumerTypes and WorkflowDescriptors.
	/// </summary>
	public class ParentTypes
	{
		#region Codes

		public static class Codes
		{
			public static string AgencyBooking
			{
				get { return JobInvoicingConsumerTypes.AgencyBooking.Code; }
			}

			public static string BillOfLading
			{
				get { return JobInvoicingConsumerTypes.AgencyBillOfLading.Code; }
			}

			public static string Customs
			{
				get { return JobInvoicingConsumerTypes.Brokerage.Code; }
			}

			public static string ForwardingBooking
			{
				get { return JobInvoicingConsumerTypes.QuotedBooking.Code; }
			}

			public static string ForwardingShipment
			{
				get { return JobInvoicingConsumerTypes.Shipment.Code; }
			}

			public static string WarehouseOrder
			{
				get { return JobInvoicingConsumerTypes.WarehouseOutwards.Code; }
			}

			public static string WarehouseReceive
			{
				get { return JobInvoicingConsumerTypes.WarehouseInwards.Code; }
			}

			public static string ForwardingConsol
			{
				get { return JobInvoicingConsumerTypes.ForwardingConsol.Code; }
			}

			public static string TransitDispatch
			{
				get { return JobInvoicingConsumerTypes.TransitDispatch.Code; }
			}
		}

		#endregion

		#region Descriptions

		public static class Descriptions
		{
			public static string AgencyBooking
			{
				get { return JobInvoicingConsumerTypes.AgencyBooking.Description; }
			}

			public static string BillOfLading
			{
				get { return JobInvoicingConsumerTypes.AgencyBillOfLading.Description; }
			}

			public static string Customs
			{
				get { return JobInvoicingConsumerTypes.Brokerage.Description; }
			}

			public static string ForwardingShipment
			{
				get { return Res.GetString("820aaa7d-c52c-4d15-b519-0e2d9fef165b", "Forwarding Shipment"); }
			}

			public static string WarehouseOrder
			{
				get { return JobInvoicingConsumerTypes.WarehouseOutwards.Description; }
			}

			public static string ForwardingBooking
			{
				get { return JobInvoicingConsumerTypes.QuotedBooking.Description; }
			}

			public static string WarehouseReceive
			{
				get { return JobInvoicingConsumerTypes.WarehouseInwards.Description; }
			}

			public static string ForwardingConsol
			{
				get { return JobInvoicingConsumerTypes.ForwardingConsol.Description; }
			}

			public static string TransitDispatch
			{
				get { return JobInvoicingConsumerTypes.TransitDispatch.Description; }
			}
		}

		#endregion

		#region List

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();
					list.AddPair(Codes.AgencyBooking, Descriptions.AgencyBooking);
					list.AddPair(Codes.BillOfLading, Descriptions.BillOfLading);
					list.AddPair(Codes.Customs, Descriptions.Customs);
					list.AddPair(Codes.ForwardingBooking, Descriptions.ForwardingBooking);
					list.AddPair(Codes.ForwardingShipment, Descriptions.ForwardingShipment);
					list.AddPair(Codes.WarehouseOrder, Descriptions.WarehouseOrder);
					list.AddPair(Codes.WarehouseReceive, Descriptions.WarehouseReceive);
					list.AddPair(Codes.ForwardingConsol, Descriptions.ForwardingConsol);
					list.AddPair(Codes.TransitDispatch, Descriptions.TransitDispatch);
				}

				return list;
			}
		}

		CodeDescriptionPairList list;

		#endregion

		#region GetParentType

		public static ICodeDescription GetParentType(DataContextType dataContext)
		{
#if DEBUG
			if (dataContext == DataContextType.DummyBusinessObject)
			{
				return new CodeDescriptionPair("DUM", "Dummy");
			}
#endif

			ZString code;
			switch (dataContext)
			{
				case DataContextType.AgencyBooking:
					code = ParentTypes.Codes.AgencyBooking;
					break;
				case DataContextType.CustomsDeclaration:
					code = ParentTypes.Codes.Customs;
					break;
				case DataContextType.ForwardingShipment:
					code = ParentTypes.Codes.ForwardingShipment;
					break;
				case DataContextType.WarehouseOrder:
					code = ParentTypes.Codes.WarehouseOrder;
					break;
				case DataContextType.WarehouseReceive:
					code = ParentTypes.Codes.WarehouseReceive;
					break;
				case DataContextType.BillOfLading:
					code = ParentTypes.Codes.BillOfLading;
					break;
				case DataContextType.ForwardingConsol:
					code = ParentTypes.Codes.ForwardingConsol;
					break;
				case DataContextType.ForwardingBooking:
					code = ParentTypes.Codes.ForwardingBooking;
					break;
				case DataContextType.HVLVConsignment:
					code = ParentTypes.Codes.ForwardingShipment;
					break;
				case DataContextType.HVLVBookingHeader:
					code = ParentTypes.Codes.ForwardingShipment;
					break;
				case DataContextType.HVLVOriginLoadList:
					code = ParentTypes.Codes.ForwardingShipment;
					break;
				case DataContextType.TransitDispatch:
					code = ParentTypes.Codes.TransitDispatch;
					break;
				default:
					throw new NotSupportedException(string.Format(Culture.Invariant, "Parent Data Context '{0}' can not be mapped to Parent Type.", dataContext.ToString()));
			}

			return new ParentTypes().List[code, StringComparison.OrdinalIgnoreCase];
		}

		#endregion
	}
}
