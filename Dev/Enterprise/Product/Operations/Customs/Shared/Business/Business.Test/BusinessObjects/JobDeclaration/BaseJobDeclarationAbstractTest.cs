using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseJobDeclaration))]
	public abstract class BaseJobDeclarationAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsReciprocalRatesConsistantWithDbFunction()
		{
			foreach (var countryCode in CountryCodesForIsReciprocalRatesTest)
			{
				AssertIsReciprocalRatesConsistantWithDbFunction(countryCode);
			}
		}

		public void TestAddInfoChildImplementIAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter()
		{
			var declaration = (BaseJobDeclaration)GetNewBusinessObject();

			if (((IAddInfoChildSupporter)declaration).AddInfoChild is BusinessObject addInfoChild)
			{
				var supporter = addInfoChild as IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter;
				if (supporter == null)
				{
					Fail($"{addInfoChild.GetType().FullName} should implement {typeof(IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter).FullName}");
				}
			}
			Assert("All is good", true);
		}

		protected virtual List<string> CountryCodesForIsReciprocalRatesTest => new List<string> { GlbCompany.CurrentCompany.GC_RN_NKCountryCode };

		protected override Type ExpectedMetadataType => GetExpectedBusinessObjectType();

		void AssertIsReciprocalRatesConsistantWithDbFunction(string countryCode)
		{
			string sql = @"SELECT IsReciprocal FROM csfn_IsReciprocalInline(@GC_PK)";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals("IsReciprocalRates for country:" + countryCode, declaration.IsReciprocalRates, IsReciprocalDb());

				if (ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
				{
					var zzRefCusConfiguration = ZZRefCusConfiguration.Get(declaration.Company) ?? ZZRefCusConfiguration.New(declaration.Company);
					zzRefCusConfiguration.ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.Yes;
					Factory.Save();
					AssertEquals("ZZC_IsReciprocalExchangeRate is 'Y', IsReciprocalRates for country:" + countryCode, declaration.IsReciprocalRates, IsReciprocalDb());

					zzRefCusConfiguration.ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.No;
					Factory.Save();
					AssertEquals("ZZC_IsReciprocalExchangeRate is 'N', IsReciprocalRates for country:" + countryCode, declaration.IsReciprocalRates, IsReciprocalDb());
				}

				bool IsReciprocalDb()
				{
					return (int)cmd.ExecuteScalar() == 1;
				}
			}
		}
	}
}
