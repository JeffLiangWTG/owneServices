//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradeDetailLookups
//
//    This class should be used for overriding collections in AutoOrgTradeDetailLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeDetailLookups : AutoOrgTradeDetailLookups
	{
		public OrgTradeDetailLookups(AutoOrgTradeDetail parent) : base(parent)
		{
		}

		new OrgTradeDetail Parent
		{
			get { return (OrgTradeDetail)base.Parent; }
		}

		#region TradeModes

		public ICodeDescriptionPairList TradeModes
		{
			get
			{
				var salesProduct = Parent.SalesProduct;
				if (salesProduct != null && salesProduct.MP_IsSystemDefined)
				{
					return GetTradeModes(salesProduct.MP_Code);
				}
				return new CodeDescriptionPairList();
			}
		}

		public static ICodeDescriptionPairList GetTradeModes(ZString productCode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
					return GetTradeModes_CustomsBrokerage();

				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					return GetTradeModes_ForwardingShipment();

				case SystemDefinedSalesProductList.Codes.LinerAgency:
					return GetTradeModes_LinerAgency();

				case SystemDefinedSalesProductList.Codes.Transport:
					return GetTradeModes_Transport();

				default:
					return new CodeDescriptionPairList();
			}
		}

		static ICodeDescriptionPairList GetTradeModes_CustomsBrokerage()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.TransportModes.Air, Res.GetString("7e447397-8c25-4660-b093-86e1afcdc613", "Air Freight"));
			result.AddPair(Constants.TransportModes.Sea, Res.GetString("9712f90f-b7a3-4d5d-b2be-c52a8af156a0", "Sea Freight"));
			result.AddPair(Constants.TransportModes.Rail, Res.GetString("bd40edee-7972-45a4-9f45-068b3cd2b8b9", "Rail Freight"));
			result.AddPair(Constants.TransportModes.Road, Res.GetString("8d260b53-dccc-41bc-bad2-0c120f366f2d", "Road Freight"));
			result.AddPair(Constants.TransportModes.Mail, Res.GetString("5e1f7c7e-4573-4389-8253-1db29cb248c2", "Mail"));

			return result;
		}

		static ICodeDescriptionPairList GetTradeModes_ForwardingShipment()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.TransportModes.Air, Res.GetString("7e447397-8c25-4660-b093-86e1afcdc613", "Air Freight"));
			result.AddPair(Constants.TransportModes.Sea, Res.GetString("9712f90f-b7a3-4d5d-b2be-c52a8af156a0", "Sea Freight"));
			result.AddPair(Constants.TransportModes.Rail, Res.GetString("bd40edee-7972-45a4-9f45-068b3cd2b8b9", "Rail Freight"));
			result.AddPair(Constants.TransportModes.Road, Res.GetString("8d260b53-dccc-41bc-bad2-0c120f366f2d", "Road Freight"));
			result.AddPair(Constants.TransportModes.Courier, Res.GetString("0faf3718-0501-4986-b141-d45df5847903", "Courier"));

			return result;
		}

		public static class LinerAgencyTradeModes
		{
			public const string BillOfLading = "BOL";
		}

		static ICodeDescriptionPairList GetTradeModes_LinerAgency()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(LinerAgencyTradeModes.BillOfLading, Res.GetString("c39be816-70a3-40a8-b277-91b24bb78c4a", "Bill of Lading"));

			return result;
		}

		public static class TransportTradeModes
		{
			public const string TransportBooking = ModuleTreeCustomerServiceMenuSectionList.Codes.TransportBooking;
		}

		static ICodeDescriptionPairList GetTradeModes_Transport()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(TransportTradeModes.TransportBooking, ModuleTreeCustomerServiceMenuSectionList.Descriptions.TransportBooking);

			return result;
		}

		#endregion

		#region TradeTypes

		public ICodeDescriptionPairList TradeTypes
		{
			get
			{
				var salesProduct = Parent.SalesProduct;
				if (salesProduct != null && salesProduct.MP_IsSystemDefined)
				{
					return GetTradeTypes(salesProduct.MP_Code, Parent.PA_TradeMode);
				}

				return new CodeDescriptionPairList();
			}
		}

		public static ICodeDescriptionPairList GetTradeTypes(ZString productCode, ZString tradeMode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
					return GetTradeTypes_CustomsBrokerage();

				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					return GetTradeTypes_ForwardingShipment(tradeMode);

				case SystemDefinedSalesProductList.Codes.LinerAgency:
					return GetTradeTypes_LinerAgency();

				case SystemDefinedSalesProductList.Codes.Transport:
					return GetTradeTypes_Transport();

				default:
					return new CodeDescriptionPairList();
			}
		}

		public static class CustomsBrokerageTradeTypes
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
		}

		public static ICodeDescriptionPairList GetTradeTypes_CustomsBrokerage()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CustomsBrokerageTradeTypes.Import, Res.GetString("dd918cd7-530d-4e67-99ae-055108df4ec0", "Import"));
			result.AddPair(CustomsBrokerageTradeTypes.Export, Res.GetString("aad2d782-b9b5-4743-8444-390b9a42cd1f", "Export"));

			return result;
		}

		public static ICodeDescriptionPairList GetTradeTypes_ForwardingShipment(ZString tradeMode)
		{
			var result = new CodeDescriptionPairList();
			switch (tradeMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.ContainerModes.Loose, Res.GetString("a9a33d71-286c-4a2d-bf2f-69ea9b3c0fb7", "Loose"));
					result.AddPair(Constants.ContainerModes.ULD, Res.GetString("e374d018-176b-49f4-8b4f-b7d574033b89", "Unit Load Device"));
					break;

				case Constants.TransportModes.Sea:
				case Constants.TransportModes.Rail:
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					break;

				case Constants.TransportModes.Road:
					result.AddPair(Constants.ContainerModes.FTL, Res.GetString("07fc48fe-993e-4678-ae3c-306880f7cecd", "Full Truck Load"));
					result.AddPair(Constants.ContainerModes.LTL, Res.GetString("319e3b6f-0423-4b9f-8155-4f145f6e1a7f", "Less Truck Load"));
					break;

				case Constants.TransportModes.Courier:
					result.AddPair(Constants.ContainerModes.Unaccompanied, Res.GetString("f697b9bf-0e9b-4c7f-8e46-486afc146948", "Unaccompanied"));
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Res.GetString("8c745885-dd85-4b3b-99d9-7a802aadd3c3", "On Board Courier"));
					break;
			}

			return result;
		}

		public static ICodeDescriptionPairList GetTradeTypes_LinerAgency()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
			result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
			result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
			result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
			result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);

			return result;
		}

		public static ICodeDescriptionPairList GetTradeTypes_Transport()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.ContainerModes.FTL, Res.GetString("71c0f3ca-46d0-4be4-91c2-8953cc22aaef", "Full Truck Load"));
			result.AddPair(Constants.ContainerModes.LTL, Res.GetString("14c13a84-8369-4600-b2de-fe3472157eae", "Less Truck Load"));

			return result;
		}

		#endregion

		#region TradeLaneStatuses

		public CodeDescriptionPairList TradeLaneStatuses
		{
			get
			{
				var result = new OrgTradeDetail.TradeLaneStatus();
				result.AddRange(new OpportunityTradeStatus());
				return result;
			}
		}

		#endregion

		#region SupplierParts

		public override OrgSupplierPartCollection SupplierParts
		{
			get
			{
				var client = Parent.Parent?.Primary;
				var collection = new OrgSupplierPartCollection(Factory, null, client, false);
				if (client != null)
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", client.PK));
				}

				return collection;
			}
		}

		#endregion

		public ICodeDescriptionPairList OrgTradeProspectPeriodEndTypes
		{
			get { return new OrgTradeProspectPeriodEndTypeList(); }
		}
	}
}
