namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public abstract class CommonExportAWBHeaderValidation : AutoExportAWBHeaderValidation
	{
		protected CommonExportAWBHeaderValidation(AutoExportAWBHeader parent)
			: base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateEH_SecurityStatus();
		}

		public void ValidateEH_SecurityStatus()
		{
			ValidateCalculatedProperty(Parent.EH_SecurityStatusInfo);
		}

		protected virtual void CheckEH_SecurityStatus()
		{
		}
	}
}
