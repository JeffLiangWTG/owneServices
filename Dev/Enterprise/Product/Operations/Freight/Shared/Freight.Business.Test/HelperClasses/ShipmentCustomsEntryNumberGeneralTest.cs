using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentCustomsEntryNumberGeneralTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown("shipment", typeof(ArgumentNullException), () => new ShipmentCustomsEntryNumberForTest(null));
			AssertNoExceptionThrown(() => new ShipmentCustomsEntryNumberForTest(Factory.New<CommonShipment>()));
		}

		public void TestEntryTypeGetSetMetodsCalled()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			customsEntryNumber.GetCusEntryNumberImplementation = () => cusEntryNumber;

			bool? getMethodCalled = null;
			bool? setMethodCalled = null;
			ZString valueToSet = null;

			customsEntryNumber.GetEntryTypeImplementation = () => { getMethodCalled = true; return ZString.Empty; };
			customsEntryNumber.SetEntryTypeImplementation = (value) => { setMethodCalled = true; valueToSet = value; };

			ZString entryType = customsEntryNumber.EntryType;
			customsEntryNumber.EntryType = "BLA";

			AssertEquals(true, getMethodCalled);
			AssertEquals(true, setMethodCalled);
			AssertEquals("BLA", valueToSet);
		}

		public void TestEntryNumberGetSetMetodsCalled()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			customsEntryNumber.GetCusEntryNumberImplementation = () => cusEntryNumber;

			bool? getMethodCalled = null;
			bool? setMethodCalled = null;
			ZString valueToSet = null;

			customsEntryNumber.GetEntryNumberImplementation = () => { getMethodCalled = true; return ZString.Empty; };
			customsEntryNumber.SetEntryNumberImplementation = (value) => { setMethodCalled = true; valueToSet = value; };

			ZString entryNumber = customsEntryNumber.EntryNumber;
			customsEntryNumber.EntryNumber = "12345";

			AssertEquals(true, getMethodCalled);
			AssertEquals(true, setMethodCalled);
			AssertEquals("12345", valueToSet);
		}

		public void TestIssueDateGetSetMetodsCalled()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			customsEntryNumber.GetCusEntryNumberImplementation = () => cusEntryNumber;

			bool? getMethodCalled = null;
			bool? setMethodCalled = null;
			ZDateTime valueToSet = ZDateTime.Empty;

			customsEntryNumber.GetIssueDateImplementation = () => { getMethodCalled = true; return ZDateTime.Empty; };
			customsEntryNumber.SetIssueDateImplementation = (value) => { setMethodCalled = true; valueToSet = value; };

			ZDateTime isseDate = customsEntryNumber.IssueDate;
			customsEntryNumber.IssueDate = new ZDateTime(2011, 1, 1);

			AssertEquals(true, getMethodCalled);
			AssertEquals(true, setMethodCalled);
			AssertEquals(new ZDateTime(2011, 1, 1), valueToSet);
		}

		public void TestExpiryDateGetSetMetodsCalled()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			customsEntryNumber.GetCusEntryNumberImplementation = () => cusEntryNumber;

			bool? getMethodCalled = null;
			bool? setMethodCalled = null;
			ZDateTime valueToSet = ZDateTime.Empty;

			customsEntryNumber.GetExpiryDateImplementation = () => { getMethodCalled = true; return ZDateTime.Empty; };
			customsEntryNumber.SetExpiryDateImplementation = (value) => { setMethodCalled = true; valueToSet = value; };

			ZDateTime expiryDate = customsEntryNumber.ExpiryDate;
			customsEntryNumber.ExpiryDate = new ZDateTime(2011, 1, 1);

			AssertEquals(true, getMethodCalled);
			AssertEquals(true, setMethodCalled);
			AssertEquals(new ZDateTime(2011, 1, 1), valueToSet);
		}

		public void TestPropertiesReadOnly()
		{
			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(Factory.New<CommonShipment>());
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);
			AssertEquals(false, customsEntryNumber.EntryNumberInfo.ReadOnly);
			AssertEquals(false, customsEntryNumber.IssueDateInfo.ReadOnly);
			AssertEquals(false, customsEntryNumber.ExpiryDateInfo.ReadOnly);
		}

		public void TestEntryTypeList()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);

			AssertContainsExactElementsInAnyOrder(GetCodeCodeDescriptionPairListAsArray(CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, "AU", false)),
				GetCodeCodeDescriptionPairListAsArray(customsEntryNumber.EntryType_List));

			GlbCompany.CurrentCompany.SetCountry("US");

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			AssertContainsExactElementsInAnyOrder(GetCodeCodeDescriptionPairListAsArray(CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, "US", true)),
				GetCodeCodeDescriptionPairListAsArray(customsEntryNumber.EntryType_List));
		}

		public void TestEntryTypeList_ForUK_BeforeBrexit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var factory = new BusinessObjectFactory();
				var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingDataGrouping("EUN");
				var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
				helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2020, 12, 31));
				helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Germany, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

				var dateBREXIT = new ZDateTime(2021, 1, 1, DateTimeKind.Utc);
				var shipment = factory.New<CommonShipment>();
				shipment.JS_SystemCreateTimeUtc = dateBREXIT.AddDays(-1);

				Assert("Pre-condition: GB is not part of EU.", !ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsMemberOfEU("GB"));
				AssertGreaterThan("Pre-condition: Shipment was created earlier then BREXIT.", dateBREXIT, shipment.JS_SystemCreateTimeUtc);

				var customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);

				var entryTypesToAssert = GetCodeCodeDescriptionPairListAsArray(customsEntryNumber.EntryType_List);
				CombineAssertions("EU entries exist in EntryType_List", () => GetCodeCodeDescriptionPairListAsArray(CusEntryNumberTypes.EU.EUCustomsEntryTypeList).ForEach(n => Assert(entryTypesToAssert.Contains(n))));
			}
		}

		public void TestEntryTypeList_ForUK_AfterBrexit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var factory = new BusinessObjectFactory();
				var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingDataGrouping("EUN");
				var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
				helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2020, 12, 31));
				helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Germany, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

				var dateBREXIT = new ZDateTime(2021, 1, 1, DateTimeKind.Utc);
				var shipment = factory.New<CommonShipment>();
				shipment.JS_SystemCreateTimeUtc = dateBREXIT.AddDays(1);

				Assert("Pre-condition: GB is not part of EU.", !ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsMemberOfEU("GB"));
				AssertLessThan("Pre-condition: Shipment was created later then BREXIT.", dateBREXIT, shipment.JS_SystemCreateTimeUtc);

				var customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);

				var entryTypesToAssert = GetCodeCodeDescriptionPairListAsArray(customsEntryNumber.EntryType_List);
				CombineAssertions("EU entries exist in EntryType_List", () => GetCodeCodeDescriptionPairListAsArray(CusEntryNumberTypes.EU.EUCustomsEntryTypeList).ForEach(n => Assert(entryTypesToAssert.Contains(n))));
			}
		}

		public void TestExemptionCodes_AU()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			AssertEquals("prerequisite", true, shipment.IsImport());
			AssertEquals(false, customsEntryNumber.IsEntryNumberInAustralia);
			AssertEquals(false, customsEntryNumber.IsExemptionCode("EXML"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("EXXX"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("CAN"));

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			AssertEquals("prerequisite", false, shipment.IsImport());
			AssertEquals(true, customsEntryNumber.IsEntryNumberInAustralia);
			AssertEquals(true, customsEntryNumber.IsExemptionCode("EXML"));
			AssertEquals(true, customsEntryNumber.IsExemptionCode("EXXX"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("CAN"));

			GlbCompany.CurrentCompany.SetCountry("US");
			AssertEquals("prerequisite", true, shipment.IsImport());
			AssertEquals("exemption codes can be applied for Australia only", false, customsEntryNumber.IsEntryNumberInAustralia);
			AssertEquals(false, customsEntryNumber.IsExemptionCode("EXML"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("EXXX"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("CAN"));

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("prerequisite", false, shipment.IsImport());
			AssertEquals("exemption codes can be applied for Australia only", false, customsEntryNumber.IsEntryNumberInAustralia);
			AssertEquals(false, customsEntryNumber.IsExemptionCode("EXML"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("EXXX"));
			AssertEquals(false, customsEntryNumber.IsExemptionCode("CAN"));
		}

		public void TestProcessCustomsEntryNumbersForSaving()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_EntryType = "CCN";
			cusEntryNumber.CE_EntryNum = "11111";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2011, 1, 1);
			cusEntryNumber.CE_ExpiryDate = new ZDateTime(2012, 1, 1);

			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);

			AssertCusEntryNumber("CCN", "11111", new ZDateTime(2011, 1, 1), new ZDateTime(2012, 1, 1), shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.EntryNumber = (ZString)"Too Long".PadRight(cusEntryNumber.CE_EntryNumInfo.MaxLength + 1, '1');
			AssertCusEntryNumber("CCN", (ZString)"Too Long".PadRight(cusEntryNumber.CE_EntryNumInfo.MaxLength, '1'), new ZDateTime(2011, 1, 1), new ZDateTime(2012, 1, 1), shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.EntryNumber = "22222";
			AssertCusEntryNumber("CCN", "22222", new ZDateTime(2011, 1, 1), new ZDateTime(2012, 1, 1), shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.EntryType = "COC";
			AssertCusEntryNumber("COC", "22222", new ZDateTime(2011, 1, 1), new ZDateTime(2012, 1, 1), shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.IssueDate = new ZDateTime(2011, 6, 6);
			AssertCusEntryNumber("COC", "22222", new ZDateTime(2011, 6, 6), new ZDateTime(2012, 1, 1), shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.ExpiryDate = new ZDateTime(2012, 9, 9);
			AssertCusEntryNumber("COC", "22222", new ZDateTime(2011, 6, 6), new ZDateTime(2012, 9, 9), shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.EntryType = "EXML";
			AssertCusEntryNumber("XML", ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, shipment.CusEntryNumbers[0]);
			AssertEquals(cusEntryNumber, shipment.CusEntryNumbers[0]);

			customsEntryNumber.EntryType = "ZZZ";
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			AssertEquals(true, cusEntryNumber.IsDeleted);
		}

		void AssertCusEntryNumber(ZString expectedEntryType, ZString expectedEntryNum, ZDateTime expectedIssueDate, ZDateTime expectedExpiryDate, CusEntryNumber cusEntryNumber)
		{
			AssertEquals(expectedEntryType, cusEntryNumber.CE_EntryType);
			AssertEquals(expectedEntryNum, cusEntryNumber.CE_EntryNum);
			AssertEquals(expectedIssueDate, cusEntryNumber.CE_IssueDate);
			AssertEquals(expectedExpiryDate, cusEntryNumber.CE_ExpiryDate);
		}

		public void TestReset()
		{
			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(Factory.New<CommonShipment>());

			customsEntryNumber.EntryType = "XXX";
			AssertEquals("XXX", customsEntryNumber.EntryType);

			customsEntryNumber.Reset();
			AssertEquals(ZString.Empty, customsEntryNumber.EntryType);
		}

		public void TestEntryTypeMaxLength()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			AssertEquals("entry type accepts 4 characters because of the exemption codes", 4, customsEntryNumber.EntryTypeMaxLength);

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			AssertEquals("entry type accepts 3 characters because does not contain exception codes", 3, customsEntryNumber.EntryTypeMaxLength);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USCHI";

				AssertEquals("entry type accepts 3 characters because does not contain exception codes", 3, customsEntryNumber.EntryTypeMaxLength);

				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "AUSYD";

				AssertEquals("entry type accepts 3 characters because does not contain exception codes", 3, customsEntryNumber.EntryTypeMaxLength);
			}
		}

		#region Implementation

		string[] GetCodeCodeDescriptionPairListAsArray(CodeDescriptionPairList pairList)
		{
			return pairList.Cast<ICodeDescription>().Select((codeDesc) => string.Format("{0}|{1}", codeDesc.Code, codeDesc.Description)).ToArray();
		}

		#endregion
	}
}
