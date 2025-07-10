using System.Collections;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataProcessingExplanation.Test;

[TestFixture]
class RefDataProcessingExceptionFixture
{
	[Test]
	public void Message()
	{
		var errorMessage = "Test exception.";
		var errorCode = ErrorCodes.IncorrectStagingDateRange;
		var refEx = new RefDataProcessingException(errorMessage, errorCode);
		var expectedMessage = $@"{errorMessage}
ErrorCode: {errorCode}, please check more details on {ErrorConstants.ErrorReferenceUrl}?anchor={errorCode.ToLower(CultureInfo.CurrentCulture)}%3A-{ErrorConstants.GetErrorNameByCode(errorCode).ToLower(CultureInfo.CurrentCulture)}";
		Assert.AreEqual(expectedMessage, refEx.Message);
	}

	[TestCaseSource(nameof(GetNonPersistentTypes))]
	public void Message_Transform_Types(string transformed, string original)
	{
		TestContext.WriteLine($"Message_Transform_Types: {transformed} -> {original}");
		var type = typeof(RefCusTariff).GetTypeFromBaseType(transformed);
		var errorMessage = $"Test exception: Error Type is {transformed}";
		var transformedMessage = $"Test exception: Error Type is {original}";
		var errorCode = ErrorCodes.IncorrectStagingDateRange;
		var refEx = new RefDataProcessingException(errorMessage, errorCode, [type]);
		var expectedMessage = $@"{transformedMessage}
ErrorCode: {errorCode}, please check more details on {ErrorConstants.ErrorReferenceUrl}?anchor={errorCode.ToLower(CultureInfo.CurrentCulture)}%3A-{ErrorConstants.GetErrorNameByCode(errorCode).ToLower(CultureInfo.CurrentCulture)}";
		Assert.AreEqual(expectedMessage, refEx.Message);
	}

	[TestCaseSource(nameof(GetNonPersistentProperties))]
	public void Message_Transform_Properties(string transformed, string original)
	{
		TestContext.WriteLine($"Message_Transform_Properties: {transformed} -> {original}");
		var errorMessage = $"Test exception: Error Property is {transformed}";
		var transformedMessage = $"Test exception: Error Property is {original}";
		var errorCode = ErrorCodes.IncorrectStagingDateRange;
		var refEx = new RefDataProcessingException(errorMessage, errorCode, [transformed]);
		var expectedMessage = $@"{transformedMessage}
ErrorCode: {errorCode}, please check more details on {ErrorConstants.ErrorReferenceUrl}?anchor={errorCode.ToLower(CultureInfo.CurrentCulture)}%3A-{ErrorConstants.GetErrorNameByCode(errorCode).ToLower(CultureInfo.CurrentCulture)}";
		Assert.AreEqual(expectedMessage, refEx.Message);
	}

	[TestCaseSource(nameof(GetNonPersistentTablePrefix))]
	public void Message_Transform_TablePrefix(string transformed, string original)
	{
		TestContext.WriteLine($"Message_Transform_TablePrefix: {transformed} -> {original}");
		var errorMessage = $"Test exception: Error TablePrefix is {transformed}";
		var transformedMessage = $"Test exception: Error TablePrefix is {original}";
		var errorCode = ErrorCodes.IncorrectStagingDateRange;
		var refEx = new RefDataProcessingException(errorMessage, errorCode, transformed);
		var expectedMessage = $@"{transformedMessage}
ErrorCode: {errorCode}, please check more details on {ErrorConstants.ErrorReferenceUrl}?anchor={errorCode.ToLower(CultureInfo.CurrentCulture)}%3A-{ErrorConstants.GetErrorNameByCode(errorCode).ToLower(CultureInfo.CurrentCulture)}";
		Assert.AreEqual(expectedMessage, refEx.Message);
	}

	static IEnumerable GetNonPersistentTypes()
	{
		var mappers = SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup()
			.Union(SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup()).DistinctBy(x => x.GetType());
		foreach (var mapper in mappers)
		{
			if (mapper.NameMapper.transformedName == nameof(RefCusRateApplicability)
			|| mapper.NameMapper.transformedName == nameof(RefCusConditionApplicability))
			{
				yield return new TestCaseData(mapper.NameMapper.transformedName, nameof(RefCusApplicability))
				.SetName($"Type Transform {mapper.NameMapper.transformedName} -> {mapper.NameMapper.originalName}, Take: {nameof(RefCusApplicability)}");
			}
			else
			{

				yield return new TestCaseData(mapper.NameMapper.transformedName, mapper.NameMapper.originalName)
				.SetName($"Type Transform {mapper.NameMapper.transformedName} -> {mapper.NameMapper.originalName}");
			}
		}

		yield return new TestCaseData(SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper.transformedName,
			SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper.originalName)
		.SetName($"Type Transform {SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper.transformedName} -> {SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper.originalName}");

		yield return new TestCaseData(SchemaMapperHelper.RateWithoutApplicabilityNameMapper.transformedName,
			SchemaMapperHelper.RateWithoutApplicabilityNameMapper.originalName)
		.SetName($"Type Transform {SchemaMapperHelper.RateWithoutApplicabilityNameMapper.transformedName} -> {SchemaMapperHelper.RateWithoutApplicabilityNameMapper.originalName}");
	}

	static IEnumerable GetNonPersistentProperties()
	{
		var mappers = SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup()
			.Union(SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup()).DistinctBy(x => x.GetType());
		foreach (var mapper in mappers)
		{
			foreach (var propertyMapper in mapper.PropertyMappers)
			{
				var result = propertyMapper.transformedProperty switch
				{
					nameof(RefCusRateApplicability.S01_StartDate) => nameof(RefCusApplicability.ZZT_StartDate),
					nameof(RefCusConditionApplicability.S07_StartDate) => nameof(RefCusApplicability.ZZT_StartDate),
					nameof(RefCusRateApplicability.S01_EndDate) => nameof(RefCusApplicability.ZZT_EndDate),
					nameof(RefCusConditionApplicability.S07_EndDate) => nameof(RefCusApplicability.ZZT_EndDate),
					_ => propertyMapper.originalProperty
				};

				yield return new TestCaseData(propertyMapper.transformedProperty, result)
				.SetName($"Property Transform {mapper.NameMapper.transformedName} -> {mapper.NameMapper.originalName} : {propertyMapper.transformedProperty} -> {result}");

			}
		}
	}

	static IEnumerable GetNonPersistentTablePrefix()
	{
		var mappers = SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup()
			.Union(SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup()).DistinctBy(x => x.GetType());
		foreach (var mapper in mappers)
		{
			if (mapper.NameMapper.transformedName == nameof(RefCusRateApplicability)
				|| mapper.NameMapper.transformedName == nameof(RefCusConditionApplicability))
			{
				yield return new TestCaseData(mapper.TablePrefixMapper.transformedTablePrefix, "ZZT")
					.SetName($"TablePrefix Transform {mapper.NameMapper.transformedName} -> {mapper.NameMapper.originalName} : {mapper.TablePrefixMapper.transformedTablePrefix} -> ZZT");
			}
			else
			{

				yield return new TestCaseData(mapper.TablePrefixMapper.transformedTablePrefix, mapper.TablePrefixMapper.originalTablePrefix)
					.SetName($"TablePrefix Transform {mapper.NameMapper.transformedName} -> {mapper.NameMapper.originalName} : {mapper.TablePrefixMapper.transformedTablePrefix} -> {mapper.TablePrefixMapper.transformedTablePrefix}");
			}
		}

	}
}
