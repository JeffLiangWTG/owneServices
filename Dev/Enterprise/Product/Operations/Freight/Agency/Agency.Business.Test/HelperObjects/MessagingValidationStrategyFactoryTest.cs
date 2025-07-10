using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class MessagingValidationStrategyFactoryTest : TestCaseWithFactory
	{
		public void TestGetStrategies()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				Env.Registry.SetFilterCriteria("Enterprise.Freight.Agency.GUI.MessagingValidationMenuCustomise", xml);

				var strategies = MessagingValidationStrategyFactory.GetStrategies();
				var strategyTypes = new[]
				{
					typeof(PortAuthorityBusinessObjectValidation),
					typeof(NZPortMessageValidationStrategy),
					typeof(DangerousGoodsManifestMessageValidationStrategy)
				};

				AssertEquals(3, strategies.Length);
				AssertContainsExactElementsInAnyOrder(strategyTypes, strategies.Select(x => x.GetType()));
				Assert(strategies.All(x => x.IsEnabled));
			}
		}

		public void TestSaveCustomiseStrategies()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var strategies = MessagingValidationStrategyFactory.GetStrategies();
				try
				{
					strategies[0].IsEnabled = false;

					MessagingValidationStrategyFactory.SaveCustomiseStrategies();

					AssertEquals(xml, Env.Registry.GetFilterCriteria("Enterprise.Freight.Agency.Business.MessagingValidationStrategyFactory"));
				}
				finally
				{
					strategies[0].IsEnabled = true;

					Factory.Save();
				}
			}
		}

		const string xml = @"<Strategies>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.PortAuthorityBusinessObjectValidation</MessageValidationStrategyType>
    <IsEnabled>false</IsEnabled>
  </Strategy>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.EIDOBusinessObjectValidation</MessageValidationStrategyType>
    <IsEnabled>false</IsEnabled>
  </Strategy>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.NZPortMessageValidationStrategy</MessageValidationStrategyType>
    <IsEnabled>true</IsEnabled>
  </Strategy>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.NZReleaseOrderMessageValidationStrategy</MessageValidationStrategyType>
    <IsEnabled>true</IsEnabled>
  </Strategy>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.DangerousGoodsManifestMessageValidationStrategy</MessageValidationStrategyType>
    <IsEnabled>true</IsEnabled>
  </Strategy>
</Strategies>";
	}
}
