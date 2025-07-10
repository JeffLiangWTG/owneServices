namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportVisitedPortValidation : Customs.Business.CusCodeDataValidation
	{
		public USExportVisitedPortValidation(USExportVisitedPort parent) : base(parent)
		{
		}
		public new USExportVisitedPort Parent => (USExportVisitedPort)base.Parent;

		protected override void CheckCY_Code()
		{
		}
	}
}
