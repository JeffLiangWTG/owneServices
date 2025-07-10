using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskSkillsForm : ZChildForm
	{
		public TaskSkillsForm(IProcessTask task) : base((BusinessObject)task)
		{
			InitializeComponent();

			this.HeadingLabel.Text = Res.GetString("C9EAEE90-CABF-4F28-A6EC-B9E5D0480BAD", "You need to complete the following competency requirements before you can work on this task.");
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public Func<IProcessTask, IProcessTask> CreateSkillLearningTaskFunction
		{
			get => SkillsControl.CreateSkillLearningTaskFunction;
			set => SkillsControl.CreateSkillLearningTaskFunction = value;
		}
	}
}
