using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public abstract class CommonCMRDataParser
	{
		protected abstract string FileNamePrefix { get; }

		protected abstract IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate);

		protected abstract string OutputXMLName { get; }

		protected abstract string DataSource { get; }

		protected const string InsufficientInfoErrorMessage = "Skipped line '{0}' as it doesn't have enough info";

		protected const string InvalidTimeErrorMessage = "Skipped line '{0}' as it's start date is after it's end date";

		protected const string CodeHasExpiredErrorMessage = "Skipped line '{0}' as it is expired";
	}
}
