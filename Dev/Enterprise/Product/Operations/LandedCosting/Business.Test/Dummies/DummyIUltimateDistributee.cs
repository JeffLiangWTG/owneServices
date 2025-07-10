using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	public class DummyIUltimateDistributee : DummyBusinessObject, IUltimateDistributee
	{
		public DummyIUltimateDistributee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		#region IUltimateDistributee Members

		public ZGuid PKExposed;
		public new ZGuid PK
		{
			get { return PKExposed; }
		}

		public ZString HumanReadableCodeExposed;
		public ZString HumanReadableCode
		{
			get { return HumanReadableCodeExposed; }
		}

		public ZPropertyInfo[] HumanReadableCodeInfosExposed;
		public ZPropertyInfo[] HumanReadableCodeInfos
		{
			get { return HumanReadableCodeInfosExposed; }
		}

		public string TableCodeExposed;
		public string TableCode
		{
			get { return TableCodeExposed; }
		}

		public ZDecimal UnitPriceInInvoiceCurrencyExposed;
		public ZDecimal UnitPriceInInvoiceCurrency
		{
			get { return UnitPriceInInvoiceCurrencyExposed; }
		}

		public ZDecimal CostInLocalCurrencyExposed;
		public ZDecimal CostInLocalCurrency
		{
			get { return CostInLocalCurrencyExposed; }
		}

		public ZDecimal ActualExposed;
		public ZDecimal Actual
		{
			get { return ActualExposed; }
		}

		public ZDecimal ActualWeightInKGExposed;
		public ZDecimal ActualWeightInKG
		{
			get { return ActualWeightInKGExposed; }
		}

		public ZDecimal ActualVolumeInM3Exposed;
		public ZDecimal ActualVolumeInM3
		{
			get { return ActualVolumeInM3Exposed; }
		}

		public ZGuid FKToProductExposed;
		public ZGuid FKToProduct
		{
			get { return FKToProductExposed; }
		}

		public ZDecimal ItemCountExposed;
		public ZDecimal ItemCount
		{
			get { return ItemCountExposed; }
		}

		public ZDecimal DutyPercentExposed;
		public ZDecimal DutyPercent
		{
			get { return DutyPercentExposed; }
		}

		public ZDecimal CustomsValueExposed;
		public ZDecimal CustomsValue
		{
			get { return CustomsValueExposed; }
		}

		public DutyTaxEntryFee LineDutyTaxEntryFeeItemsExposed;
		public DutyTaxEntryFee LineDutyTaxEntryFeeItems
		{
			get { return LineDutyTaxEntryFeeItemsExposed; }
		}

		public ZString InvoiceNumberExposed;
		public ZString InvoiceNumber
		{
			get { return InvoiceNumberExposed; }
		}

		public ZString SupplierNameExposed;
		public ZString SupplierName
		{
			get { return SupplierNameExposed; }
		}

		public ZString InvoiceCurrencyCodeExposed;
		public ZString InvoiceCurrencyCode
		{
			get { return InvoiceCurrencyCodeExposed; }
		}

		public ZString OrderNumberExposed;
		public ZString OrderNumber
		{
			get { return OrderNumberExposed; }
		}

		public ZInt OrderLineNumberExposed;
		public ZInt OrderLineNumber
		{
			get { return OrderLineNumberExposed; }
		}

		public ZString LineDescriptionExposed;
		public ZString LineDescription
		{
			get { return LineDescriptionExposed; }
		}

		public ZDecimal LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrencyExposed;
		public ZDecimal LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency
		{
			get { return LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrencyExposed; }
		}

		public LCMarginPercentages ProductSpecificLCMarginPercentageExposed;
		public LCMarginPercentages GetProductSpecificLCMarginPercentagesForFallBack(OrgHeader consignee)
		{
			return ProductSpecificLCMarginPercentageExposed ?? new LCMarginPercentages();
		}

		public ZString ProductCodeExposed;
		public ZString ProductCode
		{
			get { return ProductCodeExposed; }
		}

		public ZString ProductDepartmentExposed;
		public ZString ProductDepartment
		{
			get { return ProductDepartmentExposed; }
		}

		public ZString ProductDivisionExposed;
		public ZString ProductDivision
		{
			get { return ProductDivisionExposed; }
		}

		public IEnumerable<ICustomsFee> Fees
		{
			get { return fFees ??= Array.Empty<ICustomsFee>(); }
			set
			{
				fFees = value;
			}
		}
		IEnumerable<ICustomsFee> fFees;

		#endregion

		#region IUltimateDistributee Members

		public ZString CountryOfOriginCodeExposed;
		public ZString CountryOfOriginCode
		{
			get { return CountryOfOriginCodeExposed; }
		}

		public ZDecimal CustomsQuantityExposed;
		public ZDecimal CustomsQuantity
		{
			get { return CustomsQuantityExposed; }
		}

		public ZString CustomsUQExposed;
		public ZString CustomsUQ
		{
			get { return CustomsUQExposed; }
		}

		public ZString DutyRateDescriptionExposed;
		public ZString DutyRateDescription
		{
			get { return DutyRateDescriptionExposed; }
		}

		public ZString InvoiceUQExposed;
		public ZString InvoiceUQ
		{
			get { return InvoiceUQExposed; }
		}

		public ZDecimal LinePriceInInvoiceCurrencyExposed;
		public ZDecimal LinePriceInInvoiceCurrency
		{
			get { return LinePriceInInvoiceCurrencyExposed; }
		}

		public ZString TariffNumberExposed;
		public ZString TariffNumber
		{
			get { return TariffNumberExposed; }
		}

		public ZDecimal VolumeExposed;
		public ZDecimal Volume
		{
			get { return VolumeExposed; }
		}

		public ZString VolumeUQExposed;
		public ZString VolumeUQ
		{
			get { return VolumeUQExposed; }
		}

		public ZDecimal WeightExposed;
		public ZDecimal Weight
		{
			get { return WeightExposed; }
		}

		public ZString WeightUQExposed;
		public ZString WeightUQ
		{
			get { return WeightUQExposed; }
		}

		public ZDecimal GSTVATAmountExposed;
		public ZDecimal GSTVATAmount
		{
			get { return GSTVATAmountExposed; }
		}

		#endregion

		#region IUltimateDistributee Members

		public virtual ZDecimal CalculateOwnGstVatRate()
		{
			throw new NotImplementedException();
		}

		public virtual ZBool IsCapableOfCalculatingOwnGstVatRate
		{
			get { return false; }
		}

		#endregion
	}
}
