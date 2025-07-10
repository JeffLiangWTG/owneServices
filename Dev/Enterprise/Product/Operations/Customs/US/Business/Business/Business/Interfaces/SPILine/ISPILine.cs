using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface ISPILine
	{
		ZDateTime EffectiveDate { get; }
		USCTariff ImportTariff { get; }
		ZString ImportTariffCode { get; }
		USCCountry CountryOfOrigin { get; }
		USCCountry CountryOfExport { get; }
		ZString CountryOfOriginCode { get; }
		ZString CountryOfExportCode { get; }
		BusinessObjectFactory Factory { get; }

		ISPILine ParentTariffLine { get; }
		bool HasSecondaryChildrenLines { get; }
		IEnumerable<ISPILine> SecondaryTariffLines { get; }
	}

	public static class ISPILineExtensionMethods
	{
		public static string GetKeyForSPIList(this ISPILine spiLine)
		{
			return GetSPIListKeyOnItsOwnLevel(spiLine) +
				GetSPIListKeyForItsSecondaryLines(spiLine) +
				GetSPIListKeyForParent(spiLine);
		}

		static string GetSPIListKeyOnItsOwnLevel(ISPILine spiLine)
		{
			return spiLine.ImportTariffCode + spiLine.EffectiveDate + spiLine.CountryOfOriginCode + spiLine.CountryOfExportCode;
		}

		static string GetSPIListKeyForItsSecondaryLines(ISPILine spiLine)
		{
			string result = string.Empty;

			ZStringBuilder builder = null;

			foreach (ISPILine secondaryLine in spiLine.SecondaryTariffLines)
			{
				if (builder == null)
				{
					builder = new ZStringBuilder();
				}

				builder.Append(GetSPIListKeyOnItsOwnLevel(secondaryLine));//origin and duty date is the same as its parent
			}

			if (builder != null)
			{
				result = builder.ToString();
			}

			return result;
		}

		static string GetSPIListKeyForParent(ISPILine spiLine)
		{
			return spiLine.ParentTariffLine != null ? GetSPIListKeyOnItsOwnLevel(spiLine.ParentTariffLine) : string.Empty;
		}
	}
}
