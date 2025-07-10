using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	public class TestHelper
	{
		public TestHelper()
		{
			factory = new BusinessObjectFactory();
		}

		#region Buyer

		public OrgHeader Buyer
		{
			get
			{
				if (fBuyer == null)
				{
					fBuyer = factory.NewWithValidTestData<OrgHeader>();
					fBuyer.MainAddress.OA_Address1 = "TEST BUYER ADDRESS";
					fBuyer.OH_FullName = "TEST BUYER";
					fBuyer.OH_RL_NKClosestPort = "AUSYD";
					fBuyer.OH_IsConsignee = true;

					factory.Save();
				}

				return fBuyer;
			}
		}

		OrgHeader fBuyer;

		#endregion

		#region Supplier

		public OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = factory.NewWithValidTestData<OrgHeader>();
					fSupplier.MainAddress.OA_Address1 = "TEST SUPPLIER ADDRESS";
					fSupplier.OH_FullName = "TEST SUPPLIER";
					fSupplier.OH_RL_NKClosestPort = "USLAX";
					fSupplier.OH_IsConsignor = true;

					factory.Save();
				}

				return fSupplier;
			}
		}

		OrgHeader fSupplier;

		#endregion

		#region USDCurrency

		public RefCurrency USDCurrency
		{
			get
			{
				if (fUSDCurrency == null)
				{
					ZString uSDCurrencyString = "USD";
					fUSDCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, uSDCurrencyString);
				}

				return fUSDCurrency;
			}
		}

		RefCurrency fUSDCurrency;

		#endregion

		#region Create Order And OrderLine

		public Order CreateOrder(BusinessObjectFactory newFactory, ZString orderNo, ZByte splitNumber, ZDateTime orderDate, ZString invoiceNo, ZDateTime invoiceDate)
		{
			Order result = newFactory.New<Order>();
			result.BuyerPK = Buyer.PK;
			result.SupplierPK = Supplier.PK;
			result.JD_OrderNumber = orderNo;
			result.JD_OrderNumberSplit = splitNumber;
			result.JD_OrderDate = orderDate;
			result.JD_InvoiceNumber = invoiceNo;
			result.JD_InvoiceDate = invoiceDate;
			result.JD_RX_NKOrderCurrency = USDCurrency.RX_Code;
			result.JD_IncoTerm = Constants.IncoTerms.FreeOnBoard;

			OrderLine line = CreateOrderLine(newFactory);
			line.JO_JD = result.PK;
			result.OrderLines.Add(line);

			return result;
		}

		public OrderLine CreateOrderLine(BusinessObjectFactory newFactory)
		{
			OrderLine line = newFactory.New<OrderLine>();
			line.JO_Quantity = 10m;
			line.JO_QtyInvoiced = 6m;
			line.JO_QtyReceived = 5m;
			line.JO_F3_NKPackType = "UNT";
			line.JO_ItemPrice = 15m;
			line.JO_Description = "DUMMY PART DESCRIPTION";
			line.JO_Partno = "PARTNO1";
			line.JO_CustomAttrib1 = "CustomAttrib1";
			line.JO_CustomAttrib2 = "CustomAttrib2";
			line.JO_CustomAttrib3 = "CustomAttrib3";
			line.JO_CustomAttrib4 = "CustomAttrib4";
			line.JO_CustomAttrib5 = "CustomAttrib5";
			line.JO_CustomAttrib6 = "CustomAttrib6";
			line.JO_CustomTextBlob1 = "CustomTextBlob1";
			line.JO_PartAttrib1 = "PartAttrib1";
			line.JO_PartAttrib2 = "PartAttrib2";
			line.JO_PartAttrib3 = "PartAttrib3";
			line.JO_SerialNumber = "SerialNumber";
			line.JO_CustomDate1 = new ZDateTime(2006, 6, 1);
			line.JO_CustomDate2 = new ZDateTime(2006, 6, 2);
			line.JO_CustomDate3 = new ZDateTime(2006, 6, 3);
			line.JO_CustomDate4 = new ZDateTime(2006, 6, 4);
			line.JO_CustomDate5 = new ZDateTime(2006, 6, 5);
			line.JO_CustomDecimal1 = 1m;
			line.JO_CustomDecimal2 = 2m;
			line.JO_CustomDecimal3 = 3m;
			line.JO_CustomDecimal4 = 4m;
			line.JO_CustomDecimal5 = 5m;
			line.JO_CustomFlag1 = ZBool.True;
			line.JO_CustomFlag2 = ZBool.False;
			line.JO_CustomFlag3 = ZBool.True;
			line.JO_CustomFlag4 = ZBool.False;
			line.JO_CustomFlag5 = ZBool.True;

			return line;
		}

		#endregion

		public static void MakeConsolRelevantToDeclaration(ForwardingConsol consol, BaseJobDeclaration declaration)
		{
			var countryCode = declaration.CountryCode;
			var localPort = consol.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).RL_Code;
			var foreignPort = consol.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, countryCode)).RL_Code;
			consol.JK_RL_NKLoadPort = declaration.IsImport ? foreignPort : localPort;
			consol.JK_RL_NKDischargePort = declaration.IsImport ? localPort : foreignPort;
		}

		public static void DisableMergeRequirementForAllDeclarationsInFactory(BusinessObjectFactory factory)
		{
			foreach (BusinessObject bO in ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects)
			{
				var dec = bO as BaseJobDeclaration;
				if (dec != null)
				{
					dec.MergeManager.DisablePreSaveMergeRequirementForTesting();
				}
			}
		}

		readonly BusinessObjectFactory factory;
	}
}
