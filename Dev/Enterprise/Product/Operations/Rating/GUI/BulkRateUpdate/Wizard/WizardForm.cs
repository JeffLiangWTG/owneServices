using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Rating.GUI
{
	[TestExcludeZWinFormHasTypedConstructor]
	public partial class WizardForm : ZChildForm
	{
		#region Fields

		readonly List<WizardPage> pagesCollection;
		int currentPageIndex = -1;
		string formText = string.Empty;

		#endregion

		#region Properties

		[Browsable(false)]
		public ZButton BackButton
		{
			get { return backButton; }
		}

		[Browsable(false)]
		public ZButton NextButton
		{
			get { return nextButton; }
		}

		[Browsable(false)]
		public ZButton FinishButton
		{
			get { return finishButton; }
		}

		[Browsable(false)]
		public ZButton CancelWizardButton
		{
			get { return cancelButton; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<WizardPage> Pages
		{
			get { return pagesCollection; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WizardPage CurrentPage
		{
			get { return pagesCollection.Count > 0 && currentPageIndex >= 0 && currentPageIndex < pagesCollection.Count ? pagesCollection[currentPageIndex] : null; }
		}

		[Browsable(false)]
		public int CurrentPageIndex
		{
			get { return currentPageIndex; }
		}

		[Browsable(false)]
		public string PageHeaderTitle
		{
			get { return stepTitleLabel.Text; }
			set { stepTitleLabel.Text = value; }
		}

		[Browsable(false)]
		public string PageHeaderDescription
		{
			get { return stepDescriptionLabel.Text; }
			set { stepDescriptionLabel.Text = value; }
		}

		[Browsable(false)]
		public bool PageHeaderVisible
		{
			get { return pageHeaderPanel.Visible; }
			set { pageHeaderPanel.Visible = value; }
		}

		#endregion

		#region Ctor

		// designer requires parameterless constructor
		WizardForm()
			: this(null)
		{
		}

		public WizardForm(IBusiness businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			backButton.Text = Res.GetString("74766213-2109-491c-8c1b-bbf5385e3024", "< Back");
			nextButton.Text = Res.GetString("f45d41e8-0848-4c30-b78b-e56479b48ec7", "Next >");
			finishButton.Text = Res.GetString("a556d64f-c6e2-4902-b2fb-6503991b64db", "Finish");
			cancelButton.Text = Res.GetString("13d37949-0b3f-4014-88b8-c3c117a3a8ba", "Cancel");

			pagesCollection = new List<WizardPage>();

			backButton.Click += new EventHandler(OnBackClick);
			nextButton.Click += new EventHandler(OnNextClick);
			finishButton.Click += new EventHandler(OnFinishClick);
			cancelButton.Click += new EventHandler(OnCancelClick);
			pageHeaderPanel.Paint += new PaintEventHandler(OnPaintPanelInfo);
		}

		#endregion

		#region Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			formText = this.Text;

			PreLoadPages();

			if (pagesCollection.Count > 0 && ShowPage(0))
			{
				UpdateCaption();
				UpdateButtons();
			}

			UpdateCaption();
		}

		#endregion

		void PreLoadPages()
		{
			foreach (WizardPage page in pagesCollection)
			{
				page.Visible = false;
				page.Dock = DockStyle.Fill;
				pageContainerPanel.Controls.Add(page);
			}
		}

		bool isShowingPage;

		bool ShowPage(int index)
		{
			try
			{
				EnableButtons(false);

				if (index < 0 || index >= Pages.Count && index == currentPageIndex)
				{
					return false;
				}

				WizardPage oldPage = CurrentPage;

				if (oldPage != null)
				{
					WizardSteppingEventArgs cancelArgs = new WizardSteppingEventArgs(index > currentPageIndex ?
						WizardSteppingEventArgs.Direction.Forward : WizardSteppingEventArgs.Direction.Back);

					CurrentPage.NotifyLeaving(cancelArgs);

					if (cancelArgs.Cancel)
					{
						return false;
					}
				}

				WizardPage newPage = pagesCollection[index];
				newPage.Visible = true;
				newPage.NotifyActivated(this);
				currentPageIndex = index;

				if (oldPage != null)
				{
					oldPage.Visible = false;
				}

				return true;
			}
			finally
			{
				EnableButtons(true);
			}
		}

		void EnableButtons(bool enabled)
		{
			isShowingPage = !enabled;
			BackButton.Enabled = currentPageIndex != 0 && enabled;
			NextButton.Enabled = enabled;
			CancelWizardButton.Enabled = enabled;
			FinishButton.Enabled = enabled;
		}

		void WizardForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (isShowingPage)
			{
				e.Cancel = true;
			}
		}

		void ShowPreviousPage()
		{
			if (ShowPage(currentPageIndex - 1))
			{
				UpdateCaption();
				UpdateButtons();
			}
		}

		void ShowNextPage()
		{
			if (ShowPage(currentPageIndex + 1))
			{
				UpdateCaption();
				UpdateButtons();
			}
		}

		void ShowFinishPage()
		{
			if (ShowPage(Pages.Count - 1))
			{
				UpdateCaption();
				UpdateButtons();
			}
		}

		void UpdateCaption()
		{
			this.Text = Res.GetString("bbe05c7f-741b-4fcb-9470-f1111390b1d3", "{0} (Page {1} of {2})", formText, currentPageIndex + 1, Pages.Count);
		}

		void UpdateButtons()
		{
			if (currentPageIndex == 0)
			{
				SetFirstPageButtons();
			}
			else if (currentPageIndex == pagesCollection.Count - 1)
			{
				SetLastPageButtons();
			}
			else
			{
				SetStepPageButtons();
			}
		}

		void SetFirstPageButtons()
		{
			BackButton.Enabled = false;
			BackButton.Visible = true;
			NextButton.Visible = true;
			CancelWizardButton.Visible = true;
			FinishButton.Visible = false;
		}

		void SetStepPageButtons()
		{
			BackButton.Enabled = true;
			NextButton.Visible = currentPageIndex < pagesCollection.Count - 2;
			FinishButton.Visible = currentPageIndex == pagesCollection.Count - 2;
			CancelWizardButton.Visible = true;
		}

		void SetLastPageButtons()
		{
			BackButton.Visible = false;
			NextButton.Visible = false;
			FinishButton.Visible = false;
			CancelWizardButton.Visible = true;
			CancelWizardButton.Text = Res.GetString("d0263115-7cdd-41f1-9e7c-68e8443d3574", "Close");
		}

		#region Event Handlers

		void OnBackClick(object sender, EventArgs e)
		{
			ShowPreviousPage();
		}

		void OnNextClick(object sender, EventArgs e)
		{
			ShowNextPage();
		}

		void OnFinishClick(object sender, EventArgs e)
		{
			ShowFinishPage();
		}

		void OnCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		void OnPaintPanelInfo(object sender, PaintEventArgs e)
		{
			base.OnPaint(e);
			LinearGradientBrush backgroundBrush = new LinearGradientBrush(pageHeaderPanel.Bounds, Color.White, SystemColors.ControlLight, LinearGradientMode.Horizontal);
			e.Graphics.FillRectangle(backgroundBrush, pageHeaderPanel.Bounds);
		}

		#endregion
	}
}

