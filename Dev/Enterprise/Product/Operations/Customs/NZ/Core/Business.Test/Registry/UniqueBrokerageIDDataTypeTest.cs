using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Registry.Testing
{
	[TestedType(typeof(UniqueBrokerageIDDataType))]
	class UniqueBrokerageIDDataTypeTest : StringRegistryDataTypeTest
	{
		public void TestValidateCoreWhenSaveDuplicateNZBrokerageID()
		{
			var factory = new BusinessObjectFactory();
			var nzCompany01 = factory.New<GlbCompany>();
			var nzCompany02 = factory.New<GlbCompany>();
			nzCompany01.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany02.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany01.GC_Code = "AAA";
			nzCompany02.GC_Code = "BBB";
			nzCompany01.GC_IsActive = true;
			nzCompany02.GC_IsActive = true;
			nzCompany01.Branches.AddNew().FillWithValidTestData();
			nzCompany02.Branches.AddNew().FillWithValidTestData();
			var nzBrokerageID = NZCustomsDataRegistry.Instance.NZBrokerageID;
			nzBrokerageID.SetValue(nzCompany01.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID");
			factory.Save();

			AssertExceptionThrown("Should throw exception.", typeof(RegistryValidationException), "Company: Company, has entered the same Brokerage ID.", () =>
			{
				nzBrokerageID.SetValue(nzCompany02.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID");
				factory.Save();
			});
		}

		public void TestValidateCoreWhenSaveDuplicateNZBrokerageIDConcurrently()
		{
			var factory = new BusinessObjectFactory();
			var nzCompany01 = factory.New<GlbCompany>();
			var nzCompany02 = factory.New<GlbCompany>();
			nzCompany01.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany02.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany01.GC_Code = "AAA";
			nzCompany02.GC_Code = "BBB";
			nzCompany01.GC_IsActive = true;
			nzCompany02.GC_IsActive = true;
			nzCompany01.Branches.AddNew().FillWithValidTestData();
			nzCompany02.Branches.AddNew().FillWithValidTestData();
			var nzBrokerageID = NZCustomsDataRegistry.Instance.NZBrokerageID;
			nzBrokerageID.SetValue(nzCompany01.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID01");
			nzBrokerageID.SetValue(nzCompany02.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID02");
			factory.Save();

			AssertExceptionThrown("Should throw exception.", typeof(RegistryValidationException), "Company: Company, has entered the same Brokerage ID.", () =>
			{
				nzBrokerageID.SetValue(nzCompany01.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID");
				nzBrokerageID.SetValue(nzCompany02.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID");
				factory.Save();
			});
		}

		public void TestValidateCoreWhenSaveUniqueNZBrokerageID()
		{
			var factory = new BusinessObjectFactory();
			var nzCompany01 = factory.New<GlbCompany>();
			var nzCompany02 = factory.New<GlbCompany>();
			nzCompany01.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany02.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany01.GC_Code = "AAA";
			nzCompany02.GC_Code = "BBB";
			nzCompany01.GC_IsActive = true;
			nzCompany02.GC_IsActive = true;
			nzCompany01.Branches.AddNew().FillWithValidTestData();
			nzCompany02.Branches.AddNew().FillWithValidTestData();
			var nzBrokerageID = NZCustomsDataRegistry.Instance.NZBrokerageID;
			nzBrokerageID.SetValue(nzCompany01.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID01");
			factory.Save();

			AssertNoExceptionThrown("No exception should be thrown.", () =>
			{
				nzBrokerageID.SetValue(nzCompany02.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTNZBROKERAGEID02");
				factory.Save();
			});
		}

		public void TestBrokerIDDoesNotContainSpaces()
		{
			var factory = new BusinessObjectFactory();
			var nzCompany = factory.New<GlbCompany>();
			nzCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			nzCompany.GC_Code = "AAA";
			nzCompany.GC_IsActive = true;
			nzCompany.Branches.AddNew().FillWithValidTestData();
			var nzBrokerageID = NZCustomsDataRegistry.Instance.NZBrokerageID;
			nzBrokerageID.SetValue(nzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1795967237K");
			factory.Save();

			AssertExceptionThrown("Broker ID with space should throw exception.", typeof(RegistryValidationException), "The Brokerage ID. must only contain alphanumeric values.", () =>
			{
				nzBrokerageID.SetValue(nzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1795967237K ");
				factory.Save();
			});

			AssertExceptionThrown("Broker ID with non alph-numeric character should throw exception.", typeof(RegistryValidationException), "The Brokerage ID. must only contain alphanumeric values.", () =>
			{
				nzBrokerageID.SetValue(nzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1-7583739");
				factory.Save();
			});
		}

		protected override StringRegistryDataType GetNewDataType()
		{
			return new UniqueBrokerageIDDataType(0, 10);
		}
	}
}
