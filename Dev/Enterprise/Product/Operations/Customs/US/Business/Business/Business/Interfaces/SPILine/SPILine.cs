using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	abstract class SPILine : ISPILine
	{
		public static SPILine New(JobComInvoiceLine invoiceLine)
		{
			SPILine result = null;

			if (invoiceLine != null)
			{
				result = invoiceLine.HasEmptySupTariff ?
					new SPINormalInvoiceLine(invoiceLine) :
					new SPISupInvoiceLine(invoiceLine);
			}

			return result;
		}

		public static SPILine New(CusClassPartPivot pivot)
		{
			SPILine result = null;

			if (pivot != null)
			{
				result = pivot.CI_FormattedSupplementalTariff.IsEmpty ?
					new SPINormalPivotLine(pivot) :
					new SPISupPivotLine(pivot);
			}

			return result;
		}

		public static SPILine NewReconOrig(JobComInvoiceLine invoiceLine)
		{
			SPILine result = null;

			if (invoiceLine != null)
			{
				result = invoiceLine.US_R_OrigSupTariff.IsEmpty ?
					new ReconOrigNormalSPILine(invoiceLine) :
					new ReconOrigSupSPILine(invoiceLine);
			}

			return result;
		}

		protected SPILine(BusinessObjectFactory factory, ZDateTime dutyDate, ZString countryOfOriginCode, ISPILine parentTariffLine, ZString countryOfExportCode)
		{
			this.factory = factory;
			this.dutyDate = dutyDate;
			this.countryOfOriginCode = countryOfOriginCode;
			this.parentTariffLine = parentTariffLine;
			this.countryOfExportCode = countryOfExportCode;
		}

		readonly BusinessObjectFactory factory;
		readonly ZDateTime dutyDate;
		readonly ZString countryOfOriginCode;
		readonly ZString countryOfExportCode;
		readonly ISPILine parentTariffLine;

		#region ISPILine Members

		public ZDateTime EffectiveDate
		{
			get { return dutyDate; }
		}

		public USCCountry CountryOfOrigin
		{
			get { return factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, countryOfOriginCode); }
		}

		public ZString CountryOfOriginCode
		{
			get { return countryOfOriginCode; }
		}

		public USCCountry CountryOfExport => factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, countryOfExportCode);

		public ZString CountryOfExportCode => countryOfExportCode;

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		public bool HasSecondaryChildrenLines
		{
			get { return SecondaryTariffLines.Any(); }
		}

		public ISPILine ParentTariffLine
		{
			get { return parentTariffLine; }
		}

		public USCTariff ImportTariff
		{
			get { return ImportTariffCore; }
		}

		protected abstract USCTariff ImportTariffCore { get; }

		public ZString ImportTariffCode
		{
			get { return ImportTariffCodeCore; }
		}

		protected abstract ZString ImportTariffCodeCore { get; }

		public IEnumerable<ISPILine> SecondaryTariffLines
		{
			get { return SecondaryTariffLinesCore; }
		}

		protected abstract IEnumerable<ISPILine> SecondaryTariffLinesCore { get; }

		#endregion
	}
}
