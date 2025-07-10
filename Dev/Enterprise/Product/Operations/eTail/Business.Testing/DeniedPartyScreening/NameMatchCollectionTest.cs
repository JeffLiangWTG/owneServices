using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(NameMatchCollection))]
	sealed class NameMatchCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NameMatchCollection>
	{
		protected override NameMatchCollection GetCollectionToTest() => new NameMatchCollection(headerInfo, response);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NameMatch(new NameMatchInfo(), new ProfileNameInfo());

		ProfileHeaderInfo headerInfo;
		DpsResponse response;
		Guid name1Id;
		Guid name2Id;

		protected override void SetUp()
		{
			name1Id = Guid.NewGuid();
			name2Id = Guid.NewGuid();
			headerInfo = new ProfileHeaderInfo
			{
				ProfileNames = new List<ProfileNameInfo>
				{
					new ProfileNameInfo { ID = name1Id, FullName = "John Doe" },
					new ProfileNameInfo { ID = name2Id, FullName = "Jane Smith" }
				}
			};

			response = new DpsResponse
			{
				NameMatches = new List<NameMatchInfo>
				{
					new NameMatchInfo { MatchingNameID = name1Id, MatchingStandardizedValue = "John Doe" }
				}
			};
		}

		public void TestBuildCollection_WithMatchingNames_AddsNameMatches()
		{
			var collection = new NameMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<NameMatch>().Any(x => x.NameMatchInfo.MatchingNameID == name1Id));
			Assert(collection.OfType<NameMatch>().Any(x => x.ProfileNameInfo.ID == name2Id));
		}

		public void TestBuildCollection_WithNoProfileNames_DoesNotAddNameMatches()
		{
			headerInfo.ProfileNames = null;
			var collection = new NameMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}

		public void TestBuildCollection_WithNoMatchingNames_AddsProfileNames()
		{
			response.NameMatches = new List<NameMatchInfo>();
			var collection = new NameMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<NameMatch>().Any(x => x.NameMatchInfo == null));
		}

		public void TestBuildCollection_WithEmptyFullName_DoesNotAddProfileNames()
		{
			headerInfo.ProfileNames = new List<ProfileNameInfo>
			{
				new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "" }
			};
			var collection = new NameMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}
	}
}
