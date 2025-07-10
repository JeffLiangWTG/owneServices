using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.Foundation.FrameworkExtensions.Functional;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class GlbCompanyHelper
	{
		public static GlbCompany NewCompany(
			this BusinessObjectFactory factory,
			string countryCode = CountryCodes.Australia
		)
		{
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "X" + countryCode;
			company.SetCountry(countryCode);
			return company;
		}

		public static T WithExternalPassword<T>(
			this GlbCompany company,
			string userId = "",
			string passwordStatus = "",
			string statusReason = ""
		) where T : GlbExternalPassword
		{
			var externalPassword = company.Factory.NewWithValidTestData<T>();
			externalPassword.GP_GC = company.PK;
			externalPassword.GP_UserID = userId;
			externalPassword.GP_PasswordStatus = passwordStatus;
			externalPassword.GP_StatusReason = statusReason;
			return externalPassword;
		}

		public static Mock<ICompanyProvider> CreateMockForICompanyProvider(this ICompany company)
		{
			var companyProviderMock = new Mock<ICompanyProvider>();
			companyProviderMock.Setup(x => x.Get(It.IsAny<ZGuid>())).Returns(company.ToOption());
			return companyProviderMock;
		}

		public static ICompany RegisterWithMock(this ICompany company, Mock<ICompanyProvider> mock)
		{
			Argument.NotNull(company, nameof(company));
			mock.Setup(x => x.Get(new ZGuid(company.PK))).Returns(company.ToOption());
			return company;
		}
	}
}
