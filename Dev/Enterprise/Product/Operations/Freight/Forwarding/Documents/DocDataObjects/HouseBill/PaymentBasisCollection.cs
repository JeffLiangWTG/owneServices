using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class PaymentBasisCollection : IPaymentBasisCollection
	{
		public PaymentBasisCollection(JobCharge updateableCharge)
		{
			this.updateableCharge = Argument.NotNull(updateableCharge, nameof(updateableCharge));

			paymentBases = new Lazy<IPaymentBasis[]>(CreatePaymentBasisCollection);
		}

		readonly JobCharge updateableCharge;

		readonly Lazy<IPaymentBasis[]> paymentBases;

		#region CalculationBasis

		public ZString CalculationBasis
		{
			get
			{
				if (!calculationBasis.HasValue)
				{
					var bases = paymentBases
						.Value
						.Select(p => p.CalculationBasis)
						.Where(b => !string.IsNullOrWhiteSpace(b))
						.ToArray();

					calculationBasis = string.Join(System.Environment.NewLine, bases);
				}

				return calculationBasis.Value;
			}
		}

		ZString? calculationBasis;

		#endregion

		#region Quantity

		public ZString Quantity
		{
			get
			{
				if (!quantity.HasValue)
				{
					var quantities = paymentBases
						.Value
						.Select(p => p.Quantity)
						.Where(q => !string.IsNullOrWhiteSpace(q))
						.ToArray();

					quantity = string.Join(System.Environment.NewLine, quantities);
				}

				return quantity.Value;
			}
		}

		ZString? quantity;

		#endregion

		public int Count => paymentBases.Value.Length;

		#region Implementation

		IPaymentBasis[] CreatePaymentBasisCollection()
		{
			var rateUnitLookup = new CodeDescriptionPairList();
			var rateTypeLookup = new CodeDescriptionPairList();

			var bases = new ChargePaymentBasisCollection(updateableCharge);
			bases.Load();

			return bases.Cast<JobPaymentBasis>().Where(x => !x.PBS_IsCost)
				.Select(pb => AsPaymentBasisWrapper(pb, rateUnitLookup, rateTypeLookup))
				.ToArray();
		}

		IPaymentBasis AsPaymentBasisWrapper(JobPaymentBasis paymentBasis, ICodeDescriptionPairList rateUnitLookup, ICodeDescriptionPairList rateTypeLookup)
		{
			var wrapper = new PaymentBasis
			{
				Quantity = paymentBasis.Quantity,
				RateValue = paymentBasis.RateValue,
				RateUnit = new CodeDescription(rateUnitLookup)
				{
					Code = paymentBasis.RateUnit
				},
				RateType = new CodeDescription(rateTypeLookup)
				{
					Code = paymentBasis.PBS_RateReference
				},
				Currency = new CodeDescription(paymentBasis.Lookups.RateCurrencies)
				{
					Code = paymentBasis.RateCurrency.Code
				}
			};

			return wrapper;
		}

		IEnumerator IEnumerable.GetEnumerator() => paymentBases.Value.GetEnumerator();
		IEnumerator<IPaymentBasis> IEnumerable<IPaymentBasis>.GetEnumerator() => paymentBases.Value.AsEnumerable().GetEnumerator();

		#endregion
	}
}