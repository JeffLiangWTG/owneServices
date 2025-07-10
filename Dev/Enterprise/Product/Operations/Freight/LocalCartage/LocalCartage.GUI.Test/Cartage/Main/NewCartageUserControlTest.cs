using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class NewCartageUserControlTest : TestCaseWithFactory
	{
		public void TestSelectSchedule_HasParentTrue_ShowsError()
		{
			var cartage = Factory.New<CommonCartage>();
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			cartage.JJ_ConsignmentID = "S1/E";
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			using (var testForm = new CartageForm(cartage))
			{
				testForm.Show();
				var userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				var cartageControl = testForm.CartageControl;
				AssertEquals("Precondition: HasParent is true", true, cartage.HasParent);
				AssertNull("Precondition: ParentJob does not exist", cartage.ParentJob);
				var selectSchedulesButton_ClickMethodInfo = cartageControl.GetType().GetMethod("SelectSchedulesButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				selectSchedulesButton_ClickMethodInfo.Invoke(cartageControl, new object[] { null, EventArgs.Empty });
				AssertContains("Should show error message", "Can not change sailing for Port Transport Jobs created from other modules. Please change sailing from relevant module.", userNotify.LastMessage.Text);
			}
		}

		public void TestSelectSchedule_HasParentFalse_DoesNotShowError()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "S1/E";
			cartage.JJ_ParentID = ZGuid.Empty;
			cartage.JJ_ParentTableCode = string.Empty;
			using (var testForm = new CartageForm(cartage))
			{
				testForm.Show();
				var userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				var cartageControl = testForm.CartageControl;
				var selectSchedulesButton_ClickMethodInfo = cartageControl.GetType().GetMethod("SelectSchedulesButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
				AssertNull("Precondition: ParentJob does not exist", cartage.ParentJob);
				selectSchedulesButton_ClickMethodInfo.Invoke(cartageControl, new object[] { null, EventArgs.Empty });
				AssertEquals("Should not show error message", true, userNotify.LastMessage.WasNone);
			}
		}

		public void TestSelectSchedule_HasParentJobTrue_ShowsError()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "S1/E";
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			Factory.Save();
			using (var testForm = new CartageForm(cartage))
			{
				testForm.Show();
				var userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				var cartageControl = testForm.CartageControl;
				AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
				AssertEquals("Precondition: ParentJob exists", dtbBooking, cartage.ParentJob);
				AssertEquals("Precondition: ParentJob should not have an external TB reference", false, ((ITransportAdditionalReferenceNumbers)cartage.ParentJob).AdditionalReferenceNumbers.ToArray().Select(n => ((ICusEntryNumber)n).CE_EntryType).Contains(AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber));
				var selectSchedulesButton_ClickMethodInfo = cartageControl.GetType().GetMethod("SelectSchedulesButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				selectSchedulesButton_ClickMethodInfo.Invoke(cartageControl, new object[] { null, EventArgs.Empty });
				AssertContains("Should show error message", "Can not change sailing for Port Transport Jobs created from other modules. Please change sailing from relevant module.", userNotify.LastMessage.Text);
			}
		}

		public void TestSelectSchedule_HasParentJobTrueAndParentIsExternalTB_DoesNotShowError()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "S1/E";
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();

			var referenceNum = ((ITransportAdditionalReferenceNumbers)dtbBooking).AdditionalReferenceNumbers.AddNew();
			referenceNum.CE_EntryType = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;

			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			Factory.Save();
			using (var testForm = new CartageForm(cartage))
			{
				testForm.Show();
				var userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.OK);
				var cartageControl = testForm.CartageControl;
				AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
				AssertEquals("Precondition: ParentJob exists", dtbBooking, cartage.ParentJob);
				AssertEquals("ParentJob should have an external TB reference", true, ((ITransportAdditionalReferenceNumbers)cartage.ParentJob).AdditionalReferenceNumbers.ToArray().Select(n => ((ICusEntryNumber)n).CE_EntryType).Contains(AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber));
				var selectSchedulesButton_ClickMethodInfo = cartageControl.GetType().GetMethod("SelectSchedulesButton_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				selectSchedulesButton_ClickMethodInfo.Invoke(cartageControl, new object[] { null, EventArgs.Empty });
				AssertEquals("Should not show error message", true, userNotify.LastMessage.WasNone);
			}
		}

		public void TestAddressesLinkedLabelVisibilityAndCorrectnessForInternalCartages()
		{
			OrgHeader transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			transportProvider.OH_FullName = "ABC Transport";
			Factory.Save();
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			using (CartageForm testForm = new CartageForm(cartage))
			{
				testForm.Show();
				cartageType.RemoveCartageOrganisation();
				CartageUserControl cartageControl = testForm.CartageControl;
				cartageControl.SetupAddressesCaption(cartage);
				AssertEquals("Precondition: HasParent is true", true, cartage.HasParent);
				AssertEquals("AddressesLinkedToJobLinkLabel should be visible for Internal Job", true, cartageControl.AddressesLinkedToJobLinkLabel.Visible);
				ZString sTextExpected = "Linked to Job " + dummyCartageParent.UniqueConsignmentID;
				AssertEquals("no provider, so don't show, assume org proxy", sTextExpected, cartageControl.AddressesLinkedToJobLinkLabel.Text);
				cartageType.SetCartageOrganisation(transportProvider);
				cartageControl.SetupAddressesCaption(cartage);
				sTextExpected = "Linked to Job " + dummyCartageParent.UniqueConsignmentID + " - Transport Provider Allocated: ABC Transport";
				AssertEquals("Not OrgProxy so show", sTextExpected, cartageControl.AddressesLinkedToJobLinkLabel.Text);
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = transportProvider.PK;
				cartageType.SetCartageOrganisation(transportProvider);
				cartageControl.SetupAddressesCaption(cartage);
				sTextExpected = "Linked to Job " + dummyCartageParent.UniqueConsignmentID;
				AssertEquals("OrgProxy, so don't show, should be assumed", sTextExpected, cartageControl.AddressesLinkedToJobLinkLabel.Text);
			}
		}

		public void TestAddressesLinkedLabelVisibilityForStandalone()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			using (CartageForm testForm = new CartageForm(cartage))
			{
				testForm.Show();
				CartageUserControl cartageControl = testForm.CartageControl;
				cartageControl.SetupAddressesCaption(cartage);
				AssertEquals("AddressesLinkedToJobLinkLabel should be invisible for Internal Job", false, cartageControl.AddressesLinkedToJobLinkLabel.Visible);
			}
		}

		public void TestJobCreatingOnBind()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T10000";
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				CartageUserControl cartageUserControl = form.CartageControl;
				AssertNotNull(cartage.Job);
				AssertEquals(true, cartageUserControl.LocalClientOrgControl.Visible);
				AssertEquals(false, cartageUserControl.JobHeaderMutexErrorLabel.Visible);
				AssertEquals("", cartageUserControl.JobHeaderMutexErrorLabel.Text);
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOn_NewCartageHideClickSaveToCreateJobLabel()
		{
			var iAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			iAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(true);
			iAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			iAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(iAccounting.Object))
			{
				var cartageNew = Factory.New<CommonCartage>();
				using (CartageForm form = new CartageForm(cartageNew))
				{
					form.Show();
					var cartageUserControl = form.CartageControl;
					AssertNotNull(cartageNew.Job);
					AssertEquals(false, cartageNew.IsInDatabase);
					AssertEquals(false, cartageNew.Job.IsInDatabase);
					AssertEquals(false, cartageUserControl.ClickSaveToCreateJobLabel.Visible);
				}
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOn_ExistingCartageDisplayClickSaveToCreateJobLabel()
		{
			var iAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			iAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(true);
			iAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			iAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(iAccounting.Object))
			{
				var cartageExisting = Factory.New<CommonCartage>();
				Factory.Save();
				using (CartageForm form = new CartageForm(cartageExisting))
				{
					form.Show();
					var cartageUserControl2 = form.CartageControl;
					AssertNotNull(cartageExisting.Job);
					AssertEquals(true, cartageExisting.IsInDatabase);
					AssertEquals(false, cartageExisting.Job.IsInDatabase);
					AssertEquals(true, cartageUserControl2.ClickSaveToCreateJobLabel.Visible);
					form.FireSaveButton();
					AssertEquals(true, cartageExisting.Job.IsInDatabase);
					AssertEquals(false, cartageUserControl2.ClickSaveToCreateJobLabel.Visible);
				}
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff_NewCartageHideClickSaveToCreateJobLabel()
		{
			// mocking didn't work for the plugin
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var cartageNew = Factory.New<CommonCartage>();
			using (CartageForm form = new CartageForm(cartageNew))
			{
				form.Show();
				var cartageUserControl = form.CartageControl;
				AssertNull(cartageNew.Job);
				AssertEquals("Cartage hasn't been saved yet.", false, cartageNew.IsInDatabase);
				AssertEquals("If Job is Null, Saving is not going to Auto-Create the Job cause the registry is OFF.", false, cartageUserControl.ClickSaveToCreateJobLabel.Visible);
				form.FireSaveButton();
				AssertNull("Saving shouldn't have created the Job (registry off)", cartageNew.Job);
				AssertEquals("Label should still not be visible.", false, cartageUserControl.ClickSaveToCreateJobLabel.Visible);
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff_ExistingCartageDisplayClickSaveToCreateJobLabel()
		{
			// mocking didn't work for the plugin
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var cartageExisting = Factory.New<CommonCartage>();
			Factory.Save();
			using (CartageForm form = new CartageForm(cartageExisting))
			{
				form.Show();
				var cartageUserControl2 = form.CartageControl;
				AssertNull("Cartage exists, but job doesn't.", cartageExisting.Job);
				AssertEquals("Cartage was saved", true, cartageExisting.IsInDatabase);
				AssertEquals("If Job is Null, Saving is not going to Auto-Create the Job cause the registry is OFF.", false, cartageUserControl2.ClickSaveToCreateJobLabel.Visible);
				form.FireSaveButton();
				AssertNull("Saving shouldn't have created the Job (registry off)", cartageExisting.Job);
				AssertEquals("Label should still not be visible.", false, cartageUserControl2.ClickSaveToCreateJobLabel.Visible);
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff()
		{
			var iAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			iAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);

			using (ObjectFactory.Substitute(iAccounting.Object))
			{
				CommonCartage cartage = Factory.New<CommonCartage>();
				using (CartageForm form = new CartageForm(cartage))
				{
					form.Show();
					CartageUserControl cartageUserControl = form.CartageControl;
					AssertNull(cartage.Job);
					AssertEquals(false, cartageUserControl.LocalClientOrgControl.Visible);
					AssertEquals(true, cartageUserControl.JobHeaderMutexErrorLabel.Visible);
					AssertEquals("The registry item [Accounting --> Add Job Invoicing Record at Saving/Editing of Operations Job] has been set so that billing jobs will only be created upon entry to the Billing tab.", cartageUserControl.JobHeaderMutexErrorLabel.Text);
				}
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff_JobInactive()
		{
			var iAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			iAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			iAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			iAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(iAccounting.Object))
			{
				var cartage = Factory.New<CommonCartage>();
				var job = new JobHeader.Loader(cartage).TryCreate();
				Factory.Save();
				job.MarkAsInactive();
				Factory.Save();
				using (CartageForm form = new CartageForm(cartage))
				{
					form.Show();
					var cartageUserControl = form.CartageControl;
					AssertEquals("Job is cancelled", true, job.IsCancelled);
					AssertEquals(false, cartageUserControl.LocalClientOrgControl.Visible);
					AssertEquals(true, cartageUserControl.JobHeaderMutexErrorLabel.Visible);
					AssertEquals(@"The Job Invoicing Record has been created but is currently not active. 
Please click on ‘Billing’ tab or ‘Job Invoicing’ menu to activate the job.", cartageUserControl.JobHeaderMutexErrorLabel.Text);
				}
			}
		}

		public void TestAccountingJob_AutoCreateRegistryOff_JobAlreadyExists()
		{
			var iAccounting = new Mock<IAccounting>(MockBehavior.Strict);
			iAccounting.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			iAccounting.Setup(m => m.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			var mockRegistry = new Mock<IRegistry>();
			iAccounting.SetupGet(m => m.Registry).Returns(mockRegistry.Object);

			using (ObjectFactory.Substitute(iAccounting.Object))
			{
				CommonCartage cartage = Factory.New<CommonCartage>();
				JobHeader job = new JobHeader.Loader(cartage).TryCreate();
				Factory.Save();
				using (CartageForm form = new CartageForm(cartage))
				{
					form.Show();
					CartageUserControl cartageUserControl = form.CartageControl;
					AssertNotNull(cartage.Job);
					AssertEquals(job.PK, cartage.Job.PK);
					AssertEquals(true, cartageUserControl.LocalClientOrgControl.Visible);
					AssertEquals(false, cartageUserControl.JobHeaderMutexErrorLabel.Visible);
					AssertEquals("", cartageUserControl.JobHeaderMutexErrorLabel.Text);
				}
			}
		}

		public void TestAquireMutex_WhenMutexAlreadyAquired()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T10000";
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(cartage.PK);
			try
			{
				mutex.Lock();
				using (CartageForm form = new CartageForm(cartage))
				{
					AssertNull(cartage.Job);
					form.Show();
					CartageUserControl cartageUserControl = form.CartageControl;
					AssertEquals(false, cartageUserControl.LocalClientOrgControl.Visible);
					AssertEquals(true, cartageUserControl.JobHeaderMutexErrorLabel.Visible);
					AssertEquals("You have created the job T10000 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job T10000 to continue.", cartageUserControl.JobHeaderMutexErrorLabel.Text);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestJobHeaderMutexReleased()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T10000";
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(cartage.PK);
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				CartageUserControl cartageUserControl = form.CartageControl;
				AssertEquals(true, cartageUserControl.LocalClientOrgControl.Visible);
			}

			AssertEquals("Mutex released on dispose of the user control", false, mutex.IsLocked);
		}

		public void TestLocalClientControlVisibilityWhenJobIsCreatedOrDeleted()
		{
			var cartage = Factory.New<CommonCartage>();
			using (ZForm form = new ZForm(cartage))
			using (CartageUserControl control = new CartageUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(cartage, "");
				AssertNotNull(cartage.Job);
				AssertEquals(true, control.LocalClientOrgControl.Visible);
				AssertEquals(false, control.JobHeaderMutexErrorLabel.Visible);
				cartage.Job.Delete();
				AssertEquals(false, control.LocalClientOrgControl.Visible);
				AssertEquals(true, control.JobHeaderMutexErrorLabel.Visible);
				AssertEquals("Billing job is being deleted.", control.JobHeaderMutexErrorLabel.Text);
				new JobHeader.Loader(cartage).TryCreate();
				AssertEquals(true, control.LocalClientOrgControl.Visible);
				AssertEquals(false, control.JobHeaderMutexErrorLabel.Visible);
			}
		}

		public void TestWorkflowCustomFields()
		{
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.CartageLegWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 2);
			using (Form form = new Form())
			using (CartageUserControl control = new CartageUserControl())
			{
				control.SetDataBinding(cartage, "");
				form.Controls.Add(control);
				form.Show();
				AssertGridContainsCustomField(control.SummaryGrid, "stringField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.SummaryGrid, "intField", typeof(ZCalcEditColumnStyleInfo));
				AssertGridContainsCustomField(control.SummaryGrid, "dateTimeField", typeof(ZDateEditColumnStyleInfo));
				AssertGridContainsCustomField(control.SummaryGrid, "boolField", typeof(ZCheckBoxColumnStyleInfo));
			}
		}

		void AssertGridContainsCustomField(ZGrid grid, ZString name, Type type)
		{
			bool wasFound = false;
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.Caption == name)
				{
					wasFound = true;
					AssertContains(string.Format("__{0}__prop", name.ToUpperInvariant()), column.ColumnName);
					AssertEquals("Workflow Custom Fields", column.GroupName.Caption);
					AssertEquals(true, type.IsAssignableFrom(column.GetType()));
					AssertEquals(true, column.IsVisible);
					if (type == typeof(ZTextBoxColumnStyleInfo))
					{
						ZTextBoxColumnStyle columnStyle = (ZTextBoxColumnStyle)grid.Columns[column.ColumnName].ColumnStyle;
						AssertEquals(AutoGenCustomAddOnValue.Schema.XV_DataMaxLength, columnStyle.TextBox.MaxLength);
					}
				}
			}

			AssertEquals(name + " custom field was not found in the grid", true, wasFound);
		}

		public void TestContainersRemovedWhenLooseModeSelected()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			var legLM1 = looseMove1.CartageLegs.AddNew();
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			var legLM2 = looseMove2.CartageLegs.AddNew();
			var contMove1 = cartage.ContainerBookedMoves.AddNew();
			var legC1 = contMove1.CartageLegs.AddNew();
			var contMove2 = cartage.ContainerBookedMoves.AddNew();
			var legC2 = contMove2.CartageLegs.AddNew();
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				AssertContainsExactElementsInAnyOrder("Precondition", new[] { legLM1, legLM2, legC1, legC2 }, form.CartageControl.SummaryGrid.List);
				cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
				AssertContainsExactElementsInAnyOrder(new[] { legLM1, legLM2 }, form.CartageControl.SummaryGrid.List);
			}
		}

		public void TestLooseMovesRemovedWhenContainerModeSelected()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			var legLM1 = looseMove1.CartageLegs.AddNew();
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			var legLM2 = looseMove2.CartageLegs.AddNew();
			var contMove1 = cartage.ContainerBookedMoves.AddNew();
			var legC1 = contMove1.CartageLegs.AddNew();
			var contMove2 = cartage.ContainerBookedMoves.AddNew();
			var legC2 = contMove2.CartageLegs.AddNew();
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				AssertContainsExactElementsInAnyOrder("Precondition", new[] { legLM1, legLM2, legC1, legC2 }, form.CartageControl.SummaryGrid.List);
				cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
				AssertContainsExactElementsInAnyOrder(new[] { legC1, legC2 }, form.CartageControl.SummaryGrid.List);
			}
		}

		public void TestWarningWhenValidMovesAreToBeDeleted()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			looseMove1.EW_BookedPackCount = 10;
			var legLM1 = looseMove1.CartageLegs.AddNew();
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			var legLM2 = looseMove2.CartageLegs.AddNew();
			var contMove1 = cartage.ContainerBookedMoves.AddNew();
			contMove1.Container.JC_ContainerNum = "CONTCODE1";
			var legC1 = contMove1.CartageLegs.AddNew();
			var contMove2 = cartage.ContainerBookedMoves.AddNew();
			var legC2 = contMove2.CartageLegs.AddNew();
			UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
			using (CartageForm form = new CartageForm(cartage))
			{
				form.Show();
				AssertContainsExactElementsInAnyOrder("Precondition", new[] { legLM1, legLM2, legC1, legC2 }, form.CartageControl.SummaryGrid.List);
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.No);
				cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
				AssertEquals("Last message shown", "Changing the Container Mode to 'CNT' will remove the Loose Movements from this Job. Proceed?", userNotify.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder("All legs should remain", new[] { legLM1, legLM2, legC1, legC2 }, form.CartageControl.SummaryGrid.List);
				AssertEquals("Container Mode should stay unchanged", Constants.CartageContainerMode.Mixed, cartage.JJ_ContainerMode);
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.Yes);
				cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
				AssertEquals("Last message shown", "Changing the Container Mode to 'CNT' will remove the Loose Movements from this Job. Proceed?", userNotify.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder("Loose Moves/Legs should be deleted", new[] { legC1, legC2 }, form.CartageControl.SummaryGrid.List);
				AssertEquals("Container Mode should change to CNT", Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.No);
				cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
				AssertEquals("Last message shown", "Changing the Container Mode to 'LSE' will remove the Containers from this Job. Proceed?", userNotify.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder("All legs should remain", new[] { legC1, legC2 }, form.CartageControl.SummaryGrid.List);
				AssertEquals("Container Mode should stay unchanged", Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.Yes);
				cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
				AssertEquals("Last message shown", "Changing the Container Mode to 'LSE' will remove the Containers from this Job. Proceed?", userNotify.LastMessage.Text);
				AssertEquals("Container Moves/Legs should be deleted", 0, form.CartageControl.SummaryGrid.List.Count);
				AssertEquals("Container Mode should change to LSE", Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
			}
		}

		public void TestShouldNotThrowWhenBindingNewCartageToUserControl()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var defaultingRulesEngineConfiguration = (BooleanRegistryItem)data.FindByName("CustomBranchDefaultingRulesEngineConfiguration");
			using (defaultingRulesEngineConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				shipment.JS_UniqueConsignRef = "S1";
				shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				shipment.JS_UniqueConsignRef = "S2";
				new JobHeader.Loader(shipment).TryLoadOrCreate().JH_GE = GlbDepartment.CurrentDepartment.PK;
				var address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.OA_RN_NKCountryCode = GlbBranch.CurrentBranch.BaseCountry.Code;
				Factory.Save();
				shipment.Job.JH_OA_LocalChargesAddr = address1.PK;
				Factory.Save();
				var cartage = Helper.CreateInternalCartage(shipment);
				using (var userControl = new CartageUserControl())
				{
					AssertNoExceptionThrown("Should not throw exception 'Value cannot be null' when binding cartage to CartageUserControl", () => userControl.SetDataBinding(cartage, string.Empty));
				}
			}
		}

		public void TestShouldNotTruncateETDAndETADatesWhenSettingUpSailingForAir()
		{
			var testETD = new ZDateTime(2023, 6, 1, 13, 11, 0);
			var testETA = new ZDateTime(2023, 6, 1, 15, 12, 0);
			JobSailing sailing = Helper.CreateSailingWithEstimatedArrival(
				Helper.TestVessel1,
				"111",
				LocalCartageTestHelper.HomePort,
				LocalCartageTestHelper.OverseasPort,
				testETD,
				testETA,
				Core.Constants.TransportModes.Air);
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Core.Constants.TransportModes.Air;
			CommonBookedCtgMove booked1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			cartage.JJ_JX_Sailing = sailing.PK;

			using (var testForm = new CartageForm(cartage))
			{
				testForm.Show();
				var etdDateEdit = testForm.FindSingle<ZDateEdit>(de => de.Name == "JJ_JA_E_DEPDateEdit");
				var etaDateEdit = testForm.FindSingle<ZDateEdit>(de => de.Name == "JJ_JB_E_ARVDateEdit");
				CombineAssertions("Estimated date edit controls should not truncate date", () =>
				{
					AssertEquals("Should not truncate estimated departure date after switching date edit control to long datetime format", testETD.ToLongTimeString().ToUpperInvariant(), etdDateEdit.Text.ToUpperInvariant());
					AssertEquals("Should not truncate estimated arrival date after switching date edit control to long datetime format", testETD.ToLongTimeString().ToUpperInvariant(), etdDateEdit.Text.ToUpperInvariant());
				});
			}
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
