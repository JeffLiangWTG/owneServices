using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
    public class eHubRegistrationType
    {
        public eHubRegistrationType()
        {
            RT_PK = Guid.NewGuid();
        }

        public Guid RT_PK { get; set; }
        public string RT_ID { get; set; }
        public string RT_Description { get; set; }
        public string RT_RegistrantType { get; set; }
    }
}
