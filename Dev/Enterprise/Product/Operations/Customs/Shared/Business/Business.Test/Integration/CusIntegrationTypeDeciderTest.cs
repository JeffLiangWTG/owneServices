using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusIntegrationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestCusIntegration_CustomsWare_SubmitOutOfLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(IntegratedCountryHelper.CustomsWareCountryCodes.First()))
			{
				var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
				AssertEquals("Enterprise.Customs.CustomsWare.Business.CustomsWareIntegrationOutOfLine", cusIntegration.GetType().FullName);
			}
		}

		public void TestCusIntegration_CustomsWare_SubmitInLine()
		{
			var mock = new Mock<Integration.Customs.CustomsWare.ICustomsWareRegistry>();
			mock.Setup(m => m.SubmitOutOfLine).Returns(false);
			var registryItem = new Mock<IRegistryItem>().Object;
			mock.Setup(m => m.CustomsWareSiteID).Returns(registryItem);
			mock.Setup(m => m.Password).Returns(registryItem);
			mock.Setup(m => m.UserName).Returns(registryItem);
			using (ObjectFactory.Substitute("CustomsWare.ICustomsWareRegistry", mock.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(IntegratedCountryHelper.CustomsWareCountryCodes.First()))
			{
				var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
				AssertEquals("Enterprise.Customs.CustomsWare.Business.CustomsWareIntegration", cusIntegration.GetType().FullName);
			}
		}

		public void TestCusIntegration()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("CH", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNotNull("IE", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Netherlands))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("NL", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedArabEmirates))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("AE", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNotNull("BE", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("DE", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("CN", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
				{
					using (SetLocalCountryCustomsInterfaceRegistry(GlbCompany.CurrentCompany))
					{
						var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
						AssertEquals("ZA Type", typeof(LocalCountryCustomsInterfaceIntegrationProvider), cusIntegration.GetType());
					}
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("US", cusIntegration);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
					AssertNull("AU", cusIntegration);
				}
				foreach (var countryCode in IntegratedCountryHelper.CustomsWareCountryCodes)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
						AssertNotNull($"CustomsWareCountry->{countryCode}", cusIntegration);
					}
				}
				foreach (var countryCode in IntegratedCountryHelper.HasBuiltInDeclarationCountryCodes.Except(IntegratedCountryHelper.CustomsWareCountryCodes))
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						var cusIntegration = CusIntegrationTypeDecider.CusIntegration(Declaration);
						AssertNull($"HasBuiltInDeclarationCountry->{countryCode}", cusIntegration);
					}
				}
			});
		}

		IDisposable SetLocalCountryCustomsInterfaceRegistry(GlbCompany company)
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.CurrentFallbackLevel = new FallbackLevel(company, null, null);
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			return CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
				}

				return declaration;
			}
		}

		BaseJobDeclaration declaration;
	}
}
