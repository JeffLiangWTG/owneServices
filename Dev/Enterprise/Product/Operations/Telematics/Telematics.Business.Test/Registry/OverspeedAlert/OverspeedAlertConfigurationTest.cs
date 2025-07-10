using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test.Registry
{
	[TestedType(typeof(OverspeedAlertConfiguration))]
	class OverspeedAlertConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		FallbackLevel CurrentFallbackLevel
		{
			get
			{
				if (fallbackLevel == null)
				{
					fallbackLevel = NewFallbackLevel();
				}
				return fallbackLevel;
			}
		}
		FallbackLevel fallbackLevel;

		public void TestIsOverspeedAlertTypeShownForX()
		{
			var config = new OverspeedAlertConfiguration(CurrentFallbackLevel, Factory);
			Assert("Shoud be set to default shown for X", config.IsOverspeedAlertTypeShownForX);
			config.OverspeedAlertType = OverspeedAlertOptions.Never.Code;
			Assert("Should not be set to default shown for x", !config.IsOverspeedAlertTypeShownForX);
			config.OverspeedAlertType = OverspeedAlertOptions.Always.Code;
			Assert("Should not be set to default shown for x", !config.IsOverspeedAlertTypeShownForX);
		}

		public void TestDurationInMinutesDefaultAs60()
		{
			var config = new OverspeedAlertConfiguration(CurrentFallbackLevel, Factory);
			AssertEquals("Should get default duration in minutes as 60", new ZInt(60), config.DurationInMinutes);
		}

		public void TestDurationInMinutesAsNonZero()
		{
			var config = new OverspeedAlertConfiguration(CurrentFallbackLevel, Factory);
			config.DurationInMinutes = -4;
			AssertHasError("Should display error", config.DurationInMinutesInfo, "Enter a numeric value greater than or equal to zero for Duration.");
		}

		public void TestOverspeedAlertTypes()
		{
			var config = new OverspeedAlertConfiguration(CurrentFallbackLevel, Factory);
			AssertEquals("Should return correct default alert type", OverspeedAlertOptions.Default.Code, config.OverspeedAlertType);
			AssertEquals("Should return all alert types", OverspeedAlertOptions.CodeList, config.OverspeedAlertTypes);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var fallbackLevel = BizObj?.CurrentFallbackLevel ?? NewFallbackLevel();
			return new OverspeedAlertConfiguration(fallbackLevel, Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return (OverspeedAlertConfiguration)GetNewBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return (OverspeedAlertConfiguration)GetNewBusinessObject();
		}

		protected new OverspeedAlertConfiguration BizObj
		{
			get { return (OverspeedAlertConfiguration)base.BizObj; }
		}

		#endregion
	}
}
