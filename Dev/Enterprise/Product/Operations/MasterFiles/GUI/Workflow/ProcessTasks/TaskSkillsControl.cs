using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.DevTools.ServiceClient.Assess;
using WTG.WiseTechAcademy;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskSkillsControl : ZUserControl, IBindTo
	{
		public TaskSkillsControl()
		{
			InitializeComponent();
		}

		#region BindTo

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string BindTo
		{
			get
			{
				return bindTo;
			}
			set
			{
				bindTo = value;
				JobSkillsGrid.BindTo = value + "." + JobSkillsGrid.BindTo;
			}
		}
		string bindTo;

		#endregion

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			TasksListManager.CurrentChanged += CurrentTaskChanged;
		}

		void CurrentTaskChanged(object sender, EventArgs e)
		{
			OpenLearningCentreButton.Enabled = CurrentTask.SkillsPivots.Count > 0;
			CreateLearningTaskButton.Enabled = CurrentTask.SkillsPivots.Cast<ProcessTaskRequiredSkill>().Any(s => !s.HasCompletedLearningUnit);
		}

		CurrencyManager TasksListManager => (CurrencyManager)BindingContext[DataSource, BindTo];

		public Func<IProcessTask, IProcessTask> CreateSkillLearningTaskFunction { get; set; }

		public void OpenLearningCentre()
		{
			try
			{
				if (!JobSkillsGrid.SelectedElements.Any())
				{
					if (JobSkillsGrid.ListManager.Count > 0)
					{
						JobSkillsGrid.Select(0);
					}
				}

				var selected = (ProcessTaskRequiredSkill)JobSkillsGrid.SelectedElements.FirstOrDefault();
				if (selected != null)
				{
					var url = GetUrl(selected);
					if (string.IsNullOrEmpty(url))
					{
						Globals.Message.Show(Res.GetString("bbacb481-973a-40ba-bc2a-bceb62791eaf", "This learning unit cannot be accessed."));
					}
					else
					{
						SafeStartProcess(url);
					}
				}
			}
			catch (UriFormatException ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(ex.Message);
			}

			void SafeStartProcess(string url)
			{
				try
				{
					WebUrlLauncher.Launch(url);
				}
				catch (Win32Exception ex)
				{
					ErrorReporter.ReportOnce("HelpUrl", "Failed to open web address for: " + url, ex);
					Globals.Message.ShowInformation(Res.GetString("3E110DE5-782F-4207-8D85-6414EAE0D896", "The web address '{0}' could not be opened. Please try copy and pasting it into your web browser instead.", url));
				}
			}
		}

		string GetUrl(ProcessTaskRequiredSkill requiredSkill)
		{
			string url = default;
			if (!requiredSkill.P9S_HS.IsEmpty)
			{
				return null;
			}
			else if (requiredSkill.P9S_Aspect is { IsValid: true } aspectPK)
			{
				var assessServiceClient = ObjectFactory.Get<IAssessServiceClient>();
				var urlResponse = assessServiceClient.GetLearningUnitUrlAsync(aspectPK.ToGuid()).GetAwaiter().GetResult();
				if (urlResponse.IsSuccess)
				{
					url = urlResponse.Content;
				}
			}
			else if (requiredSkill.P9S_WiseTechAcademySubjectCode is { IsEmpty: false } subjectCode)
			{
				url = ObjectFactory.Get<IWiseTechAcademyApiClient>().GetWiseTechAcademyUrl(subjectCode).AbsoluteUri;
			}
			return url;
		}

		void OpenLearningCentreButton_Click(object sender, EventArgs e)
		{
			OpenLearningCentre();
		}

		void JobSkillsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			OpenLearningCentre();
		}

		void CreateLearningTaskButton_Click(object sender, EventArgs e)
		{
			var task = CreateSkillLearningTaskFunction?.Invoke(CurrentTask);
			if (task != null)
			{
				if (((BusinessObject)task).IsInDatabase)
				{
					Globals.Message.ShowInformation(Res.GetString("17FC19A2-7098-419C-89F0-4D07C50AD8EE", "A learning task with description '{0}' already exists.", task.P9_Description));
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("2FE66363-7B85-4E11-9ADB-06B621DC188B", "A learning task with description '{0}' has been created.", task.P9_Description));
				}
			}
		}

		protected ProcessTask CurrentTask => (ProcessTask)BindingContext[DataSource, BindTo]?.GetCurrent();
	}
}
