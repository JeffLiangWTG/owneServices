using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class OrgSalesCollectionTestHelper
	{
		public OrgSalesCollectionTestHelper(OrgSalesCollection testCollection, BusinessObjectFactory factory)
		{
			TestCollection = testCollection;
			Factory = factory;
		}

		readonly OrgSalesCollection TestCollection;
		readonly BusinessObjectFactory Factory;

		public IEnumerable<OrgSales> FindOrgSales(string product, string origin, string destination, bool isActual)
		{
			return TestCollection.Cast<OrgSales>().Where(sales =>
				sales.ProductCode == product
				&& sales.OriginCode == origin
				&& sales.DestinationCode == destination
				&& sales.IsActual == isActual);
		}

		public IEnumerable<OrgSales> FindOrgSales(string product, string origin, string destination, Guid buyer, Guid supplier, bool isActual)
		{
			return TestCollection.Cast<OrgSales>().Where(sales =>
				sales.ProductCode == product
				&& sales.OriginCode == origin
				&& sales.DestinationCode == destination
				&& sales.OW_OH_Buyer == buyer
				&& sales.OW_OH_Supplier == supplier
				&& sales.IsActual == isActual);
		}

		public OrgTradeDetail FindOrgTradeDetailByMode(OrgSales sales, string mode)
		{
			return sales.TradeDetails.Cast<OrgTradeDetail>().FirstOrDefault(tradeDetail => tradeDetail.PA_TradeMode == mode);
		}

		public OrgTradeDetail FindOrgTradeDetailByModeAndType(OrgSales sales, string mode, string type)
		{
			return sales.TradeDetails.Cast<OrgTradeDetail>().FirstOrDefault(tradeDetail => tradeDetail.PA_TradeMode == mode && tradeDetail.PA_TradeType == type);
		}

		public BusinessObject CreateShipment(ZString origin, ZString destination, ZString transportMode, ZString packingMode, ZGuid consignee, ZGuid consignor)
		{
			return CreateShipment(origin, destination, transportMode, packingMode, consignee, consignor, ZDateTime.Today);
		}

		public BusinessObject CreateShipment(ZString origin, ZString destination, ZString transportMode, ZString packingMode, ZGuid consignee, ZGuid consignor, ZDateTime estDate)
		{
			return CreateShipment(origin, destination, transportMode, packingMode, consignee, consignor, estDate, ZBool.False);
		}

		public BusinessObject CreateShipment(ZString origin, ZString destination, ZString transportMode, ZString packingMode, ZGuid consignee, ZGuid consignor, ZDateTime estDate, ZBool isShipping)
		{
			return CreateShipment(origin, destination, transportMode, packingMode, "STD", consignee, consignor, estDate, ZBool.False);
		}

		public BusinessObject CreateShipment(ZString origin, ZString destination, ZString transportMode, ZString packingMode, ZString shipmentType, ZGuid consignee, ZGuid consignor, ZDateTime estDate, ZBool isShipping)
		{
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

			shipment["ConsigneePK"] = consignee;
			shipment["ConsignorPK"] = consignor;
			shipment[JobShipmentSchema.JS_RL_NKOrigin.Name] = origin;
			shipment[JobShipmentSchema.JS_RL_NKDestination.Name] = destination;
			shipment[JobShipmentSchema.JS_TransportMode.Name] = transportMode;
			shipment[JobShipmentSchema.JS_PackingMode.Name] = packingMode;
			shipment[JobShipmentSchema.JS_E_DEP.Name] = estDate;
			shipment[JobShipmentSchema.JS_IsShipping.Name] = isShipping;
			shipment[JobShipmentSchema.JS_ShipmentType.Name] = shipmentType;

			return shipment;
		}

		public Enterprise.Integration.Customs.IBaseJobDeclaration CreateDeclaration(ZString origin, ZString destination, ZGuid supplier, ZGuid importer, ZString transportMode)
		{
			var declaration = (Enterprise.Integration.Customs.IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration.JE_RL_NKOrigin = origin;
			declaration[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = destination;
			declaration.JE_OH_Supplier = supplier;
			declaration.JE_OH_Importer = importer;
			declaration[JobDeclarationSchema.Constants.JE_TransportMode] = transportMode;

			return declaration;
		}

		public OrgHeader GetNewOrg(string code)
		{
			OrgHeader existingOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code);
			if (existingOrg != null)
			{
				existingOrg.Addresses.RemoveAndDeleteAll();
				existingOrg.Delete();
			}

			OrgHeader newOrg = Factory.New<OrgHeader>();
			newOrg.OH_Code = code;
			newOrg.MainAddress.OA_Address1 = "Test Address";

			return newOrg;
		}

		public void CancelQuote(BusinessObject quote)
		{
			MethodInfo cancelQuoteMethod = quote.GetType().GetMethod("CancelQuote", Array.Empty<Type>());
			cancelQuoteMethod.Invoke(quote, Array.Empty<object>());
		}

		public BusinessObject AddRateEntry(BusinessObject quote, ZString mode, ZString origin, ZString destination, ZGuid consignee, ZGuid consignor)
		{
			var addRateEntryMethod = quote.GetType()
				.GetMethod("AddRateEntryViaReflection", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString) });
			var entry = (BusinessObject)addRateEntryMethod.Invoke(quote, new object[] { mode, ZString.Empty, origin, destination });
			entry[RateEntrySchema.TI_RateStartDate] = ZDate.Today.AddMonths(-6);

			if (mode == "FCL")
			{
				entry[RateEntrySchema.TI_RC.Name] = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}

			if (!consignee.IsEmpty)
			{
				entry[RateEntrySchema.TI_OH_Consignee] = consignee;
			}

			if (!consignor.IsEmpty)
			{
				entry[RateEntrySchema.TI_OH_Consignor] = consignor;
			}

			return entry;
		}

		internal void DeleteOrganisation(OrgHeader org)
		{
			if (org != null)
			{
				var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, org.PK));
				LoadAndDeleteAll<JobDocAddress>(JobDocAddressSchema.E2_OA_Address, orgAddress.PK);
				orgAddress.Delete();

				var companyData = Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, org.PK));
				var invoiceGroup = Factory.LoadTop1<OrgInvoiceRollupOrGroup>(new ZQuery(OrgInvoiceRollupOrGroupSchema.PG_OB, companyData.PK));
				invoiceGroup.Delete();
				companyData.Delete();

				org.Delete();
			}
		}

		internal void DeleteBizo(BusinessObject bizo)
		{
			if (bizo is OrgHeader)
			{
				DeleteOrganisation(bizo as OrgHeader);
			}
			else if (bizo != null)
			{
				bizo.Delete();
			}
		}

		void LoadAndDeleteAll<T>(CargoWise.Schema.SchemaColumn column, object value) where T : BusinessObject
		{
			var results = Factory.Load<T>(new ZQuery(column, value));
			foreach (var result in results)
			{
				result.Delete();
			}
		}
	}
}
