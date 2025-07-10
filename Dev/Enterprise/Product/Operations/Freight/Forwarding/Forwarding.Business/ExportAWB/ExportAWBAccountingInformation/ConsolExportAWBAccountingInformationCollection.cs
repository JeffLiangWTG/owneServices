using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBAccountingInformationCollection : ExportAWBAccountingInformationCollection
	{
		public ConsolExportAWBAccountingInformationCollection(ExportAWBHeader master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		public new ConsolExportAWBAccountingInformation this[int index]
		{
			get { return (ConsolExportAWBAccountingInformation)(Elements[index]); }
		}

		public new ConsolExportAWBAccountingInformation AddNew()
		{
			return (ConsolExportAWBAccountingInformation)base.AddNew();
		}
	}
}
