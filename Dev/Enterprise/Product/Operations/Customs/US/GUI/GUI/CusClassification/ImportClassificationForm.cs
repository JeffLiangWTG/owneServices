using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportClassificationForm : BaseClassificationForm
	{
		public ImportClassificationForm(CusClassification classification)
			: base(classification)
		{
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return new CusClassificationUserControl();
		}

		public override string FormCaption
		{
			get { return "HTS Lookup Code"; }
		}

		protected override SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.ImportClassificationAudit; }
		}
	}
}
