using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBSpecialHandlingCollection : DependentBusinessObjectCollection<ExportAWBSpecialHandling, ExportAWBHeader>
	{
		public ExportAWBSpecialHandlingCollection(ExportAWBHeader parent)
			: base(parent)
		{
			MaxCountValidationEnable(9, Res.GetString("9c2ed5d0-b2cf-cdbf-41b6-ac07b899e903", "A maximum of nine Special Handling Codes is possible for the FWB message"));
		}
	}
}
