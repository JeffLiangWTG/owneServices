using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using static CargoWise.RefDbRepo.FRReferenceData.Services.UniversalDataHelper;

namespace CargoWise.RefDbRepo.FRReferenceData.Services;

public class TAXCodeListAttributeNameGenerator : BaseCodeListAttributeNameGenerator
{
	internal protected override string DataGrouping => "FR";

	internal protected override string CodeType => "TAX";

	protected override (string Name, string Description, string DataType, bool IsMandatory, bool AllowDuplicates, bool IsValueMandatory)[] Attributes => AttributeInfos;

	protected override Dependency[] GetDependencies(DateTime publicationTime)
	{
		var taxCodeTypeGenerator = new TAXCodeTypeGenerator();
		return [new Dependency($"{taxCodeTypeGenerator.DataGrouping} {taxCodeTypeGenerator.Description} Code Type", publicationTime, DependencyType.Required)];
	}

	public static class AttributeNames
	{
		public const string IsNationalIncome = "IsNationalIncome";
		public const string IsDeferredPayment = "IsDeferredPayment";
		public const string IsPortTax = "IsPortTax";
		public const string IsAI2Applicable = "IsAI2Applicable";
		public const string IsBaseVAT = "IsBaseVAT";
		public const string Precalculable = "Precalculable";
		public const string IsGuaranteeConsumed = "IsGuaranteeConsumed";
		public const string Nature = "Nature";
		public const string CalculationType = "CalculationType";
		public const string ApplicationTerritory = "ApplicationTerritory";
		public const string EuropeanCode = "EuropeanCode";
		public const string IsFictional = "IsFictional";
	}

	internal static (string Name, string Description, string DataType, bool IsMandatory, bool AllowDuplicates, bool IsValueMandatory)[] AttributeInfos => [
		(AttributeNames.IsNationalIncome, "Is National Income", Constants.AttributeDataTypes.Boolean, true, false, true),
		(AttributeNames.IsDeferredPayment, "Is Deferred Payment", Constants.AttributeDataTypes.Boolean, true, false, true),
		(AttributeNames.IsPortTax, "Is Port Tax", Constants.AttributeDataTypes.Boolean, true, false, true),
		(AttributeNames.IsAI2Applicable, "Is AI2 Applicable", Constants.AttributeDataTypes.Boolean, true, false, true),
		(AttributeNames.IsBaseVAT, "Is Base VAT", Constants.AttributeDataTypes.Boolean, true, false, true),
		(AttributeNames.Precalculable, "Tax Precalculable Applicability", Constants.AttributeDataTypes.String, true, false, true),
		(AttributeNames.IsGuaranteeConsumed, "Is Guarantee Consumed", Constants.AttributeDataTypes.Boolean, true, false, true),
		(AttributeNames.Nature, "Nature", Constants.AttributeDataTypes.String, true, false, true),
		(AttributeNames.CalculationType, "Calculation Type", Constants.AttributeDataTypes.String, true, false, true),
		(AttributeNames.ApplicationTerritory, "Application Territory", Constants.AttributeDataTypes.String, false, true, true),
		(AttributeNames.EuropeanCode, "European Tax Code", Constants.AttributeDataTypes.String, false, false, true),
		(AttributeNames.IsFictional, "Is Fictional", Constants.AttributeDataTypes.Boolean, true, false, true)
	];
}
