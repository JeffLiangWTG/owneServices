using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingPendingTransactionsNotificationGroup))]
	sealed class EInvoicingPendingTransactionsNotificationGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			var notificationGroup = new EInvoicingPendingTransactionsNotificationGroup();
			AssertNoError(notificationGroup.GroupPKInfo, "Please enter a Group.");
			AssertNoError(notificationGroup.DateTypeInfo, "Please select the date criteria.");
			AssertNoError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.GroupPK = ZGuid.BrettsGuid;
			AssertNoError(notificationGroup.GroupPKInfo, "Please enter a Group.");
			AssertHasError(notificationGroup.DateTypeInfo, "Please select the date criteria.");
			AssertNoError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.GroupPK = ZGuid.Empty;
			notificationGroup.DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate;
			AssertHasError(notificationGroup.GroupPKInfo, "Please enter a Group.");
			AssertNoError(notificationGroup.DateTypeInfo, "Please select the date criteria.");
			AssertNoError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.GroupPK = ZGuid.BrettsGuid;
			notificationGroup.Days = 365;
			AssertNoError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.Days = -1;
			AssertHasError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.Days = 366;
			AssertHasError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.Days = 365;
			AssertNoError(notificationGroup.DaysInfo, "The day value must be less than or equal to the maximum 365.");

			notificationGroup.GroupPK = ZGuid.Invalid;
			AssertHasError(notificationGroup.GroupPKInfo, "Enter a valid Group.");

			notificationGroup.DateType = "123";
			AssertHasError(notificationGroup.DateTypeInfo, "Enter a valid Date.");
		}

		public void TestDaysMaxLength()
		{
			var notificationGroup = new EInvoicingPendingTransactionsNotificationGroup();
			AssertEquals(3, notificationGroup.DaysInfo.MaxLength);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
			=> new EInvoicingPendingTransactionsNotificationGroup
			{
				GroupPK = CargoWise.Types.ZGuid.BrettsGuid,
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate,
				Days = 1
			};

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
