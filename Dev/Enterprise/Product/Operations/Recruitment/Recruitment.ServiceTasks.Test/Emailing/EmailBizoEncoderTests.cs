using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	sealed class EmailBizoEncoderTests : TestCaseWithFactory
	{
		[StressTest]
		public void TestGenerateCanFindOriginalBizo()
		{
			var bizosToTest = Enumerable.Range(0, 1000)
				.Select(_ => Factory.NewWithValidTestData<JobConversation>())
				.Concat(new BusinessObject[]
					{
						Factory.NewWithValidTestData<DummyBaseBusinessObject>(),
						Factory.NewWithValidTestData<DummyBaseBusinessObject>(),
						Factory.NewWithValidTestData<OrgHeader>(),
						Factory.NewWithValidTestData<GlbStaff>(),
						Factory.NewWithValidTestData<GlbGroup>(),
					})
				.ToArray();

			Factory.Save();

			var retrievedThroughAddress = bizosToTest
				.Select(EmailBizoEncoder.EncodeBizo)
				.Select(message => EmailBizoEncoder.DecodeBizo(Factory, message))
				.ToList();

			AssertSequencesEqual(bizosToTest, retrievedThroughAddress);
		}

		public void TestBackwardsCompatibility()
		{
			var parent = Factory.NewWithValidTestData<DummyBusinessObject>();
			var bizo = Factory.NewWithPrimaryKey<JobConversation>(new Guid("d2ed699f-edba-4c3f-96fd-6ff4160be2db"));
			bizo.FillWithValidTestData();

			bizo.JCC_ParentTableCode = parent.TablePrefix;
			bizo.JCC_ParentID = parent.PK;

			Factory.Save();

			var assertMsg = "The address generator needs to be backwards compatible since we need to handle responses over upgrades and can't transform data in other peoples mailboxes";
			var encoded = "8Z9p7dK67T9Mlv1v9BYL4ttKQ0M";
			AssertEquals(assertMsg, bizo, EmailBizoEncoder.DecodeBizo(Factory, encoded));
		}

		public void TestNoHitForDodgyBizo()
		{
			var b1key = EmailBizoEncoder.EncodeBizo(Factory.NewWithValidTestData<DummyBusinessObject>());
			var invalidKey = "z" + b1key.Substring(1);

			var dbHits = Db.Connection.ExecutedCommandCount;
			AssertEquals(null, EmailBizoEncoder.DecodeBizo(Factory, invalidKey));
			AssertEquals("If a key has been modified it should throw off the hashsum check, therefore don't hit the DB", dbHits, Db.Connection.ExecutedCommandCount);
		}

		public void TestBadInput()
		{
			CombineAssertions("You can receive emails from any random source, so we don't want to throw whenever the address doesn't exactly match our format.", () =>
			{
				AssertNull("Encoded bizo pattern doesn't match a known pattern", EmailBizoEncoder.DecodeBizo(Factory, "abc"));
				AssertNull("Encoded bizo pattern doesn't match a known pattern", EmailBizoEncoder.DecodeBizo(Factory, new string('a', 100)));
				AssertNull("Contains invalid characters", EmailBizoEncoder.DecodeBizo(Factory, "&$A@.%^&*()"));
				AssertNull("Empty string", EmailBizoEncoder.DecodeBizo(Factory, string.Empty));
				AssertNull("Null input", EmailBizoEncoder.DecodeBizo(Factory, null));

				for (var i = 1; i < 30; i++)
				{
					AssertNull("Random lengths of string - " + i, EmailBizoEncoder.DecodeBizo(Factory, new string('a', i)));
				}
			});
		}
	}
}
