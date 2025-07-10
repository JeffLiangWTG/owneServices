using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.DataProcessingExplanation
{
	public static class ErrorConstants
	{
		static List<string> jsonConfigFiles = new List<string> { "CargoWise.RefDbRepo.DataProcessingExplanation.config.json" };

		static IConfiguration Config
		{
			get
			{
				if (_config == null)
				{
					var builder = new ConfigurationBuilder();
					foreach (var jsonConfigFile in jsonConfigFiles)
					{
						builder.AddJsonFile(jsonConfigFile);
					}
					_config = builder.Build();
				}
				return _config;
			}
		}
		static IConfiguration _config;

		public static void AddJsonFile(string path)
		{
			jsonConfigFiles.Add(path);
			_config = null;
		}

		public static string ErrorReferenceUrl => Config[nameof(ErrorReferenceUrl)];

		public static string GetErrorNameByCode(string errorCode)
		{
			if (ErrorCodeAndNames.ContainsKey(errorCode))
			{
				return ErrorCodeAndNames[errorCode];
			}
			return string.Empty;
		}

		static Dictionary<string, string> ErrorCodeAndNames = new Dictionary<string, string> {
			{ErrorCodes.CircularDependency, nameof(ErrorCodes.CircularDependency) },
			{ErrorCodes.RootElementNotValid, nameof(ErrorCodes.RootElementNotValid) },
			{ErrorCodes.IncorrectXmlMetaData, nameof(ErrorCodes.IncorrectXmlMetaData) },
			{ErrorCodes.DbEntityValidationError, nameof(ErrorCodes.DbEntityValidationError) },
			{ErrorCodes.IncorrectXmlDataContent, nameof(ErrorCodes.IncorrectXmlDataContent) },

			{ErrorCodes.MultipleOrNoneRelatedEntities, nameof(ErrorCodes.MultipleOrNoneRelatedEntities) },
			{ErrorCodes.MultipleFKsBetweenTablePrefixes, nameof(ErrorCodes.MultipleFKsBetweenTablePrefixes) },
			{ErrorCodes.IncorrectSafeDateRange, nameof(ErrorCodes.IncorrectSafeDateRange) },
			{ErrorCodes.ReturnAllObjectsForOperationOtherThanEqual, nameof(ErrorCodes.ReturnAllObjectsForOperationOtherThanEqual) },
			{ErrorCodes.UnableToConvertToSafeValue, nameof(ErrorCodes.UnableToConvertToSafeValue) },
			{ErrorCodes.MultipleExpirableKeyProperties, nameof(ErrorCodes.MultipleExpirableKeyProperties) },
			{ErrorCodes.IncorrectStagingDateRange, nameof(ErrorCodes.IncorrectStagingDateRange) },
			{ErrorCodes.IncorrectXmlSchema, nameof(ErrorCodes.IncorrectXmlSchema) },
			{ErrorCodes.OverlappingDateRange, nameof(ErrorCodes.OverlappingDateRange) },
			{ErrorCodes.InvalidDatePropertyType, nameof(ErrorCodes.InvalidDatePropertyType) },
			{ErrorCodes.InvalidDataMatch, nameof(ErrorCodes.InvalidDataMatch) },
			{ErrorCodes.NotSupportedNKColumn, nameof(ErrorCodes.NotSupportedNKColumn) },
			{ErrorCodes.NotSupportedKeyProperty, nameof(ErrorCodes.NotSupportedKeyProperty) }
		};
	}

	public static class ErrorCodes
	{
		public const string CircularDependency = "PRS-00001";
		public const string RootElementNotValid = "PRS-00002";
		public const string IncorrectXmlMetaData = "PRS-00003";
		public const string DbEntityValidationError = "PRS-00004";
		public const string IncorrectXmlDataContent = "PRS-00005";

		public const string MultipleOrNoneRelatedEntities = "MER-00001";
		public const string MultipleFKsBetweenTablePrefixes = "MER-00002";
		public const string IncorrectSafeDateRange = "MER-00003";
		public const string ReturnAllObjectsForOperationOtherThanEqual = "MER-00004";
		public const string UnableToConvertToSafeValue = "MER-00005";
		public const string MultipleExpirableKeyProperties = "MER-00006";
		public const string IncorrectStagingDateRange = "MER-00007";
		public const string IncorrectXmlSchema = "MER-00008";
		public const string OverlappingDateRange = "MER-00009";
		public const string InvalidDatePropertyType = "MER-00010";
		public const string InvalidDataMatch = "MER-00011";
		public const string NotSupportedNKColumn = "MER-00012";
		public const string NotSupportedKeyProperty = "MER-00013";
	}
}
