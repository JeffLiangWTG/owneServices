using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbEmailAddress))]
	sealed class GlbEmailAddressTest : EnterpriseBusinessObjectTestCase
	{
		#region Static

		public void TestLoad()
		{
			var emailAddress = Factory.New<GlbEmailAddress>();
			emailAddress.GI_EmailAddress = "andrew.luong@wisetechglobal.com";

			AssertEquals(emailAddress, GlbEmailAddress.Load(Factory, "andrew.luong@wisetechglobal.com"));

			AssertEquals(emailAddress, GlbEmailAddress.Load(Factory, "Andrew Luong <ANDREW.LUONG@WISETECHGLOBAL.COM>"));

			AssertNull(GlbEmailAddress.Load(Factory, ""));
		}

		public void TestLoad_MultipleAddresses()
		{
			var anotherFactory = new BusinessObjectFactory();
			var emailAddress1 = anotherFactory.New<GlbEmailAddress>();
			emailAddress1.GI_EmailAddress = "email@1.com";
			var emailAddress2 = anotherFactory.New<GlbEmailAddress>();
			emailAddress2.GI_EmailAddress = "email@2.com";
			anotherFactory.Save();

			var emailAddresses = GlbEmailAddress.Load(Factory, new ZString[] { "<Andrew Luong> email@1.com", "email@2.com" });
			AssertContainsExactElementsInAnyOrder(
				new[] { emailAddress1.PK, emailAddress2.PK },
				emailAddresses.Select(x => x.PK));

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbEmailAddressSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, Factory);
		}

		public void TestLoadOrNew()
		{
			var emailAddress = GlbEmailAddress.LoadOrNew(Factory, "aaa@a.com");
			AssertEquals("aaa@a.com", emailAddress.GI_EmailAddress);

			var emailAddress2 = GlbEmailAddress.LoadOrNew(Factory, "aaa@a.com");
			AssertEquals("Should return the same business object", emailAddress, emailAddress2);
		}

		public void TestLoadOrNew_WithoutEmailAddress()
		{
			AssertExceptionThrown(typeof(ArgumentException), () =>
			{
				GlbEmailAddress.LoadOrNew(Factory, "");
			});
		}

		#endregion

		#region Properties

		public void TestGI_EmailAddress_TrimsWhiteSpace()
		{
			var emailAddress = Factory.New<GlbEmailAddress>();
			emailAddress.GI_EmailAddress = " andrew.luong@wisetechglobal.com ";
			AssertEquals("andrew.luong@wisetechglobal.com", emailAddress.GI_EmailAddress);
		}

		public void TestGI_EmailAddress_RemovesDisplayName()
		{
			var emailAddress = Factory.New<GlbEmailAddress>();

			emailAddress.GI_EmailAddress = "Andrew Luong <andrew.luong@wisetechglobal.com>";
			AssertEquals("andrew.luong@wisetechglobal.com", emailAddress.GI_EmailAddress);

			emailAddress.GI_EmailAddress = "Space at end <andrew.luong@wisetechglobal.com> ";
			AssertEquals("andrew.luong@wisetechglobal.com", emailAddress.GI_EmailAddress);

			emailAddress.GI_EmailAddress = "Andrew <>";
			AssertEquals("Andrew <>", emailAddress.GI_EmailAddress);
		}

		#endregion

		#region Concurrency

		public void TestConcurrencyOn_DeliveryStatus()
		{
			var factoryMaster = new BusinessObjectFactory();
			var factoryOther = new BusinessObjectFactory();

			using (GetFactoryIsolater(factoryMaster))
			using (GetFactoryIsolater(factoryOther))
			{
				var emailAddress = factoryMaster.NewWithValidTestData<GlbEmailAddress>();
				emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				factoryMaster.Save();

				var emailAddressOtherVersion = factoryOther.Load<GlbEmailAddress>(emailAddress.PK);
				emailAddressOtherVersion.GI_DeliveryStatus = string.Empty;
				factoryOther.Save();

				emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;
				try
				{
					factoryMaster.Save();
					Assert("Save Successful", true);
				}
				catch (ZSaveConcurrencyException)
				{
					Fail("Expecting no concurrency exception due to policy being ignore");
				}
			}
		}

		public void TestConcurrencyOn_ReportTimeUtc()
		{
			var factoryMaster = new BusinessObjectFactory();
			var factoryOther = new BusinessObjectFactory();

			using (GetFactoryIsolater(factoryMaster))
			using (GetFactoryIsolater(factoryOther))
			{
				var emailAddress = factoryMaster.NewWithValidTestData<GlbEmailAddress>();
				emailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow.AddDays(-2);
				factoryMaster.Save();

				var emailAddressOtherVersion = factoryOther.Load<GlbEmailAddress>(emailAddress.PK);
				emailAddressOtherVersion.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow.AddDays(-1);
				factoryOther.Save();

				emailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;
				try
				{
					factoryMaster.Save();
					Assert("Save Successful", true);
				}
				catch (ZSaveConcurrencyException)
				{
					Fail("Expecting no concurrency exception due to policy being ignore");
				}
			}
		}

		#endregion
	}
}
