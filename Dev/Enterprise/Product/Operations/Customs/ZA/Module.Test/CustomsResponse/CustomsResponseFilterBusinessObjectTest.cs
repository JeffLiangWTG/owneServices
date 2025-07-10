using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CustomsResponseFilterBusinessObject))]
	sealed class CustomsResponseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLRNFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.LRNNumber];
			filter.IsActive = true;
			filter.Property = "00626166JSA20160331008480";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, false, false, false, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, true, true, true, true, true, true, true, false);
			filter.Property = "00626166JSA20160331008482";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, false, false, true, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, true, true, false, true, false);
			filter.Property = "00626166JSA20160331008483";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, true, true, true, true, false);
			filter.Property = "00626166";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, true, false);
			filter.Property = "DBN";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, true, true, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, true, true, false, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, false, false, true, false, false);
		}

		public void TestTimeReceivedFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleDateFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.TimeReceived];
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2016, 11, 20);
			filter.Property2 = new ZDateTime(2016, 11, 21);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, true, false, true, true, false);
		}

		public void TestReceivingProfileFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.ReceivingProfile];
			filter.IsActive = true;
			filter.Property = "62";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, false, false);
			filter.Property = "6";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, false, false);
			filter.Property = "2811";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, true, true, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, true, true, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, false, false, true, true, false);
		}

		public void TestAgentCodeFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.AgentCode];
			filter.IsActive = true;
			filter.Property = "00626166";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, false, false);
			filter.Property = "00";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, true, true, false, false, false);
			filter.Property = "2811";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, true, true, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, true, true, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, false, false, true, true, false);
		}

		public void TestMRNFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.MRNNumber];
			filter.IsActive = true;
			filter.Property = "JSA201603315000938";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, false, false, false, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, true, true, true, true, true, true, true, false);
			filter.Property = "JSA";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, false, false);
			filter.Property = "20161028";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, true, true, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, true, true, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, false, false, true, true, false);
		}

		public void TestMasterTransportDocumentFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.MasterTransportDocument];
			filter.IsActive = true;
			filter.Property = "083-01203226";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, false, false, false, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, true, true, true, true, true, true, true, false);
			filter.Property = "083";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, false, false);
			filter.Property = "26297A";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, true, true, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, true, true, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, false, false, true, true, false);
		}

		public void TestCustomsOfficeFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.CustomsOffice];
			filter.IsActive = true;
			filter.Property = "JSA";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
		}

		public void TestContainerFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.Container];
			filter.IsActive = true;
			filter.Property = "SUDU1769365";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, false, false, false, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, true, true, true, true, true, true, true, false);
			filter.Property = "SUDU";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, true, true, false, false, false);
			filter.Property = "11";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, false, false, false, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, true, true, true, true, true, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, true, true, true, false, false, false);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, false, false, false, true, true, false);
		}

		public void TestEntryStatusFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.EntryStatus];
			filter.IsActive = true;
			filter.Property = "1";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
		}

		public void TestLinkedToAJobFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.LinkedToAJob];
			filter.IsActive = true;
			filter.Property0 = false;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, true, false, true, true, false);
			filter.Property0 = true;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, false, true, false, false, false);
		}

		public void TestAgentFilter()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			agent1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00626166", Core.Constants.CountryCodes.SouthAfrica);
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			agent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00281124", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.Agent];
			filter.IsActive = true;
			filter.Property = agent1.PK;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, false, false, false, false, true, true, false);
			filter.Property = agent2.PK;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, true, true, false, false, false, false, false);
		}

		public void TestMessageTypeFilter()
		{
			var collection = new CustomsResponseCollection(Factory);
			var filterBO = new CustomsResponseFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO[CustomsResponseFilterBusinessObject.Constants.MessageType];
			filter.IsActive = true;
			filter.Property = CustomsResponseMessageTypeList.Codes.RES;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, true, true, true, true, false);
			filter.Property = "AAA";
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, false, false, false, false, false, false, false, false, false);
			filter.Property = "";
			collection.LoadWithMoreFiltering(filterBO.Filter);
			AssertCollectionContainsMessages(collection, true, true, true, true, true, true, true, true, false);
		}

		void AssertCollectionContainsMessages(CustomsResponseCollection collection, bool containsMessage1, bool containsMessage2, bool containsMessage3, bool containsMessage4, bool containsMessage5, bool containsMessage6, bool containsMessage7, bool containsMessage8, bool containsMessage9)
		{
			AssertCollectionContainsMessage(1, message1, collection, containsMessage1);
			AssertCollectionContainsMessage(1, message1Cart, collection, containsMessage1);
			AssertCollectionContainsMessage(2, message2, collection, containsMessage2);
			AssertCollectionContainsMessage(2, message2Cart, collection, containsMessage2);
			AssertCollectionContainsMessage(3, message3, collection, containsMessage3);
			AssertCollectionContainsMessage(3, message3Cart, collection, containsMessage3);
			AssertCollectionContainsMessage(4, message4, collection, containsMessage4);
			AssertCollectionContainsMessage(4, message4Cart, collection, containsMessage4);
			AssertCollectionContainsMessage(5, message5, collection, containsMessage5);
			AssertCollectionContainsMessage(5, message5Cart, collection, containsMessage5);
			AssertCollectionContainsMessage(6, message6, collection, containsMessage6);
			AssertCollectionContainsMessage(7, message7, collection, containsMessage7);
			AssertCollectionContainsMessage(7, message7Cart, collection, containsMessage7);
			AssertCollectionContainsMessage(8, message8, collection, containsMessage8);
			AssertCollectionContainsMessage(8, message8Cart, collection, containsMessage8);
			AssertCollectionContainsMessage(9, message9, collection, containsMessage9);
			AssertCollectionContainsMessage(9, message9Cart, collection, containsMessage9);
		}

		void AssertCollectionContainsMessage(int index, EDIMessage message, CustomsResponseCollection collection, bool containsMessage)
		{
			if (containsMessage)
			{
				AssertCollectionContains("Collection should contains message" + index, message, collection);
			}
			else
			{
				AssertCollectionNotContains("Collection should NOT contains message" + index, message, collection);
			}
		}

		EDIMessage message1;
		EDIMessage message1Cart;
		EDIMessage message2;
		EDIMessage message2Cart;
		EDIMessage message3;
		EDIMessage message3Cart;
		EDIMessage message4;
		EDIMessage message4Cart;
		EDIMessage message5;
		EDIMessage message5Cart;
		EDIMessage message6;
		EDIMessage message7;
		EDIMessage message7Cart;
		EDIMessage message8;
		EDIMessage message8Cart;
		EDIMessage message9;
		EDIMessage message9Cart;

		protected override void SetUp()
		{
			base.SetUp();
			var sarsCART = "SARSCART";
			message1 = CreateMessage(1, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008480", "JSA201603315000938", "00626166", "JSA", "083-01203226", "1", new string[] { "SUDU1769365" });
			message1Cart = CreateMessage(11, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008480", "JSA201603315000938", "00626166", "JSA", "083-01203226", "1", new string[] { "SUDU1769365" }, "", sarsCART);
			message2 = CreateMessage(2, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008481", "JSA201603315000939", "00626166", "JSA", "083-01203227", "1", new string[] { "SUDU1769366" });
			message2Cart = CreateMessage(21, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008481", "JSA201603315000939", "00626166", "JSA", "083-01203227", "1", new string[] { "SUDU1769366" }, "", sarsCART);
			message3 = CreateMessage(3, new ZDateTime(2016, 11, 21), "00281124", "00281124DBN20161028014767", "DBN201610285058442", "00281124", "DBN", "SUDUSUDU26297A20SYN0", "6", new string[] { "CNT1111", "CNT1122" });
			message3Cart = CreateMessage(31, new ZDateTime(2016, 11, 21), "00281124", "00281124DBN20161028014767", "DBN201610285058442", "00281124", "DBN", "SUDUSUDU26297A20SYN0", "6", new string[] { "CNT1111", "CNT1122" }, "", sarsCART);
			message4 = CreateMessage(4, new ZDateTime(2016, 11, 21), "00281124", "00281124DBN20161028014768", "DBN201610285058443", "00281124", "DBN", "SUDUSUDU26297A20SYN0", "6", System.Array.Empty<string>());
			message4Cart = CreateMessage(41, new ZDateTime(2016, 11, 21), "00281124", "00281124DBN20161028014768", "DBN201610285058443", "00281124", "DBN", "SUDUSUDU26297A20SYN0", "6", System.Array.Empty<string>(), "", sarsCART);
			message5 = CreateMessage(5, new ZDateTime(2016, 11, 21), "", "", "", "", "", "", "", System.Array.Empty<string>());
			message5Cart = CreateMessage(51, new ZDateTime(2016, 11, 21), "", "", "", "", "", "", "", System.Array.Empty<string>(), "", sarsCART);
			message7 = CreateMessage(7, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008482", "JSA201603315000940", "00626166", "JSA", "083-01203228", "1", new string[] { "SUDU1769367" }, "963");
			message7Cart = CreateMessage(71, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008482", "JSA201603315000940", "00626166", "JSA", "083-01203228", "1", new string[] { "SUDU1769367" }, "963", sarsCART);
			message8 = CreateMessage(8, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008483", "JSA201603315000941", "00626166", "JSA", "083-01203229", "1", new string[] { "SUDU1769368" }, "964");
			message8Cart = CreateMessage(81, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008483", "JSA201603315000941", "00626166", "JSA", "083-01203229", "1", new string[] { "SUDU1769368" }, "964", sarsCART);
			message9 = CreateMessage(9, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008480", "JSA201603315000942", "00626166", "JSA", "083-01203228", "1", new string[] { "SUDU1769369" });
			message9Cart = CreateMessage(91, new ZDateTime(2016, 11, 20), "62", "00626166JSA20160331008480", "JSA201603315000942", "00626166", "JSA", "083-01203228", "1", new string[] { "SUDU1769369" }, "", sarsCART);
			message9.EM_MessageType = CustomsResponseMessageTypeList.Codes.GEN;
			message9Cart.EM_MessageType = CustomsResponseMessageTypeList.Codes.GEN;
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = "6";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message6 = Factory.New<CUSRESEDIMessage>();
			message6.EM_ReceiveTransmit = "RCV";
			message6.EM_Status = "QUE";
			message6.EM_MessageNum = "IN1";
			message6.EM_SystemCreateTimeUtc = new ZDateTime(2016, 11, 22);
			message6.EM_EI = interchange.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			message6.EM_LinkedObject = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
		}

		CUSRESEDIMessage CreateMessage(int interchangeNum, ZDateTime receivedTime, string receivingProfile, string lrn, string mrn, string agent, string customsOffice, string transportDocumentNumber, string entryStatus, string[] containerNumbers, string lrnType = "", string interchangeRecipient = "SARSDECT")
		{
			var messageText = ZString.Format(@"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+{0}:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+{3}::ZZZ'
LOC+14+A2::ZZZ'
GIS+{5}:120:ZZZ:N'
{6}
NAD+AG+{2}'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:{4}'
DTM+137:20160331:102'
RFF+ABT:{1}'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'", lrn, mrn, agent, customsOffice, transportDocumentNumber, entryStatus, string.Format(containerNumbers.Length > 0 ? "EQD+CN+{0}'" : "{0}", string.Join("\'EQD+CN+", containerNumbers)));
			if (!string.IsNullOrEmpty(lrnType))
			{
				messageText = messageText.Replace("BGM+962", "BGM+" + lrnType);
			}

			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_From = "SARS";
			interchange.EI_To = "TEST";
			interchange.EI_InterchangeType = "ZAC";
			interchange.EI_InterchangeNum = interchangeNum.ToString();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_HeaderText = ZString.Format("UNB+UNOB:4+" + interchangeRecipient + "+{0}::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'", receivingProfile);
			var message = Factory.New<CUSRESEDIMessage>();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = messageText.Replace("\r\n", "");
			message.EM_MessageNum = "IN1";
			message.EM_SystemCreateTimeUtc = receivedTime;
			message.EM_EI = interchange.PK;
			return message;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CustomsResponseFilterBusinessObject();
		}
	}
}
