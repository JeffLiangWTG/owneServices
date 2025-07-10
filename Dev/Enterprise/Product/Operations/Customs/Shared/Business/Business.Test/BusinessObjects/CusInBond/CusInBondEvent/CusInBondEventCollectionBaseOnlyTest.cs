using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusInBondEventCollection<CusInBondEventForTest>))]
	sealed class CusInBondEventCollectionBaseOnlyTest : ActiveBusinessObjectCollectionTestCase<CusInBondEventCollection<CusInBondEventForTest>>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CusInBondEventCollectionForTest(header, ZString.Empty));
		}

		public void TestSetDefaultsForNewElement()
		{
			var collection = new CusInBondEventCollectionForTest(header, CusInBondEventTypes.Codes.Transshipment);
			AssertEquals(CusInBondEventTypes.Codes.Transshipment, collection.AddNew().BN_Type);
		}

		public void TestFilter()
		{
			var seal = CreateCusInBondEvent(CusInBondEventTypes.Codes.Seal);
			var incident = CreateCusInBondEvent(CusInBondEventTypes.Codes.Incident);
			var transshipment = CreateCusInBondEvent(CusInBondEventTypes.Codes.Transshipment);
			var collection = new CusInBondEventCollectionForTest(header, CusInBondEventTypes.Codes.Transshipment);
			var collectionFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Seal", false, seal.MatchesFilter(collectionFilter));
				AssertEquals("Incident", false, incident.MatchesFilter(collectionFilter));
				AssertEquals("Transshipment", true, transshipment.MatchesFilter(collectionFilter));
			});
		}

		protected override CusInBondEventCollection<CusInBondEventForTest> GetCollectionToTest() => new CusInBondEventCollectionForTest(header, CusInBondEventTypes.Codes.Transshipment);

		protected override void SetUp()
		{
			base.SetUp();
			header = (CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
		}
		CusInBondHeader header;

		CusInBondEventForTest CreateCusInBondEvent(ZString type)
		{
			var result = Factory.New<CusInBondEventForTest>();
			result.BN_Type = type;
			result.BN_BH = header.PK;
			return result;
		}

		class CusInBondEventCollectionForTest : CusInBondEventCollection<CusInBondEventForTest>
		{
			public CusInBondEventCollectionForTest(CusInBondHeader master, ZString typeToMatch)
				: base(master, typeToMatch)
			{ }
		}
	}
}
