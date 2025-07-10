using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(RegistrationMatch))]
	public class RegistrationMatchTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RegistrationMatch(new RegistrationCodeMatchInfo(), new ProfileRegistrationCodeInfo());
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new RegistrationMatch(new RegistrationCodeMatchInfo(), null));
		}
		public void TestRegCodeItemModel_WithHighMatchScore()
		{
			var model = new RegistrationMatch(GetRegistrationCodeMatchInfo(100), GetProfileRegistrationCodeInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.RegistrationCodeMatchInfo);
				AssertEquals("Registration", model.Type);
				AssertEquals("DummyType : DummyNumber", model.Name);
				AssertEquals(100, model.Score);
				AssertEquals(RiskLevel.High, model.Level);
			});
		}

		public void TestRegCodeItemModel_WithEmptyMatchScore()
		{
			var model = new RegistrationMatch(null, GetProfileRegistrationCodeInfo());
			CombineAssertions(() =>
			{
				AssertNull(model.RegistrationCodeMatchInfo);
				AssertEquals("Registration", model.Type);
				AssertEquals("DummyType : DummyNumber", model.Name);
				AssertEquals(0, model.Score);
				AssertEquals(RiskLevel.Low, model.Level);
			});
		}
		public void TestRegCodeItemModel_WithLowMatchScore()
		{
			var model = new RegistrationMatch(GetRegistrationCodeMatchInfo(80), GetProfileRegistrationCodeInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.RegistrationCodeMatchInfo);
				AssertEquals("Registration", model.Type);
				AssertEquals("DummyType : DummyNumber", model.Name);
				AssertEquals(80, model.Score);
				AssertEquals(RiskLevel.Low, model.Level);
			});
		}

		public void TestRegCodeItemModel_WithArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new RegistrationMatch(GetRegistrationCodeMatchInfo(10), null));
		}

		ProfileRegistrationCodeInfo GetProfileRegistrationCodeInfo(Guid? pk = null, Guid? profilePK = null)
		{
			return new ProfileRegistrationCodeInfo { ID = pk ?? Guid.NewGuid(), IdType = "DummyType", IdNumber = "DummyNumber", IdCountry = "Dummy", SourceProfileID = profilePK ?? Guid.NewGuid() };
		}

		RegistrationCodeMatchInfo GetRegistrationCodeMatchInfo(int score, Guid? profilePK = null)
		{
			return new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "1234", RegCodeType = "1234", RegCodeValue = "AU" }, MatchingRegistrationCodeID = Guid.NewGuid(), MatchingRegistrationCodeScore = score, SourceProfileID = profilePK ?? Guid.NewGuid() };
		}
	}
}

