using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationViewStmNumsCollection))]
	sealed class OrganisationViewStmNumsCollectionTest : ActiveBusinessObjectCollectionTestCase<OrganisationViewStmNumsCollection>
	{
		public void TestRelationshipFilter()
		{
			var creationFactory = new BusinessObjectFactory();
			var header = creationFactory.NewWithValidTestData<OrgHeader>();

			var stmNum1 = creationFactory.New<OrganisationViewStmNums>();
			stmNum1.SN_Type = "AAA";
			stmNum1.SN_Owner = header.PK;

			var stmNum2 = creationFactory.New<OrganisationViewStmNums>();
			stmNum2.SN_Type = "BBB";
			stmNum2.SN_Owner = header.PK;

			var stmNum3 = creationFactory.New<OrganisationViewStmNums>();
			stmNum3.SN_Type = "XXX";
			stmNum3.SN_Owner = ZGuid.NewZGuid();

			creationFactory.Save();

			header = Factory.Load<OrgHeader>(header.PK);
			var collection = new OrganisationViewStmNumsCollection(header);

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "BBB" }, collection.Select(v => v.SN_Type));
		}

		public void TestTryGetNumberFountain()
		{
			var creationFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var header = creationFactory.NewWithValidTestData<OrgHeader>();

			var ssccStmNum = creationFactory.New<OrganisationViewStmNums>();
			ssccStmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			ssccStmNum.SN_Prefix = "1234567";
			ssccStmNum.SN_Owner = header.PK;

			var anotherStmNum = creationFactory.New<OrganisationViewStmNums>();
			anotherStmNum.SN_Type = "FOO";
			anotherStmNum.SN_Owner = header.PK;

			creationFactory.Save();

			var org = Factory.Load<OrgHeader>(header.PK);
			AssertNull("Prefix is not match", org.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, ""));
			AssertNull("Prefix is not match", org.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, null));
			AssertStmAreEquals("SSCCBarCode fountain", ssccStmNum, org.OrgFountains.TryGetStmNums(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "1234567"));

			AssertNull("Matched by code, but fountain not created - case of incorrect setup", org.OrgFountains.TryGetNumberFountain("FOO", ""));

			AssertNull("Not matched by code", org.OrgFountains.TryGetNumberFountain("BAR", ""));
			AssertNull("Not matched by code", org.OrgFountains.TryGetStmNums("BAR", ""));
		}

		public void TestTryGetNumberFountain_EmptyPrefix()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();

				var emptyTRF = header.OrgFountains.AddNew();
				emptyTRF.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				emptyTRF.SN_Prefix = "";
				emptyTRF.SN_MaximumValue = 99999999;
				emptyTRF.SN_Owner = header.PK;

				var actualTRF = header.OrgFountains.AddNew();
				actualTRF.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				actualTRF.SN_Prefix = "1234567";
				actualTRF.SN_MaximumValue = 99999999;
				actualTRF.SN_Owner = header.PK;
				Factory.Save();

				AssertStmAreEquals("Should return actualTRF", emptyTRF, header.OrgFountains.TryGetStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, ""));
				var fountainEmptyTRF = header.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "");
				AssertEquals("00000001", fountainEmptyTRF.GetNextFormatted(Factory));

				var fountainActualTRF = header.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");
				AssertEquals("123456700000001", fountainActualTRF.GetNextFormatted(Factory));
			}
		}

		public void TestTryGetNumberFountain_NoEmptyPrefixes()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();

				var actualTRF = header.OrgFountains.AddNew();
				actualTRF.SN_Type = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
				actualTRF.SN_Prefix = "1234567";
				actualTRF.SN_MaximumValue = 99999999;
				actualTRF.SN_Owner = header.PK;
				Factory.Save();

				AssertEquals("There are no ranges with an empty prefix, should return null as we allow entry of empty prefixes.", null, header.OrgFountains.TryGetStmNums(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, ""));
				AssertEquals("There are no ranges with an empty prefix, should return null as we allow entry of empty prefixes.", null, header.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, ""));

				var fountainActualTRF = header.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, "1234567");
				AssertEquals("123456700000001", fountainActualTRF.GetNextFormatted(Factory));
			}
		}

		void AssertStmAreEquals(string message, OrganisationViewStmNums expectedStmNum, OrganisationViewStmNums actualStmNum)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("SN_Type", expectedStmNum.SN_Type, actualStmNum.SN_Type);
				AssertEquals("SN_Owner", expectedStmNum.SN_Owner, actualStmNum.SN_Owner);
				AssertEquals("SN_Owner", expectedStmNum.SN_Prefix, actualStmNum.SN_Prefix);
			});
		}

		public void TestTryGetNumberFountainByZoneId()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var factory = new BusinessObjectFactory();
				var header = factory.NewWithValidTestData<OrgHeader>();

				var stmNum1 = factory.New<OrganisationViewStmNums>();
				stmNum1.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
				stmNum1.SN_Prefix = "11111";
				stmNum1.SN_MinimumValue = 10;
				stmNum1.SN_MaximumValue = 20;
				stmNum1.SN_ZoneIDPrefix = "A";
				stmNum1.SN_Owner = header.PK;

				var stmNum2 = factory.New<OrganisationViewStmNums>();
				stmNum2.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
				stmNum2.SN_Prefix = "22222";
				stmNum2.SN_MinimumValue = 2000;
				stmNum2.SN_MaximumValue = 3000;
				stmNum2.SN_ZoneIDPrefix = "B";
				stmNum2.SN_Owner = header.PK;

				factory.Save();

				var fountain1 = header.OrgFountains.TryGetNumberFountainByZoneID("A", OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber);
				AssertEndsWith("stmNum1 was found", "10", fountain1.GetNextFormatted(factory));

				var fountain2 = header.OrgFountains.TryGetNumberFountainByZoneID("B", OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber);
				AssertNull(fountain2);

				var fountain3 = header.OrgFountains.TryGetNumberFountainByZoneID("A", OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse);
				AssertNull(fountain3);

				var fountain4 = header.OrgFountains.TryGetNumberFountainByZoneID("B", OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse);
				AssertEndsWith("stmNum2 was found", "2000", fountain4.GetNextFormatted(factory));
			}
		}

		public void TestTryGetNumberFountainWithPrefix()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var factory = new BusinessObjectFactory();
				var header = factory.NewWithValidTestData<OrgHeader>();

				var ssccStmNum = factory.New<OrganisationViewStmNums>();
				ssccStmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
				ssccStmNum.SN_Prefix = "1234567";
				ssccStmNum.SN_Owner = header.PK;

				var ssccStmNum2 = factory.New<OrganisationViewStmNums>();
				ssccStmNum2.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
				ssccStmNum2.SN_Prefix = "9999999";
				ssccStmNum2.SN_Owner = header.PK;

				factory.Save();

				var fountain1 = header.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "1234567");
				Assert(fountain1.GetNextFormatted(factory).StartsWith("01234567"));

				var stmNums1 = header.OrgFountains.TryGetStmNums(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "1234567");
				AssertStmAreEquals("Should return stmnum with prefix 1234567", ssccStmNum, stmNums1);

				var fountain2 = header.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "9999999");
				Assert(fountain2.GetNextFormatted(factory).StartsWith("09999999"));

				var stmNums2 = header.OrgFountains.TryGetStmNums(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, "9999999");
				AssertStmAreEquals("Should return stmnum with prefix 9999999", ssccStmNum2, stmNums2);
			}
		}

		public void TestGetFTZPrefixList()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<OrgHeader>();

			var ftzStmNum1 = factory.New<OrganisationViewStmNums>();
			ftzStmNum1.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			ftzStmNum1.SN_ZoneIDPrefix = "1234567";
			ftzStmNum1.SN_ClientPrefix = "AAA";
			ftzStmNum1.SN_Owner = header.PK;

			var ftzStmNum2 = factory.New<OrganisationViewStmNums>();
			ftzStmNum2.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			ftzStmNum2.SN_ZoneIDPrefix = "123456789";
			ftzStmNum2.SN_ClientPrefix = "AAA";
			ftzStmNum2.SN_Owner = header.PK;

			var ftzStmNum3 = factory.New<OrganisationViewStmNums>();
			ftzStmNum3.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			ftzStmNum3.SN_ZoneIDPrefix = "9999999";
			ftzStmNum3.SN_ClientPrefix = "AAA";
			ftzStmNum3.SN_Owner = header.PK;

			var ssccStmNum = factory.New<OrganisationViewStmNums>();
			ssccStmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			ssccStmNum.SN_Prefix = "1111111";
			ssccStmNum.SN_Owner = header.PK;

			var ftwStmNum1 = factory.New<OrganisationViewStmNums>();
			ftwStmNum1.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			ftwStmNum1.SN_ZoneIDPrefix = "8888888";
			ftwStmNum1.SN_ClientPrefix = "AAA";
			ftwStmNum1.SN_Owner = header.PK;

			var ftwStmNum2 = factory.New<OrganisationViewStmNums>();
			ftwStmNum2.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			ftwStmNum2.SN_ZoneIDPrefix = "987654321";
			ftwStmNum2.SN_ClientPrefix = "AAA";
			ftwStmNum2.SN_Owner = header.PK;
			factory.Save();

			var list = header.OrgFountains.GetFTZPrefixList(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse, OrganisationViewStmNums.Schema.OldSN_ZoneIDPrefixMaxLength);
			Assert(!list.ContainsCode("1234567"));
			Assert(!list.ContainsCode("123456789"));
			Assert(!list.ContainsCode("9999999"));
			Assert(!list.ContainsCode("1111111"));
			Assert(list.ContainsCode("8888888"));
			Assert(!list.ContainsCode("987654321"));

			list = header.OrgFountains.GetFTZPrefixList(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse, OrganisationViewStmNums.Schema.SN_ZoneIDPrefixMaxLength);
			Assert(!list.ContainsCode("1234567"));
			Assert(!list.ContainsCode("123456789"));
			Assert(!list.ContainsCode("9999999"));
			Assert(!list.ContainsCode("1111111"));
			Assert(list.ContainsCode("8888888"));
			Assert(list.ContainsCode("987654321"));

			list = header.OrgFountains.GetFTZPrefixList(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber, OrganisationViewStmNums.Schema.OldSN_ZoneIDPrefixMaxLength);
			Assert(list.ContainsCode("1234567"));
			Assert(!list.ContainsCode("123456789"));
			Assert(list.ContainsCode("9999999"));
			Assert(!list.ContainsCode("1111111"));
			Assert(!list.ContainsCode("8888888"));
			Assert(!list.ContainsCode("987654321"));

			list = header.OrgFountains.GetFTZPrefixList(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber, OrganisationViewStmNums.Schema.SN_ZoneIDPrefixMaxLength);
			Assert(list.ContainsCode("1234567"));
			Assert(list.ContainsCode("123456789"));
			Assert(list.ContainsCode("9999999"));
			Assert(!list.ContainsCode("1111111"));
			Assert(!list.ContainsCode("8888888"));
			Assert(!list.ContainsCode("987654321"));
		}

		#region Implementation

		public OrgHeader Header
		{
			get { return header ?? (header = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader header;

		protected override OrganisationViewStmNumsCollection GetCollectionToTest()
		{
			return new OrganisationViewStmNumsCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrganisationViewStmNums>();
		}

		#endregion
	}
}
