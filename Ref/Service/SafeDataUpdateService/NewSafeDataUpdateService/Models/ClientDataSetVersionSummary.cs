namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class ClientDataSetVersionSummary
	{
		public string DataSet { get; set; }
		public System.DateTime LastDataChangedTime { get; set; }
		public int NoOfCustomersUpdated { get; set; }
		public int NoOfCustomersFailed { get; set; }
		public int NoOfProductionCustomersFailed { get; set; }
	}
}
