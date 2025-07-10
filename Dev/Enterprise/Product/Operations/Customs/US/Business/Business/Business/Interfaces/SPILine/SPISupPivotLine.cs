using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class SPISupPivotLine : SPILine
	{
		public SPISupPivotLine(CusClassPartPivot pivot)
			: base(pivot.Factory, pivot.EffectiveDate, pivot.CD_UC_NKCountryOfOrigin, SPILine.New(pivot.Parent), pivot.CD_UC_NKCountryOfExport)
		{
			this.pivot = pivot;
		}

		readonly CusClassPartPivot pivot;

		protected override CargoWise.Types.ZString ImportTariffCodeCore
		{
			get { return pivot.CI_SupplementalTariff; }
		}

		protected override USCTariff ImportTariffCore
		{
			get { return pivot.ImportSupTariff; }
		}

		protected override IEnumerable<ISPILine> SecondaryTariffLinesCore
		{
			get
			{
				if (pivot.CI_CI_Parent.IsEmpty)
				{
					yield return new SPINormalPivotLine(pivot);
				}

				foreach (CusClassPartPivot secondaryLine in pivot.Children)
				{
					if (secondaryLine.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT && secondaryLine.CD_ProductClaim != SecondarySpecProgIndicatorList.Codes.V)
					{
						if (!secondaryLine.CI_FormattedSupplementalTariff.IsEmpty && !secondaryLine.CI_FormattedSupplementalTariff.Equals(pivot.CI_FormattedSupplementalTariff))
						{
							yield return new SPISupPivotLine(secondaryLine);
						}

						yield return new SPINormalPivotLine(secondaryLine);
					}
				}
			}
		}
	}
}
