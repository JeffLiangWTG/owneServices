using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class HouseBillColumnProviderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetColumnProviderForAUDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var auDeclaration = Factory.New<Customs.AU.Declaration.Business.JobDeclaration>();
				var trackingDeclaration = new TrackingDeclaration(auDeclaration);

				AssertEquals(typeof(AUCusDecHouseBillColumnProvider), HouseBillColumnProviderTypeDecider.GetColumnProviderForDeclaration(trackingDeclaration).GetType());
			}
		}

		public void TestGetColumnProviderForUSIMPDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var usDeclaration = Factory.New<Customs.US.Business.JobDeclaration>();
				usDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var trackingDeclaration = new TrackingDeclaration(usDeclaration);

				AssertEquals(typeof(USIMPCusDecHouseBillColumnProvider), HouseBillColumnProviderTypeDecider.GetColumnProviderForDeclaration(trackingDeclaration).GetType());
			}
		}

		public void TestGetColumnProviderForUSDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var usDeclaration = Factory.New<Customs.US.Business.JobDeclaration>();
				usDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				var trackingDeclaration = new TrackingDeclaration(usDeclaration);

				AssertEquals(typeof(BaseCusDecHouseBillColumnProvider), HouseBillColumnProviderTypeDecider.GetColumnProviderForDeclaration(trackingDeclaration).GetType());
			}
		}

		public void TestGetColumnProviderForOtherDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var declaration = Factory.New<Customs.Business.BaseJobDeclaration>();
				var trackingDeclaration = new TrackingDeclaration(declaration);

				AssertEquals(typeof(BaseCusDecHouseBillColumnProvider), HouseBillColumnProviderTypeDecider.GetColumnProviderForDeclaration(trackingDeclaration).GetType());
			}
		}
	}
}
