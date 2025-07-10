using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgSalesProduct))]
	sealed class OrgSalesProductTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestMP_Code_ReadOnly()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();

			salesProduct.MP_IsSystemDefined = false;
			AssertEquals(false, salesProduct.MP_CodeInfo.ReadOnly);

			salesProduct.MP_IsSystemDefined = true;
			AssertEquals(true, salesProduct.MP_CodeInfo.ReadOnly);
		}

		public void TestMP_Name_ReadOnly()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();

			salesProduct.MP_IsSystemDefined = false;
			AssertEquals(false, salesProduct.MP_NameInfo.ReadOnly);

			salesProduct.MP_IsSystemDefined = true;
			AssertEquals(true, salesProduct.MP_NameInfo.ReadOnly);
		}

		public void TestMP_IsSystemDefined_ReadOnly()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			AssertEquals(true, salesProduct.MP_IsSystemDefinedInfo.ReadOnly);
		}

		public void TestIsActualsSupported()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_IsSystemDefined = false;
			AssertEquals(false, salesProduct.IsActualsSupported);

			salesProduct.MP_IsSystemDefined = true;
			AssertEquals(true, salesProduct.IsActualsSupported);
		}

		public void TestIsAutoGenerateQuoteSupported()
		{
			AssertIsAutoGenerateQuoteSupported(SystemDefinedSalesProductList.Codes.CustomsBrokerage, true);
			AssertIsAutoGenerateQuoteSupported(SystemDefinedSalesProductList.Codes.ForwardingShipment, true);
			AssertIsAutoGenerateQuoteSupported(SystemDefinedSalesProductList.Codes.LinerAgency, true);
			AssertIsAutoGenerateQuoteSupported(SystemDefinedSalesProductList.Codes.Transport, false);
			AssertIsAutoGenerateQuoteSupported(SystemDefinedSalesProductList.Codes.Warehouse, true);

			AssertIsAutoGenerateQuoteSupported("ENT", false);
			AssertIsAutoGenerateQuoteSupported("", false);
		}

		void AssertIsAutoGenerateQuoteSupported(ZString productCode, ZBool expected)
		{
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			if (salesProduct == null)
			{
				salesProduct = Factory.New<OrgSalesProduct>();
				salesProduct.MP_Code = productCode;
			}

			AssertEquals(expected, salesProduct.IsAutoGenerateQuoteSupported);
		}

		public void TestIsAutoGenerateSpotQuoteSupported()
		{
			AssertIsAutoGenerateSpotQuoteSupported(SystemDefinedSalesProductList.Codes.CustomsBrokerage, false);
			AssertIsAutoGenerateSpotQuoteSupported(SystemDefinedSalesProductList.Codes.ForwardingShipment, true);
			AssertIsAutoGenerateSpotQuoteSupported(SystemDefinedSalesProductList.Codes.LinerAgency, false);
			AssertIsAutoGenerateSpotQuoteSupported(SystemDefinedSalesProductList.Codes.Transport, false);
			AssertIsAutoGenerateSpotQuoteSupported(SystemDefinedSalesProductList.Codes.Warehouse, false);

			AssertIsAutoGenerateSpotQuoteSupported("ENT", false);
			AssertIsAutoGenerateSpotQuoteSupported("", false);
		}

		void AssertIsAutoGenerateSpotQuoteSupported(ZString productCode, ZBool expected)
		{
			var salesProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			if (salesProduct == null)
			{
				salesProduct = Factory.New<OrgSalesProduct>();
				salesProduct.MP_Code = productCode;
			}

			AssertEquals(expected, salesProduct.IsAutoGenerateSpotQuoteSupported);
		}

		public void TestAllowedLocationTypes()
		{
			AssertEquals(ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.InternationalZone,
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).AllowedLocationTypes);

			AssertEquals(ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.City | ViewLocationType.InternationalZone,
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).AllowedLocationTypes);

			AssertEquals(ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.City | ViewLocationType.InternationalZone,
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).AllowedLocationTypes);

			AssertEquals(ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.State | ViewLocationType.City | ViewLocationType.TransportZone,
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).AllowedLocationTypes);

			AssertEquals(ViewLocationType.UNLOCO | ViewLocationType.Country,
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).AllowedLocationTypes);
		}

		public void TestLocationArrangement()
		{
			AssertEquals(OrgSalesProductLocationArrangement.SingleLocation, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).LocationArrangement);
			AssertEquals(OrgSalesProductLocationArrangement.OriginDestination, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).LocationArrangement);
			AssertEquals(OrgSalesProductLocationArrangement.OriginDestination, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).LocationArrangement);
			AssertEquals(OrgSalesProductLocationArrangement.OriginDestination, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).LocationArrangement);
			AssertEquals(OrgSalesProductLocationArrangement.SingleLocation, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).LocationArrangement);

			AssertEquals(OrgSalesProductLocationArrangement.SingleLocation, GetOrCreateSalesProduct("ENT").LocationArrangement);
			AssertEquals(OrgSalesProductLocationArrangement.SingleLocation, GetOrCreateSalesProduct("").LocationArrangement);
		}

		public void TestAllowedAssoicationTargets()
		{
			AssertEquals(OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).AllowedAssociationTargets);
			AssertEquals(OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).AllowedAssociationTargets);
			AssertEquals(OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).AllowedAssociationTargets);
			AssertEquals(OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).AllowedAssociationTargets);
			AssertEquals(OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).AllowedAssociationTargets);

			AssertEquals(OrgSalesProductAssociationTarget.OrgSales, GetOrCreateSalesProduct("ENT").AllowedAssociationTargets);
			AssertEquals(OrgSalesProductAssociationTarget.OrgSales, GetOrCreateSalesProduct("").AllowedAssociationTargets);
		}

		public void TestSalesPropertiesForMatching()
		{
			var sales = Factory.New<OrgSales>();

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).SalesMatchingOptions.SalesPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid)),
					Tuple.Create(OrgSales.Schema.OW_DestinationID, typeof(ZGuid)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).SalesMatchingOptions.SalesPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid)),
					Tuple.Create(OrgSales.Schema.OW_DestinationID, typeof(ZGuid)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).SalesMatchingOptions.SalesPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid)),
					Tuple.Create(OrgSales.Schema.OW_DestinationID, typeof(ZGuid)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).SalesMatchingOptions.SalesPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgSales.Schema.OW_ServiceDescription, typeof(ZString)),
					Tuple.Create(OrgSales.Schema.OW_WW, typeof(ZGuid)),
					Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).SalesMatchingOptions.SalesPropertiesForMatching);

			AssertArrayEqualsByElements(
				Array.Empty<string>(),
				GetOrCreateSalesProduct("ENT").SalesMatchingOptions.SalesPropertiesForMatching);
		}

		public void TestTradeDetailPropertiesForMatching()
		{
			var sales = Factory.New<OrgSales>();

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeMode, typeof(ZString)),
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeType, typeof(ZString)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).SalesMatchingOptions.TradeDetailPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeMode, typeof(ZString)),
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeType, typeof(ZString)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).SalesMatchingOptions.TradeDetailPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeMode, typeof(ZString)),
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeType, typeof(ZString)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).SalesMatchingOptions.TradeDetailPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeMode, typeof(ZString)),
					Tuple.Create(OrgTradeDetail.Schema.PA_TradeType, typeof(ZString)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).SalesMatchingOptions.TradeDetailPropertiesForMatching);

			AssertArrayEqualsByElements(
				new[]
				{
					Tuple.Create(OrgTradeDetail.Schema.PA_OP, typeof(ZGuid)),
				},
				GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).SalesMatchingOptions.TradeDetailPropertiesForMatching);

			AssertArrayEqualsByElements(
				Array.Empty<string>(),
				GetOrCreateSalesProduct("ENT").SalesMatchingOptions.TradeDetailPropertiesForMatching);
		}

		#region MandatoryFields

		public void TestServiceIsMandatory()
		{
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).ServiceIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).ServiceIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).ServiceIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).ServiceIsMandatory);
			AssertEquals(true, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).ServiceIsMandatory);

			AssertEquals(false, GetOrCreateSalesProduct("ENT").ServiceIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct("").ServiceIsMandatory);
		}

		public void TestModeIsMandatory()
		{
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).ModeIsMandatory);
			AssertEquals(true, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).ModeIsMandatory);
			AssertEquals(true, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).ModeIsMandatory);
			AssertEquals(true, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).ModeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).ModeIsMandatory);

			AssertEquals(false, GetOrCreateSalesProduct("ENT").ModeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct("").ModeIsMandatory);
		}

		public void TestTypeIsMandatory()
		{
			AssertEquals(true, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.CustomsBrokerage).TypeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.ForwardingShipment).TypeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.LinerAgency).TypeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Transport).TypeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse).TypeIsMandatory);

			AssertEquals(false, GetOrCreateSalesProduct("ENT").TypeIsMandatory);
			AssertEquals(false, GetOrCreateSalesProduct("").TypeIsMandatory);
		}

		#endregion

		#region Allowed Fields

		public void TestIsBuyerAllowed_Warehouse()
		{
			var product = GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse);

			var sales = Factory.New<OrgSales>();
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			AssertEquals(true, product.IsBuyerAllowed(sales));

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			AssertEquals(false, product.IsBuyerAllowed(sales));

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			AssertEquals(false, product.IsBuyerAllowed(sales));

			sales.OW_Service = "";
			AssertEquals(false, product.IsBuyerAllowed(sales));
		}

		public void TestIsBuyerAllowed_CustomProducts()
		{
			var product = GetOrCreateSalesProduct("ENT");
			var sales = Factory.New<OrgSales>();
			AssertEquals(true, product.IsBuyerAllowed(sales));
		}

		public void TestIsSupplierAllowed_Warehouse()
		{
			var product = GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse);

			var sales = Factory.New<OrgSales>();
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			AssertEquals(false, product.IsSupplierAllowed(sales));

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			AssertEquals(true, product.IsSupplierAllowed(sales));

			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			AssertEquals(false, product.IsSupplierAllowed(sales));

			sales.OW_Service = "";
			AssertEquals(false, product.IsSupplierAllowed(sales));
		}

		public void TestIsSupplierAllowed_CustomProducts()
		{
			var product = GetOrCreateSalesProduct("ENT");
			var sales = Factory.New<OrgSales>();
			AssertEquals(true, product.IsSupplierAllowed(sales));
		}

		public void TestIsContainerTypeAllowed_Warehouse()
		{
			var product = GetOrCreateSalesProduct(SystemDefinedSalesProductList.Codes.Warehouse);

			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.ProspectDetail.PAP_RequiresPacking = true;
			AssertEquals(true, product.IsContainerTypeAllowed(tradeDetail));

			tradeDetail.ProspectDetail.PAP_RequiresPacking = false;
			AssertEquals(false, product.IsContainerTypeAllowed(tradeDetail));
		}

		public void TestIsContainerTypeAllowed_CustomProducts()
		{
			var product = GetOrCreateSalesProduct("ENT");
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			AssertEquals(true, product.IsContainerTypeAllowed(tradeDetail));
		}

		#endregion

		#endregion

		#region Save

		public void TestSave_FormLayoutDataSerializedCorrectly()
		{
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			var aaaColumnDefintion = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			aaaColumnDefintion.XC_Name = "AAA";
			Factory.Save();

			salesProduct.FormLayout.TradeLaneGridColumnDefinitions.AddNew().GenCustomColumnDefinitionFk = aaaColumnDefintion.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var salesProductInOtherFactory = otherFactory.Load<OrgSalesProduct>(salesProduct.PK);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProductCustomColumnDefinition>.PKOnlyComparer,
				new[] { aaaColumnDefintion },
				salesProductInOtherFactory.FormLayout.TradeLaneCustomColumnDefinitionCollection.Cast<OrgSalesProductCustomColumnDefinition>());

			AssertContainsExactElementsInAnyOrder(
				new[] { aaaColumnDefintion.PK },
				salesProductInOtherFactory.FormLayout.TradeLaneGridColumnDefinitions.Cast<SalesProductTradeLaneGridColumnDefinition>().Select(x => x.GenCustomColumnDefinitionFk));
		}

		#endregion

		#region Activation and Deactivation

		public void TestCanCancel()
		{
			var systemDefinedSalesProduct = Factory.New<OrgSalesProduct>();
			systemDefinedSalesProduct.MP_Code = "TST";
			systemDefinedSalesProduct.MP_Name = "Test Product";
			systemDefinedSalesProduct.MP_IsSystemDefined = true;
			AssertEquals("Can Cancel for system defined", "Sales Product - TST - Test Product cannot be deactivated as it is a system defined sales product.", systemDefinedSalesProduct.CanCancel());

			var nonSystemDefinedSalesProduct = Factory.New<OrgSalesProduct>();
			nonSystemDefinedSalesProduct.MP_IsSystemDefined = false;
			AssertNull("Can Cancel for non system defined", nonSystemDefinedSalesProduct.CanCancel());
		}

		public void TestCanReactivate()
		{
			var systemDefinedSalesProduct = Factory.New<OrgSalesProduct>();
			systemDefinedSalesProduct.MP_Code = "TST";
			systemDefinedSalesProduct.MP_Name = "Test Product";

			systemDefinedSalesProduct.MP_IsSystemDefined = true;
			AssertEquals("Can Reactivate for system defined", "Sales Product - TST - Test Product cannot be activated as it is a system defined sales product.", systemDefinedSalesProduct.CanReactivate());

			var nonSystemDefinedSalesProduct = Factory.New<OrgSalesProduct>();
			nonSystemDefinedSalesProduct.MP_IsSystemDefined = false;
			AssertNull("Can Reactivate for non system defined", nonSystemDefinedSalesProduct.CanReactivate());
		}

		public void TestHumanReadableName()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_IsSystemDefined = true;
			salesProduct.MP_Code = "TST";
			salesProduct.MP_Name = "Test Product";
			AssertEquals("Human Readable Name", "Sales Product - TST - Test Product", salesProduct.HumanReadableName);
		}

		#endregion

		#region Logging

		public void TestIsAutoLogged()
		{
			IAutoAdminLogTarget salesProduct = Factory.New<OrgSalesProduct>();
			AssertEquals(true, salesProduct.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region CustomColumnsBusinessObject

		public void TestGetCustomGridColumns()
		{
			var sales = Factory.New<OrgSales>();
			var salesProduct = Factory.New<OrgSalesProduct>();
			var allColumns = new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Lane);
			var columnsToInclude = new SalesProductTradeLaneGridColumnDefinitionCollection(salesProduct);

			var columnDef1 = allColumns.AddNew();
			var columnDef2 = allColumns.AddNew();
			var columnDef3 = allColumns.AddNew();
			columnDef1.XC_Name = "CustomStr";
			columnDef2.XC_Name = "CustomInt";
			columnDef3.XC_Name = "CustomDate";
			columnDef1.XC_Type = AddOnColumnDataType.Codes.String;
			columnDef2.XC_Type = AddOnColumnDataType.Codes.Integer;
			columnDef3.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var gridColumn1 = columnsToInclude.AddNew();
			var gridColumn2 = columnsToInclude.AddNew();
			var gridColumn3 = columnsToInclude.AddNew();
			gridColumn1.GenCustomColumnDefinitionFk = ZGuid.NewZGuid();
			gridColumn2.GenCustomColumnDefinitionFk = columnDef3.PK;
			gridColumn3.GenCustomColumnDefinitionFk = columnDef1.PK;

			var result = OrgSalesProduct.GetCustomGridColumns(sales, allColumns.Cast<OrgSalesProductCustomColumnDefinition>().ToList(), columnsToInclude).ToList();
			AssertEquals(2, result.Count);
			AssertEquals("CustomDate", result[0].CustomColumnDefinition.Name);
			AssertEquals("CustomStr", result[1].CustomColumnDefinition.Name);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#endregion

		#region Implementation

		OrgSalesProduct GetOrCreateSalesProduct(string productCode)
		{
			var result = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, productCode);
			if (result == null)
			{
				result = Factory.New<OrgSalesProduct>();
				result.MP_Code = productCode;
			}

			return result;
		}

		#endregion
	}
}
