using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageUserControl : ZUserControl
	{
		public CartageUserControl()
		{
			InitializeComponent();
#if DEBUG
			ExcludeFromResourceCheck();
#endif

			SummaryGrid.ColorContextKey = LegGridColourScheme.LegColourKey;
			SummaryGrid.ShareActiveColorScheme = false;

			new UNDGDataItemFormManager(SummaryGrid, "BookedCtgMove").Initialize();

			CartageDetailsGroupBox.AllowOutsideOfParent();
		}

		void ExcludeFromResourceCheck()
		{
			MissingResourceStringChecker.ExcludeFromTest(FirstDocAddressControl);
			MissingResourceStringChecker.ExcludeFromTest(SecondDocAddressControl);
			MissingResourceStringChecker.ExcludeFromTest(ThirdDocAddressControl);

			TypeDescriptor.AddAttributes(FirstDocAddressControl, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(SecondDocAddressControl, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(ThirdDocAddressControl, new SuppressFormsLocalizedTestAttribute());
		}

		CommonCartage Cartage
		{
			get { return (CommonCartage)BindingSource.DataSource; }
		}

		void Hook()
		{
			if (Cartage != null)
			{
				workflowCustomFields = WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(SummaryGrid, Cartage.CartageLegs, true);
				Cartage.JobCreating += new EventHandler(OnJobCreating);
				Cartage.JobDeleting += new EventHandler(OnJobDeleting);
				Cartage.CartageFactorySaved += new EventHandler(OnCartageFactorySaved);
			}
		}

		void Unhook()
		{
			if (workflowCustomFields != null)
			{
				workflowCustomFields.Dispose();
				workflowCustomFields = null;
			}

			if (Cartage != null)
			{
				Cartage.JobCreating -= new EventHandler(OnJobCreating);
				Cartage.JobDeleting -= new EventHandler(OnJobDeleting);
				Cartage.CartageFactorySaved -= new EventHandler(OnCartageFactorySaved);
			}
		}

		IDisposable workflowCustomFields;

		void OnCartageFactorySaved(Object sender, EventArgs e)
		{
			if (Cartage != null)
			{
				ShowOrHideClickSaveToCreateJobMessage(false);
			}
		}

		void OnJobCreating(object sender, EventArgs e)
		{
			HideJobHeaderCreationCoveringLabel();

			if (Cartage != null && Cartage.Job != null)
			{
				// Enforcing databinding to newly created JobHeader; overwise system will return "blank" from the previous cached binding
				LocalClientOrgControl.SetDataBinding(Cartage.Job.JH_OA_LocalChargesAddr_ZAddress, "");
			}
		}

		void OnJobDeleting(object sender, EventArgs e)
		{
			ShowJobHeaderCreationCoveringLabel(Res.GetString("D07376ED-A510-4B09-A556-93989595CD18", "Billing job is being deleted."));
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Unhook();

				if (Job != null)
				{
					Job.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember parameter not supported", nameof(dataMember));
			}

			Unhook();

			if (dataSource != null)
			{
				LoadOrCreateJob((CommonCartage)dataSource);
			}

			base.SetDataBinding(dataSource, "");

			Hook();
		}

		void LoadOrCreateJob(CommonCartage cartage)
		{
			var jobCreationLoadError = "";

			if (ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(cartage))
			{
				var loader = new JobHeader.Loader(cartage);
				JobHeader mutexJob = loader.TryLoadOrCreateWithMutex();
				if (mutexJob == null)
				{
					jobCreationLoadError = loader.GetJobCreationError();
				}
			}
			else
			{
				if (cartage.Job == null || cartage.Job.IsCancelled)
				{
					var jobLoader = new JobHeader.Loader(cartage);
					var hasInactiveJobHeader = jobLoader.HasInactiveJobHeader(GlbCompany.CurrentCompany);
					if (hasInactiveJobHeader)
					{
						jobCreationLoadError = Res.GetString("B98DBA80-6E36-47CF-8FCB-15232689C863", @"The Job Invoicing Record has been created but is currently not active. 
Please click on ‘Billing’ tab or ‘Job Invoicing’ menu to activate the job.");
					}
					else
					{
						jobCreationLoadError = Res.GetString("f5938e07-05e2-40c2-86d3-2530536d3871", "The registry item [Accounting --> Add Job Invoicing Record at Saving/Editing of Operations Job] has been set so that billing jobs will only be created upon entry to the Billing tab.");
					}
				}
			}

			Job = cartage.Job;

			if (Job == null || Job.IsCancelled)
			{
				ShowJobHeaderCreationCoveringLabel(jobCreationLoadError);
				ShowOrHideClickSaveToCreateJobMessage(false);
			}
			else
			{
				Job.Parent = cartage;
				HideJobHeaderCreationCoveringLabel();

				var showClickSaveToCreateJobMessage = cartage.IsInDatabase && !Job.IsInDatabase;
				ShowOrHideClickSaveToCreateJobMessage(showClickSaveToCreateJobMessage);
			}
		}
		JobHeader Job;

		void ShowOrHideClickSaveToCreateJobMessage(bool show)
		{
			ClickSaveToCreateJobLabel.Text = show ? Res.GetString("b0998fce-94ca-4423-83d2-07d6631e4316", "Click Save to create accounting job.") : "";
			ClickSaveToCreateJobLabel.Visible = show;
		}

		void ShowJobHeaderCreationCoveringLabel(string message)
		{
			LocalClientOrgControl.Visible = false;
			JobHeaderMutexErrorLabel.Text = message;
			JobHeaderMutexErrorLabel.Visible = true;
		}

		void HideJobHeaderCreationCoveringLabel()
		{
			JobHeaderMutexErrorLabel.Visible = false;
			JobHeaderMutexErrorLabel.Text = "";
			LocalClientOrgControl.Visible = true;
		}

		public void SetupSailingForAir()
		{
			JJ_JV_NKVesselFindBox.Visible = false;
			JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("CartageUserControl|VoyageTextBox|Flight", "Flight");
			SailingDetailsGroupBox.Text = Res.GetString("CartageUserControl|SailingDetailsGroupBox|FlightDetails", "Flight Details");
			ChangeDateFormatWithoutUpdatingDataSource(JJ_JA_E_DEPDateEdit, ZDateTimePickerFormat.Long);
			ChangeDateFormatWithoutUpdatingDataSource(JJ_JB_E_ARVDateEdit, ZDateTimePickerFormat.Long);
		}

		public void SetupSailingForSea()
		{
			JJ_JV_NKVesselFindBox.Visible = true;
			JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("CartageUserControl|VoyageTextBox|Voyage", "Voyage");
			SailingDetailsGroupBox.Text = Res.GetString("CartageUserControl|SailingDetailsGroupBox|SailingDetails", "Sailing Details");
			JJ_JA_E_DEPDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			JJ_JB_E_ARVDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
		}

		internal void SetupAddressesCaption(CommonCartage cartage)
		{
			cartage.MarkAsHavingAddressesReordered();
			cartage.ResetMainAddressesForBinding();

			SetAddressCaption(FirstDocAddressControl, cartage.FirstDocAddress);
			SetAddressCaption(SecondDocAddressControl, cartage.SecondDocAddress);
			SetAddressCaption(ThirdDocAddressControl, cartage.ThirdDocAddress);
			SetAddressCaption(FourthDocAddressControl, cartage.FourthDocAddress);

			MainJobPanel.Visible = cartage.HasParent;

			if (cartage.CartageParent != null)
			{
				ZString localTransportProviderText = "";
				OrgHeader orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
				ZGuid orgProxyPK = orgProxy != null ? orgProxy.PK : ZGuid.Empty;

				if (cartage.LocalTransportProvider != null && cartage.LocalTransportProvider.PK != orgProxyPK)
				{
					localTransportProviderText = " " + Res.GetString("0965a4e0-fb91-4f41-b183-9b55a102edef", "- Transport Provider Allocated: {0}", cartage.LocalTransportProviderAddress.EffectiveCompanyNameTruncated);
				}
				AddressesLinkedToJobLinkLabel.Text = Res.GetString("CartageUserControl|LinkedToJobNo", "Linked to Job {0}{1}", cartage.CartageParent.UniqueConsignmentID, localTransportProviderText);
			}
		}

		internal void ContainerModeChanged(CommonCartage cartage)
		{
			GrossWeightCalcDropEdit.Visible = cartage.IsContainerised;
		}

		void SetAddressCaption(ZDocAddressControl addressControl, JobDocAddress docAddress)
		{
			addressControl.Visible = docAddress != null;
			addressControl.Text = docAddress != null ? docAddress.AddressCaption : ZString.Empty;
		}

		public void SetupSailingDates(bool isCartageExport)
		{
			ExportDatesPanel.Visible = isCartageExport;
			ImportDatesPanel.Visible = !isCartageExport;
		}

		void SelectSchedulesButton_Click(object sender, EventArgs e)
		{
			if (!Cartage.HasParentOrParentJobThatIsNotAnExternalTransportBooking)
			{
#if DEBUG
				if (!Globals.IsTest)
#endif
				{
					SailingIFindBox helper = new SailingIFindBox(Cartage, (ZForm)ParentForm);
					helper.ShowModuleFromISailingParent();
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("23e3d7c9-8e4b-4dcd-86da-9e9a580eff7b", "Can not change sailing for Port Transport Jobs created from other modules. Please change sailing from relevant module."));
			}
		}

		void AddressesLinkedToJobLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (Cartage.HasParent)
			{
				ZController controller = ZControllerFactory.Create(Cartage.CartageParent.ControllerID);
				controller.ShowEditForm((BusinessObject)Cartage.CartageParent);
			}
		}

		void ChangeDateFormatWithoutUpdatingDataSource(ZDateEdit dateEdit, ZDateTimePickerFormat zDateTimePickerFormat)
		{
			var savedBindingMember = dateEdit.GetBindingMember();
			dateEdit.SetBindingMember(string.Empty);
			dateEdit.DateTimeFormat = zDateTimePickerFormat;
			dateEdit.SetBindingMember(savedBindingMember);
		}
	}
}
