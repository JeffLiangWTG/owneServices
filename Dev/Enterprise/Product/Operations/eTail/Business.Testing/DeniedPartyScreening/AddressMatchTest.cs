using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(AddressMatch))]
	public class AddressMatchTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddressMatch(new AddressMatchInfo(), new ProfileAddressInfo());
		}

		public void TestAddressItemModel_WithEmptyMatchSCore()
		{
			var model = new AddressMatch(null, GetProfileAddressInfo());
			CombineAssertions(() =>
			{
				AssertNull(model.AddressMatchInfo);
				AssertEquals(0, model.Score);
				AssertEquals(RiskLevel.Low, model.Level);
				AssertEquals("Address", model.Type);
			});
		}

		public void TestAddressItemModel_WithHighMatchSCore()
		{
			var model = new AddressMatch(GetAddressMatchInfo(85), GetProfileAddressInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.AddressMatchInfo);
				AssertEquals("BCCA天安数码城软件大道", model.Name);
				AssertEquals(RiskLevel.High, model.Level);
				AssertEquals(85, model.Score);
			});
		}

		public void TestAddressItemModel_WithMediumMatchSCore()
		{
			var model = new AddressMatch(GetAddressMatchInfo(65), GetProfileAddressInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.AddressMatchInfo);
				AssertEquals("BCCA天安数码城软件大道", model.Name);
				AssertEquals(RiskLevel.Medium, model.Level);
				AssertEquals(65, model.Score);
			});
		}

		public void TestAddressItemModel_WithLowMatchSCore()
		{
			var model = new AddressMatch(GetAddressMatchInfo(60), GetProfileAddressInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.AddressMatchInfo);
				AssertEquals("BCCA天安数码城软件大道", model.Name);
				AssertEquals(RiskLevel.Low, model.Level);
				AssertEquals(60, model.Score);
			});
		}

		public void TestAddressItemModel_WithArgumentNullExcpetion()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new AddressMatch(GetAddressMatchInfo(60), null));
		}

		ProfileAddressInfo GetProfileAddressInfo(Guid? pk = null, Guid? profilePK = null)
		{
			return new ProfileAddressInfo { ID = pk ?? Guid.NewGuid(), Street = "软件大道", City = "天安数码城", StateProvince = "A", Country = "B", PostCode = "C", Language = "ZH-CN", SourceProfileID = profilePK ?? Guid.NewGuid() };
		}

		AddressMatchInfo GetAddressMatchInfo(int score, Guid? profilePK = null)
		{
			return new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "软件大道", Address2 = "天安数码城", City = "A", State = "B", PostCode = "C", Country = "ZH-CN", AdditionalAddressLine = "D" }, MatchingAddressID = Guid.NewGuid(), MatchingAddressScore = score, SourceProfileID = profilePK ?? Guid.NewGuid() };
		}
	}
}

