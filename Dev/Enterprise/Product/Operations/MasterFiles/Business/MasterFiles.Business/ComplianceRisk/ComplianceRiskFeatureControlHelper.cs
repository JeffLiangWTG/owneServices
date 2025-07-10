using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public static class ComplianceRiskFeatureControlHelper
	{
		/// <summary>
		/// Returns true if Compliance Risk Feature Control Rule is enabled in the client license. False otherwise.
		/// ℹ️ Note: developers can set the Compliance Risk Feature Control Rule environment variable; for local testing.
		/// </summary>
		public static bool HasIntegrateComplinaceWiseToCustomsDeclarationModuleEnabled()
		{
			return HasFeatureControlEnabled(LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule);
		}

		public static bool HasComplianceWiseCommodityScreeningEnable()
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest && hasComplianceWiseCommodityScreeningEnableForTest != null)
			{
				return hasComplianceWiseCommodityScreeningEnableForTest.Value;
			}
#endif
			return HasFeatureControlEnabled(LicenceFeatureCodeList.Codes.ComplianceWiseCommodityScreening);
		}

#if DEBUG
		[ThreadSafe]
		static bool? hasComplianceWiseCommodityScreeningEnableForTest = true;

		public static IDisposable GetIngoreComplianceWiseCommodityScreeningEnableForTest()
		{
			return new IngoreComplianceWiseCommodityScreeningEnableForTest();
		}

		class IngoreComplianceWiseCommodityScreeningEnableForTest : IDisposable
		{
			public IngoreComplianceWiseCommodityScreeningEnableForTest()
			{
				hasComplianceWiseCommodityScreeningEnableForTest = null;
			}

			public void Dispose()
			{
				hasComplianceWiseCommodityScreeningEnableForTest = true;
			}
		}
#endif

		public static bool HasIntegrateComplinaceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice()
		{
			return HasFeatureControlEnabled(LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationManageRiskStatusOnCommercialInvoice);
		}

		static bool HasFeatureControlEnabled(string featureCode)
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(featureCode);
			return featureData != null
				&& featureData.TryDeserializeParameterAsJson<ComplianceRiskFeatureControlRule>(out var featureControlRule)
				&& featureControlRule.Enabled;
		}
	}

	public sealed class ComplianceRiskFeatureControlRule
	{
		/// <summary>
		/// ℹ️ Note: this is a DTO class which is deserialized from JSON. Backwards compatibility is important!
		/// </summary>
		public bool Enabled { get; set; }
	}
}
