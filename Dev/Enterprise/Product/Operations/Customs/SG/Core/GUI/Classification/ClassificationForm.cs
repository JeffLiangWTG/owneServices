using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class ClassificationForm : Customs.GUI.BaseClassificationForm
	{
		public ClassificationForm(Classification businessEntity)
			: base(businessEntity)
		{
		}

		protected override SecurityCheckpoint AuditSecurity
		{
			get { return Env.Security.ExportClassificationAudit; }
		}

		protected override Customs.GUI.BaseClassificationUserControl GetUserControl()
		{
			return new ClassificationUserControl();
		}
	}
}
