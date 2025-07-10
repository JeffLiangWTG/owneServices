using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public class WhsRFRegistryInfo
	{
		public string ClientCode { get; set; }
		public string UOMType { get; set; }
		public bool EnableErrorAudio { get; set; }
		public string Language { get; set; }
		public WhsPickGroupInfo PickGroup { get; set; }
		public WhsPickMethodInfo PickMethod { get; set; }
		public WhsAreaInfo PickArea { get; set; }
		public PrinterInfo Printer { get; set; }
		public string EquipmentRegistrationNumber { get; set; }
	}
}
