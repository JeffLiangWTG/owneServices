using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseSupplementaryCodeProvider))]
	public abstract class BaseSupplementaryCodeProviderTest<TSupplementaryCodeProvider, TSupplementaryCode> : TestCaseWithFactory
		where TSupplementaryCodeProvider : BaseSupplementaryCodeProvider
		where TSupplementaryCode : BaseSupplementaryCode
	{
		public void TestAdditionalSupplementaryCodesStartingOrder()
		{
			AssertEquals("AdditionalSupplementaryCodesStartingOrder", ExpectedAdditionalSupplementaryCodesStartingOrder, supplementaryCodeProvider.CodesStartingOrder);
		}

		public virtual void TestNumberOfAdditionalSupplementaryCodes()
		{
			AssertEquals("NumberOfAdditionalSupplementaryCodes", ExpectedNumberOfAdditionalSupplementaryCodes, supplementaryCodeProvider.NumberOfCodes);
		}

		public void TestGetNewValidation()
		{
			var supplementaryCode = GetSupplementaryCode();
			var validator = supplementaryCodeProvider.GetNewValidation(supplementaryCode);
			AssertNotNull("GetNewValidation should never return null", validator);
			AssertEquals("Validator Type", ExpectedGetNewValidationReturnType, validator.GetType());
		}

		public void TestGetNewLookups()
		{
			var supplementaryCode = GetSupplementaryCode();
			var lookups = supplementaryCodeProvider.GetNewLookups(supplementaryCode);
			AssertNotNull("GetNewLookups should never return null", lookups);
			AssertEquals("Lookup Type", ExpectedGetNewLookupsReturnType, lookups.GetType());
		}

		public void TestGetNewSupplementaryCodePropertyChangedNotifier()
		{
			var supplementaryCode = GetSupplementaryCode();
			AssertNotNull("GetNewSupplementaryCodePropertyChangedNotifier() return value should not be null", supplementaryCodeProvider.GetNewSupplementaryCodePropertyChangedNotifier(supplementaryCode));
		}

		protected virtual ZString CountryCodeForSupplementaryCodeProvider => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		protected abstract ZShort ExpectedAdditionalSupplementaryCodesStartingOrder { get; }
		protected abstract ZShort ExpectedNumberOfAdditionalSupplementaryCodes { get;  }
		protected abstract Type ExpectedGetNewValidationReturnType { get; }
		protected abstract Type ExpectedGetNewLookupsReturnType { get; }

		protected virtual TSupplementaryCodeProvider CreateSupplementaryCodeProvider() =>
			(TSupplementaryCodeProvider)Activator.CreateInstance(typeof(TSupplementaryCodeProvider), CountryCodeForSupplementaryCodeProvider);

		protected virtual TSupplementaryCode GetSupplementaryCode() => Factory.New<TSupplementaryCode>();

		protected override void SetUp()
		{
			base.SetUp();
			supplementaryCodeProvider = CreateSupplementaryCodeProvider();
		}
		TSupplementaryCodeProvider supplementaryCodeProvider;
	}
}
