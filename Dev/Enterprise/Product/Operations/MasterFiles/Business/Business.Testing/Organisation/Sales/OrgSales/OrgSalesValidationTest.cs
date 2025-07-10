using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOW_OriginID_SystemDefined()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, true);

			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ZGuid.Empty;
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, false);
			AssertHasError(Sales.OW_OriginIDInfo, "Please enter an Origin or a Destination.");

			Sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			Sales.Validation.ValidateOW_OriginID();
			AssertNoErrors(Sales.OW_OriginIDInfo);

			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			AssertNoErrors(Sales.OW_OriginIDInfo);

			Sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AU", "RN").PK;
			AssertHasWarnings(Sales.OW_OriginIDInfo);
		}

		public void TestCheckOW_OriginID_Brokerage()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, true);

			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ZGuid.Empty;
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, false);
			AssertHasError(Sales.OW_OriginIDInfo, "Please enter an Origin.");

			Sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			Sales.Validation.ValidateOW_OriginID();
			AssertHasError(Sales.OW_OriginIDInfo, "Please enter an Origin.");

			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			AssertNoErrors(Sales.OW_OriginIDInfo);
		}

		public void TestCheckOW_OriginID_CustomProduct()
		{
			Sales.OW_MP_Product = Factory.New<IOrgSalesProduct>().Identifier;
			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, true);

			Sales.OW_DestinationID = ZGuid.Empty;
			Sales.OW_OriginID = ZGuid.Empty;
			AssertNoErrors(Sales.OW_OriginIDInfo);
		}

		public void TestCheckOW_OriginID_Warehouse()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).Identifier;
			Sales.OW_OriginID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, true);

			Sales.OW_OriginID = ZGuid.Empty;
			AssertListValidationInvalidCodeError(Sales.OW_OriginIDInfo, false);
			AssertHasError(Sales.OW_OriginIDInfo, "Please enter a Warehouse or a Location.");

			Sales.OW_WW = ZGuid.Empty;
			Sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			AssertNoErrors(Sales.OW_OriginIDInfo);

			Sales.OW_WW = Factory.New<IWhsWarehouse>().PK;
			Sales.OW_OriginID = ZGuid.Empty;
			AssertNoErrors(Sales.OW_OriginIDInfo);
		}

		public void TestCheckOW_DestinationID_SystemDefined()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_DestinationIDInfo, true);

			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ZGuid.Empty;
			AssertListValidationInvalidCodeError(Sales.OW_DestinationIDInfo, false);
			AssertHasError(Sales.OW_DestinationIDInfo, "Please enter an Origin or a Destination.");

			Sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			Sales.Validation.ValidateOW_DestinationID();
			AssertNoErrors(Sales.OW_DestinationIDInfo);

			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			AssertNoErrors(Sales.OW_DestinationIDInfo);

			Sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AU", "RN").PK;
			AssertHasWarnings(Sales.OW_DestinationIDInfo);
		}

		public void TestCheckOW_DestinationID_Brokerage()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_DestinationIDInfo, true);

			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ZGuid.Empty;
			AssertNoErrors(Sales.OW_DestinationIDInfo);
		}

		public void TestCheckOW_DestinationID_CustomProduct()
		{
			Sales.OW_MP_Product = Factory.New<IOrgSalesProduct>().Identifier;
			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(Sales.OW_DestinationIDInfo, true);

			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_DestinationID = ZGuid.Empty;
			AssertNoErrors(Sales.OW_DestinationIDInfo);
		}

		public void TestCheckOW_Service_Warehouse()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).Identifier;
			Sales.OW_Service = "XXX";
			AssertListValidationInvalidCodeError(Sales.OW_ServiceInfo, true);

			Sales.OW_Service = ZString.Empty;
			AssertListValidationInvalidCodeError(Sales.OW_ServiceInfo, false);
			AssertMandatoryValidationError(Sales.OW_ServiceInfo, true);

			Sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			AssertNoErrors(Sales.OW_ServiceInfo);
		}

		public void TestCheckOW_Service_CustomProduct()
		{
			Sales.OW_MP_Product = Factory.New<IOrgSalesProduct>().Identifier;
			Sales.OW_Service = "XXX";
			AssertListValidationInvalidCodeError(Sales.OW_ServiceInfo, true);

			Sales.OW_Service = ZString.Empty;
			AssertListValidationInvalidCodeError(Sales.OW_ServiceInfo, false);
			AssertMandatoryValidationError(Sales.OW_ServiceInfo, false);
		}

		public void TestCheckOW_WW_Warehouse()
		{
			Sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse).Identifier;

			Sales.OW_WW = ZGuid.Empty;
			Sales.Validation.ValidateOW_WW();
			AssertHasError(Sales.OW_WWInfo, "Please enter a Warehouse or a Location.");

			Sales.OW_OriginID = ZGuid.Empty;
			Sales.OW_WW = Factory.New<IWhsWarehouse>().PK;
			AssertNoErrors(Sales.OW_OriginIDInfo);

			Sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL").PK;
			Sales.OW_WW = ZGuid.Empty;
			AssertNoErrors(Sales.OW_OriginIDInfo);
		}

		public void TestCheckOW_WW_CustomProduct()
		{
			Sales.OW_MP_Product = Factory.New<IOrgSalesProduct>().Identifier;
			Sales.OW_WW = ZGuid.Empty;
			Sales.Validation.ValidateOW_WW();
			AssertNoErrors(Sales.OW_WWInfo);
		}

		#region Implementation

		OrgSales Sales
		{
			get
			{
				if (fSales == null)
				{
					OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
					org.IsBoundToOrganisationForm = true;
					fSales = org.SalesCollection.AddNew();
				}

				return fSales;
			}
		}

		OrgSales fSales;

		#endregion
	}
}
