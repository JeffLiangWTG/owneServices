using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailAddressForSendValidationTest : TestCaseWithFactory
	{
		public void TestValidateEmailAddress()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			dummyBizObj.Z0_VarCharMax = "invalid";

			using (dummyBizObj.SuspendValidationTesting())
			{
				var validation = new EmailAddressForSendValidation(Factory);
				validation.ValidateEmailAddress(dummyBizObj.Z0_VarCharMaxInfo);
				AssertHasError(dummyBizObj.Z0_VarCharMaxInfo, @"The email address ""invalid"" is invalid.");
				AssertNoWarnings(dummyBizObj.Z0_VarCharMaxInfo);

				dummyBizObj.Z0_VarCharMax = "andrew.luong@wisetechglobal.com";
				dummyBizObj.Z0_VarCharMaxInfo.ClearAllNotifications();
				validation.ValidateEmailAddress(dummyBizObj.Z0_VarCharMaxInfo);
				AssertNoErrors(dummyBizObj.Z0_VarCharMaxInfo);
				AssertNoWarnings(dummyBizObj.Z0_VarCharMaxInfo);

				var glbEmailAddress = GlbEmailAddress.LoadOrNew(Factory, "andrew.luong@wisetechglobal.com");
				glbEmailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				glbEmailAddress.GI_DeliveryReportTimeUtc = new ZDateTime(2002, 2, 2, 1, 10, 0);

				dummyBizObj.Z0_VarCharMaxInfo.ClearAllNotifications();
				validation.ValidateEmailAddress(dummyBizObj.Z0_VarCharMaxInfo);
				AssertNoErrors(dummyBizObj.Z0_VarCharMaxInfo);
				AssertHasWarning(dummyBizObj.Z0_VarCharMaxInfo, @"The last email sent to ""andrew.luong@wisetechglobal.com"" received a Non-Delivery Receipt (at 02-Feb-02 01:10:00).");
			}
		}

		public void TestValidateCommaSeparatedEmailAddresses()
		{
			var anotherFactory = new BusinessObjectFactory();
			var adlEmail = GlbEmailAddress.LoadOrNew(anotherFactory, "andrew.luong@wisetechglobal.com");
			adlEmail.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			adlEmail.GI_DeliveryReportTimeUtc = new ZDateTime(2002, 2, 2);
			var risEmail = GlbEmailAddress.LoadOrNew(anotherFactory, "richard.smith@wisetechglobal.com");
			risEmail.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			risEmail.GI_DeliveryReportTimeUtc = new ZDateTime(2003, 3, 3);
			var scwEmail = GlbEmailAddress.LoadOrNew(anotherFactory, "samuel.wang@wisetechglobal.com");
			scwEmail.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;
			scwEmail.GI_DeliveryReportTimeUtc = new ZDateTime(2004, 4, 4);

			anotherFactory.Save();

			var dummyBizObj = Factory.New<DummyBusinessObject>();
			dummyBizObj.Z0_VarCharMax = "andrew.luong@wisetechglobal.com, richard.smith@wisetechglobal.com, samuel.wang@wisetechglobal.com, invalid";

			using (dummyBizObj.SuspendValidationTesting())
			{
				var validation = new EmailAddressForSendValidation(Factory);
				validation.ValidateCommaSeparatedEmailAddresses(dummyBizObj.Z0_VarCharMaxInfo);

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						Tuple.Create(NotificationType.Warning.EnumValueName, $"The last email sent to \"andrew.luong@wisetechglobal.com\" received a Non-Delivery Receipt (at {adlEmail.GI_DeliveryReportTimeUtc})."),
						Tuple.Create(NotificationType.Warning.EnumValueName, $"The last email sent to \"richard.smith@wisetechglobal.com\" received a Non-Delivery Receipt (at {risEmail.GI_DeliveryReportTimeUtc})."),
						Tuple.Create(NotificationType.Error.EnumValueName, $"The email address \"invalid\" is invalid.")
					},
					dummyBizObj.Z0_VarCharMaxInfo.Notifications.Select(x => Tuple.Create(x.Type.EnumValueName, x.Message)));
			}

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbEmailAddressSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, Factory);
		}

		public void TestValidateEmptyEmailAddress()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			dummyBizObj.Z0_VarCharMax = "";

			using (dummyBizObj.SuspendValidationTesting())
			{
				var validation = new EmailAddressForSendValidation(Factory);
				validation.ValidateEmailAddress(dummyBizObj.Z0_VarCharMaxInfo);
				AssertHasError(dummyBizObj.Z0_VarCharMaxInfo, @"The email address """" is invalid.");
				AssertNoWarnings(dummyBizObj.Z0_VarCharMaxInfo);
			}
		}
	}
}
