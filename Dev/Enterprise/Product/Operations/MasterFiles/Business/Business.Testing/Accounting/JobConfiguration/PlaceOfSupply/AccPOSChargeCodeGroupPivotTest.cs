using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(AccPOSChargeCodeGroupPivot))]
	sealed class AccPOSChargeCodeGroupPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();

			AssertEquals("GRP_GroupType", "POS", pivot.GRP_GroupType);
			AssertEquals("GRP_MemberTableCode", AccChargeCodeSchema.Constants.Prefix, pivot.GRP_MemberTableCode);
		}

		public void TestReadOnlyProperties()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();

			Assert("GRP_GroupType ReadOnly", pivot.GRP_GroupTypeInfo.ReadOnly);
			Assert("GRP_MemberTableCode ReadOnly", pivot.GRP_MemberTableCodeInfo.ReadOnly);
		}

		public void TestChargeCodeGroupAndCompany()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			Assert(pivot.GRP_GRO_Group.IsEmpty);
			AssertNull("ChargeCodeGroup", pivot.ChargeCodeGroup);
			AssertNull("Company", pivot.Company);

			var group = Factory.New<AccPOSChargeCodeGroup>();
			AssertNotNull(group.Company);

			pivot.GRP_GRO_Group = group.PK;
			AssertEquals(group, pivot.ChargeCodeGroup);
			AssertEquals("Pivot gets Company from its ChargeCodeGroup", pivot.Company, group.Company);
		}

		public void TestLookups()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			Assert(pivot.Lookups is AccPOSChargeCodeGroupPivotViewLookups);
		}

		public void TestUniqueIndexViolationHandler_UniqueInAGroup()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			var pivot1 = group.ChargeCodePivots.AddNew();
			pivot1.GRP_MemberID = chargeCode.PK;

			Factory.Save();

			var pivot2 = group.ChargeCodePivots.AddNew();
			pivot2.GRP_MemberID = chargeCode.PK;

			AssertUniqueIndexViolationHandler(pivot2, $"Charge Code '{chargeCode.AC_Code}' is already a member of this group.");
		}

		public void TestUniqueIndexViolationHandler_UniqueInAnyPOSGroup()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var group1 = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			var pivot1 = group1.ChargeCodePivots.AddNew();
			pivot1.GRP_MemberID = chargeCode.PK;

			Factory.Save();

			var group2 = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			var pivot2 = group2.ChargeCodePivots.AddNew();
			pivot2.GRP_MemberID = chargeCode.PK;

			AssertUniqueIndexViolationHandler(pivot2, $"Charge Code '{chargeCode.AC_Code}' has been already added to a different configuration group.");
		}

		public void TestUniqueIndexViolationHandler_TryToHandlePrimaryKeyViolation()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode1 = newFactory.NewWithValidTestData<AccChargeCode>();
			var chargeCode2 = newFactory.NewWithValidTestData<AccChargeCode>();

			var group1 = newFactory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			var pivot1 = group1.ChargeCodePivots.AddNew();
			pivot1.GRP_MemberID = chargeCode1.PK;

			newFactory.Save();

			var pivot2 = Factory.NewWithPrimaryKey<AccPOSChargeCodeGroupPivot>(pivot1.PK.ToGuid());
			pivot2.GRP_GRO_Group = group1.PK;
			pivot2.GRP_MemberID = chargeCode2.PK;

			// Primary Key violation does not provide Unique Index Name but we will get a generic message with a new unique index name if it is added later.
			AssertUniqueIndexViolationHandler(pivot2, $"Unique index violation. Index name: ");
		}

		void AssertUniqueIndexViolationHandler(IBusinessObjectInternals castPivot, string message)
		{
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				var notificationHandler = new TestNotificationHandler();
				castPivot.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notificationHandler, ex.IndexNameIfUniqueIndexViolation);

				AssertEquals("Message", message, notificationHandler.LastErrorMessage);
				AssertEquals("Caption", "Charge Code Group for the Place of Supply configuration", notificationHandler.LastErrorCaption);
			}
		}

		#region TestNotificationHandler

		class TestNotificationHandler : INotificationHandler
		{
			public string LastErrorMessage;
			public string LastErrorCaption;
			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				LastErrorMessage = message;
				LastErrorCaption = caption;
			}

			void INotificationHandler.ReportInformation(string message, string caption)
			{
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var newFactory = new BusinessObjectFactory();
			var chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			// Make a unique Charge Code
			var newFactoryInstanceId = newFactory._Instance.ToString();
			chargeCode.AC_Code = chargeCode.AC_Code.Substring(0, chargeCode.AC_Code.Length - newFactoryInstanceId.Length) + newFactoryInstanceId;
			newFactory.Save();

			var result = (AccPOSChargeCodeGroupPivot)base.GetNewBusinessObject();
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			result.GRP_GRO_Group = group.PK;
			result.GRP_MemberID = chargeCode.PK;

			return result;
		}

		#endregion
	}
}
