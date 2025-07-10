namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBSecurityStatusLineCollection : Forwarding.AWB.Business.ExportAWBSecurityStatusLineCollection
	{
		public ExportAWBSecurityStatusLineCollection(ExportAWBHeader master)
			: base(master)
		{
		}

		public new ExportAWBSecurityStatusLine this[int index]
		{
			get { return (ExportAWBSecurityStatusLine)(base[index]); }
		}

		public new ExportAWBSecurityStatusLine AddNew()
		{
			return (ExportAWBSecurityStatusLine)base.AddNew();
		}
	}
}
