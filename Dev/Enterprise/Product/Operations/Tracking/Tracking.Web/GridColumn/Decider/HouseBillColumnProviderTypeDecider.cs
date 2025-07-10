using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	public static class HouseBillColumnProviderTypeDecider
	{
		public static GridColumnProvider GetColumnProviderForDeclaration(TrackingDeclaration trackingDeclaration)
		{
			if (trackingDeclaration != null)
			{
				var declaration = trackingDeclaration.Declaration;

				if (declaration is Customs.AU.Declaration.Business.JobDeclaration auDeclaration)
				{
					return new AUCusDecHouseBillColumnProvider(auDeclaration);
				}

				if (declaration is Customs.US.Business.JobDeclaration usDeclaration && usDeclaration.IsImport)
				{
					return new USIMPCusDecHouseBillColumnProvider(usDeclaration);
				}
			}

			return new BaseCusDecHouseBillColumnProvider();
		}
	}
}
