using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GenApprovalRequestTest<RequestType> : EnterpriseBusinessObjectTestCase
			where RequestType : GenApprovalRequest
	{
		public void TestCreatedUserNotNull()
		{
			var currentUser = Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentUser;
			var factory = new BusinessObjectFactory();
			var requestInFactory = (RequestType)GetNewBusinessObjectForDeleteTest(factory);
			factory.Save();

			var reloadFromFactory = (new BusinessObjectFactory()).Load<RequestType>(requestInFactory.PK);

			AssertNotNull(reloadFromFactory.CreatedUser);
			AssertEquals(reloadFromFactory.CreatedUser_FullName, currentUser.FullName);
		}

		public void TestCreatedTimeLocal()
		{
			var currentUser = Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentUser;
			var factory = new BusinessObjectFactory();
			var requestInFactory = (RequestType)GetNewBusinessObjectForDeleteTest(factory);
			var time = ZDateTime.Now;
			requestInFactory.XP_SystemCreateTimeUtc = time;
			factory.Save();

			AssertEquals(requestInFactory.CreatedTimeLocal, time.ToLocalBranchTime());
		}

		public void TestApprovedUser_FullName()
		{
			var currentUser = Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentUser;
			var factory = new BusinessObjectFactory();
			var requestInFactory = (RequestType)GetNewBusinessObjectForDeleteTest(factory);
			factory.Save();

			AssertEquals(ZString.Empty, requestInFactory.ApprovedUser_FullName);

			requestInFactory.XP_GS_NKApprovingUser1 = Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentUser.Initials;

			AssertEquals(currentUser.FullName, requestInFactory.ApprovedUser_FullName);
		}

		public void TestApprovalStatusConcurency()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var requestInFactory1 = (RequestType)GetNewBusinessObjectForDeleteTest(factory1);
			factory1.Save();

			var requestInFactory2 = factory2.Load<RequestType>(requestInFactory1.PK);
			requestInFactory2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			factory2.Save();

			requestInFactory1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
		}

		public void TestHumanReadableNameCore()
		{
			var requestInFactory = (RequestType)GetNewBusinessObject();
			requestInFactory.XP_RequestID = "One, Two, Three";

			AssertEquals("Approval Request - One, Two, Three", requestInFactory.HumanReadableName);
		}

		public void TestGetApprovingUsers()
		{
			var requestInFactory = (RequestType)GetNewBusinessObject();
			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			var user3 = Factory.NewWithValidTestData<GlbStaff>();
			var user4 = Factory.NewWithValidTestData<GlbStaff>();
			var user5 = Factory.NewWithValidTestData<GlbStaff>();
			var user6 = Factory.NewWithValidTestData<GlbStaff>();
			requestInFactory.XP_GS_NKApprovingUser1 = user1.GS_Code;
			AssertEquals(1, requestInFactory.GetApprovingUserPKs().Count);
			requestInFactory.XP_GS_NKApprovingUser2 = user2.GS_Code;
			AssertEquals(2, requestInFactory.GetApprovingUserPKs().Count);
			requestInFactory.XP_GS_NKApprovingUser3 = user3.GS_Code;
			AssertEquals(3, requestInFactory.GetApprovingUserPKs().Count);
			requestInFactory.XP_GS_NKApprovingUser4 = user4.GS_Code;
			AssertEquals(4, requestInFactory.GetApprovingUserPKs().Count);
			requestInFactory.XP_GS_NKApprovingUser5 = user5.GS_Code;
			AssertEquals(5, requestInFactory.GetApprovingUserPKs().Count);
			requestInFactory.XP_GS_NKApprovingUser6 = user6.GS_Code;
			AssertEquals(6, requestInFactory.GetApprovingUserPKs().Count);
			requestInFactory.XP_GS_NKApprovingUser1 = "";
			AssertEquals(5, requestInFactory.GetApprovingUserPKs().Count);
		}

		public void TestShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges()
		{
			var request = GetNewBusinessObject();
			AssertEquals("Should update audit fields if only children have changes", true, ((IUpdateAuditFields)request).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = (RequestType)base.GetNewBusinessObjectForDeleteTest(factory);
			bizo.XP_ParentID = ZGuid.NewZGuid();

			return bizo;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = (RequestType)base.GetNewBusinessObject();
			bizo.XP_ParentID = ZGuid.NewZGuid();

			return bizo;
		}
	}
}
