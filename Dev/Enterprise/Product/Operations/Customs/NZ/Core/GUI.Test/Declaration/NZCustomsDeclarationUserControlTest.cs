using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	[TestedType(typeof(NZCustomsDeclarationUserControl))]
	public class NZCustomsDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<NZCustomsDeclarationUserControl, JobDeclaration>
	{
		public void TestJE_ContainerModeLabelVisibility()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				AssertEquals("JE_ContainerModeBoundDropDownEditVisible", false, userControl.JE_ContainerModeBoundDropDownEditVisibleForTesting);
			});
		}

		public void TestTranshipmentRequestTab()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("ITR Request tab page is visible always for TSW Export Write-off", userControl.TranshipmentRequestTabPage.TabVisible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				Assert("Not visible if not TSW", !userControl.TranshipmentRequestTabPage.TabVisible);
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("Not visible if not ECI", !userControl.TranshipmentRequestTabPage.TabVisible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("ITR is visible for import write-off (ICR)", userControl.TranshipmentRequestTabPage.TabVisible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				Assert("Not visible for legacy import write-off", !userControl.TranshipmentRequestTabPage.TabVisible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("Visible when all conditions met", userControl.TranshipmentRequestTabPage.TabVisible);
			});
		}

		public void TestRightTabOnTheRightTabcontrolIsSelectedByDefault()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				AssertEquals(userControl.OrganisationsTabPage, userControl.RightTabControl.SelectedTab);
			});
		}

		public void TestControlLabelsChangeProperly()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				AssertEquals("House Bill", userControl.HouseBillParcelPostTextEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Master Bill", userControl.JE_MasterBillForAirBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Flight Number", userControl.JE_VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				AssertEquals("House Bill", userControl.HouseBillParcelPostTextEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Ocean Bill", userControl.JE_MasterBillForSeaBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Voyage", userControl.JE_VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
				AssertEquals("Parcel No", userControl.HouseBillParcelPostTextEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				Assert("Master Bill not visible", !userControl.JE_MasterBillForSeaBoundTextBox.Visible);
				Assert("Voyage not visible", !userControl.JE_VoyageFlightNoBoundTextBox.Visible);
			});
		}

		public void TestControlVisibility()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals("Incoterm Explanation Button - now visible in NZ / WI00045362", true, userControl.IncoTermExplainButton.Visible);
				AssertEquals("JE_ContainerModeBoundDropDownEdit", false, userControl.JE_ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("BondedWarehouseGroupBox", true, userControl.BondedWarehouseDocAddressControl.Visible);
				AssertEquals("FolioNumberTextBox", false, userControl.FolioNumberTextBox.Visible);
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", true, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("ConsolidatedDeclarationAdviceLabel", false, userControl.ConsolidatedDeclarationAdviceLabel.Visible);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Owner reference label", true, userControl.OwnersReferenceTextBox.Visible);
				AssertEquals("JE_ContainerCountCalcEdit", false, userControl.JE_ContainerCountCalcEdit.Visible);
				AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit", false, userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("TotalNoOfPacksCalcDropEdit", true, userControl.TotalNoOfPacksCalcDropEdit.Visible);
				AssertEquals("BondedWarehouseGroupBox", true, userControl.BondedWarehouseDocAddressControl.Visible);

				declaration.JE_MessageType = "";
				AssertEquals("SupplierOrganisationControl", "Supplier", userControl.SupplierOrganisationControl.Text);
				AssertEquals("ImporterOrganisationControl", "Importer", userControl.ImporterOrganisationControl.Text);
				AssertEquals("BondedWarehouseGroupBox", true, userControl.BondedWarehouseDocAddressControl.Visible);
			});
		}

		public void TestControlVisibilityForImport()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				var goodsLocatedDropEdit = userControl.Controls.Find("GoodsLocatedDropEdit", true).Single() as ZDropEdit;
				AssertEquals("GoodsLocationGroupBox.Visible", true, goodsLocatedDropEdit.Visible);
				AssertEquals("GoodsLocationDropEdit.Visible", true, goodsLocatedDropEdit.Visible);
				AssertEquals("ApplicationCodeDropEdit.Visible", true, userControl.ApplicationCodeDropEdit.Visible);
				AssertEquals("NotifyPartyDocAddressControl.Visible", false, userControl.NotifyPartyDocAddressControl.Visible);
				AssertEquals("DeliveryDestinationPartyDocAddressControl.Visible", true, userControl.DeliveryDestinationPartyDocAddressControl.Visible);
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("ProcessingPortDropEdit.Visible", false, userControl.ProcessingPortDropEdit.Visible);
			});
		}

		public void TestControlVisibilityForExport()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible - not for TSW entry", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("SupplierOrganisationControl", "Supplier", userControl.SupplierOrganisationControl.Text);
				AssertEquals("ImporterOrganisationControl", "Main Importer", userControl.ImporterOrganisationControl.Text);
				AssertEquals("JE_IsZeroRatedAllDropEdit.Visible", false, userControl.JE_IsZeroRatedAllDropEdit.Visible);
				var goodsLocatedDropEdit = userControl.Controls.Find("GoodsLocatedDropEdit", true).Single() as ZDropEdit;
				var goodsLocationDropEdit = userControl.Controls.Find("GoodsLocationDropEdit", true).Single() as ZDropEdit;
				AssertEquals("GoodsLocatedDropEdit.Visible", true, goodsLocatedDropEdit.Visible);
				AssertEquals("GoodsLocationDropEdit.Visible", true, goodsLocationDropEdit.Visible);
				AssertEquals("ApplicationCodeDropEdit.Visible", true, userControl.ApplicationCodeDropEdit.Visible);
				AssertEquals("ProcessingPortDropEdit.Visible", false, userControl.ProcessingPortDropEdit.Visible);
				AssertEquals("NotifyPartyDocAddressControl.Visible", false, userControl.NotifyPartyDocAddressControl.Visible);
				AssertEquals("DeliveryDestinationPartyDocAddressControl.Visible", true, userControl.DeliveryDestinationPartyDocAddressControl.Visible);
			});
		}

		public void TestControlVisibilityForECIWriteOff()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("FolioNumberTextBox.Visible", false, userControl.FolioNumberTextBox.Visible);
				AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit.Visible", false, userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("JE_ContainerModeBoundDropDownEdit.Visible", false, userControl.JE_ContainerModeBoundDropDownEdit.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("FolioNumberTextBox.Visible", false, userControl.FolioNumberTextBox.Visible);
				AssertEquals("JE_TotalNoOfPiecesBoundCalcEdit.Visible", false, userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("JE_ContainerModeBoundDropDownEdit.Visible", false, userControl.JE_ContainerModeBoundDropDownEdit.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("NotifyPartyDocAddressControl.Visible", true, userControl.NotifyPartyDocAddressControl.Visible);
				AssertEquals("DeliveryDestinationPartyDocAddressControl.Visible", true, userControl.DeliveryDestinationPartyDocAddressControl.Visible);
			});
		}

		public void TestControlVisibilityForTSWImport()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				AssertEquals("ApplicationCodeDropEdit.Visible", true, userControl.ApplicationCodeDropEdit.Visible);
				AssertEquals("TransactionNatureDropEdit.Visible", true, userControl.TransactionNatureDropEdit.Visible);
				var goodsLocatedDropEdit = userControl.Controls.Find("GoodsLocatedDropEdit", true).Single() as ZDropEdit;
				var goodsLocationDropEdit = userControl.Controls.Find("GoodsLocationDropEdit", true).Single() as ZDropEdit;
				AssertEquals("GoodsLocatedDropEdit.Visible", true, goodsLocatedDropEdit.Visible);
				AssertEquals("GoodsLocationDropEdit.Visible", true, goodsLocationDropEdit.Visible);
				AssertEquals("TSWCombinedStatusTextBox.Visible", true, userControl.TSWCombinedStatusTextBox.Visible);
				AssertEquals("NotifyPartyDocAddressControl.Visible", false, userControl.NotifyPartyDocAddressControl.Visible);
				AssertEquals("DeliveryDestinationPartyDocAddressControl.Visible", true, userControl.DeliveryDestinationPartyDocAddressControl.Visible);
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("ProcessingPortDropEdit.Visible", false, userControl.ProcessingPortDropEdit.Visible);
			});
		}

		public void TestControlVisibilityTSWExport()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("ApplicationCodeDropEdit.Visible", true, userControl.ApplicationCodeDropEdit.Visible);
				AssertEquals("ProcessingPortDropEdit.Visible", false, userControl.ProcessingPortDropEdit.Visible);
			});
		}

		public void TestControlVisibilityTSWImport()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("ApplicationCodeDropEdit.Visible", true, userControl.ApplicationCodeDropEdit.Visible);
				AssertEquals("ProcessingPortDropEdit.Visible", false, userControl.ProcessingPortDropEdit.Visible);
			});
		}

		public void TestControlVisibilitySwitchingBetweenECIAndFormalEntry()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("JE_EntryAuthorisationDateDateEdit.Visible", false, userControl.JE_EntryAuthorisationDateDateEdit.Visible);
				AssertEquals("PaymentTermsDropEdit.Visible", true, userControl.PaymentTermsDropEdit.Visible);
				AssertEquals("GoodsValueCalcFindBox.Visible", false, userControl.GoodsValueCalcFindBox.Visible);
				AssertEquals("CustomsWeightCalcDropEdit.Visible", false, userControl.CustomsWeightCalcDropEdit.Visible);
				AssertEquals("CodeInfoTabControl.Visible", true, userControl.CodeInfoTabControl.Visible);
				AssertEquals("CustomsProcessingGroupBox.Visible", true, userControl.CustomsProcessingGroupBox.Visible);
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("JE_EntryAuthorisationDateDateEdit.Visible", true, userControl.JE_EntryAuthorisationDateDateEdit.Visible);
				AssertEquals("PaymentTermsDropEdit.Visible", true, userControl.PaymentTermsDropEdit.Visible);
				AssertEquals("GoodsValueCalcFindBox.Visible", false, userControl.GoodsValueCalcFindBox.Visible);
				AssertEquals("CustomsWeightCalcDropEdit.Visible", false, userControl.CustomsWeightCalcDropEdit.Visible);
				AssertEquals("CodeInfoTabControl.Visible", true, userControl.CodeInfoTabControl.Visible);
				AssertEquals("CustomsProcessingGroupBox.Visible", true, userControl.CustomsProcessingGroupBox.Visible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", true, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("JE_EntryAuthorisationDateDateEdit.Visible", false, userControl.JE_EntryAuthorisationDateDateEdit.Visible);
				AssertEquals("PaymentTermsDropEdit.Visible", true, userControl.PaymentTermsDropEdit.Visible);
				AssertEquals("GoodsValueCalcFindBox.Visible", false, userControl.GoodsValueCalcFindBox.Visible);
				AssertEquals("CustomsWeightCalcDropEdit.Visible", false, userControl.CustomsWeightCalcDropEdit.Visible);
				AssertEquals("CodeInfoTabControl.Visible", true, userControl.CodeInfoTabControl.Visible);
				AssertEquals("CustomsProcessingGroupBox.Visible", true, userControl.CustomsProcessingGroupBox.Visible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("JE_EntryAuthorisationDateDateEdit.Visible", false, userControl.JE_EntryAuthorisationDateDateEdit.Visible);
				AssertEquals("PaymentTermsDropEdit.Visible", false, userControl.PaymentTermsDropEdit.Visible);
				AssertEquals("GoodsValueCalcFindBox.Visible", true, userControl.GoodsValueCalcFindBox.Visible);
				AssertEquals("CustomsWeightCalcDropEdit.Visible", true, userControl.CustomsWeightCalcDropEdit.Visible);
				AssertEquals("CodeInfoTabControl.Visible", false, userControl.CodeInfoTabControl.Visible);
				AssertEquals("CustomsProcessingGroupBox.Visible", false, userControl.CustomsProcessingGroupBox.Visible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("JE_SoldOrConsignedDropEdit.Visible", false, userControl.JE_SoldOrConsignedDropEdit.Visible);
				AssertEquals("JE_EntryAuthorisationDateDateEdit.Visible", false, userControl.JE_EntryAuthorisationDateDateEdit.Visible);
				AssertEquals("PaymentTermsDropEdit.Visible", false, userControl.PaymentTermsDropEdit.Visible);
				AssertEquals("GoodsValueCalcFindBox.Visible", true, userControl.GoodsValueCalcFindBox.Visible);
				AssertEquals("CustomsWeightCalcDropEdit.Visible", true, userControl.CustomsWeightCalcDropEdit.Visible);
				AssertEquals("CodeInfoTabControl.Visible", false, userControl.CodeInfoTabControl.Visible);
				AssertEquals("CustomsProcessingGroupBox.Visible", false, userControl.CustomsProcessingGroupBox.Visible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("PaymentTermsDropEdit.Visible", true, userControl.PaymentTermsDropEdit.Visible);
				AssertEquals("NotifyPartyDocAddressControl.Visible", true, userControl.NotifyPartyDocAddressControl.Visible);
				AssertEquals("DeliveryDestinationPartyDocAddressControl.Visible", true, userControl.DeliveryDestinationPartyDocAddressControl.Visible);
			});
		}

		public void TestControlVisibilitySwitchingBetweenCompletionAndFormalEntry()
		{
			NZCustomsDeclarationUserControlTestRunner((userControl, declaration) =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("JE_OriginalEntryNumberTextBox.Visible", false, userControl.JE_OriginalEntryNumberTextBox.Visible);
				AssertEquals("OriginalEntryTypeDropEdit.Visible", false, userControl.OriginalEntryTypeDropEdit.Visible);
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
				AssertEquals("JE_OriginalEntryNumberTextBox.Visible", true, userControl.JE_OriginalEntryNumberTextBox.Visible);
				AssertEquals("OriginalEntryTypeDropEdit.Visible", true, userControl.OriginalEntryTypeDropEdit.Visible);
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("JE_OriginalEntryNumberTextBox.Visible", true, userControl.JE_OriginalEntryNumberTextBox.Visible);
				AssertEquals("OriginalEntryTypeDropEdit.Visible", true, userControl.OriginalEntryTypeDropEdit.Visible);
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("JE_OriginalEntryNumberTextBox.Visible", false, userControl.JE_OriginalEntryNumberTextBox.Visible);
				AssertEquals("OriginalEntryTypeDropEdit.Visible", false, userControl.OriginalEntryTypeDropEdit.Visible);
			});
		}

		public void TestDisposeUnhooksEventsProperly()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals("should be no events hooked before we start", true, jobDeclaration.PermitCodes.CountChangedIsNull_DebugOnly);
			AssertEquals("should be no events hooked before we start", true, jobDeclaration.OtherInfos.CountChangedIsNull_DebugOnly);
			using (var control = new CustomsDeclarationUserControlForTesting())
			{
				control.JobDeclaration = jobDeclaration;
				control.OnLoadForTesting();
				AssertEquals("event is hooked after load", false, jobDeclaration.PermitCodes.CountChangedIsNull_DebugOnly);
				AssertEquals("event is hooked after load", false, jobDeclaration.OtherInfos.CountChangedIsNull_DebugOnly);
			}

			AssertEquals("event is unhooked after dispose", true, jobDeclaration.PermitCodes.CountChangedIsNull_DebugOnly);
			AssertEquals("event is unhooked after dispose", true, jobDeclaration.OtherInfos.CountChangedIsNull_DebugOnly);
		}

		public void TestResolutionChangeForMinimumSizeForm()
		{
			using (var form = new DeclarationForm(Declaration))
			{
				var userControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as NZCustomsDeclarationUserControl;
				form.Show();
				Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				var expectedGroupBoxHeight = (int)Math.Ceiling((359 * (ControlDpiScalingHelper.DpiY / ControlDpiScalingHelper.BaseDpiY)) - 0.5);
				AssertEquals("ShipmentDetailsGroupBox sizing - height should not have been overridden from that set in designer", expectedGroupBoxHeight, userControl.ShipmentDetailsGroupBox.Size.Height);
			}
		}

		public void TestDeliveryNotificationsTabPageShouldFollowOrganizationsTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm())
			{
				using (var control = new NZCustomsDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(declaration, ".");
					form.Show();
					var deliveryNotificationsTabPageIndex = control.RightTabControl.TabPages.IndexOfKey("DeliveryNotificationsTabPage");
					var organizationsTabPageIndex = control.RightTabControl.TabPages.IndexOf(control.OrganisationsTabPage);
					AssertEquals("DeliveryNotificationsTabPage should follow OrganizationsTabPage.", organizationsTabPageIndex + 1, deliveryNotificationsTabPageIndex);
				}
			}
		}

		public void TestConsolidatedJobIndicator()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm())
				using (var control = new NZCustomsDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(declaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should not display when EnableConsolidatedEntries is No.", !control.ConsolidatedDeclarationAdviceLabel.Visible);
				}

				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				using (var form = new ZForm())
				using (var control = new NZCustomsDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(consolidatedDeclaration.LeadDeclaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should not display when EnableConsolidatedEntries is No.", !control.ConsolidatedDeclarationAdviceLabel.Visible);
				}
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm())
				using (var control = new NZCustomsDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(declaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should not display for a regular declaration.", !control.ConsolidatedDeclarationAdviceLabel.Visible);
				}

				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				using (var form = new ZForm())
				using (var control = new NZCustomsDeclarationUserControl())
				{
					form.Controls.Add(control);
					form.SetDataBinding(consolidatedDeclaration.LeadDeclaration, ".");
					form.Show();
					Assert("ConsolidatedJobIndicator should display for a Consolidated Declaration.", control.ConsolidatedDeclarationAdviceLabel.Visible);
				}
			}
		}

		public class CustomsDeclarationUserControlForTesting : NZCustomsDeclarationUserControl
		{
			public void OnLoadForTesting()
			{
				base.OnLoad(new EventArgs());
			}
		}

		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}

				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;
		void NZCustomsDeclarationUserControlTestRunner(Action<NZCustomsDeclarationUserControl_ForTesting, JobDeclaration> action) => UserControlTestRunner(action);
		void UserControlTestRunner<TControl, TBusinessObject>(Action<TControl, TBusinessObject> action)
			where TBusinessObject : BusinessObject where TControl : ZUserControl, new()
		{
			var bizObj = Factory.New<TBusinessObject>();
			using (var form = new ZForm())
			using (var control = new TControl())
			{
				form.Controls.Add(control);
				form.SetDataBinding(bizObj, ".");
				form.Show();
				action(control, bizObj);
			}
		}
	}

	class NZCustomsDeclarationUserControl_ForTesting : NZCustomsDeclarationUserControl
	{
		public new ZDropEdit ProcessingPortDropEdit => base.ProcessingPortDropEdit;
		public new ZDateEdit JE_EntryAuthorisationDateDateEdit => base.JE_EntryAuthorisationDateDateEdit;
		public new ZTemplateTabControl CodeInfoTabControl => base.CodeInfoTabControl;
		public new ZDropEdit PaymentTermsDropEdit => base.PaymentTermsDropEdit;
		public new ZDropEdit JE_SoldOrConsignedDropEdit => base.JE_SoldOrConsignedDropEdit;
		public new ZGroupBox CustomsProcessingGroupBox => base.CustomsProcessingGroupBox;
		public new ZCalcFindBox GoodsValueCalcFindBox => base.GoodsValueCalcFindBox;
		public new ZCalcDropEdit CustomsWeightCalcDropEdit => base.CustomsWeightCalcDropEdit;
		public new ZArchitecture.ZTextBox JE_OriginalEntryNumberTextBox => base.JE_OriginalEntryNumberTextBox;
		public new ZDropEdit JE_IsZeroRatedAllDropEdit => base.JE_IsZeroRatedAllDropEdit;
		public new ZDropEdit TransactionNatureDropEdit => base.TransactionNatureDropEdit;
		public new ZArchitecture.ZTextBox TSWCombinedStatusTextBox => base.TSWCombinedStatusTextBox;
		public new ZDropEdit ApplicationCodeDropEdit => base.ApplicationCodeDropEdit;
		public new TranshipmentRequestTabPage TranshipmentRequestTabPage => base.TranshipmentRequestTabPage;
		public new ZDocAddressControl NotifyPartyDocAddressControl => base.NotifyPartyDocAddressControl;
		public new ZDocAddressControl DeliveryDestinationPartyDocAddressControl => base.DeliveryDestinationPartyDocAddressControl;
	}
}
