using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBSecurityStatusLineCollection : DependentBusinessObjectCollection<ExportAWBSecurityStatusLine, ExportAWBHeader>
	{
		public ExportAWBSecurityStatusLineCollection(ExportAWBHeader master)
			: base(master)
		{
		}
	}
}
