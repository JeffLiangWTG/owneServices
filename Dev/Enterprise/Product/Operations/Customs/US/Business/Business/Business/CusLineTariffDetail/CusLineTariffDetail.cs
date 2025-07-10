using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusLineTariffDetail : AutoCusLineTariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new partial class Schema : AutoCusLineTariffDetail.Schema
		{
			public const string AdditionalTariffFormatted = "AdditionalTariffFormatted";
		}

		#endregion

		#region Override Properties

		[BusinessObjectTestExclude]
		public override ZString BZ_Tariff
		{
			get => base.BZ_Tariff;
			set
			{
				var valueToSet = TariffFormatter.Format(value);
				var hasChanges = valueToSet != BZ_Tariff;
				base.BZ_Tariff = valueToSet;
				if (hasChanges && !IsCopying)
				{
					if (InvoiceLine is JobComInvoiceLine invoiceLine)
					{
						var unitOfMeasure = ZString.Empty;
						if (!valueToSet.IsEmpty)
						{
							var tariff = Factory.GetCachedValue(valueToSet.PadRight(USCTariff.Schema.UE_TariffMaxLength) + invoiceLine.EffectiveDateForDutyRate.ToString(), () => new USCTariff.Loader(Factory).LoadBestMatch(valueToSet, invoiceLine.EffectiveDateForDutyRate));
							if (tariff != null)
							{
								unitOfMeasure = tariff.UE_Unit1;
							}
						}

						BZ_UQ1 = unitOfMeasure;
					}

					BZ_Value = ZDecimal.Zero;
				}
			}
		}

		public override ZString BZ_UQ1
		{
			get => base.BZ_UQ1;
			set
			{
				var hasChanges = BZ_UQ1 != value;
				base.BZ_UQ1 = value;
				if (hasChanges && !IsCopying)
				{
					BZ_Qty1 = ZDecimal.Zero;
				}
			}
		}

		#endregion

		#region New Properties

		#region AdditionalTariffFormatted

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.ImportTariffs))]
		[MaxLength(15)]
		public ZString AdditionalTariffFormatted
		{
			get => TariffFormatter.DisplayFormat(BZ_Tariff);
			set
			{
				ZString oldValue = AdditionalTariffFormatted;
				ZString formattedValue = value.ExcludeChars(" .");
				ZString newValue = formattedValue.Length > 10 ? formattedValue.SubstringSafe(0, 10) : formattedValue;
				BZ_Tariff = newValue;
				AdditionalTariffFormattedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdditionalTariffFormattedInfo => GetZPropertyInfo(CusLineTariffDetail.Schema.AdditionalTariffFormatted);

		TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = new TariffFormatter());
		TariffFormatter tariffFormatter;

		#endregion

		#endregion

		#region Lookups

		public new CusLineTariffDetailLookups Lookups
		{
			get { return (CusLineTariffDetailLookups)base.Lookups; }
		}

		protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups()
		{
			return new CusLineTariffDetailLookups(this);
		}

		#endregion
	}
}
