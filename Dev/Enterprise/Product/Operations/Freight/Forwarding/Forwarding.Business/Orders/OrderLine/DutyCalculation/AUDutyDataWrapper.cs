using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public delegate void SetDutyResultDelegate(ZGuid pK, DutyResult dutyResult);
	public delegate void SetFeeResultDelegate(ZGuid pK, ZString feeType, ZDecimal feeAmount);

	class AUDutyDataWrapper : ICMRDutyData, ICustomsDetails, IUnitConverterDataProvider
	{
		public AUDutyDataWrapper(OrderLine orderLine, bool willBeSACEntry, BusinessObjectFactory factory, SetDutyResultDelegate setDutyResult, SetFeeResultDelegate setFeeResult)
		{
			this.OrderLine = orderLine;
			this.factory = factory;

			this.setDutyResult = setDutyResult;
			this.setFeeResult = setFeeResult;

			this.willBeSACEntry = willBeSACEntry;
		}

		public readonly OrderLine OrderLine;
		readonly BusinessObjectFactory factory;
		readonly SetDutyResultDelegate setDutyResult;
		readonly SetFeeResultDelegate setFeeResult;
		readonly bool willBeSACEntry;

		#region ICMRDutyData Members

		ZDecimal ICMRDutyData.CustomsValue
		{
			get { return OrderLine.CustomsValue; }
		}

		ZDecimal ICMRDutyData.CountervailingDuty
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal ICMRDutyData.DumpingDuty
		{
			get { return ZDecimal.Zero; }
		}

		BusinessObjectFactory ICMRDutyData.Factory
		{
			get { return factory; }
		}

		public bool IsProductClassified
		{
			get { return !DutyData.FirstTariffNumber.IsEmpty; }
		}

		DutyDataFromInvoiceLine DutyData
		{
			get
			{
				if (!dutyData.HasValue)
				{
					dutyData = new DutyDataFromInvoiceLine();

					if (OrderLine.Product != null && OrderLine.Product.IsClassifiedFor(Core.Constants.CountryCodes.Australia, new ZString[] { Customs.Common.ClassificationType.IMP }))
					{
						IDutyDataFromInvoiceLineProvider provider = factory.Load(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgSupplierPart>(), OrderLine.Product.PK) as IDutyDataFromInvoiceLineProvider;
						if (provider != null)
						{
							Order order = OrderLine.Order;

							ZDateTime dutyDate = order.GetMilestoneActualDate(Events.Arrival).IsValid ? order.GetMilestoneActualDate(Events.Arrival) : (order.GetMilestoneEstimatedDate(Events.Arrival).IsValid ? order.GetMilestoneEstimatedDate(Events.Arrival) : ZDateTime.Today);
							dutyData = provider.GetDutyDataFromInvoiceLine(dutyDate, order.BuyerPK, order.SupplierPK);
						}
					}
				}
				return dutyData.Value;
			}
		}
		DutyDataFromInvoiceLine? dutyData;

		ZDecimal ICMRDutyData.FirstQty
		{
			get { return ConvertFromOrderLine(DutyData.FirstUQ); }
		}

		ZDecimal ICMRDutyData.SecondQty
		{
			get { return ConvertFromOrderLine(DutyData.SecondUQ); }
		}

		ZDecimal ConvertFromOrderLine(ZString customsUQ)
		{
			ZDecimal result = ZDecimal.Zero;

			UnitConverter unitConverter = new UnitConverter(this);

			if (OrderLine.JO_ActualWeight > 0 && unitConverter.Convertible(OrderLine.JO_UnitOfWeight, customsUQ))
			{
				result = unitConverter.Convert(OrderLine.JO_ActualWeight, OrderLine.JO_UnitOfWeight, customsUQ);
			}
			else if (OrderLine.JO_ActualVolume > 0 && unitConverter.Convertible(OrderLine.JO_UnitOfVolume, customsUQ))
			{
				result = unitConverter.Convert(OrderLine.JO_ActualVolume, OrderLine.JO_UnitOfVolume, customsUQ);
			}
			else if (OrderLine.JO_Quantity > 0 && unitConverter.Convertible(OrderLine.JO_F3_NKPackType, customsUQ))
			{
				result = unitConverter.Convert(OrderLine.JO_Quantity, OrderLine.JO_F3_NKPackType, customsUQ);
			}

			return result;
		}

		bool ICMRDutyData.IsGSTDeferred
		{
			get { return OrderLine.Order != null && OrderLine.Order.Buyer != null && OrderLine.Order.Buyer.MiscServ.OM_IMIsGSTDeferred; }
		}

		ZBool ICMRDutyData.IsNature20
		{
			get { return ZBool.False; }
		}

		bool ICMRDutyData.IsSubjectToDutyAndTax
		{
			get { return !willBeSACEntry; }
		}

		bool ICMRDutyData.IsDutyAndTaxEstimatedForWH
		{
			get { return false; }
		}

		bool ICMRDutyData.IsNotLowValueShipment
		{
			get { return !willBeSACEntry; }
		}

		Money ICMRDutyData.ManualDutyAmount
		{
			get { return null; }
		}

		ZDecimal ICMRDutyData.OtherDutyFactor
		{
			get { return ZDecimal.Zero; }
		}

		DutyDataFromInvoiceLine ICMRDutyData.RandomLineDutyData
		{
			get { return DutyData; }
		}

		ZDecimal ICMRDutyData.TransportAndInsuranceInAUD
		{
			get
			{
				List<Customs.Common.JobComInvCharge> charges = new List<Customs.Common.JobComInvCharge>();

				charges.AddRange(OrderLine.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight));
				charges.AddRange(OrderLine.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance));
				charges.AddRange(OrderLine.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight));
				charges.AddRange(OrderLine.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance));

				Money result = Money.Empty;
				CurrencyConverter converter = ((ICommonInvoice)OrderLine).CurrencyConverter;

				foreach (Customs.Common.JobComInvCharge charge in charges)
				{
					result = converter.Add(result, charge.Money);
				}

				return converter.ConvertExact(result, GlbCompany.CurrentCompany.LocalCurrency).Amount;
			}
		}

		#endregion

		#region ILineDutyData Members

		void ILineDutyData.SetDutyResult(DutyResult dutyResult)
		{
			setDutyResult(OrderLine.PK, dutyResult);
		}

		void ILineDutyData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
			setFeeResult(OrderLine.PK, feeType, feeAmount);
		}

		#endregion

		#region ICustomsDetails Members

		ZString ICustomsDetails.TariffNumber
		{
			get { return DutyData.FirstTariffNumber + " " + DutyData.StatCode; }
		}

		ZString ICustomsDetails.CustomsUQ
		{
			get { return DutyData.FirstUQ; }
		}

		ZDecimal ICustomsDetails.CustomsQuantity
		{
			get { return ((ICMRDutyData)this).FirstQty; }
		}

		#endregion

		ZString IUnitConverterDataProvider.CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		BusinessObjectFactory IUnitConverterDataProvider.Factory
		{
			get { return this.factory; }
		}

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
		{
			var product = OrderLine.Product;
			return product != null ? product.GetUnitConversionFactorsFromProductUnits() : System.Array.Empty<IUnitConverter>();
		}

		OrgSupplierPart IUnitConverterDataProvider.Product
		{
			get { return this.OrderLine.Product; }
		}

		ZGuid IUnitConverterDataProvider.SupplierFK
		{
			get
			{
				var order = OrderLine.Order;
				return order != null ? order.SupplierPK : ZGuid.Empty;
			}
		}

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions
		{
			get
			{
				var product = OrderLine.Product;
				return product != null && product.HasSpecificUnitConversions();
			}
		}

		ZString IUnitConverterDataProvider.Type
		{
			get { return RPTypeList.Codes.AllAreas; }
		}
	}
}
