using System;
using System.Collections.Generic;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class AddressMatchModelTest : TestCase
	{
		public void TestConstructorArgumentNull()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => _ = new AddressMatchModel(null, new List<AddressMatchInfo>()));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new AddressMatchModel(new List<ProfileAddressInfo>(), null));
			});
		}

		public void TestTotalScreenedDeniedItems()
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

			var model = new AddressMatchModel(profileAddressInfos, addressMatchInfos);

			CombineAssertions(() =>
			{
				AssertEquals(4, model.TotalScreenedDeniedItems.Count);
				AssertScreenedDeniedAddressItemModel(addressMatchInfos[0], profileAddressInfos[0], displayScore, ScoreGrades.High, model.TotalScreenedDeniedItems[0]);
				AssertScreenedDeniedAddressItemModel(addressMatchInfos[1], profileAddressInfos[0], displayScore, ScoreGrades.High, model.TotalScreenedDeniedItems[1]);
				AssertScreenedDeniedAddressItemModel(addressMatchInfos[2], profileAddressInfos[1], displayScore, ScoreGrades.High, model.TotalScreenedDeniedItems[2]);
				AssertScreenedDeniedAddressItemModel(null, profileAddressInfos[2], string.Empty, ScoreGrades.Low, model.TotalScreenedDeniedItems[3]);
			});
		}

		void AssertScreenedDeniedAddressItemModel(AddressMatchInfo addressMatchInfo, ProfileAddressInfo profileAddressInfo, string expectedDisplayScore, ScoreGrades expectedScoreGrade, ScreenedDeniedAddressItemModel model)
		{
			AssertEquals(addressMatchInfo, model.AddressMatchInfo);
			AssertEquals(profileAddressInfo, model.ProfileAddressInfo);
			AssertEquals(expectedDisplayScore, model.DisplayScore);
			AssertEquals(expectedScoreGrade, model.ScoreGrade);
		}
	}
}
