using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class SPINormalPivotLine : SPILine
	{
		public SPINormalPivotLine(CusClassPartPivot pivot)
			: base(pivot.Factory, pivot.EffectiveDate, pivot.CD_UC_NKCountryOfOrigin, SPILine.New(pivot.Parent), pivot.CD_UC_NKCountryOfExport)
		{
			this.pivot = pivot;
		}

		readonly CusClassPartPivot pivot;

		protected override CargoWise.Types.ZString ImportTariffCodeCore
		{
			get { return pivot.TariffNumber; }
		}

		protected override USCTariff ImportTariffCore
		{
			get { return pivot.ImportTariff; }
		}

		protected override IEnumerable<ISPILine> SecondaryTariffLinesCore
		{
			get
			{
				if (pivot.CI_FormattedSupplementalTariff.IsEmpty)
				{
					foreach (CusClassPartPivot secondaryLine in pivot.Children)
					{
						if (secondaryLine.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT && secondaryLine.CD_ProductClaim != SecondarySpecProgIndicatorList.Codes.V)
						{
							yield return new SPINormalPivotLine(secondaryLine);
						}
					}
				}
			}
		}
	}
}
