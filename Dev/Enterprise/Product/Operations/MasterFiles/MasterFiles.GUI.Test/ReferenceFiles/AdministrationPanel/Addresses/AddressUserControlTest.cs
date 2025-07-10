using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressUserControlTest : TestCaseWithFactory
	{
		const string recordsExist = "Some records meeting your filter criteria exist in \"Records Modified in This Session\" grid.";

		public void TestLoadAddressEntityByGridSelection()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				var uniqStr = Guid.NewGuid().ToString();
				var uniqStr1 = Guid.NewGuid().ToString();
				var uniqStr2 = Guid.NewGuid().ToString();
				orgAddress.OA_Address1 = uniqStr1;
				orgAddress.OA_Address2 = uniqStr;
				orgAddress.OA_ValidationStatus = "INV";
				jobDocAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				jobDocAddress.E2_AddressOverride = true;
				jobDocAddress.E2_Address1 = uniqStr2;
				jobDocAddress.E2_Address2 = uniqStr;
				jobDocAddress.E2_ValidationStatus = "INV";
				Factory.Save();

				var filter = FilterBusinessObject["Address2"] as ModuleTextFilter;
				filter.Property = uniqStr;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				FilterControl.FirePerformSearch();
				AddressUserControl.AddressDetailControl.Address2Control.Text = "";
				FilterControl.Grid.PerformMouseDownForTest(0, 1);
				AssertEquals(MatchingUniqStr(), AddressUserControl.AddressDetailControl.Address1Control.Text);
				AddressUserControl.AddressDetailControl.Address2Control.Text = "";
				FilterControl.Grid.PerformMouseDownForTest(1, 1);
				AssertEquals(MatchingUniqStr(), AddressUserControl.AddressDetailControl.Address1Control.Text);
				string MatchingUniqStr()
				{
					return ((MDMAdminPanelAddressView)AddressUserControl.AddressDetailControl.CurrentDataItem).AddressEntity.PK.Equals(orgAddress.PK) ? uniqStr1.ToUpper() : uniqStr2.ToUpper();
				}
			}
		}

		public void TestMessageWhenSearching()
		{
			SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				for (int i = 0; i < 15; i++)
				{
					var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
					orgAddress1.OA_Address1 = "ADMINPANELORGADDRESS1ADDRESS" + i;
					orgAddress1.OA_ValidationStatus = AddressValidationStatus.Verified;
				}
				Factory.Save();

				var filter1 = FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
				filter1.Property = AddressValidationStatus.Verified;
				filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter1.IsActive = true;

				var filter2 = FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter2.Property = "ADMINPANELORGADDRESS1ADDRESS";
				filter2.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter2.IsActive = true;

				UnitTestUserNotification.Instance.ClearMessages();
				FilterControl.FirePerformSearch();
				AssertEquals("Show 10 records", 10, AddressUserControl.Manager.AdminPanelAddressCollection.Count);
				AssertContains("The message should contain 'Found 15 records.'", "Found 15 records.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains($"The message should not contain '{recordsExist}'", recordsExist, UnitTestUserNotification.Instance.LastMessage.Text);

				var collection = AddressUserControl.Manager.AdminPanelProcessedAddressCollection;
				var query = new ZQuery(MDMAdminPanelAddressViewSchema.MDM_ValidationStatus, AddressValidationStatus.Verified);
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_Address1, "ADMINPANELORGADDRESS1ADDRESS11");
				collection.Load(query);

				UnitTestUserNotification.Instance.ClearMessages();
				FilterControl.FirePerformSearch();
				AssertEquals("Show 10 records", 10, AddressUserControl.Manager.AdminPanelAddressCollection.Count);
				AssertContains("The message should contain 'Found 14 records'", "Found 14 records.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"The message should contain '{recordsExist}'", recordsExist, UnitTestUserNotification.Instance.LastMessage.Text);

				filter2.Property = "ADMINPANELORGADDRESS1ADDRESS1";
				UnitTestUserNotification.Instance.ClearMessages();
				FilterControl.FirePerformSearch();
				AssertEquals("Show 5 records", 5, AddressUserControl.Manager.AdminPanelAddressCollection.Count);
				AssertNotContains("The message should not contain 'Found 5 records'", "Found 5 records", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"The message should contain '{recordsExist}'", recordsExist, UnitTestUserNotification.Instance.LastMessage.Text);

				filter2.Property = "ADMINPANELORGADDRESS1ADDRESS11";
				UnitTestUserNotification.Instance.ClearMessages();
				FilterControl.FirePerformSearch();
				AssertEquals("Show 0 records", 0, AddressUserControl.Manager.AdminPanelAddressCollection.Count);
				AssertNotContains("The message should not contain 'Found 0 records'", "Found 0 records", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("The message should contain 'Found no records'", "Found no records", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"The message should contain '{recordsExist}'", recordsExist, UnitTestUserNotification.Instance.LastMessage.Text);

				filter2.Property = "ADMINPANELORGADDRESS1ADDRESS111";
				UnitTestUserNotification.Instance.ClearMessages();
				FilterControl.FirePerformSearch();
				AssertEquals("Show 0 records", 0, AddressUserControl.Manager.AdminPanelAddressCollection.Count);
				AssertNotContains("The message should not contain 'Found 0 records'", "Found 0 records", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("The message should contain 'Found no records'", "Found no records", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains($"The message should not contain '{recordsExist}'", recordsExist, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowBackgroundValidationStatusIconAndHideWhenAutoVerifyIsFinsihed()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new ZForm())
			using (var addressControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
			{
				form.Controls.Add(addressControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
				Factory.Save();

				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
				filter.Property = AddressValidationStatus.ToBeVerified;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();

				AssertEquals("The BackgroundValidationStatusIcon should be shown", true, filterControl.BackgroundValidationStatusIcon.Visible);

				addressControl.AutoVerifyFinished += delegate
				{
					AssertEquals("The BackgroundValidationStatusIcon should be hidden", false, filterControl.BackgroundValidationStatusIcon.Visible);
				};
			}
		}

		public void TestPreviousAutoValidationShouldBeKilledWhenClickFind()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var webAddress = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort());
			AddressValidationService.SetAvailableWebServiceAddress(webAddress);
			var serviceUri = new Uri(webAddress + AddressValidationService.Constants.ValidationServiceName + "/");

			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET", "PUT", "POST" },
				Processor = (_, request) => new Tuple<int, string>(401, string.Empty),
				Uri = serviceUri,
				ContentType = "application/json"
			};

			try
			{
				testExternalValidationService.Start();

				using (var form = new ZForm())
				using (var addressControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
				{
					form.Controls.Add(addressControl);
					form.Show();

					for (int i = 0; i < 10; i++)
					{
						var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
						orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS" + i;
						orgAddress1.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
					}
					Factory.Save();

					var filterControl = addressControl.FilterControl;
					var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
					filter.Property = AddressValidationStatus.ToBeVerified;
					filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					filter.IsActive = true;

					var filter2 = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
					filter2.Property = "ORGADDRESS1_ADDRESS";
					filter2.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
					filter2.IsActive = true;

					Task.Factory.StartNew(() =>
					{
						filterControl.FirePerformSearch();
					}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();
					AssertEquals("The Background Validation should be started", true, filterControl.BackgroundValidationStatusIcon.Visible);

					Task.Factory.StartNew(() =>
					{
						filterControl.FirePerformSearch();
					}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();
					addressControl.AutoVerifyCanceled += delegate
					{
						AssertEquals("The Background Validation should be suspended", false, filterControl.BackgroundValidationStatusIcon.Visible);
					};
				}
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
				AddressValidationService.SetAvailableWebServiceAddress();
			}
		}

		[UseSnapshotProtection]
		public void TestShowMessageWhenRecoredNumberLargerThanRegistry()
		{
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_ValidationStatus = AddressValidationStatus.Invalid;

			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_ValidationStatus = AddressValidationStatus.Invalid;

			var orgAddress3 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress3.OA_ValidationStatus = AddressValidationStatus.Invalid;

			Factory.Save();

			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessages();
				AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);

				using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
				{
					using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
					{
						FilterControl.FirePerformSearch();
						AssertEquals(2, FilterControl.GridCollection.Count);
						AssertEquals("Should contain information message", "Found at least 3 records.\r\nOnly showing the top 2 records.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}

				UnitTestUserNotification.Instance.ClearMessages();

				using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
				{
					using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
					{
						FilterControl.FirePerformSearch();
						AssertEquals(2, FilterControl.GridCollection.Count);
						AssertEquals("Should contain information message", "Found 3 records.\r\nOnly showing the top 2 records.", UnitTestUserNotification.Instance.LastMessage.Text);
					}

					UnitTestUserNotification.Instance.ClearMessages();

					using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
					{
						FilterControl.FirePerformSearch();
						AssertEquals(3, FilterControl.GridCollection.Count);
						AssertEquals("Should not contain information message", null, UnitTestUserNotification.Instance.LastMessage.Text);

						var orgAddress4 = Factory.NewWithValidTestData<OrgAddress>();
						orgAddress4.OA_ValidationStatus = AddressValidationStatus.Invalid;
						Factory.Save();
						var orgAddress4View = Factory.Load<MDMAdminPanelAddressView>(orgAddress4.PK);
						AddressUserControl.Manager.AdminPanelProcessedAddressCollection.Add(orgAddress4View);
						FilterControl.FirePerformSearch();
						AssertEquals(3, FilterControl.GridCollection.Count);
						AssertContains($"The message should contain '{recordsExist}'", recordsExist, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}

				UnitTestUserNotification.Instance.ClearMessages();

				var codeFilter = FilterControl.FilterBusinessObject["Address1"] as ModuleTextFilter;
				codeFilter.IsActive = true;
				codeFilter.Property = "TESTDUP";
				codeFilter.ComparisonOperator = "starts with";
				FilterControl.FirePerformSearch();
				AssertEquals(0, FilterControl.GridCollection.Count);
				AssertEquals("Should contain information message", "Found no records.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddressSuggestionControlAfterRefreshGrid()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressSuggestionControl = form.UserControl.AddressDetailControl.AddressSuggestionControl;
				((IList)form.Manager.AdminPanelProcessedAddressCollection).Insert(0, GetMDMAddress());
				form.UserControl.ProcessedGrid.PerformMouseDownForTest(0, 1);
				var addressForValidation = (form.UserControl.ProcessedGrid.GetCurrent() as MDMAdminPanelAddressView).AddressEntity as OrgAddress;

				addressForValidation.ValidationStatus = AddressValidationStatus.Invalid;
				form.UserControl.RefreshGrid(null, new[] { addressForValidation.PK });
				AssertEquals(AddressValidationStatus.Invalid, addressForValidation.ValidationStatus);
				Assert("AddressSuggestionControl is enabled when validation status is Invalid.", addressSuggestionControl.Enabled);

				addressForValidation.ValidationStatus = AddressValidationStatus.ManuallyVerified;
				form.UserControl.RefreshGrid(null, new[] { addressForValidation.PK });
				AssertEquals(AddressValidationStatus.ManuallyVerified, addressForValidation.ValidationStatus);
				Assert("AddressSuggestionControl is enabled when validation status is ManuallyVerified.", addressSuggestionControl.Enabled);

				addressForValidation.ValidationStatus = AddressValidationStatus.Verified;
				form.UserControl.RefreshGrid(null, new[] { addressForValidation.PK });
				AssertEquals(AddressValidationStatus.Verified, addressForValidation.ValidationStatus);
				Assert("AddressSuggestionControl is disabled when validation status is Verified.", !addressSuggestionControl.Enabled);
			}
		}

		public void TestAddressDetailControlMustBeSynchronizedWhenSwitchDatasInOneGrid()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var addressView2 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var filterGrid = form.UserControl.FilterControl.FilteredGrid;
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView1);
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView2);

				filterGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(addressView2.PK, detailControl.AddressForValidation.EntityPK);

				filterGrid.PerformMouseDownForTest(1, 1);
				AssertEquals(addressView1.PK, detailControl.AddressForValidation.EntityPK);
			}
		}

		public void TestAddressDetailControlMustBeSynchronizedWhenSwitchDataBetweenTwoGrids()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var addressView2 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var filterGrid = form.UserControl.FilterControl.FilteredGrid;
				var processedGrid = form.UserControl.ProcessedGrid;
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView1);
				((IList)form.Manager.AdminPanelProcessedAddressCollection).Insert(0, addressView2);

				filterGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(addressView1.PK, detailControl.AddressForValidation.EntityPK);

				processedGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(addressView2.PK, detailControl.AddressForValidation.EntityPK);
			}
		}

		public void TestRecordInUnprocessedGridMustBeMovedIntoProcessedGridAfterEditing()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var saveButton = form.UserControl.AddressDetailControl.SaveButton;
				var filterGrid = form.UserControl.FilterControl.FilteredGrid;
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView1);

				filterGrid.PerformMouseDownForTest(0, 1);
				detailControl.Address1Control.Focus();
				detailControl.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				detailControl.Address2Control.Focus();

				saveButton.Focus();
				saveButton.PerformClick();

				AssertEquals(0, form.Manager.AdminPanelAddressCollection.Count);
				AssertEquals(1, form.Manager.AdminPanelProcessedAddressCollection.Count);
				AssertEquals(addressView1.PK, form.Manager.AdminPanelProcessedAddressCollection[0].PK);
			}
		}

		public void TestRecordinProcessedGridMustBeNotMovedAfterEditing()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var saveButton = form.UserControl.AddressDetailControl.SaveButton;
				var processedGrid = form.UserControl.ProcessedGrid;
				((IList)form.Manager.AdminPanelProcessedAddressCollection).Insert(0, addressView1);

				processedGrid.PerformMouseDownForTest(0, 1);
				detailControl.Address1Control.Focus();
				detailControl.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				detailControl.Address2Control.Focus();

				saveButton.Focus();
				saveButton.PerformClick();

				AssertEquals(0, form.Manager.AdminPanelAddressCollection.Count);
				AssertEquals(1, form.Manager.AdminPanelProcessedAddressCollection.Count);
				AssertEquals(addressView1.PK, form.Manager.AdminPanelProcessedAddressCollection[0].PK);
			}
		}

		MDMAdminPanelAddressView GetMDMAddress()
		{
			var collection = new MDMAdminPanelAddressCollection(Factory);
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress.OA_Address2 = "ORGADDRESS1_ADDRESS2";
			orgAddress.OA_PostCode = "210036";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_RN_NKCountryCode = "AU";
			var addressInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo.OAI_OA_Address = orgAddress.PK;
			addressInfo.OAI_IsPrimary = true;
			addressInfo.OAI_AdditionalInfo = "ADDITIONAL ADDRESS";
			orgAddress.OA_ValidationStatus = "INV";
			Factory.Save();

			collection.Load(new ZQuery(MDMAdminPanelAddressViewSchema.PK, orgAddress.PK));
			return collection[0];
		}

		public void TestBackgroudValidationListForJobDocAddress()
		{
			Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
			Env.Security.OrgAddressDetailsModify.IsAllowed = false;
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new ZForm())
			using (var addressControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
			{
				form.Controls.Add(addressControl);
				form.Show();

				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
				jobDocAddress.E2_Address1 = "NYVADDRESS";
				Factory.Save();

				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter.Property = "NYVADDRESS";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				var filter2 = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
				filter2.Property = AddressValidationStatus.ToBeVerified;
				filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter2.IsActive = true;

				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();

				AssertEquals(1, addressControl.backgroundValidationList.Count);
			}
		}

		public void TestBackgroudValidationListForOrgAddressAndIsMainAddress()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new ZForm())
			using (var addressControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
			{
				form.Controls.Add(addressControl);
				form.Show();

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var mainAddress = orgHeader.MainAddress;
				mainAddress.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
				mainAddress.OA_Address1 = "NYVADDRESS";
				Factory.Save();

				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter.Property = "NYVADDRESS";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				var filter2 = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
				filter2.Property = AddressValidationStatus.ToBeVerified;
				filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter2.IsActive = true;

				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();
				AssertEquals(0, addressControl.backgroundValidationList.Count);

				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();
				AssertEquals(1, addressControl.backgroundValidationList.Count);
			}
		}

		public void TestBackgroudValidationListForOrgAddressAndIsNotMainAddress()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new ZForm())
			using (var addressControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
			{
				form.Controls.Add(addressControl);
				form.Show();

				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
				address.OA_Address1 = "NYVADDRESS";
				Factory.Save();

				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter.Property = "NYVADDRESS";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				var filter2 = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
				filter2.Property = AddressValidationStatus.ToBeVerified;
				filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter2.IsActive = true;

				Env.Security.OrgAddressDetailsModify.IsAllowed = false;
				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();
				AssertEquals(0, addressControl.backgroundValidationList.Count);

				Env.Security.OrgAddressDetailsModify.IsAllowed = true;
				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();
				AssertEquals(1, addressControl.backgroundValidationList.Count);
			}
		}

		public void TestSaveButtonWhenDataItemChange()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;

			using (var form = new ZForm())
			using (var addressControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
			{
				form.Controls.Add(addressControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_PostCode = "210036";
				orgAddress1.OA_City = "LONDONDERRY";
				orgAddress1.OA_State = "NSW";
				orgAddress1.OA_RN_NKCountryCode = "AU";
				orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress1.OA_ValidationStatus = "INV";

				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress2.OA_Address1 = "ORGADDRESS2_ADDRESS1";
				orgAddress2.OA_PostCode = "210036";
				orgAddress2.OA_City = "LONDONDERRY";
				orgAddress2.OA_State = "NSW";
				orgAddress2.OA_RN_NKCountryCode = "AU";
				orgAddress2.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress2.OA_ValidationStatus = "INV";
				Factory.Save();

				var saveButton = addressControl.AddressDetailControl.SaveButton;
				var filterControl = addressControl.FilterControl;
				filterControl.FirePerformSearch();
				AssertEquals("The save button should be shown", true, saveButton.Visible);
				AssertEquals("The save button should not be enabled", false, saveButton.Enabled);

				filterControl.Grid.PerformMouseDownForTest(0, 1);
				addressControl.AddressDetailControl.Address2Control.Focus();
				addressControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
				addressControl.AddressDetailControl.Address1Control.Focus();
				AssertEquals("The save button should be enabled", true, saveButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				filterControl.Grid.PerformMouseDownForTest(1, 1);
				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The save button should not be enabled", false, saveButton.Enabled);

				filterControl.Grid.PerformMouseDownForTest(0, 1);
				AssertEquals("The save button should be enabled", true, saveButton.Enabled);
				AssertEquals("The text should be 'ORGADDRESS1_ADDRESS2'", "ORGADDRESS1_ADDRESS2", addressControl.AddressDetailControl.Address2Control.Text);

				saveButton.PerformClick();
				AssertEquals("The save button should not be enabled", false, saveButton.Enabled);

				addressControl.AddressDetailControl.AddressCodeControl.Focus();
				addressControl.AddressDetailControl.AddressCodeControl.Text = "TESTCODE";
				addressControl.AddressDetailControl.Address1Control.Focus();
				AssertEquals("The save button should be enabled", true, saveButton.Enabled);

				var savedOrgAddress1 = Factory.Load<OrgAddress>(orgAddress1.PK);
				AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1'", "ORGADDRESS1_ADDRESS1", savedOrgAddress1.OA_Address1);
				AssertEquals("The address2 should be 'ORGADDRESS1_ADDRESS2'", "ORGADDRESS1_ADDRESS2", savedOrgAddress1.OA_Address2);
			}
		}

		public void TestSaveDialogWhenDataItemChange_ClickCancelButton()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_ValidationStatus = "INV";

				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress2.OA_Address1 = "ORGADDRESS2_ADDRESS1";
				orgAddress2.OA_ValidationStatus = "INV";
				Factory.Save();

				FilterControl.FirePerformSearch();
				FilterControl.Grid.PerformMouseDownForTest(0, 1);
				AddressUserControl.AddressDetailControl.Address2Control.Focus();
				AddressUserControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
				AddressUserControl.AddressDetailControl.Address1Control.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				FilterControl.Grid.PerformMouseDownForTest(1, 1);
				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				var orgaddress = Factory.Load<OrgAddress>(orgAddress1.PK);
				AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1'", "ORGADDRESS1_ADDRESS1", orgaddress.OA_Address1);
				AssertEquals("The address2 should be empty", "", orgaddress.OA_Address2);
			}
		}

		public void TestSaveDialogWhenDataItemChange_ClickYesButton()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_PostCode = "210036";
				orgAddress1.OA_City = "LONDONDERRY";
				orgAddress1.OA_State = "NSW";
				orgAddress1.OA_RN_NKCountryCode = "AU";
				orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress1.OA_ValidationStatus = "INV";

				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress2.OA_Address1 = "ORGADDRESS2_ADDRESS1";
				orgAddress2.OA_PostCode = "210036";
				orgAddress2.OA_City = "LONDONDERRY";
				orgAddress2.OA_State = "NSW";
				orgAddress2.OA_RN_NKCountryCode = "AU";
				orgAddress2.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress2.OA_ValidationStatus = "INV";
				Factory.Save();

				FilterControl.FirePerformSearch();
				FilterControl.Grid.PerformMouseDownForTest(0, 1);
				AddressUserControl.AddressDetailControl.Address2Control.Focus();
				AddressUserControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
				AddressUserControl.AddressDetailControl.Address1Control.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				FilterControl.Grid.PerformMouseDownForTest(1, 1);
				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

				var orgaddress = Factory.Load<OrgAddress>(orgAddress1.PK);
				AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1'", "ORGADDRESS1_ADDRESS1", orgaddress.OA_Address1);
				AssertEquals("The address2 should be 'ORGADDRESS1_ADDRESS2'", "ORGADDRESS1_ADDRESS2", orgaddress.OA_Address2);
			}
		}

		public void TestSaveDialogWhenDataItemChange_ClickNoButton()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_ValidationStatus = "INV";

				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress2.OA_Address1 = "ORGADDRESS2_ADDRESS1";
				orgAddress2.OA_ValidationStatus = "INV";
				Factory.Save();

				FilterControl.FirePerformSearch();
				FilterControl.Grid.PerformMouseDownForTest(0, 1);
				AddressUserControl.AddressDetailControl.Address2Control.Focus();
				AddressUserControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
				AddressUserControl.AddressDetailControl.Address1Control.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				FilterControl.Grid.PerformMouseDownForTest(1, 1);

				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!AddressUserControl.PreviousDataItemView.HasChanges);
				Assert(!AddressUserControl.PreviousDataItemView.AddressEntity.HasChanges);

				FilterControl.Grid.PerformMouseDownForTest(0, 1);
				AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1'", "ORGADDRESS1_ADDRESS1", AddressUserControl.AddressDetailControl.Address1Control.Text);
				AssertEquals("The address2 should be empty", "", AddressUserControl.AddressDetailControl.Address2Control.Text);
			}
		}

		public void TestShouldSelectPreviousItemWhenSaveFailed()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_PostCode = "210036";
				orgAddress1.OA_City = "LONDONDERRY";
				orgAddress1.OA_State = "NSW";
				orgAddress1.OA_RN_NKCountryCode = "AU";
				orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress1.OA_ValidationStatus = "INV";

				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress2.OA_Address1 = "ORGADDRESS2_ADDRESS1";
				orgAddress2.OA_PostCode = "210036";
				orgAddress2.OA_City = "LONDONDERRY";
				orgAddress2.OA_State = "NSW";
				orgAddress2.OA_RN_NKCountryCode = "AU";
				orgAddress2.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress2.OA_ValidationStatus = "INV";
				Factory.Save();

				FilterControl.FirePerformSearch();
				FilterControl.Grid.PerformMouseDownForTest(0, 1);
				AddressUserControl.AddressDetailControl.Address1Control.Focus();
				AddressUserControl.AddressDetailControl.Address1Control.Text = "";
				AddressUserControl.AddressDetailControl.Address2Control.Focus();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				FilterControl.Grid.PerformMouseDownForTest(1, 1);
				AssertEquals("There are errors that need to be corrected before this Address (ORGADDRESS1_ADDRESS1) can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should select previous item.", FilterControl.Grid.CurrentRowIndex, 0);
			}
		}

		public void TestShouldPromptUserToSaveWhenSwitchDataInOneGrid()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var addressView2 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var filterGrid = form.UserControl.FilterControl.FilteredGrid;
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView1);
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView2);

				filterGrid.PerformMouseDownForTest(0, 1);
				detailControl.Address1Control.Focus();
				detailControl.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				detailControl.Address2Control.Focus();

				UnitTestUserNotification.Instance.ClearMessages();
				filterGrid.PerformMouseDownForTest(1, 1);

				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldPromptUserToSaveWhenSwitchDataBetweenTwoGrids()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var addressView2 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var filterGrid = form.UserControl.FilterControl.FilteredGrid;
				var processedGrid = form.UserControl.ProcessedGrid;
				((IList)form.Manager.AdminPanelAddressCollection).Insert(0, addressView1);
				((IList)form.Manager.AdminPanelProcessedAddressCollection).Insert(0, addressView2);

				filterGrid.PerformMouseDownForTest(0, 1);
				detailControl.Address1Control.Focus();
				detailControl.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				detailControl.Address2Control.Focus();

				UnitTestUserNotification.Instance.ClearMessages();
				processedGrid.PerformMouseDownForTest(0, 1);

				AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldNotPromptUserToSaveWhenSwitchDataBackFromAnEmptyGrid()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var processedGrid = form.UserControl.ProcessedGrid;
				((IList)form.Manager.AdminPanelProcessedAddressCollection).Insert(0, addressView1);

				processedGrid.PerformMouseDownForTest(0, 1);
				detailControl.Address1Control.Focus();
				detailControl.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				detailControl.Address2Control.Focus();

				form.UserControl.SetDataBindingForAddressDetailControl(false);
				UnitTestUserNotification.Instance.ClearMessages();
				processedGrid.PerformMouseDownForTest(0, 1);

				AssertNotContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldIgnoreValidationErrorsWhenAutoVerify()
		{
			using (var form = new ZForm())
			{
				form.Controls.Add(AddressUserControl);
				form.Show();

				var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress1.OA_Address2 = "ORGADDRESS1_ADDRESS2";
				orgAddress1.OA_PostCode = "210036";
				orgAddress1.OA_City = "BEIJING";
				orgAddress1.OA_RN_NKCountryCode = "CN";
				orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress1.OA_ValidationStatus = "INV";

				var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
				au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
				Factory.Save();

				FilterControl.FirePerformSearch();
				var addressView = AddressUserControl.AddressDetailControl.CurrentDataItemView;
				var address = addressView.AddressEntity as OrgAddress;
				address.OA_Address1 = "ORGADDRESS1_ADDRESS1_TEST";
				address.State = "STATE";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AddressUserControl.AddressDetailControl.SaveSingleRecord(addressView);
				AssertEquals("There are errors that need to be corrected before this Address (ORGADDRESS1_ADDRESS1) can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				var savedOrgAddress1 = Factory.Load<OrgAddress>(orgAddress1.PK);
				AssertNotEquals("The data is not saved.", "ORGADDRESS1_ADDRESS1_TEST", savedOrgAddress1.Address1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AddressUserControl.AddressDetailControl.SaveSingleRecord(addressView, true);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				savedOrgAddress1 = Factory.Load<OrgAddress>(orgAddress1.PK);
				AssertEquals("The data is saved.", "ORGADDRESS1_ADDRESS1_TEST", savedOrgAddress1.Address1);
			}
		}

		public void TestAutoVerifySkipRecordWhichIsBeingEdited()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new ZForm())
			using (var addressUserControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())))
			{
				form.Controls.Add(addressUserControl);
				form.Show();

				var addressList = new List<OrgAddress>();
				for (int i = 0; i < 3; i++)
				{
					var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
					orgAddress.OA_Address1 = "ORGADDRESS1_ADDRESS" + (i + 1);
					orgAddress.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
					addressList.Add(orgAddress);
				}

				Factory.Save();

				var filterControl = addressUserControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
				filter.Property = AddressValidationStatus.ToBeVerified;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				var address1Filter = filterControl.FilterBusinessObject["Address1"] as ModuleTextFilter;
				address1Filter.IsActive = true;
				address1Filter.Property = "ORGADDRESS1_ADDRESS";
				address1Filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

				Task.Factory.StartNew(() =>
				{
					filterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();

				var address = addressUserControl.Manager.AdminPanelAddressCollection[2];
				address.HasAddressInfoChanges = true;

				AssertEquals(addressList.Count, addressUserControl.Manager.AdminPanelAddressCollection.Count);
				AssertEquals("The BackgroundValidationStatusIcon should be shown", true, filterControl.BackgroundValidationStatusIcon.Visible);

				addressUserControl.AutoVerifyFinished += delegate
				{
					var skippedAddesses = addressUserControl.Manager.AdminPanelAddressCollection.Cast<MDMAdminPanelAddressView>().Where(x => x.AutoVerifyState == AddressAutoVerifyState.Skipped);
					AssertEquals(1, skippedAddesses.Count());
					AssertEquals(addressList.Count - 1, addressUserControl.Manager.AdminPanelProcessedAddressCollection.Count);
				};
			}
		}

		public void TestAutoVerifyAutoStart()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS";
			orgAddress1.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var registryItemValues = new List<bool>() { true, false };
			foreach (var value in registryItemValues)
			{
				using (SystemDataRegistry.Instance.BackgroundValidationSuspended.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				using (var form = new AdministrationPanelFormForTest(Factory))
				{
					form.Show();

					var userControl = form.UserControl;
					var filterControl = form.UserControl.FilterControl;
					var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
					filter.Property = AddressValidationStatus.ToBeVerified;
					filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					filter.IsActive = true;

					Task.Factory.StartNew(() =>
					{
						filterControl.FirePerformSearch();
					}, CancellationToken.None, TaskCreationOptions.None, new SynchronousTaskSchedulerForTest()).Wait();

					AssertEquals(value, !userControl.AutoVerifying);
					addressUserControl.CancelBackgroundValidation();
				}
			}
		}

		public void TestShouldRemoveAddressAutoVerifiedMarkAfterSaving()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			{
				form.Show();

				var addressView1 = GetMDMAddress();
				var detailControl = form.UserControl.AddressDetailControl;
				var processedGrid = form.UserControl.ProcessedGrid;
				((IList)form.Manager.AdminPanelProcessedAddressCollection).Insert(0, addressView1);

				processedGrid.PerformMouseDownForTest(0, 1);
				detailControl.Address1Control.Focus();
				detailControl.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				addressView1.AutoVerifyState = AddressAutoVerifyState.Verified;
				detailControl.Address2Control.Focus();

				detailControl.SaveButton.Focus();
				detailControl.SaveButton.PerformClick();
				AssertEquals(AddressAutoVerifyState.Waiting, addressView1.AutoVerifyState);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			FilterControl.Grid.Sort = AddressesFilterBusinessObject.AddressFilterConstants.AddressCode;
		}

		protected override void TearDown()
		{
			if (!AddressUserControl.IsDisposed)
			{
				AddressUserControl.Dispose();
			}
		}

		FilterStripBusinessObject FilterBusinessObject => FilterControl?.FilterBusinessObject;
		AddressesFilterControl FilterControl => AddressUserControl?.FilterControl;
		AddressUserControlForTest AddressUserControl => addressUserControl ?? (addressUserControl = new AddressUserControlForTest(new AdministrationPanelManager(new BusinessObjectFactory())));
		AddressUserControlForTest addressUserControl;
	}
}
