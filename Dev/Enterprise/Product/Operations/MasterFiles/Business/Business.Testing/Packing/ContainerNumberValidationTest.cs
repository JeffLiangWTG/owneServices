using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContainerNumberValidationTest : TestCaseWithDummy
	{
		public void TestErrorIfInvalid()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Description = "FAKE4100011";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.ErrorIfInvalid(Dummy.Z0_DescriptionInfo);
				AssertNoNotifications("Should not have notification for valid description", Dummy.Z0_DescriptionInfo);

				Dummy.Z0_Description = "FAKE4100012";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.ErrorIfInvalid(Dummy.Z0_DescriptionInfo);
				AssertHasError("Should have error for invalid description", Dummy.Z0_DescriptionInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");

				Dummy.Z0_Description = "";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.ErrorIfInvalid(Dummy.Z0_DescriptionInfo);
				AssertNoNotifications("Should not have notification for empty description", Dummy.Z0_DescriptionInfo);
			}
		}

		public void TestWarnIfInvalid()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Description = "FAKE4100011";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.WarnIfInvalid(Dummy.Z0_DescriptionInfo);
				AssertNoNotifications("Should not have notification for valid description", Dummy.Z0_DescriptionInfo);

				Dummy.Z0_Description = "FAKE4100012";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.WarnIfInvalid(Dummy.Z0_DescriptionInfo);
				AssertHasWarning("Should have warning for invalid description", Dummy.Z0_DescriptionInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");

				Dummy.Z0_Description = "";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.WarnIfInvalid(Dummy.Z0_DescriptionInfo);
				AssertNoNotifications("Should not have notification for empty description", Dummy.Z0_DescriptionInfo);
			}
		}

		public void TestAddNotificationIfInvalid()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_Description = "FAKE4100011";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.AddNotificationIfInvalid(Dummy.Z0_DescriptionInfo, NotificationTypes.Error);
				AssertNoNotifications("Should not have notification for valid description", Dummy.Z0_DescriptionInfo);

				Dummy.Z0_Description = "FAKE4100012";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.AddNotificationIfInvalid(Dummy.Z0_DescriptionInfo, NotificationTypes.Error);
				AssertHasError("Should have error for invalid description when passed an error type", Dummy.Z0_DescriptionInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");

				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.AddNotificationIfInvalid(Dummy.Z0_DescriptionInfo, NotificationTypes.Warning);
				AssertHasWarning("Should have warning for invalid description when passed a warning type", Dummy.Z0_DescriptionInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");

				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.AddNotificationIfInvalid(Dummy.Z0_DescriptionInfo, NotificationTypes.MessageError);
				AssertHasMessageError("Should have message error for invalid description when passed a message error type", Dummy.Z0_DescriptionInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");

				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.AddNotificationIfInvalid(Dummy.Z0_DescriptionInfo, NotificationTypes.None);
				AssertNoNotifications("Should not have notification for invalid description when passed a type of none", Dummy.Z0_DescriptionInfo);

				Dummy.Z0_Description = "";
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				ContainerNumberValidation.AddNotificationIfInvalid(Dummy.Z0_DescriptionInfo, NotificationTypes.Error);
				AssertNoNotifications("Should not have notification for empty description", Dummy.Z0_DescriptionInfo);
			}
		}

		public void TestContainerNumberValidation()
		{
			string containerError = ContainerNumberValidation.GetContainerNumberError("12345678905");
			Assert("All numerics is invalid", containerError != null && !string.IsNullOrEmpty(containerError));
		}

		public void TestGetContainerNumberError()
		{
			AssertEquals("Wrong number of characters", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", ContainerNumberValidation.GetContainerNumberError("123"));
			AssertEquals("Wrong number of letters", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", ContainerNumberValidation.GetContainerNumberError("ABCDE123456"));
			AssertEquals("Wrong number of letters", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", ContainerNumberValidation.GetContainerNumberError("ABC12345678"));
			AssertEquals("Wrong number of letters", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.", ContainerNumberValidation.GetContainerNumberError("12345678901"));
			AssertEquals("Wrong number of letters", "Container number does not have a valid check (last) digit. The check digit should be 5.", ContainerNumberValidation.GetContainerNumberError("TEST7777776"));
			AssertEquals("No error on container number", null, ContainerNumberValidation.GetContainerNumberError("TEST7777775"));
		}

		public void TestIsValidContainerNumber()
		{
			CombineAssertions(delegate
			{
				var values = new[]
				{
					new { CN = "FAKE4100010", IsValid = false },
					new { CN = "FAKE4100011", IsValid = true },
					new { CN = "fake4100011", IsValid = true },
					new { CN = "FAKE4100012", IsValid = false },
					new { CN = "ANMZ1237891", IsValid = true },
					new { CN = "Garbage", IsValid = false },
					new { CN = "F@KE4100010", IsValid = false },
					new { CN = "F]KE4100010", IsValid = false },
					new { CN = "FAKE4/00016", IsValid = false },
					new { CN = "FAKE4:00016", IsValid = false },
				};

				for (int i = 0; i < values.Length; i++)
				{
					string message = string.Format("'{0}' = {1}", values[i].CN, values[i].IsValid ? "valid" : "invalid");
					AssertEquals(message, values[i].IsValid, ContainerNumberValidation.IsValidContainerNumber(values[i].CN));
				}
			});
		}
	}
}
