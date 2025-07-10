using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class QuotaCustomsUnitDefaultingStrategy<TJobComInvoiceLine> : CustomsUnitDefaultingStrategy<TJobComInvoiceLine>
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public QuotaCustomsUnitDefaultingStrategy(Func<TJobComInvoiceLine, RefCusQuota> getCusQuota,
			IsConvertibleFrom isConvertibleFrom = null, ZPropertyInfo[] additionalValueChangedInfos = null,
			ZPropertyInfo[] additionalCustomsUnitQtyInfos = null)
		{
			deinitialiseActions = new List<Action>();
			this.getCusQuota = Argument.NotNull(getCusQuota, nameof(getCusQuota));
			this.additionalCustomsUnitQtyInfos = additionalCustomsUnitQtyInfos;
			this.additionalValueChangedInfos = additionalValueChangedInfos;
			IsConvertibleFrom = isConvertibleFrom ?? ((unitQty, list, countryCode, factory) => false);
		}

		public IsConvertibleFrom IsConvertibleFrom { get; }

		readonly Func<TJobComInvoiceLine, RefCusQuota> getCusQuota;

		readonly List<Action> deinitialiseActions;

		readonly ZPropertyInfo[] additionalCustomsUnitQtyInfos;

		readonly ZPropertyInfo[] additionalValueChangedInfos;

		public override void Initialise(TJobComInvoiceLine invoiceLine)
		{
			var valueChangedInfos = new[] { invoiceLine.JI_ConcessionOrderInfo };

			if (additionalValueChangedInfos != null)
			{
				valueChangedInfos = valueChangedInfos.Union(additionalValueChangedInfos.WhereNotNull()).ToArray();
			}

			foreach (var valueChangedInfo in valueChangedInfos)
			{
				valueChangedInfo.ValueChanged += ValueChanged;
				deinitialiseActions.Add(() =>
				{
					valueChangedInfo.ValueChanged -= ValueChanged;
				});
			}

			void ValueChanged(object sender, EventArgs args)
			{
				DefaultUOMs(invoiceLine);
			}
		}

		public override void Deinitialise(TJobComInvoiceLine invoiceLine)
		{
			base.Deinitialise(invoiceLine);
			foreach (var action in deinitialiseActions)
			{
				action();
			}
		}

		public override void DefaultUOMs(TJobComInvoiceLine invoiceLine)
		{
			var cusQuota = getCusQuota(invoiceLine);
			if (cusQuota == null)
			{
				return;
			}

			var unitOfMeasure = cusQuota.ZXQ_UnitOfMeasure;
			var unitQtyInfos = new[]
			{
				invoiceLine.JI_CustomsUnitQtyInfo, invoiceLine.JI_CustomsSecondUnitQtyInfo, invoiceLine.JI_CustomsThirdUnitQtyInfo
			};

			if (additionalCustomsUnitQtyInfos != null)
			{
				unitQtyInfos = unitQtyInfos.Union(additionalCustomsUnitQtyInfos.WhereNotNull()).ToArray();
			}

			var unitQtyValues = unitQtyInfos.Select(x => (ZString)x.Value).Where(x => !x.IsEmpty).ToImmutableHashSet();

			if (unitQtyValues.Contains(unitOfMeasure) ||
				IsConvertibleFrom(unitOfMeasure, unitQtyValues, invoiceLine.CustomsCountryCode, invoiceLine.Factory))
			{
				return;
			}

			foreach (var unitQtyInfo in unitQtyInfos)
			{
				if (unitQtyInfo.Value.IsEmpty)
				{
					unitQtyInfo.Value = unitOfMeasure;
					break;
				}
			}
		}
	}
}
