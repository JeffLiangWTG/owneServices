using System.Windows.Forms;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class MNRSurveyForm : ZTemplateForm
	{
		public MNRSurveyForm(MNRSurvey survey) : base(survey)
		{
			InitializeComponent();
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region GLOW link open in browser

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/MNRSurvey", survey.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", survey.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region MNRSurvey

		MNRSurvey survey
		{
			get { return (MNRSurvey)DataSource; }
		}

		#endregion
	}
}
