using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestGlbBranchValidation : BusinessObjectValidationTestCase
	{
		#region ValidateGB_Code

		public void TestValidateGB_Code()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			GlbBranch branch1 = company.Branches.AddNew();

			branch1.GB_Code = "";
			Assert("Error expected - Branch Code is required", branch1.GB_CodeInfo.HasErrors());
			branch1.GB_Code = "AB";
			Assert("Error expected - The length of Branch Code should be 3", branch1.GB_CodeInfo.HasErrors());
			branch1.GB_Code = "ABC";
			Assert("No error expected", !branch1.GB_CodeInfo.HasErrors());

			GlbBranch branch2 = company.Branches.AddNew();
			branch2.GB_Code = "ABC";
			branch1.Validation.ValidateGB_Code();
			Assert("Error expected - Branch Code should be unique for a Company", branch1.GB_CodeInfo.HasErrors());
			Assert("Error expected - Branch Code should be unique for a Company", branch2.GB_CodeInfo.HasErrors());

			branch2.GB_Code = "EFG";
			branch1.Validation.ValidateGB_Code();
			Assert("No error expected", !branch1.GB_CodeInfo.HasErrors());
			Assert("No error expected", !branch2.GB_CodeInfo.HasErrors());
		}

		#endregion

		#region ValidateGB_IsActive

		string InsertCountAndCsvTasks(string taskName, ICollection<IStmScheduleTask> tasks, INotificationType notificationType)
		{
			var tasksList = GlbBranchValidation.ServiceTaskNameSeperator + String.Join(GlbBranchValidation.ServiceTaskNameSeperator, tasks.Select(task => task.S5_ScheduleDescription));

			if (notificationType == CargoWise.ComponentModel.NotificationType.Error)
			{
				return String.Format("This branch has {0} active {1}. Please amend the {1} to a new branch, deactivate, or delete them before deactivating this branch.\r\nThe active {1} assigned to this branch are {2}", tasks.Count, taskName, tasksList);
			}
			else if (notificationType == CargoWise.ComponentModel.NotificationType.Warning)
			{
				return String.Format("This branch has {0} inactive {1}. You may wish to amend the {1} to a new branch, or delete them before deactivating this branch.\r\nThe inactive {1} assigned to this branch are {2}", tasks.Count, taskName, tasksList);
			}
			else
			{
				throw new ArgumentException(nameof(notificationType) + " should be either Error or Warning.");
			}
		}

		public void TestValidateGb_IsActive_ServiceTasks()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertCannotDeactivateWithTasks<IServiceTaskSchedule>("service tasks", Constants.ServiceTask.ParentTableCode, allowInactiveServiceTasks: true);
			}
		}

		public void TestValidateGb_IsActive_NewServiceTasks()
		{
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertCannotDeactivateWithStmServiceTasks("service tasks");
			}
		}

		public void TestValidateGB_IsActive_ScheduleReports()
		{
			AssertCannotDeactivateWithTasks<IReportScheduleTask>("scheduled reports", Constants.ReportSchedule.ParentTableCode);
		}

		public void TestValidateGB_IsActive_WithActiveStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_GB_HomeBranch = branch.PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_GB_HomeBranch = branch.PK;

			Factory.Save();

			var error = "This branch has 2 active staff. Please amend the staff to a new Home Branch, deactivate them, or delete them before deactivating this branch.";

			branch.GB_IsActive = true;
			AssertNoError(branch.GB_IsActiveInfo, error);

			branch.GB_IsActive = false;
			AssertHasError(branch.GB_IsActiveInfo, error);
		}

		public void TestValidateGB_IsActive_WithInActiveStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_IsActive = false;

			Factory.Save();

			branch.GB_IsActive = false;
			var notification = "This branch has 1 inactive staff. You may wish to amend the staff to a new Home Branch, or delete them before deactivating this branch.";

			AssertHasWarning(branch.GB_IsActiveInfo, notification);
		}

		public void TestValidateGB_IsActive_WithNoStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			Factory.Save();

			branch.GB_IsActive = false;

			AssertNoNotifications(branch.GB_IsActiveInfo);
		}

		public void TestValidateGB_IsActive_ArchiveManagerSchedules()
		{
			AssertCannotDeactivateWithTasks<IArchiveScheduleTask>("archive manager schedules", Constants.ArchiveManager.ParentTableCode);
		}

		public void TestValidateGB_IsActive_UniversalCopy()
		{
			AssertCannotDeactivateWithTasks<IUniversalCopyScheduleTask>("universal copy schedules", Constants.ScheduleUniversalCopyTask.ParentTableCode);
		}

		void AssertCannotDeactivateWithTasks<T>(string taskName, string parentTableCode, bool allowInactiveServiceTasks = false) where T : IStmScheduleTask
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchWithTasks = company.Branches.AddNew();
			branchWithTasks.GB_Code = "ABC";

			var tasks = Enumerable.Range(0, 5).Select(i =>
			{
				var task = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<T>());
				task.S5_ParentTableCode = parentTableCode;
				task.S5_GB = branchWithTasks.PK;
				task.S5_ScheduleDescription = "Task " + i;
				return task;
			}).ToList();

			Factory.Save();

			var notificationMessage = InsertCountAndCsvTasks(taskName, tasks, CargoWise.ComponentModel.NotificationType.Error);

			branchWithTasks.GB_IsActive = true;
			AssertNoError(branchWithTasks.GB_IsActiveInfo, notificationMessage);

			branchWithTasks.GB_IsActive = false;
			AssertHasError(branchWithTasks.GB_IsActiveInfo, notificationMessage);

			if (allowInactiveServiceTasks)
			{
				tasks.ForEach(task => task.S5_IsActive = false);
				Factory.Save();

				branchWithTasks.GB_IsActive = false;
				notificationMessage = InsertCountAndCsvTasks(taskName, tasks, CargoWise.ComponentModel.NotificationType.Warning);
				AssertHasWarning(branchWithTasks.GB_IsActiveInfo, notificationMessage);
			}
		}

		void AssertCannotDeactivateWithStmServiceTasks(string taskName)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchWithTasks = company.Branches.AddNew();
			branchWithTasks.GB_Code = "ABC";

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var tasks = Enumerable.Range(0, 5).Select(i =>
				{
					var task = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmServiceTaskBranchValidationAdapter>());
					task.S5_ParentTableCode = "SST";
					task.S5_GB = branchWithTasks.PK;
					task.S5_ScheduleDescription = "Task " + i;
					return task;
				}).ToList();

				Factory.Save();

				var notificationMessage =
					InsertCountAndCsvTasks(taskName, tasks, CargoWise.ComponentModel.NotificationType.Error);

				branchWithTasks.GB_IsActive = true;
				AssertNoError(branchWithTasks.GB_IsActiveInfo, notificationMessage);

				branchWithTasks.GB_IsActive = false;
				AssertHasError(branchWithTasks.GB_IsActiveInfo, notificationMessage);

				tasks.ForEach(task => task.S5_IsActive = false);
				Factory.Save();

				branchWithTasks.GB_IsActive = false;
				notificationMessage = InsertCountAndCsvTasks(taskName, tasks,
					CargoWise.ComponentModel.NotificationType.Warning);
				AssertHasWarning(branchWithTasks.GB_IsActiveInfo, notificationMessage);
			}
		}

		public void TestValidateGB_IsActive()
		{
			var branches = Factory.Load<GlbBranch>(new ZQuery());
			branches.ToList().ForEach(branch => branch.GB_IsActive = false);

			var company1 = Factory.New<GlbCompany>();

			var branch1 = company1.Branches.AddNew();
			branch1.GB_IsActive = true;
			branch1.GB_Code = "UT1";
			branch1.GB_BranchName = "Test1";

			var branch2 = company1.Branches.AddNew();
			branch2.GB_IsActive = true;
			branch2.GB_Code = "UT2";
			branch2.GB_BranchName = "Test2";

			Factory.Save();

			branch1.GB_IsActive = false;
			Assert("No Error expected", !branch1.GB_IsActiveInfo.HasErrors());

			branch2.GB_IsActive = false;
			Assert("Error expected - Last active branch cannot be deactivated", branch2.GB_IsActiveInfo.HasErrors());
		}

		public void TestValidateGB_IsActive_WebBranch()
		{
			var currentWebBranch = DataRegistry.Instance.WebBranch;
			try
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				Factory.Save();
				DataRegistry.Instance.WebBranch = branch.PK.ToGuid();

				branch.GB_IsActive = false;
				AssertHasError(branch.GB_IsActiveInfo, "This branch is the web branch and cannot be deactivated. Please change the branch under Registry > Web > Web Branch in order to deactivate this branch.");
			}
			finally
			{
				DataRegistry.Instance.WebBranch = currentWebBranch;
			}
		}

		public void TestValidateGB_IsActive_WebBranchDefault()
		{
			var currentWebBranch = DataRegistry.Instance.WebBranch;
			var branch = Factory.Load<GlbBranch>(currentWebBranch);

			branch.GB_IsActive = false;
			AssertNoErrors(branch.GB_IsActiveInfo);
		}

		#endregion

		#region ValidateBranchName

		public void TestValidateBranchName()
		{
			Branch.GB_BranchName = "";
			Assert("Error expected - Branch Name is required", Branch.GB_BranchNameInfo.HasErrors());
			Branch.GB_BranchName = "name";
			Assert("No error expected", !Branch.GB_BranchNameInfo.HasErrors());
		}

		#endregion

		#region ValidateAddresses

		public void TestValidateAddress1()
		{
			Branch.GB_Address1 = "address1";
			Assert("No error expected", !Branch.GB_Address1Info.HasErrors());
			Branch.GB_Address1 = "";
			Assert("Error expected - Branch Address1 is required", Branch.GB_Address1Info.HasErrors());
		}

		public void TestGC_ValidationStatus()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_ValidationStatus = AddressValidationStatus.Invalid;
			AssertHasError(branch.GB_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			branch.GB_ValidationStatus = AddressValidationStatus.Verified;
			AssertNoError(branch.GB_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

			Env.Instance.Registry.EnableAddressValidationWebService = false;
			branch.GB_ValidationStatus = AddressValidationStatus.Invalid;
			AssertNoError(branch.GB_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
		}

		public void TestNoNeedToValidateInactiveAddresses()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				var branch = Factory.New<GlbBranch>();
				branch.GB_IsActive = true;
				branch.GB_RN_NKCountryCode = "AU";
				branch.GB_ValidationStatus = AddressValidationStatus.Invalid;
				AssertHasError(branch.GB_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				branch.ClearAllNotifications();

				branch.GB_IsActive = false;
				branch.Validation.ValidateGB_ValidationStatus();
				AssertNoError(branch.GB_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			}

			ErrorReporter.Clear();
		}

		#endregion

		#region ValidateCity

		public void TestValidateCityWhenShouldValidateAddressIsFalse()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			Branch.GB_City = "city";
			Assert("No error expected", !Branch.GB_CityInfo.HasErrors());

			Branch.GB_City = ZString.Empty;
			Assert("Error expected - Branch City is required", Branch.GB_CityInfo.HasErrors());
		}

		public void TestMandatoryValidationAppliesForCityWhenManuallyVerifying()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			Branch.GB_City = "Sydney";
			Branch.ValidationStatus = AddressValidationStatus.Verified;
			Branch.Validation.ValidateGB_City();
			AssertNoError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.Verified;
			Branch.Validation.ValidateGB_City();
			AssertNoError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = "Sydney";
			Branch.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			Branch.Validation.ValidateGB_City();
			AssertNoError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			Branch.Validation.ValidateGB_City();
			AssertNoError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = "Sydney";
			Branch.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Branch.Validation.ValidateGB_City();
			AssertNoError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Branch.Validation.ValidateGB_City();
			AssertHasError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = "Sydney";
			Branch.ValidationStatus = AddressValidationStatus.ToBeVerified;
			Branch.Validation.ValidateGB_City();
			AssertNoError(Branch.GB_CityInfo, "Please enter a City.");

			Branch.GB_City = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.ToBeVerified;
			Branch.Validation.ValidateGB_City();
			AssertHasError(Branch.GB_CityInfo, "Please enter a City.");
		}

		#endregion

		#region Validate Phone Numbers

		public void TestGB_Phone_Formatted()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";

			Branch.GB_GC = company.PK;

			Branch.GB_Phone_Formatted = "";
			Assert("Error expected - Branch Phone is required", Branch.GB_Phone_FormattedInfo.HasErrors());

			Branch.GB_RN_NKCountryCode = "CN";
			Branch.GB_Phone_Formatted = "156 0113 1981";
			AssertNoNotifications("Valid local number, no notifications expected.", Branch.GB_Phone_FormattedInfo);

			Branch.GB_RN_NKCountryCode = string.Empty;
			Branch.GB_Phone_Formatted = "156 0113 1982";
			AssertHasErrors("Empty country code. Error expected", Branch.GB_Phone_FormattedInfo);
		}

		public void TestGB_Fax_Formatted()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";

			Branch.GB_GC = company.PK;

			Branch.GB_RN_NKCountryCode = "CN";
			Branch.GB_Fax_Formatted = "156 0113 1981";
			AssertNoNotifications("Valid local number, no notifications expected.", Branch.GB_Fax_FormattedInfo);

			Branch.GB_RN_NKCountryCode = string.Empty;
			Branch.GB_Fax_Formatted = "156 0113 1982";
			AssertHasErrors("Empty country code. Error expected", Branch.GB_Fax_FormattedInfo);
		}

		public void TestGB_Phone_DoesNotValidateWhenInactive()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";

			Branch.GB_GC = company.PK;
			Branch.GB_IsActive = false;
			Branch.GB_Phone_Formatted = string.Empty;
			AssertEquals("No Error expected - Branch is Inactive", false, Branch.GB_Phone_FormattedInfo.HasErrors());

			Branch.GB_Phone_Formatted = "ZZZ";
			AssertEquals("No Error expected - Branch is Inactive", false, Branch.GB_Phone_FormattedInfo.HasErrors());

			Branch.GB_IsActive = true;
			Branch.GB_Phone_Formatted = String.Empty;
			AssertEquals("Error expected - Phone number must not be empty when active", true, Branch.GB_Phone_FormattedInfo.HasErrors());

			Branch.GB_Phone_Formatted = "ZZZ";
			AssertEquals("Error expected - Company is Active", true, Branch.GB_Phone_FormattedInfo.HasErrors());

			Branch.GB_Phone_Formatted = "0455555555";
			AssertEquals("No Error expected - Phone is valid", false, Branch.GB_Phone_FormattedInfo.HasErrors());
		}

		#endregion

		#region ValidateGB_RL_NKHomePort

		public void TestValidateGB_RL_NKHomePort()
		{
			var arbitraryCode = (Factory.LoadTop1<RefUNLOCO>(new ZQuery())).RL_Code;

			Branch.GB_RL_NKHomePort = "";
			Assert("Error expected - Branch Home Port is required", Branch.GB_RL_NKHomePortInfo.HasErrors());

			Branch.GB_RL_NKHomePort = "RRTTT";
			Assert("Error expected - invalid port", Branch.GB_RL_NKHomePortInfo.HasErrors());

			Branch.GB_RL_NKHomePort = arbitraryCode;
			Assert("Error expected - valid port", !Branch.GB_RL_NKHomePortInfo.HasErrors());

			var deniedStaff = new BusinessObjectFactory().NewWithValidTestData<GlbStaff>();
			deniedStaff.GS_IsOperational = true;
			deniedStaff.GS_IsActive = true;
			deniedStaff.Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(deniedStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var branch2 = (Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.NotEqual, arbitraryCode)));
				branch2.Validation.ValidateGB_RL_NKHomePort();
				Assert("No errors if already valid, unchanged and security right denied", !branch2.GB_RL_NKHomePortInfo.HasErrors());
				var originalValue = branch2.GB_RL_NKHomePort;
				branch2.GB_RL_NKHomePort = "ASFGH";
				branch2.GB_RL_NKHomePort = originalValue;
				Assert("No errors if already valid, unchanged and security right denied", !branch2.GB_RL_NKHomePortInfo.HasErrors());
				branch2.GB_RL_NKHomePort = arbitraryCode;
				Assert("Error expected", branch2.GB_RL_NKHomePortInfo.HasErrors());
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}
		}

		public void TestCheckHomePortBelongsToChosenCountry()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			Branch.GB_GC = company.PK;

			Branch.Validation.ValidateGB_RL_NKHomePort();
			AssertNoWarnings(Branch.GB_RL_NKHomePortInfo);

			company.GC_RN_NKCountryCode = "NZ";
			Branch.Validation.ValidateGB_RL_NKHomePort();
			AssertNoWarnings(Branch.GB_RL_NKHomePortInfo);

			company.GC_RN_NKCountryCode = "";
			Branch.GB_RL_NKHomePort = "AUSYD";
			Branch.Validation.ValidateGB_RL_NKHomePort();
			AssertNoWarnings(Branch.GB_RL_NKHomePortInfo);

			company.GC_RN_NKCountryCode = "AU";
			Branch.GB_RL_NKHomePort = "AUSYD";
			Branch.Validation.ValidateGB_RL_NKHomePort();
			AssertNoWarnings(Branch.GB_RL_NKHomePortInfo);

			company.GC_RN_NKCountryCode = "NZ";
			Branch.GB_RL_NKHomePort = "AUSYD";
			Branch.Validation.ValidateGB_RL_NKHomePort();
			string wrnMsg = String.Format("The Home Port entered belongs to a different country/region to the country/region code entered on the company specified. Please ensure this is correct. If the home port is correct It is strongly recommend that you create a company for {0} as accounting information will be entered against this company", Branch.HomePort.RL_RN_NKCountryCode);
			AssertHasWarning(Branch.GB_RL_NKHomePortInfo, wrnMsg);
		}

		#endregion

		#region ValidateGB_GC

		public void TestValidateGB_GC()
		{
			Branch.GB_GC = ZGuid.Empty;
			Branch.Validation.ValidateGB_GC();
			Assert("Error expected - Company is required", Branch.GB_GCInfo.HasErrors());
			Branch.GB_GC = ZGuid.Invalid;
			Assert("Error expected - Company is not valid", Branch.GB_GCInfo.HasErrors());
			Branch.GB_GC = ZGuid.NewZGuid();
			Assert("No error expected", !Branch.GB_GCInfo.HasErrors());
		}

		#endregion

		#region ValidateGB_AccountingGroupCode

		public void TestValidateGB_AccountingGroupCode()
		{
			Branch.GB_AccountingGroupCode = "BRA";
			Branch.Validation.ValidateGB_AccountingGroupCode();
			AssertEquals("Error expected - Invalid code", true, Branch.GB_AccountingGroupCodeInfo.HasErrors());

			var newCodeCollection = new BranchManagementCodeDescriptionBoolCollection();
			var code1 = newCodeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCodeCollection);

			Branch.GB_AccountingGroupCode = "BRB";
			Branch.Validation.ValidateGB_AccountingGroupCode();
			AssertEquals("No error expected - Valid code", false, Branch.GB_AccountingGroupCodeInfo.HasErrors());
		}

		#endregion

		#region ValidateGB_OH

		public void TestValidateGB_OH()
		{
			Branch.GB_OH_OrgProxy = ZGuid.Empty;
			Assert("No error expected", !Branch.GB_OH_OrgProxyInfo.HasErrors());
			Branch.GB_OH_OrgProxy = ZGuid.Invalid;
			Assert("Error expected - Organisation is not valid", Branch.GB_OH_OrgProxyInfo.HasErrors());
			Branch.GB_OH_OrgProxy = ZGuid.NewZGuid();
			Assert("No error expected", !Branch.GB_OH_OrgProxyInfo.HasErrors());
		}

		#endregion

		#region ValidationGB_PostCode

		public void TestValidationGB_PostCodeInCasesCountrySpecificRulesApplying()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			Branch.GB_RN_NKCountryCode = Constants.CountryCodes.Australia;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			Branch.GB_PostCode = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.Verified;
			Branch.Validation.ValidateGB_PostCode();
			AssertNoErrors(Branch.GB_PostCodeInfo);

			Branch.GB_PostCode = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Branch.Validation.ValidateGB_PostCode();
			AssertHasErrors(Branch.GB_PostCodeInfo);

			Branch.GB_PostCode = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.Unverifiable;
			Branch.Validation.ValidateGB_PostCode();
			AssertHasErrors(Branch.GB_PostCodeInfo);

			Branch.GB_PostCode = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.CountryNotAvailable;
			Branch.Validation.ValidateGB_PostCode();
			AssertHasErrors(Branch.GB_PostCodeInfo);
		}

		#endregion

		#region ValidationGB_State

		public void TestValidationGB_StateInCasesCountrySpecificRulesApplying()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			Branch.GB_RN_NKCountryCode = Constants.CountryCodes.Australia;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

			Branch.GB_State = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.Verified;
			Branch.Validation.ValidateGB_State();
			AssertNoErrors(Branch.GB_StateInfo);

			Branch.GB_State = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Branch.Validation.ValidateGB_State();
			AssertHasErrors(Branch.GB_StateInfo);

			Branch.GB_State = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.Unverifiable;
			Branch.Validation.ValidateGB_State();
			AssertHasErrors(Branch.GB_StateInfo);

			Branch.GB_State = ZString.Empty;
			Branch.ValidationStatus = AddressValidationStatus.CountryNotAvailable;
			Branch.Validation.ValidateGB_State();
			AssertHasErrors(Branch.GB_StateInfo);
		}

		#endregion

		#region GB_Email

		public void TestGB_Email()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Email = "test@test.com";
			AssertNoErrors("Email is fine", branch.GB_EmailInfo);

			branch.GB_Email = "invalid";
			AssertHasErrors("Email is not required, but was entered invalid", branch.GB_EmailInfo);

			branch.GB_Email = "";
			AssertNoErrors("Email is not required, and is empty", branch.GB_EmailInfo);
		}

		#endregion

		#region GB_WebAddress

		public void TestGB_WebAddress()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_WebAddress = "www.cargowise.com";
			AssertNoErrors(branch.GB_WebAddressInfo);

			branch.GB_WebAddress = "invalid";
			AssertHasErrors(branch.GB_WebAddressInfo);

			branch.GB_WebAddress = "";
			AssertNoErrors(branch.GB_WebAddressInfo);
		}

		#endregion

		#region Implementation

		ZGuid InitialProxyOrgPK;

		GlbBranch branch;
		GlbBranch Branch
		{
			get { return branch ?? (branch = Factory.New<GlbBranch>()); }
		}

		protected override void SetUp()
		{
			InitialProxyOrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = InitialProxyOrgPK;
		}

		#endregion
	}
}
