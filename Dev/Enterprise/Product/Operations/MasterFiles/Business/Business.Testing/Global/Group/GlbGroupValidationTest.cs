using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GlbGroupValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation

		public void TestValidateDomainName()
		{
			// No domains in the registry
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Sheriarty";
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			group.DomainName = "";
			AssertNoErrors(group.DomainNameInfo);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			group.DomainName = "";
			AssertNoErrors(group.DomainNameInfo);

			//No domains in the registry but domain is set on the group
			group.DomainName = "fake.domain";
			AssertNoErrors(group.DomainNameInfo);

			// 1 domain
			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			var aDRegistry = ObjectFactory.Get<IADRegistry>();
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			group.DomainName = "";
			AssertNoErrors(group.DomainNameInfo);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			group.DomainName = "";
			AssertNoErrors(group.DomainNameInfo);

			// 2 domains
			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2 };
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			// AD Enabled with User Only
			ObjectFactory.Get<IADRegistry>().EntitiesToSync = EntitiesToSync.UsersOnly;
			group.DomainName = "";
			AssertNoErrors(group.DomainNameInfo);
			// AD Enabled with User And Group
			ObjectFactory.Get<IADRegistry>().EntitiesToSync = EntitiesToSync.UsersAndGroups;
			group.DomainName = "";
			AssertHasWarning(group.DomainNameInfo, "The default domain will be used during the next synchronization with Active Directory.");
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			group.DomainName = "";
			AssertHasWarning(group.DomainNameInfo, "The default domain will be used during the next synchronization with Active Directory.");

			group.DomainName = "something";
			AssertHasError(group.DomainNameInfo, "Enter a valid Domain Name.");

			group.DomainName = "domain1";
			AssertNoErrors(group.DomainNameInfo);

			group.DomainName = "domain2";
			AssertNoErrors(group.DomainNameInfo);

			//system group should be allowed not having a domain name
			var sysGroup = Factory.New<GlbGroup>();
			sysGroup.GG_IsSystemDefined = true;
			sysGroup.GG_DomainName = "";
			AssertNoErrors(sysGroup.DomainNameInfo);
		}

		public void TestValidateGG_Desc_ADEnabled_ShouldBeUnique()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().GroupNamePrefix = string.Empty;

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Desc = "Sheriarty";
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Desc = "Sheriarty";

			AssertHasError(group2.GG_DescInfo, "The Group Description has been duplicated and must be unique.");
		}

		public void TestValidateGG_Desc_ADDisabled()
		{
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Desc = "Sheriarty";
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Desc = "Sheriarty";

			AssertNoErrors(group2.GG_DescInfo);
		}

		public void TestValidateGG_Desc_ADEnabledButNoPrefixInRegistry()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().GroupNamePrefix = string.Empty;

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Sheriarty";

			AssertNoErrors(group.GG_DescInfo);
		}

		public void TestValidateGG_Desc_ADEnabledLoginNamePrefixInRegistry()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().GroupNamePrefix = "EDI-PRD-";

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Sheriarty";

			AssertHasError(group.GG_DescInfo, "Description must begin with 'EDI-PRD-'.");

			group.GG_Desc = "EDI-prd-Sheriarty";
			AssertNoErrors(group.GG_DescInfo);
		}

		public void TestValidateGG_Desc_ADEnabledLoginNamePrefixInRegistry_SystemGroup()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().GroupNamePrefix = "EDI-PRD-";

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Sheriarty";
			AssertHasError(group.GG_DescInfo, "Description must begin with 'EDI-PRD-'.");

			group.GG_IsSystemDefined = true;
			group.RunPreSaveValidation();
			AssertNoErrors(group.GG_DescInfo);
		}

		public void TestValidateGG_Code()
		{
			Group.GG_Code = "";
			Assert("GG_Code is empty, expecting error", Group.GG_CodeInfo.HasErrors());

			Group.GG_Code = "ABC";
			Assert("GG_Code is not empty, not expecting error", !Group.GG_CodeInfo.HasNotifications());
			Factory.Save();

			GlbGroup group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "ABC";
			Assert("GG_Code is not unique, expecting error", group2.GG_CodeInfo.HasErrors());
		}

		public void TestValidateGG_Desc()
		{
			Group.GG_Desc = "";
			Assert("GG_Desc is empty, expecting error", Group.GG_DescInfo.HasErrors());

			Group.GG_Desc = "ABC";
			Assert("GG_Desc is not empty, not expecting error", !Group.GG_DescInfo.HasNotifications());
		}

		public void TestValidateGG_Desc_ADEnabled_WithInvalidOUException()
		{
			var expectedMessage = string.Format(@"Cannot connect to Active Directory: Invalid OU: {0}.
Please contact your system administrator or try again later.", TestConstants.InvalidOU);
			CheckExceptionOnGG_Desc(expectedMessage, new InvalidOUException(TestConstants.InvalidOU));
		}

		public void TestValidateGG_Desc_ADEnabled_WithCOMException()
		{
			var expectedMessage = string.Format(@"Cannot connect to Active Directory: {0}.
Please contact your system administrator or try again later.", "blah");
			CheckExceptionOnGG_Desc(expectedMessage, new COMException("blah"));
		}

		public void TestValidateGG_Desc_ADEnabled_WithDirectoryServicesException()
		{
			CheckExceptionOnGG_Desc(@"Cannot connect to Active Directory: domain1.
Please contact your system administrator or try again later.",
				new DirectoryServicesException("domain1"));
		}

		void CheckExceptionOnGG_Desc(string expectedMessage, Exception exception)
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var adUser = new Mock<IADUser>();
			var adGroup = new Mock<IADEntity>();
			var adEntityProviderMock = new Mock<IADEntityProvider>();
			adEntityProviderMock.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			adEntityProviderMock.Setup(m => m.GetADGroup(It.IsAny<IGlbGroup>())).Returns(adGroup.Object);

			var directorySearcherProviderMock = new Mock<IDirectorySearcherProvider>(MockBehavior.Strict);
			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherProviderMock.Setup(a => a.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(directorySearcherMock.Object);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Managers");

			directorySearcherMock.Setup(a => a.FindGroup("Managers", null)).Throws(exception);

			using (ObjectFactory.Substitute(directorySearcherProviderMock.Object))
			{
				var existingGroup = Factory.NewWithValidTestData<GlbGroup>();
				existingGroup.GG_Desc = "test1";
				existingGroup.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				AssertNoError(existingGroup.GG_DescInfo, expectedMessage);

				Factory.Save();

				existingGroup.GG_Desc = "Managers";
				AssertHasError(existingGroup.GG_DescInfo, expectedMessage);
			}
		}

		public void TestValidateGG_IsActive()
		{
			Group.GG_IsActive = false;
			Group.Validation.ValidateAll();
			AssertEquals("Can deactive non-all group", false, Group.GG_IsActiveInfo.HasErrors());

			GlbGroup allGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);
			allGroup.GG_IsActive = false;
			AssertEquals("Cannot deactiveate all group", true, allGroup.GG_IsActiveInfo.HasErrors());
			allGroup.GG_IsActive = true;
			AssertEquals("Can activate all group", false, allGroup.GG_IsActiveInfo.HasErrors());
		}

		public void TestValidateGG_IsActive_ForADLinkedGroups()
		{
			Group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Group.GG_IsActive = true;
			AssertNoErrors(Group.GG_IsActiveInfo);

			Group.GG_IsActive = false;
			AssertHasError(Group.GG_IsActiveInfo, "Groups linked to Active Directory cannot be deactivated. The group should be disconnected from Active Directory first.");
		}

		public void TestValidateGG_IsActive_DeactivateWithStaffsAttached()
		{
			var group1 = Factory.New<GlbGroup>();
			var staff1 = group1.Staff.AddNew();
			group1.Staff.AddNew();
			group1.Staff.AddNew();
			group1.GG_IsActive = true;
			group1.RunPreSaveValidation();
			group1.Validation.ValidateGG_IsActive();
			AssertNoErrors(group1.GG_IsActiveInfo);

			group1.GG_IsActive = false;
			group1.Validation.ValidateGG_IsActive();
			AssertHasError(group1.GG_IsActiveInfo, "Members need to be Detached from the Group.");

			group1.Staff.Remove(staff1.PK);
			group1.GG_IsActive = false;
			group1.Validation.ValidateGG_IsActive();
			AssertHasError(group1.GG_IsActiveInfo, "Members need to be Detached from the Group.");

			group1.Staff.RemoveAll();
			group1.GG_IsActive = false;
			group1.Validation.ValidateGG_IsActive();
			AssertNoErrors(group1.GG_IsActiveInfo);
		}

		public void TestValidateGG_GC_NotMandatoryIfNotSalesTeam()
		{
			var team = Factory.NewWithValidTestData<GlbGroup>();
			team.IsGlobal = false;

			team.GG_GC = GlbCompany.CurrentCompany.PK;
			AssertMandatoryValidationError(team.GG_GCInfo, false);

			team.GG_GC = ZGuid.Empty;
			AssertMandatoryValidationError(team.GG_GCInfo, false);
		}

		public void TestValidateGG_GC_MandatoryIfNotGlobalAndSalesTeam()
		{
			var team = Factory.NewWithValidTestData<SalesTeam>();
			team.IsGlobal = false;

			team.GG_GC = GlbCompany.CurrentCompany.PK;
			AssertMandatoryValidationError(team.GG_GCInfo, false);

			team.GG_GC = ZGuid.Empty;
			AssertMandatoryValidationError(team.GG_GCInfo, true);

			team.IsGlobal = true;
			team.Validation.ValidateGG_GC();
			AssertMandatoryValidationError(team.GG_GCInfo, false);
		}

		public void TestValidateGlbStaffAddressIsValidForScheduleTaskRecipient()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			group.Staff.Add(staff2);

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			scheduleTaskRecipient.S6_GG = group.PK;

			Factory.Save();

			group.Validation.ValidateAll();
			AssertHasRowError(group, "Group members must have at least one valid email address because group is assigned to recipient of scheduled reports.");

			staff2.GS_EmailAddress = "test@test.com";
			Factory.Save();

			group.Validation.ValidateAll();
			AssertNoRowError(group, "Group members must have at least one valid email address because group is assigned to recipient of scheduled reports.");
		}

		public void TestValidateGG_Desc_ADEnabled_WithNameInConflict()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var adUser = new Mock<IADUser>();
			var adGroup = new Mock<IADEntity>();
			var adEntityProviderMock = new Mock<IADEntityProvider>();
			adEntityProviderMock.Setup(m => m.GetADUser(It.IsAny<IGlbStaff>())).Returns(adUser.Object);
			adEntityProviderMock.Setup(m => m.GetADGroup(It.IsAny<IGlbGroup>())).Returns(adGroup.Object);

			var directorySearcherProviderMock = new Mock<IDirectorySearcherProvider>(MockBehavior.Strict);
			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherProviderMock.Setup(a => a.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(directorySearcherMock.Object);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Managers");

			directorySearcherMock.Setup(a => a.FindUser("Elijah.Wood", null)).Returns(DummyDirectoryEntryWrapper.CreateUser("Elijah.Wood"));
			directorySearcherMock.Setup(a => a.FindGroup("Managers", null)).Returns(directoryEntry);
			directorySearcherMock.Setup(a => a.FindGroup("Another.Group", null)).Returns(DummyDirectoryEntryWrapper.CreateGroup("Another.Group"));

			using (ObjectFactory.Substitute(directorySearcherProviderMock.Object))
			{
				var existingGroup = Factory.NewWithValidTestData<GlbGroup>();
				existingGroup.GG_Desc = "test2";
				existingGroup.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				Factory.Save();

				// Conflict with linked group
				var newGroup1 = Factory.New<GlbGroup>();
				newGroup1.GG_Desc = "Managers";
				AssertHasError(newGroup1.GG_DescInfo, "Description already used in Active Directory and linked to another object.");

				//No conflict with not linked but existing group
				newGroup1.GG_Desc = "Another.Group";
				AssertNoErrors(newGroup1.GG_DescInfo);

				//No Conflict with non-existing group
				newGroup1.GG_Desc = "Test";
				AssertNoErrors(newGroup1.GG_DescInfo);

				//Conflict with user existing in AD
				var newGroup2 = Factory.New<GlbGroup>();
				newGroup2.GG_Desc = "Elijah.Wood";
				AssertHasError(newGroup2.GG_DescInfo, "Description already used in Active Directory and linked to another object.");

				// No conflict
				newGroup2.GG_Desc = "New.Group";
				AssertNoErrors(newGroup2.GG_DescInfo);
			}
		}

		public void TestValidateGG_Type()
		{
			var group = GetNewBusinessObject();
			AssertEquals("Precondition", GlbGroupTypeList.Codes.Staff, group.GG_Type);
			AssertNoErrors("Default should have no errors", group.GG_TypeInfo);

			group.GG_Type = string.Empty;
			AssertHasError(group.GG_TypeInfo, "Please enter a value.");

			group.GG_Type = GlbGroupTypeList.Codes.Staff;
			AssertNoErrors("Staff should be valid", group.GG_TypeInfo);

			group.GG_Type = "AAA";
			AssertHasError(group.GG_TypeInfo, "Enter a valid selection.");

			group.GG_Type = GlbGroupTypeList.Codes.Organisation;

			if (IsContactTypeAllowed)
			{
				AssertNoErrors("Contact should be valid", group.GG_TypeInfo);
			}
			else
			{
				AssertHasError(group.GG_TypeInfo, "Enter a valid selection.");
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Group = Factory.New<GlbGroup>();
		}

		GlbGroup Group;

		protected virtual bool IsContactTypeAllowed => true;

		protected virtual GlbGroup GetNewBusinessObject()
		{
			return Factory.New<GlbGroup>();
		}

		#endregion
	}
}
