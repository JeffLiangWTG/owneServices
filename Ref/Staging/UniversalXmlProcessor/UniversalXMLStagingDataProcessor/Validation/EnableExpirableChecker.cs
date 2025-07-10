using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DataProcessingExplanation;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class EnableExpirableChecker
	{
		readonly IMetadataProvider metadataProvider;

		public EnableExpirableChecker(IMetadataProvider metadataProvider)
		{
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			this.metadataProvider = metadataProvider;
		}

		public void ValidateWrapper(IStagingDataWrapper wrapper)
		{
			var entityType = wrapper.GetStagingType();
			if (!ShouldValidateEnableExpirable(entityType))
			{
				return;
			}

			var tablePrefix = entityType.GetTablePrefix();
			var startDateValue = wrapper.GetWrapperValue(tablePrefix + "_StartDate");
			var endDateValue = wrapper.GetWrapperValue(tablePrefix + "_EndDate");
			ValidateAndThrowExceptionIfConditionsAreNotMet(metadataProvider.EnableExpirable, startDateValue, endDateValue);
		}

		public void ValidateNewestObjectFromSafeDb(object newestObjectFromSafeDb)
		{
			var entityType = newestObjectFromSafeDb.GetEntityType();
			if (!ShouldValidateEnableExpirable(entityType))
			{
				return;
			}

			var tablePrefix = entityType.GetTablePrefix();
			var startDateValue = newestObjectFromSafeDb.GetValue(tablePrefix + "_StartDate");
			var endDateValue = newestObjectFromSafeDb.GetValue(tablePrefix + "_EndDate");
			ValidateAndThrowExceptionIfConditionsAreNotMet(metadataProvider.EnableExpirable, startDateValue, endDateValue);
		}

		static void ValidateAndThrowExceptionIfConditionsAreNotMet(bool enableExpirable, object startDateValue, object endDateValue)
		{
			if ((enableExpirable
				&& (startDateValue == null || endDateValue == null)) ||
				(!enableExpirable
				&& (startDateValue != null || endDateValue != null)))
			{
				throw new RefDataProcessingException("The setup for EnableExpirable is invalid. Current matched StartDate and EndDate values from the database differ from values provided on the XML.", ErrorCodes.InvalidDataMatch);
			}
		}

		public static bool ShouldValidateEnableExpirable(Type entityType)
		{
			return EnableExpireTypeConfiguration.TypesToEnableExpirable.Any(x => x.Name == entityType.Name);
		}
	}
}
