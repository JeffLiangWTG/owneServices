using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(AddressMatchCollection))]
	sealed class AddressMatchCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AddressMatchCollection>
	{
		protected override AddressMatchCollection GetCollectionToTest() => new AddressMatchCollection(headerInfo, response);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AddressMatch(new AddressMatchInfo(), new ProfileAddressInfo());

		ProfileHeaderInfo headerInfo;
		DpsResponse response;
		Guid address1Id;
		Guid address2Id;

		protected override void SetUp()
		{
			address1Id = Guid.NewGuid();
			address2Id = Guid.NewGuid();
			headerInfo = new ProfileHeaderInfo
			{
				ProfileAddresses = new List<ProfileAddressInfo>
				{
					new ProfileAddressInfo { ID = address1Id, Street = "123 Main St", City = "Anytown", StateProvince = "CA", PostCode = "12345", Country = "USA" },
					new ProfileAddressInfo { ID = address2Id, Street = "456 Elm St", City = "Othertown", StateProvince = "NY", PostCode = "67890", Country = "USA" }
				}
			};

			response = new DpsResponse
			{
				AddressMatches = new List<AddressMatchInfo>
				{
					new AddressMatchInfo { MatchingAddressID = address1Id, MatchingStandardizedValue = "123 Main St" }
				}
			};
		}

		public void TestBuildCollection_WithMatchingAddresses_AddsAddressMatches()
		{
			var collection = new AddressMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<AddressMatch>().Any(x => x.AddressMatchInfo.MatchingAddressID == address1Id));
			Assert(collection.OfType<AddressMatch>().Any(x => x.ProfileAddressInfo.ID == address2Id));
		}

		public void TestBuildCollection_WithNoProfileAddresses_DoesNotAddAddressMatches()
		{
			headerInfo.ProfileAddresses = null;
			var collection = new AddressMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}

		public void TestBuildCollection_WithNoMatchingAddresses_AddsProfileAddresses()
		{
			response.AddressMatches = new List<AddressMatchInfo>();
			var collection = new AddressMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<AddressMatch>().Any(x => x.AddressMatchInfo == null));
		}

		public void TestBuildCollection_WithEmptyAddressParts_DoesNotAddProfileAddresses()
		{
			headerInfo.ProfileAddresses = new List<ProfileAddressInfo>
			{
				new ProfileAddressInfo { ID = Guid.NewGuid(), Street = "", City = "", StateProvince = "", PostCode = "", Country = "" }
			};
			var collection = new AddressMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}
	}
}

