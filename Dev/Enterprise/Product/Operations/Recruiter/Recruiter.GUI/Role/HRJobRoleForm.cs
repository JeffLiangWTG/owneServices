using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobRoleForm : ZTemplateForm
	{
		public HRJobRoleForm(HRJobRole jobRole) : base(jobRole)
		{
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
