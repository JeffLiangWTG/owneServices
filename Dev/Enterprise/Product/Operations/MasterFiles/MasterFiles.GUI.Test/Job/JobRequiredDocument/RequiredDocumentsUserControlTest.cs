using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RequiredDocumentsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAttribDisplayValueCharacterCasing()
		{
			var organisation = Factory.New<OrgHeader>();

			var docManagerSupport = (IDocManagerSupport)organisation;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(organisation, "TST");

			using var form = new ZOrganisationsForm(organisation);
			using var userControl = new RequiredDocumentsUserControl();
			form.Controls.Add(userControl);
			form.Show();

			userControl.SetDataBinding(storageMain, "");
			userControl.Show();
			var attribDisplayValueColumnStyleInfo = (ZMultiControlColumnStyleInfo)userControl.AttributesGrid.GetColumnStyle("D0_AttribDisplayValue");
			AssertEquals(CharacterCasing.Normal, attribDisplayValueColumnStyleInfo.CharacterCasing);
		}

		[RequiresSTA]
		public void TestCountryDependantControls_ChinaSpecific()
		{
			var newOrganisation = Factory.NewWithValidTestData<OrgHeader>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				using (var form = new ZOrganisationsForm(newOrganisation))
				using (var newUserControl = new RequiredDocumentsUserControl())
				{
					form.Controls.Add(newUserControl);
					form.Show();

					var returnedToShipperDateEdit = form.FindSingle<ZDateEdit>("ReturnedToShipperDateEdit");
					Assert(returnedToShipperDateEdit.Visible);

					var rcvdFromBrokerDateEditButton = form.FindSingle<ZDateEdit>("RcvdFromBrokerDateEdit");
					Assert(rcvdFromBrokerDateEditButton.Visible);

					var sentToBrokerDateEdit = form.FindSingle<ZDateEdit>("SentToBrokerDateEdit");
					Assert(sentToBrokerDateEdit.Visible);

					var documentsGrid = form.FindSingle<ZGrid>("DocumentsGrid");
					CombineAssertions(() =>
					{
						Assert(documentsGrid.Columns.Contains("EQ_ReturnToShipper"));
						Assert(documentsGrid.Columns.Contains("EQ_RcvFromCustomsBroker"));
						Assert(documentsGrid.Columns.Contains("EQ_SntToCustomsBroker"));
					});
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (var form = new ZOrganisationsForm(newOrganisation))
				using (var newUserControl = new RequiredDocumentsUserControl())
				{
					form.Controls.Add(newUserControl);
					form.Show();

					var returnedToShipperDateEdit = form.FindSingle<ZDateEdit>("ReturnedToShipperDateEdit");
					Assert(!returnedToShipperDateEdit.Visible);

					var rcvdFromBrokerDateEditButton = form.FindSingle<ZDateEdit>("RcvdFromBrokerDateEdit");
					Assert(!rcvdFromBrokerDateEditButton.Visible);

					var sentToBrokerDateEdit = form.FindSingle<ZDateEdit>("SentToBrokerDateEdit");
					Assert(!sentToBrokerDateEdit.Visible);

					var documentsGrid = form.FindSingle<ZGrid>("DocumentsGrid");
					CombineAssertions(() =>
					{
						Assert(!documentsGrid.Columns.Contains("EQ_ReturnToShipper"));
						Assert(!documentsGrid.Columns.Contains("EQ_RcvFromCustomsBroker"));
						Assert(!documentsGrid.Columns.Contains("EQ_SntToCustomsBroker"));
					});
				}
			}
		}

		[RequiresSTA]
		public void TestButtonVisibility()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var docManagerSupport = (IDocManagerSupport)organisation;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(organisation, "TST");

			using (var form = new ZOrganisationsForm(organisation))
			using (var userControl = new RequiredDocumentsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.SetDataBinding(storageMain, "");
				userControl.Show();
				AssertEquals(false, userControl.DISDataButton.Visible);
			}
		}

		public void TestDISReferenceNumberFountainStrategyErrors()
		{
			var disHost = new DummyDISHost(Factory);

			var provider = new DummyDISHostProvider(disHost);

			using (var form = new ZForm(provider))
			using (var userControl = new RequiredDocumentsUserControlForTest())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.Show();
				AssertEquals("precondition", true, userControl.DISDataButton.Visible);
				AssertEquals("precondition", true, Enterprise.Environment.Env.Security.CustomsDISEdit.IsAllowed);

				// test with warning and edit form
				((DummyDISReferenceNumberFountainStrategy)disHost.DISReferenceNumberFountainStrategy).State = new StrategyState(
					Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Warning,
					"test DIS warning"
				);
				userControl.ShownDISForm = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.DISDataButton.PerformClick();

				AssertEquals("test DIS warning", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Edit", userControl.ShownDISForm);

				// test with error and edit form
				((DummyDISReferenceNumberFountainStrategy)disHost.DISReferenceNumberFountainStrategy).State = new StrategyState(
					Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Error,
					"test DIS error"
				);
				userControl.ShownDISForm = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.DISDataButton.PerformClick();

				AssertEquals("test DIS error", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DIS form should not be displayed on error", null, userControl.ShownDISForm);

				// test with ok state
				((DummyDISReferenceNumberFountainStrategy)disHost.DISReferenceNumberFountainStrategy).State = new StrategyState(
					Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Ok,
					null
				);
				userControl.ShownDISForm = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.DISDataButton.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Edit", userControl.ShownDISForm);

				// test with null state
				((DummyDISReferenceNumberFountainStrategy)disHost.DISReferenceNumberFountainStrategy).State = null;
				userControl.ShownDISForm = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.DISDataButton.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Edit", userControl.ShownDISForm);

				// test with error and view form
				Enterprise.Environment.Env.Security.CustomsDISEdit.IsAllowed = false;
				((DummyDISReferenceNumberFountainStrategy)disHost.DISReferenceNumberFountainStrategy).State = new StrategyState(
					Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Error,
					"test DIS error"
				);
				userControl.ShownDISForm = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.DISDataButton.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("View", userControl.ShownDISForm);
			}
		}

		class RequiredDocumentsUserControlForTest : RequiredDocumentsUserControl
		{
			protected override void ShowDISForm(Form mainForm, IDISHost disHost, bool isEditAllowed)
			{
				ShownDISForm = isEditAllowed ? "Edit" : "View";
			}

			public string ShownDISForm { get; set; }
		}

		class DummyDISHostProvider : BusinessObjectCollection<DummyDocumentHost>, IDISHostProvider
		{
			public DummyDISHostProvider(IDISHost disHost) : base(disHost.Factory)
			{
				DISHost = disHost;
			}

			public IDISHost DISHost { get; }
		}

		class DummyDocumentHost : NonPersistentBusinessObject, IHaveRequiredDocuments
		{
			public abstract class Schema
			{
				public const string TableName = DummyBizoSchema.Constants.TableName;
			}

			public DummyDocumentHost(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString UniqueConsignRef => ZString.Empty;
			public ZString HouseBill => ZString.Empty;
			public ZString MasterBill => ZString.Empty;
			public OrgHeader ExportBroker => null;
			public ZString TableCode => ZString.Empty;
			public JobRequiredDocumentDependentCollection RequiredDocuments => null;
			public BusinessObject UltimateDocumentParent => null;
			public Logs Logs => null;
			public IReadOnlyList<ZString> AdditionalRefTypes => Array.Empty<ZString>();

			public override SchemaGuidColumn PKSchemaColumn => DummyBizoSchema.PK;

			public void PreLogAllDocumentsReceivedEvents()
			{
			}
		}

		class DummyDISHost : IDISHost
		{
			public DummyDISHost(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			public bool NeedToDoPreFormAction()
			{
				return false;
			}

			public bool DoPreFormAction()
			{
				return false;
			}

			public BusinessObjectFactory Factory { get; }
			public ZGuid PK { get; } = ZGuid.NewZGuid();
			public ZBool ShowDISFeatures => true;
			public ZGuid BranchPK => ZGuid.Empty;
			public ZGuid CompanyPK => ZGuid.Empty;
			public IEnumerable<string> ApplicationCodes => Array.Empty<string>();
			public IHaveRequiredDocuments RequiredDocumentsProvider { get; }
			public IEnumerable<IeDoc> EDocs { get; }
			public ZString JobNumber => ZString.Empty;

			public IControllerIDProvider ControllerIDProvider => null;
			public ZString HumanReadable => "Dummy";

			public ZBool DISEditable => Enterprise.Environment.Env.Security.CustomsDISEdit.IsAllowed;

			public ZString ImporterName => ZString.Empty;
			public IEnumerable<ZString> ErrorMessages => Array.Empty<ZString>();
			public Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategy DISReferenceNumberFountainStrategy { get; } = new DummyDISReferenceNumberFountainStrategy();

			public event EventHandler DISFeatureVisibilityChanged
			{
				add { }
				remove { }
			}
		}

		class DummyDISReferenceNumberFountainStrategy : Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategy
		{
			public StrategyState State { private get; set; }

			Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategyState Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategy.CheckState()
			{
				return State;
			}

			Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategyState Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategy.CheckStateForAddingNewRecord()
			{
				return State;
			}

			ZString Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategy.GetDISReferenceNumber()
			{
				return ZString.Empty;
			}

			IUniqueIndexFailureHandler Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategy.GetUniqueIndexFailureHandler(BusinessObject businessObject)
			{
				return null;
			}
		}

		class StrategyState : Enterprise.Integration.Customs.Shared.IDISReferenceNumberFountainStrategyState
		{
			public StrategyState(Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity severity, string message)
			{
				Severity = severity;
				Message = message;
			}

			public Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity Severity { get; }
			public string Message { get; }
		}

		[RequiresSTA]
		public void TestButtonCaptions()
		{
			var disHost = new DummyDISHost(Factory);
			DummyDISHostProvider provider = new DummyDISHostProvider(disHost);

			using (var form = new ZForm(provider))
			using (var userControl = new RequiredDocumentsUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.SetDataBinding(provider, "");
				userControl.Show();
				AssertEquals("View/Edit Dummy Data", userControl.DISDataButton.CaptionResourceString.Caption);
			}
		}
	}
}
