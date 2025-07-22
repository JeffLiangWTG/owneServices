namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly
{
    public class CustomerCodeIP
    {
        public string CustomerIP { get; set; }
        public List<string> CustomerCodeCollection { get; set; } = new List<string>();
        public CustomerCodeIP(string customerIP, string customerCode)
        {
            CustomerIP = customerIP;
            CustomerCodeCollection.Add(customerCode);
        }
    }
}
