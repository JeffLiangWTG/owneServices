using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class TariffCustomsUnitDefaultingStrategy<TJobComInvoiceLine> : CustomsUnitDefaultingStrategy<TJobComInvoiceLine>
		where TJobComInvoiceLine : BaseJobComInvoiceLine
	{
		public TariffCustomsUnitDefaultingStrategy(Func<TJobComInvoiceLine, ITariff> getTariff, bool defaultFirstUnitOnly = false)
			: this(getTariff, (true, !defaultFirstUnitOnly, !defaultFirstUnitOnly, !defaultFirstUnitOnly, !defaultFirstUnitOnly))
		{
		}

		public TariffCustomsUnitDefaultingStrategy(Func<TJobComInvoiceLine, ITariff> getTariff, (bool defaultFirstUnit, bool defaultSecondUnit, bool defaultThirdUnit, bool defaultFourthUnit, bool defaultFifthUnit) defaultUnits)
		{
			defaultFirstUnit = defaultUnits.defaultFirstUnit;
			defaultSecondUnit = defaultUnits.defaultSecondUnit;
			defaultThirdUnit = defaultUnits.defaultThirdUnit;
			defaultFourthUnit = defaultUnits.defaultFourthUnit;
			defaultFifthUnit = defaultUnits.defaultFifthUnit;
			this.getTariff = getTariff;
			deinitialiseActions = new List<Action>();
		}

		protected readonly Func<TJobComInvoiceLine, ITariff> getTariff;
		readonly bool defaultFirstUnit;
		readonly bool defaultSecondUnit;
		readonly bool defaultThirdUnit;
		readonly bool defaultFourthUnit;
		readonly bool defaultFifthUnit;
		protected List<Action> deinitialiseActions;

		public override void Initialise(TJobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_TariffInfo.ValueChanged += ValueChanged;
			invoiceLine.JI_JZInfo.ValueChanged += AttachedToInvoice;
			AttachedToInvoice(null, null);

			deinitialiseActions.Add(() =>
			{
				invoiceLine.JI_TariffInfo.ValueChanged -= ValueChanged;
				invoiceLine.JI_JZInfo.ValueChanged -= AttachedToInvoice;
				var declaration = invoiceLine.Declaration;
				if (declaration != null)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= ValueChanged;
				}
			});

			void AttachedToInvoice(object sender, EventArgs args)
			{
				var declaration = invoiceLine.Declaration;
				if (declaration != null)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= ValueChanged;
					declaration.JE_MessageTypeInfo.ValueChanged += ValueChanged;
				}
			}

			void ValueChanged(object sender, EventArgs args)
			{
				DefaultUOMs(invoiceLine);
			}
		}

		public override void Deinitialise(TJobComInvoiceLine invoiceLine)
		{
			foreach (var action in deinitialiseActions)
			{
				action();
			}
		}

		public override void DefaultUOMs(TJobComInvoiceLine invoiceLine)
		{
			var tariff = getTariff?.Invoke(invoiceLine);
			if (tariff != null)
			{
				SetUnitIf(defaultFirstUnit, invoiceLine.JI_CustomsUnitQtyInfo, tariff.UQ1);
				SetUnitIf(defaultSecondUnit, invoiceLine.JI_CustomsSecondUnitQtyInfo, tariff.UQ2);
				SetUnitIf(defaultThirdUnit, invoiceLine.JI_CustomsThirdUnitQtyInfo, tariff.UQ3);
				SetUnitIf(defaultFourthUnit, invoiceLine.JI_CustomsFourthUnitQtyInfo, tariff.UQ4);
				SetUnitIf(defaultFifthUnit, invoiceLine.JI_CustomsFifthUnitQtyInfo, tariff.UQ5);

				void SetUnitIf(bool condition, ZPropertyInfo info, ZString value)
				{
					if (condition)
					{
						info.Value = value.SubstringSafe(0, info.MaxLength);
					}
				}
			}
		}
	}
}
