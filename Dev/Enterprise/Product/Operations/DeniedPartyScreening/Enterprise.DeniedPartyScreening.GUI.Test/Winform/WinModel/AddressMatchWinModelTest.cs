using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class AddressMatchWinModelTest : TestCase
	{
		public void TestConstructorArgumentNull()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new AddressMatchWinModel(null, true));
			});
		}

		public void TestScreenedDeniedItems()
		{
			var profileId1 = Guid.NewGuid();
			var profileId2 = Guid.NewGuid();
			var profileId3 = Guid.NewGuid();

			var addressId1 = Guid.NewGuid();
			var addressId2 = Guid.NewGuid();
			var addressId3 = Guid.NewGuid();
			var profileAddressInfos = new List<ProfileAddressInfo>()
			{
				new ProfileAddressInfo { ID = addressId1, Street = "Test Street A", City = "", StateProvince = "Test State", Country = "CN", PostCode = "123", Language = "Chinese", SourceProfileID = profileId1 },
				new ProfileAddressInfo { ID = addressId2, Street = "Test Street B", City = "Test City", StateProvince = "", Country = "CN", PostCode = "123", Language = "Chinese", SourceProfileID = profileId2 },
				new ProfileAddressInfo { ID = addressId3, Street = "Test Street C", City = "Test City D", StateProvince = "Test State E", Country = "CN", PostCode = "123", Language = "Chinese", SourceProfileID = profileId3 },
			};

			var score = 100;
			var displayScore = "100%";
			var addressMatchInfos = new List<AddressMatchInfo>()
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "", Address2 = "Test Address ", City = "Test City", State = "Test State", PostCode = "123", Country = "CN", AdditionalAddressLine = "" }, MatchingAddressID = addressId1, MatchingAddressScore = score, SourceProfileID = profileId1 },
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "Test Address F", Address2 = "Test Address I", City = "", State = "Test State", PostCode = "123", Country = "CN", AdditionalAddressLine = "" }, MatchingAddressID = addressId1, MatchingAddressScore = score, SourceProfileID = profileId1 },
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "Test Address G", Address2 = "Test Address J", City = "Test City", State = "Test State", PostCode = "123", Country = "CN", AdditionalAddressLine = "" }, MatchingAddressID = addressId2, MatchingAddressScore = score, SourceProfileID = profileId2 },
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "Test Address H", Address2 = "Test Address K", City = "Test City", State = "Test State L", PostCode = "123", Country = "CN", AdditionalAddressLine = "" }, MatchingAddressID = Guid.NewGuid(), MatchingAddressScore = score, SourceProfileID = profileId1 },
			};

			var model = new AddressMatchWinModel(new AddressMatchModel(profileAddressInfos, addressMatchInfos), true);

			CombineAssertions(() =>
			{
				AssertEquals(3, model.ScreenedDeniedItems.Count);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel(@"Test City
Test State
123
CN", @"Test Street A
Test State
123
CN", displayScore, ScoreGrades.High, score, model.ScreenedDeniedItems[0]);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel(@"Test Address F
Test State
123
CN", @"Test Street A
Test State
123
CN", displayScore, ScoreGrades.High, score, model.ScreenedDeniedItems[1]);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel(@"Test Address G
Test City
Test State
123
CN", @"Test Street B
Test City
123
CN", displayScore, ScoreGrades.High, score, model.ScreenedDeniedItems[2]);

				AssertEquals(1, model.OtherScreenedDeniedItems.Count);
				AssertEquals(@"Test Street C
Test City D
Test State E
123
CN", model.OtherScreenedDeniedItems.First().DeniedParty);

				AssertEquals(true, model.MatchViewVisibility);
			});

			model = new AddressMatchWinModel(new AddressMatchModel(profileAddressInfos, addressMatchInfos), false);

			AssertEquals(false, model.MatchViewVisibility);
		}

		public void TestExpanderWinModel()
		{
			var model = new AddressMatchWinModel(new AddressMatchModel(new List<ProfileAddressInfo>(), new List<AddressMatchInfo>()), true);
			CombineAssertions(() =>
			{
				AssertEquals("Address", model.ExpanderTitle);
				AssertEquals("Address", model.OddName);
				AssertEquals("Addresses", model.PluralName);
			});
		}
	}
}
