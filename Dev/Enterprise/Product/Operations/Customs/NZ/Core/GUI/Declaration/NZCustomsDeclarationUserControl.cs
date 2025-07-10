using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class NZCustomsDeclarationUserControl : Customs.GUI.BaseCustomsDeclarationUserControl
	{
		public NZCustomsDeclarationUserControl()
		{
			InitializeComponent();
			JE_ApplicationCodeBoundDropEdit.Visible = false;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				declaration.JE_GoodsLocatedAtInfo.ValueChanged -= JE_GoodsLocatedAtInfo_ValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			declaration = JobDeclaration;
			if (declaration != null)
			{
				declaration.JE_GoodsLocatedAtInfo.ValueChanged += JE_GoodsLocatedAtInfo_ValueChanged;
			}

			JE_GoodsLocatedAtInfo_ValueChanged(dataSource, EventArgs.Empty);
		}

		void JE_GoodsLocatedAtInfo_ValueChanged(object sender, EventArgs e)
		{
			SetGoodsLocationDropEditVisible();
		}

		void SetGoodsLocationDropEditVisible()
		{
			var declaration = JobDeclaration;
			GoodsLocationDropEdit.Visible = declaration != null && declaration.JE_GoodsLocatedAtVisible && !declaration.JE_Cal_GoodsLocation_ReadOnly;
		}

		#region OnLoad
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MakeUnwantedControlsInvisible();
			MoveDeliveryNotificationsTabPageAfterOrganizationsTabPage();
			DisplayConsolidatedDeclarationAdviceIfNeeded();

			JobDeclaration.PermitCodes.CountChanged += new CollectionCountChangedEventHandler(PermitCodes_CountChanged);
			JobDeclaration.OtherInfos.CountChanged += new CollectionCountChangedEventHandler(OtherInfos_CountChanged);
			UpdatePermitCodeCountInTitle();
			UpdateOtherInfosCountInTitle();
		}
		#endregion

		#region DisplayConsolidatedDeclarationAdviceIfNeeded

		void DisplayConsolidatedDeclarationAdviceIfNeeded()
		{
			ConsolidatedDeclarationAdviceLabel.Visible = ConsolidatedEntriesEnabled && JobDeclaration != null && ConsolidatedDeclaration.IsConsolidated(JobDeclaration);
		}

		bool ConsolidatedEntriesEnabled => RawDataRegistry.Instance.EnableConsolidatedEntries.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		#endregion

		#region MakeUnwantedControlsInvisible
		void MakeUnwantedControlsInvisible()
		{
			JE_ContainerModeBoundDropDownEdit.Visible = false;
			BondedWarehouseDocAddressControl.Visible = true;
			FolioNumberTextBox.Visible = false;
			JE_ContainerCountCalcEdit.Visible = false;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
		}
		#endregion

		#region JobDeclaration_ControlVisibilityChanged

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			// base.HandleDeclarationControlVisibilityChangedCore();
			// DO NOT CALL BASE. Please check with Ben before implementing this again.
			// There are things in Base that should be in AU, not in base and this 
			// is today's workaround until I can refactor.

			SetControlVisibilityAndCaptionsForTransportDetails();
			ChangeCaptionOfImporterAndSupplier();
			SetControlVisibilityForEntryStyle();
			SetControlVisibilityAndCaptionsForZeroRating();
			SetControlVisibilityForTradeSingleWindow();
			DeliveryNotificationsTabPage?.DeliveryNotificationsUserControl?.HandleDeclarationControlVisibilityChanged();
		}

		protected override bool IsVoyageFlightNoVisible
		{
			get { return JobDeclaration.IsAir || JobDeclaration.IsSea; }
		}
		#endregion

		#region SetControlVisibilityAndCaptionsForTransportDetails
		protected void SetControlVisibilityAndCaptionsForTransportDetails()
		{
			JE_MasterBillForSeaBoundTextBox.Visible = JobDeclaration.IsSea;
			JE_MasterBillForAirBoundTextBox.Visible = JobDeclaration.IsAir;
			if (JE_MasterBillForSeaBoundTextBox.Visible)
			{
				JE_MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
			if (JE_MasterBillForAirBoundTextBox.Visible)
			{
				JE_MasterBillForAirBoundTextBox.GetExtension<ILabelCaptionRenderer>().Refresh();
			}

			JE_ExportDateBoundDateEdit.GetExtension<ILabelCaptionRenderer>().Refresh();
			JE_DateOfArrivalBoundDateEdit.GetExtension<ILabelCaptionRenderer>().Refresh();
			HouseBillParcelPostTextEdit.GetExtension<ILabelCaptionRenderer>().Refresh();
			SetVoyageFlightNoVisible();

			VesselFindBox.Visible = JobDeclaration.IsSea;
		}
		#endregion

		#region SetControlVisibilityForEntryStyle
		void SetControlVisibilityForEntryStyle()
		{
			bool isECIWriteOff = JobDeclaration.IsECIWriteoff;
			bool isExport = JobDeclaration.IsExport;
			bool isPeriodic = JobDeclaration.IsPeriodic;
			bool isCompletion = JobDeclaration.IsCompletion;
			bool isTSWDeclaration = JobDeclaration.IsTSWDeclaration;

			JE_SoldOrConsignedDropEdit.Visible = isExport && !isECIWriteOff && !isTSWDeclaration;
			JE_EntryAuthorisationDateDateEdit.Visible = isPeriodic && !isECIWriteOff;
			PaymentTermsDropEdit.Visible = isTSWDeclaration || !isECIWriteOff;
			GoodsValueCalcFindBox.Visible = isECIWriteOff;
			CustomsWeightCalcDropEdit.Visible = isECIWriteOff;
			CodeInfoTabControl.Visible = !isECIWriteOff;
			CustomsProcessingGroupBox.Visible = !isECIWriteOff;
			ShipmentDetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(450, isECIWriteOff ? 447 : 359, true);
			JE_OriginalEntryNumberTextBox.Visible = isCompletion;
			OriginalEntryTypeDropEdit.Visible = isCompletion;
			TranshipmentRequestTabPage.TabVisible = (JobDeclaration as ITranshipmentRequestParent).IsTranshipmentRequestRelevant;
		}
		#endregion

		void SetControlVisibilityAndCaptionsForZeroRating()
		{
			bool notExport = !JobDeclaration.IsExport;
			JE_IsZeroRatedAllDropEdit.Visible = notExport;
		}

		#region SetControlVisibilityForTradeSingleWindow
		protected void SetControlVisibilityForTradeSingleWindow()
		{
			TransactionNatureDropEdit.Visible = JobDeclaration.JE_TransactionNatureVisible;
			GoodsLocatedDropEdit.Visible = JobDeclaration.JE_GoodsLocatedAtVisible;
			SetGoodsLocationDropEditVisible();
			TSWCombinedStatusTextBox.Visible = JobDeclaration.JE_TSWCombinedStatusVisible;
			NotifyPartyDocAddressControl.Visible = JobDeclaration.NotifyPartyDocumentaryAddressVisible;
			DeliveryDestinationPartyDocAddressControl.Visible = JobDeclaration.DeliveryDestinationPartyDocAddressVisible;
			ProcessingPortDropEdit.Visible = JobDeclaration.ProcessingPortVisible;
		}
		#endregion

		#region ChangeCaptionOfImporterAndSupplier
		protected void ChangeCaptionOfImporterAndSupplier()
		{
			if (JobDeclaration.IsExport && !JobDeclaration.IsECIWriteoff)
			{
				SupplierOrganisationControl.Text = Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.NZCustomsDeclarationUserControl|Supplier", "Supplier");
				ImporterOrganisationControl.Text = Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.NZCustomsDeclarationUserControl|MainImporter", "Main Importer");
			}
			else if (JobDeclaration.IsImport && !JobDeclaration.IsECIWriteoff)
			{
				SupplierOrganisationControl.Text = Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.NZCustomsDeclarationUserControl|MainSupplier", "Main Supplier");
				ImporterOrganisationControl.Text = Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.NZCustomsDeclarationUserControl|Importer", "Importer");
			}
			else
			{
				SupplierOrganisationControl.Text = Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.NZCustomsDeclarationUserControl|Supplier", "Supplier");
				ImporterOrganisationControl.Text = Enterprise.Customs.NZ.GUI.Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.NZCustomsDeclarationUserControl|Importer", "Importer");
			}
		}
		#endregion

		#region JE_ContainerModeBoundDropDownEditVisible
		protected override bool JE_ContainerModeBoundDropDownEditVisible
		{
			get { return false; }
		}
		#endregion

		#region JobDeclaration
		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}
		#endregion

		#region Permit Codes and Other Infos Tab Control Title Updating
		void PermitCodes_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdatePermitCodeCountInTitle();
		}

		void UpdatePermitCodeCountInTitle()
		{
			PermitCodeTabPage.Text = Enterprise.Customs.NZ.GUI.Res.GetString("3d5f9e05-cbb9-435f-976d-039e4edfba0f", "Permits ({0})", JobDeclaration.PermitCodes.Count.ToString());
		}

		void OtherInfos_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateOtherInfosCountInTitle();
		}

		void UpdateOtherInfosCountInTitle()
		{
			OtherInfoTabPage.Text = Enterprise.Customs.NZ.GUI.Res.GetString("7b0ad3df-bd59-49e1-a1dc-815e64d36c7f", "Other Infos ({0})", JobDeclaration.OtherInfos.Count.ToString());
		}
		#endregion

		void VesselAndFlightLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(NZEDIMenu.UrlCustomsFindVesselOrFlight);
		}

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}

		void MoveDeliveryNotificationsTabPageAfterOrganizationsTabPage()
		{
			var organizationsTabPageIndex = RightTabControl.TabPages.IndexOf(OrganisationsTabPage);
			RightTabControl.TabPages.Remove(DeliveryNotificationsTabPage);
			RightTabControl.TabPages.Insert(DeliveryNotificationsTabPage, organizationsTabPageIndex + 1);
		}
	}
}
