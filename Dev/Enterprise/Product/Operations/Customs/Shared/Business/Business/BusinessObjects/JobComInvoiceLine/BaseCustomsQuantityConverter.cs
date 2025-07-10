using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface ISecondCustomsQuantity
	{
		ZPropertyInfo SecondCustomsUQInfo { get; }
		ZPropertyInfo SecondCustomsQtyInfo { get; }
	}

	public class BaseCustomsQuantityConverter
	{
		public BaseCustomsQuantityConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
		{
			if (invoiceLine == null)
			{
				throw new ApplicationException("Must pass non null invoice line to constructor");
			}
			if (customsQuantityInfo == null)
			{
				throw new ApplicationException("Must pass non QuantityInfo to constructor");
			}
			if (customsUnitOfQuantityInfo == null)
			{
				throw new ApplicationException("Must pass non null UnitOfQuantityInfo to constructor");
			}
			this.InvoiceLine = invoiceLine;
			this.customsQuantityInfo = customsQuantityInfo;
			this.customsUnitOfQuantityInfo = customsUnitOfQuantityInfo;
			CalculateConversionFactor();
		}

		public readonly BaseJobComInvoiceLine InvoiceLine;

		protected void CalculateCustomsQty()
		{
			var newCustomsQuantity = CalculateCustomsQuantityCore();

			if (newCustomsQuantity != 0 || customsQuantityInfo.ReadOnly)
			{
				customsQuantityInfo.Value = newCustomsQuantity;
			}
		}

		protected virtual ZDecimal CalculateCustomsQuantityCore()
		{
			return decimal.Round(CustomsConversionFactor * InvoiceLine.JI_InvoiceQuantity, 5);
		}

		public void CalculateFromNetWeightToCustomsQty()
		{
			var customsUQ = (ZString)customsUnitOfQuantityInfo.Value;
			if (InvoiceLine.CanConvertFromNetWeightToCustomsUnit(customsUQ))
			{
				customsQuantityInfo.Value = CalculateFromNetWeightToCustomsQtyCore();
			}
		}

		public virtual ZDecimal CalculateFromNetWeightToCustomsQtyCore()
		{
			return Core.Constants.Weight.Convert(InvoiceLine.JI_NetWeight, InvoiceLine.JI_NetWeightUQ, (ZString)customsUnitOfQuantityInfo.Value);
		}

		public void CalculateLineWeightOrQuantityFromCustomsQty()
		{
			if (!InvoiceLine.IsSettingInvoiceQuantity)
			{
				ZDecimal customsQuantity = (ZDecimal)customsQuantityInfo.Value;
				if (CustomsConversionFactor > 0 && customsQuantity > 0)
				{
					InvoiceLine.JI_InvoiceQuantity = (1 / CustomsConversionFactor) * customsQuantity;
				}
			}
		}

		public void CalculateCustomsFactorAndQty()
		{
			CalculateCustomsFactorAndQty(false);
		}

		public void CalculateCustomsFactorAndQty(bool isLoading)
		{
			fCustomsConversionFactor = null;
			if (!isLoading)
			{
				CalculateConversionFactor();
				CalculateCustomsQty();
				CalculateCountrySpecificQuantity();
			}
		}

		public ZDecimal CustomsConversionFactor
		{
			get { return fCustomsConversionFactor ?? 0; }
			protected set { fCustomsConversionFactor = value; }
		}

		#region Implementation

		protected ZPropertyInfo customsQuantityInfo;
		protected ZPropertyInfo customsUnitOfQuantityInfo;

		protected OrgSupplierPart Part
		{
			get { return InvoiceLine.Part; }
		}

		ZDecimal? fCustomsConversionFactor;

		protected internal UnitConverter UnitConverter
		{
			get { return InvoiceLine.UnitConverter; }
		}

		protected virtual void CalculateConversionFactor()
		{
			ZString customsUnitOfQuantity = (ZString)customsUnitOfQuantityInfo.Value;
			if (!customsUnitOfQuantity.IsEmpty)
			{
				if (Part != null && IsPartSpecificConversion)
				{
					CustomsConversionFactor = CalculatePartSpecificConversionFactor();
				}

				if (!fCustomsConversionFactor.HasValue && !InvoiceLine.JI_InvoiceUQ.IsEmpty)
				{
					CustomsConversionFactor = UnitConverter.ConversionFactor(InvoiceLine.JI_InvoiceUQ, customsUnitOfQuantity);
				}
			}
		}

		/// <summary>
		/// Override this to calculate Other quantities like ISS in AU AddInfo Items
		/// </summary>
		protected virtual void CalculateCountrySpecificQuantity()
		{
		}

		/// <summary>
		/// AU six beer tariffs have non-dutiable portion. It needs special calculation.
		/// If that's the case, override this to return true and override CalculateCountrySpecificPartConversionFactor()
		/// </summary>
		protected virtual bool IsPartSpecificConversion
		{
			get { return false; }
		}

		/// <summary>
		/// Override IsCountrySpecificPartConversion to true to make this Calculation kick in.
		/// AU six beer tariffs have non-dutiable portion. It needs special calculation. Override this to do this.
		/// </summary>
		/// <returns>CustomsConversionFactor to calculate CustomsQuantity from InvoiceQuantity</returns>
		protected virtual ZDecimal CalculatePartSpecificConversionFactor()
		{
			return 0m;
		}

		#endregion

	}
}
