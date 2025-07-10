using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(RegistrationMatchCollection))]
	sealed class RegistrationMatchCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RegistrationMatchCollection>
	{
		protected override RegistrationMatchCollection GetCollectionToTest() => new RegistrationMatchCollection(headerInfo, response);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new RegistrationMatch(new RegistrationCodeMatchInfo(), new ProfileRegistrationCodeInfo());

		ProfileHeaderInfo headerInfo;
		DpsResponse response;
		Guid regCode1Id;
		Guid regCode2Id;

		protected override void SetUp()
		{
			regCode1Id = Guid.NewGuid();
			regCode2Id = Guid.NewGuid();
			headerInfo = new ProfileHeaderInfo
			{
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>
				{
					new ProfileRegistrationCodeInfo { ID = regCode1Id, IdNumber = "12345" },
					new ProfileRegistrationCodeInfo { ID = regCode2Id, IdNumber = "67890" }
				}
			};

			response = new DpsResponse
			{
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>
				{
					new RegistrationCodeMatchInfo { MatchingRegistrationCodeID = regCode1Id, MatchingStandardizedValue = "12345" }
				}
			};
		}

		public void TestBuildCollection_WithMatchingRegistrationCodes_AddsRegistrationMatches()
		{
			var collection = new RegistrationMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<RegistrationMatch>().Any(x => x.RegistrationCodeMatchInfo.MatchingRegistrationCodeID == regCode1Id));
			Assert(collection.OfType<RegistrationMatch>().Any(x => x.ProfileRegistrationCodeInfo.ID == regCode2Id));
		}

		public void TestBuildCollection_WithNoProfileRegistrationCodes_DoesNotAddRegistrationMatches()
		{
			headerInfo.ProfileRegistrationCodes = null;
			var collection = new RegistrationMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}

		public void TestBuildCollection_WithNoMatchingRegistrationCodes_AddsProfileRegistrationCodes()
		{
			response.RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>();
			var collection = new RegistrationMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<RegistrationMatch>().Any(x => x.RegistrationCodeMatchInfo == null));
		}

		public void TestBuildCollection_WithEmptyIdNumber_DoesNotAddProfileRegistrationCodes()
		{
			headerInfo.ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>
			{
				new ProfileRegistrationCodeInfo { ID = Guid.NewGuid(), IdNumber = "" }
			};
			var collection = new RegistrationMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}
	}
}
