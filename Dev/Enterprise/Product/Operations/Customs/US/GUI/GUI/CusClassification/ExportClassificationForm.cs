using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportClassificationForm : BaseClassificationForm
	{
		public ExportClassificationForm(CusClassification classification)
			: base(classification)
		{
			this.classification = classification;
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return new ExportClassificationUserControl();
		}

		public override string FormCaption
		{
			get { return "Schedule B Lookup Code"; }
		}

		protected override SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.ExportClassificationAudit; }
		}
	}
}
