using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class SundryChargesValueSourceTest : TestCaseWithFactory
	{
		public void TestType()
		{
			Sundry.D4_SundriesJobType = "BOB";
			AssertEquals("BOB", ValueProviders[Keys.SundryChargesType].GetValue(Generator, ""));
			Sundry.D4_SundriesJobType = "BLT";
			AssertEquals("BLT", ValueProviders[Keys.SundryChargesType].GetValue(Generator, ""));
		}

		public void TestMode()
		{
			Sundry.D4_SundryJobMode = "BOB";
			AssertEquals("BOB", ValueProviders[Keys.SundryChargesMode].GetValue(Generator, ""));
			Sundry.D4_SundryJobMode = "BLT";
			AssertEquals("BLT", ValueProviders[Keys.SundryChargesMode].GetValue(Generator, ""));
		}

		public void TestActivity()
		{
			Sundry.D4_SundryJobActivity = "BOB";
			AssertEquals("BOB", ValueProviders[Keys.SundryChargesActivity].GetValue(Generator, ""));
			Sundry.D4_SundryJobActivity = "BLT";
			AssertEquals("BLT", ValueProviders[Keys.SundryChargesActivity].GetValue(Generator, ""));
		}

		#region Implementation
		NumberGeneratorValueProviderCollection ValueProviders
		{
			get
			{
				if (valueProvider == null)
				{
					valueProvider = new NumberGeneratorValueProviderCollection();
					valueProvider.AddRange(new SundryChargesValueSource(Sundry));
				}

				return valueProvider;
			}
		}

		NumberGeneratorValueProviderCollection valueProvider;
		SundryCharges Sundry
		{
			get
			{
				return sundry ?? (sundry = Factory.New<SundryCharges>());
			}
		}

		SundryCharges sundry;
		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}

				return generator;
			}
		}

		NumberGenerator generator;
		#endregion
	}
}
