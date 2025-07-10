using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.MacroValueProviders;

namespace Enterprise.Customs.Business.ValueProviders
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	class DutyRate : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DutyRate({businessobjecttype},{businessobjectpkasguid})>",
				ResString.GetMultilingualString("45fd4052-3bc8-4047-9844-70d80a93a10a",
				@"Will return the Import Duty Rate from the specified business object type. Valid types: {0}, {1}, {2}, {3}. You will need to have the PK for the relevant business object for a value to be returned.",
				"OrgSupplierPart", "CusClassification", "CusClassPartpivot", "CusClassPartpivotTax"),
				new List<(string, object)> { ("<DutyRate(CusClassification, <InvoiceLine.JI_OP>)>", "5.00000") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string boTypeDescription = fRegex.Match(macro).Groups[1].Value;
			string pkAsString = fRegex.Match(macro).Groups[2].Value;
			ZGuid pk = new ZGuid(pkAsString);

			switch (boTypeDescription.ToUpper())
			{
				case "ORGSUPPLIERPART":
					return GetPropertyValue(pk, typeof(OrgSupplierPart), "DutyRateForCurrentCountry", "");

				case "CUSCLASSIFICATION":
					return GetPropertyValue(pk, typeof(BaseCusClassification), "DutyRateForCurrentCountry", "");

				case "CUSCLASSPARTPIVOT":
					return GetPropertyValue(pk, typeof(BaseCusClassPartPivot), "DutyRateForCurrentCountry", "");

				case "CUSCLASSPARTPIVOTTAX":
					return GetPropertyValue(pk, typeof(BaseCusClassPartPivot), "TaxRateForCurrentCountry", "");

				case "":
					return "";

				default:
					throw new MacroEvaluationException("boTypeDescription '" + boTypeDescription + "' is not defined for duty rate.", this);
			}
		}

		object GetPropertyValue(ZGuid pK, Type boType, string propertyName, object defaultValue)
		{
			var bo = Factory.Load(boType, pK);
			var propertyInfo = boType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			return bo != null ? propertyInfo.GetValue(bo, null) : defaultValue;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)dutyrate(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)([^\s]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
