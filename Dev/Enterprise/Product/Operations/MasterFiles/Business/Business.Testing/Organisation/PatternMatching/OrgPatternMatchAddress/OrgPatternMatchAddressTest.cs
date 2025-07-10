using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatchAddress))]
	sealed class OrgPatternMatchAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSettingMatchOrgApprovesOrgMatchApproval()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();

			OrgPatternMatchAddress matchAddress = Factory.New<OrgPatternMatchAddress>();
			matchAddress.P3_ParentID = dummyParent.PK;

			matchAddress.P3_OH_MatchOrg = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("Should be able to set P3_OH_MatchOrg without an OrgMatchApproval record", false, matchAddress.P3_OH_MatchOrg.IsEmpty);

			OrgMatchApproval matchApproval = new OrgMatchApproval.Loader(Factory).LoadOrCreate(matchAddress, OrgMatchApprovalType.DummyType);
			matchAddress.P3_OH_MatchOrg = Factory.New(typeof(OrgHeader)).PK;

			AssertEquals("OrgMatchApproval should be approved", true, matchApproval.IsApproved);
			AssertEquals("OrgMatchApproval should be approved", matchAddress.P3_OH_MatchOrg, matchApproval.P2_OH_MatchOrg1);
			AssertEquals("OrgMatchApproval should be approved", matchAddress.P3_OH_MatchOrg, matchApproval.P2_OH_MatchOrg2);
			AssertEquals("OrgMatchApproval should be approved by me", GlbStaff.CurrentUser.GS_Code, matchApproval.P2_MatchUser1);
			AssertEquals("OrgMatchApproval should be approved by me", GlbStaff.CurrentUser.GS_Code, matchApproval.P2_MatchUser1);
		}

		public void TestUniqueIndexFailureHandler()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();

			TestOrgPatternMatchAddress matchAddress1 = Factory.New<TestOrgPatternMatchAddress>();
			matchAddress1.P3_ParentID = dummyParent.PK;
			matchAddress1.P3_AddressType = "XXX";

			TestOrgPatternMatchAddress matchAddress2 = Factory.New<TestOrgPatternMatchAddress>();
			matchAddress2.P3_ParentID = dummyParent.PK;
			matchAddress2.P3_AddressType = "XXX";

			try
			{
				Factory.Save();
				Fail("Expected a unique index violation exception");
			}
			catch (ZSaveException ex)
			{
				string uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;
				AssertEquals("1 bizo should be involved with the unique constraint violation", 1, ex.BusinessObjects.Length);
				TestOrgPatternMatchAddress addressInError = (TestOrgPatternMatchAddress)ex.BusinessObjects[0];

				AssertEquals("Correct unique constraint should be violated", addressInError.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Single(), uniqueIndexName);
				MockNotificationHandler notifier = new MockNotificationHandler();
				addressInError.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notifier, addressInError.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Single());
				AssertEquals("User should be notified of the situation", "Another user has changed this record", notifier.LastErrorCaption);
				AssertEquals("User should be notified of the situation", "Another user has already made changes to this record. You must re-open this form and re-apply your changes to continue.", notifier.LastErrorMessage);
			}
		}

		class MockNotificationHandler : INotificationHandler
		{
			public string LastErrorMessage;
			public string LastErrorCaption;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				LastErrorMessage = message;
				LastErrorCaption = caption;
			}

			public void ReportInformation(string message, string caption)
			{
				throw new NotSupportedException();
			}
		}

		class TestOrgPatternMatchAddress : OrgPatternMatchAddress
		{
			public TestOrgPatternMatchAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
			{
				get { return base.UniqueIndexFailureHandlers; }
			}
		}
	}
}
