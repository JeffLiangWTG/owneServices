using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	/// <summary>
	/// Summary description for CoLoadWizardForm.
	/// </summary>
	public partial class CoLoadWizardForm : ZChildForm
	{
#if DEBUG
		internal ZButton NextButtonInternal => NextButton;
		internal ZButton BackButtonInternal => BackButton;
#endif

		public CoLoadWizardForm()
		{
			ConstructMe();
		}

		public CoLoadWizardForm(CoLoadWizardShipment businessEntity) : base(businessEntity)
		{
			ConstructMe();
		}

		public override string FormHeading
		{
			get { return Res.GetString("14fc3530-889a-4c44-ac9a-d2e0541a0eb8", "Co-Load Wizard"); }
		}

		void ConstructMe()
		{
			CurrentWizardPageIndex = 0;
		}

		public int CurrentWizardPageIndex
		{
			get
			{
				return fCurrentWizardPageIndex;
			}
			set
			{
				if (value != fCurrentWizardPageIndex && value < WizardPages.Count)
				{
					WizardSteppingEventArgs wizardSteppingEventArgs = new WizardSteppingEventArgs(CurrentWizardPage.WizardPageStep, ((WizardPage)WizardPages[value]).WizardPageStep);
					OnCurrentWizardPageChanging(wizardSteppingEventArgs);
					if (!wizardSteppingEventArgs.Cancel)
					{
						WizardSteppedEventArgs wizardSteppedEventArgs = new WizardSteppedEventArgs(wizardSteppingEventArgs.NextStep);

						CurrentWizardPageStep = wizardSteppedEventArgs.CurrentStep;
						OnCurrentWizardPageChanged();
					}
				}
				else if (value == WizardPages.Count)
				{
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		public CoLoadWizardSteps CurrentWizardPageStep
		{
			get
			{
				if (CurrentWizardPage != null)
				{
					return CurrentWizardPage.WizardPageStep;
				}
				else
				{
					return CoLoadWizardSteps.NotSet;
				}
			}
			set
			{
				for (int i = 0; i < WizardPages.Count; i++)
				{
					WizardPage page = WizardPages[i] as WizardPage;
					if (page.WizardPageStep == value)
					{
						fCurrentWizardPageIndex = i;
						break;
					}
				}
			}
		}

		[DpiState(DpiState.Unscaled)]
		public WizardPage CurrentWizardPage
		{
			get
			{
				return (WizardPage)WizardPages[CurrentWizardPageIndex];
			}
		}

		[DpiState(DpiState.Unscaled)]
		public int LastWizardPage
		{
			get
			{
				return WizardPages.Count - 1;
			}
		}

		[ThreadStatic]
		static Image fCoLoadWizardStartImage;
		public static Image CoLoadWizardStartImage
		{
			get
			{
				if (fCoLoadWizardStartImage == null)
				{
					System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Enterprise.Freight.CFS.GUI.CoLoadWizard.CoLoadWizardImages", typeof(CoLoadWizardForm).Assembly);
					fCoLoadWizardStartImage = ((Image)(resources.GetObject("CoLoadWizStart.Image")));
				}
				return fCoLoadWizardStartImage;
			}
		}

		[ThreadStatic]
		static Image fConsigneeImage;
		public static Image ConsigneeImage
		{
			get
			{
				if (fConsigneeImage == null)
				{
					System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Enterprise.Freight.CFS.GUI.CoLoadWizard.CoLoadWizardImages", typeof(CoLoadWizardForm).Assembly);
					fConsigneeImage = ((Image)(resources.GetObject("ConsigneePicture.Image")));
				}
				return fConsigneeImage;
			}
		}

		[ThreadStatic]
		static Image fConsignorImage;
		public static Image ConsignorImage
		{
			get
			{
				if (fConsignorImage == null)
				{
					System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Enterprise.Freight.CFS.GUI.CoLoadWizard.CoLoadWizardImages", typeof(CoLoadWizardForm).Assembly);
					fConsignorImage = ((Image)(resources.GetObject("ConsignorPicture.Image")));
				}
				return fConsignorImage;
			}
		}

		[ThreadStatic]
		static Image fFinishWizardImage;
		public static Image FinishWizardImage
		{
			get
			{
				if (fFinishWizardImage == null)
				{
					System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Enterprise.Freight.CFS.GUI.CoLoadWizard.CoLoadWizardImages", typeof(CoLoadWizardForm).Assembly);
					fFinishWizardImage = ((Image)(resources.GetObject("FinishWizard.Image")));
				}
				return fFinishWizardImage;
			}
		}

		#region Implementation

		int fCurrentWizardPageIndex;
		ArrayList fWizardPages;

		protected ArrayList WizardPages
		{
			get
			{
				if (fWizardPages == null)
				{
					fWizardPages = new ArrayList();
				}
				return fWizardPages;
			}
		}

		protected void OnCurrentWizardPageChanging(WizardSteppingEventArgs e)
		{
			if (CurrentWizardPage != null)
			{
				((CoLoadWizardShipment)BusinessEntity).OnStepping(e);
			}
		}

		protected void OnCurrentWizardPageChanged()
		{
			BackButton.Enabled = (CurrentWizardPageIndex != 0);
			if (CurrentWizardPageIndex == LastWizardPage)
			{
				NextButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("73E3B777-A8E9-4f96-B8C1-7585A8C7BAC5", "&Finish");
			}
			else
			{
				NextButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("8973B43F-B471-45ec-B3B8-CF2A6DCC4639", "&Next >");
			}
			WizardPage wizardPage = (WizardPage)WizardPages[CurrentWizardPageIndex];

			if (wizardPage != null)
			{
				wizardPage.Visible = true;
				foreach (WizardPage page in WizardPages)
				{
					if (page != wizardPage)
					{
						page.Visible = false;
					}
				}
				if (wizardPage.DefaultFocusedControl != null)
				{
					wizardPage.DefaultFocusedControl.Focus();
				}
			}
		}

		void LoadWizardPages()
		{
			WizardPages.Add(new StartCoLoadWizardPage());
			WizardPages.Add(new CaptureUltimateConsignmentDetails());
			WizardPages.Add(new ConsignorWizardPage());
			WizardPages.Add(new AddConsignorWizardPage());
			WizardPages.Add(new ConsigneeWizardPage());
			WizardPages.Add(new AddConsigneeWizardPage());
			WizardPages.Add(new ConfirmCreateSubHouseWizardPage());
			WizardPages.Add(new AnotherShipmentWizardPage());
			WizardPages.Add(new WizardFinishedWizardPage());
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			LoadWizardPages();
			foreach (WizardPage page in WizardPages)
			{
				page.Visible = false;
				CurrentWizardPagePanel.Controls.Add(page);
				page.BringToFront();
			}
			OnCurrentWizardPageChanged();
		}

		void BackButton_Click(object sender, EventArgs e)
		{
			CurrentWizardPageIndex--;
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			CurrentWizardPageIndex++;
		}

		#endregion
	}
}

