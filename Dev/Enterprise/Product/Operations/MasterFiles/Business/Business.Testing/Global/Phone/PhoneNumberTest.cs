using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PhoneNumber))]
	sealed class PhoneNumberTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestFormattedForBinding()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var phoneNumber = new PhoneNumber(dummy.Z0_NVarCharInfo, null, null);

			dummy.Z0_NVarChar = "000";
			AssertEquals("Should be wrapped", "000", phoneNumber.FormattedForBinding);

			phoneNumber.FormattedForBinding = "999";
			AssertEquals("Should be wrapped", "999", dummy.Z0_NVarChar);

			AssertEquals("Should have same MaxLength", dummy.Z0_NVarCharInfo.MaxLength, phoneNumber.FormattedForBindingInfo.MaxLength);

			dummy.ReadOnly = true;
			AssertEquals("Precondition", true, dummy.Z0_NVarCharInfo.ReadOnly);
			AssertEquals("Should have same ReadOnly", true, phoneNumber.FormattedForBindingInfo.ReadOnly);

			dummy.ReadOnly = false;
			AssertEquals("Precondition", false, dummy.Z0_NVarCharInfo.ReadOnly);
			AssertEquals("Should have same ReadOnly", false, phoneNumber.FormattedForBindingInfo.ReadOnly);
		}

		public void TestSettingFormattedForBindingInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				testOrg.MainAddress.PhoneNumber.FormattedForBinding = "123456";

				//Assert
				AssertEquals(true, dedupeStarted);
			}
		}

		public void TestSettingFormattedForBindingDoesNotInvokeDeduplication()
		{
			//Arrange
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = false;

			//Act
			testOrg.MainAddress.PhoneNumber.FormattedForBinding = "123456";

			//Assert
			AssertEquals(false, dedupeStarted);
		}

		public void TestIsPublishedForBinding()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var phoneNumber = new PhoneNumber(dummy.Z0_NVarCharInfo, dummy.Z0_BoolInfo, null);

			dummy.Z0_Bool = false;
			AssertEquals("Should be wrapped", false, phoneNumber.IsPublishedForBinding);

			phoneNumber.IsPublishedForBinding = true;
			AssertEquals("Should be wrapped", true, dummy.Z0_Bool);
		}

		public void TestFormattedLocalNumberIfLoggedInSameCountryForBinding()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var phoneNumber = new PhoneNumber(dummy.Z0_NVarCharInfo, null, dummy.Z0_DescriptionInfo);

			dummy.Z0_Description = "9 Hours, 9 Persons, 9 Doors";
			AssertEquals("Should be wrapped", "9 Hours, 9 Persons, 9 Doors", phoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		public void TestICustomTextTemplateContext()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var phoneNumber = new PhoneNumber(staff.GS_WorkPhone_FormattedInfo, null, null);

			Assert(phoneNumber is ICustomTextTemplateContext);

			var context = phoneNumber.GetTextTemplateContextBusinessObject(staff, new KBindingMemberInfo());
			var contextId = phoneNumber.GetTextTemplateContextID(staff, new KBindingMemberInfo(AutoGlbStaff.Schema.GS_WorkPhone));

			AssertEquals(1, context.Length);
			AssertEquals(staff, context[0]);
			AssertEquals("GlbStaff.GS_WorkPhone", contextId);
			AssertEquals(phoneNumber, phoneNumber.GetTextTemplateContextBusinessObject(null, new KBindingMemberInfo())[0]);
			AssertEquals(".GS_WorkPhone", phoneNumber.GetTextTemplateContextID(null, new KBindingMemberInfo(AutoGlbStaff.Schema.GS_WorkPhone)));

			var obj = new object();
			AssertEquals(phoneNumber, phoneNumber.GetTextTemplateContextBusinessObject(obj, new KBindingMemberInfo())[0]);
			AssertEquals(".GS_WorkPhone", phoneNumber.GetTextTemplateContextID(obj, new KBindingMemberInfo(AutoGlbStaff.Schema.GS_WorkPhone)));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var phoneNumber = new PhoneNumber(dummy.Z0_NVarCharInfo, null, null);
			return phoneNumber;
		}

		public void TestToString()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var phoneNumber = new PhoneNumber(dummy.Z0_NVarCharInfo, null, null);

			dummy.Z0_NVarChar = "000";
			AssertEquals("Should return FormattedForBinding", "000", phoneNumber.ToString());
		}

		#endregion
	}
}
