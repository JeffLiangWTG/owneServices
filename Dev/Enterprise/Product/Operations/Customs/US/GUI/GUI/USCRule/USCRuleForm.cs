using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCRuleForm : ZTemplateForm
	{
		public USCRuleForm(USCRule rule)
			: base(rule)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			AddUSCRuleUserControl();
		}

		void AddUSCRuleUserControl()
		{
			USCRuleUserControl control = new USCRuleUserControl();
			control.Dock = DockStyle.Fill;
			MainTabControl.TabPages[0].Controls.Add(control);
		}

		public override string FormCaption
		{
			get { return "Rule"; }
		}

		internal ZTabControl MainTabControlExposedForTesting => MainTabControl;
	}
}
