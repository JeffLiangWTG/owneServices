using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignRefCountryStates))]
	public class CampaignRefCountryStatesTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bioz = Factory.New<CampaignRefCountryStates>();
			bioz.RW_RN_NKCountryCode = "AU";
			return bioz;
		}
	}
}
