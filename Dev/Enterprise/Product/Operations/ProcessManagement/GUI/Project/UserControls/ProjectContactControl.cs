using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectContactControl : ZUserControl
	{
		public ProjectContactControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				var colorTheme = SystemDataRegistry.Instance.ColorTheme;
				ContactEmailBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (MainForm != null)
			{
				ContactEmailBox.Click += ClientContactEmailEventHandler;
			}
		}

		ProjectForm MainForm
		{
			get
			{
				if (mainForm == null)
				{
					mainForm = FindForm() as ProjectForm;
				}
				return mainForm;
			}
		}
		ProjectForm mainForm;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Project != null)
			{
				SetContactBoxesAbility();
				Project.WKP_OC_ContactInfo.ValueChanged += new EventHandler(delegate
				{ SetContactBoxesAbility(); });
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Project != null)
			{
				Project.WKP_OC_ContactInfo.ValueChanged -= new EventHandler(delegate
				{ SetContactBoxesAbility(); });
			}
		}

		Project Project
		{
			get { return (Project)CurrentDataItem; }
		}

		void SetContactBoxesAbility()
		{
			ContactEmailBox.Enabled = !Project.ContactEmail.IsEmpty;
		}

		EventHandler ClientContactEmailEventHandler
		{
			get
			{
				return new EventHandler((o, e) => HandleSendEmail());
			}
		}

		void HandleSendEmail()
		{
			var project = MainForm.BusinessEntity as Project;
			if (project != null)
			{
				EmailSender.HandleEventHandler(MainForm, new ProjectContactForEmail(project), null);
			}
		}

#if DEBUG
		public ZArchitecture.ZTextBox GetContactEmailBox()
		{
			return ContactEmailBox;
		}
#endif
	}
}
