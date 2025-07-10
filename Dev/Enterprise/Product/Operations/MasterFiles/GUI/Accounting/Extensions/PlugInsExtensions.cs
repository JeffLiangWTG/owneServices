namespace Enterprise.MasterFiles.GUI
{
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.PlugIn;

	public static class PlugInsExtensions
	{
		public static void AddJobInvoicing(this PlugIns plugIns, IJobInvoicingSupporter invoicingSupporter)
		{
			plugIns.Add(ControllerIDs.JobInvoicing, invoicingSupporter.JobInvoicingSecurity);
		}
	}
}
