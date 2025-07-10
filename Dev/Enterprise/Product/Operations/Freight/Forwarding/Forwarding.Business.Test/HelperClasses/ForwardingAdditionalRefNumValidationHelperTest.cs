using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingAdditionalRefNumValidationHelperTest : TestCaseWithFactory
	{
		public void TestFormatValidationRUC()
		{
			ZString number;

			number = "AnObviouslyWrongRUC";
			Assert(number + " should be invalid.", !ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(number));

			number = "6AU123456789119ExtraStuffThatMakesThisNumberWayTooLongEvenThoughTheStartIsValid";
			Assert(number + " should be invalid.", !ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(number));

			number = "6AU123456789D0VAHK11N E";
			Assert(number + " should be invalid.", !ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(number));

			number = "6AU123456789119D0VAHK11N";
			Assert(number + " should be valid.", ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(number));

			number = "6AU123456789D0VAHK11N";
			Assert(number + " should be valid.", ForwardingAdditionalRefEntryNumValidationHelper.IsRUCFormatValid(number));
		}

		public void TestGetDuplicateRUCParent_Shipment()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "1234";

			var shipment1EntryNum = shipment1.Numbers.AddNew();
			shipment1EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			shipment1EntryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var shipment2 = otherFactory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "abcd";

			var shipment2EntryNum = shipment2.Numbers.AddNew();
			shipment2EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			shipment2EntryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";

			var consol1 = otherFactory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "ded";

			var consol1EntryNum = consol1.Numbers.AddNew();
			consol1EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			consol1EntryNum.CE_EntryNum = "6AU123456789119ALDU1N";

			otherFactory.Save();

			var duplicateNumParent = ForwardingAdditionalRefEntryNumValidationHelper.GetDuplicateRUCParent(shipment2EntryNum) as ForwardingShipment;
			AssertNotNull(duplicateNumParent);
			AssertEquals("The parent of the duplicate number should be " + shipment1.HumanReadableName, shipment1.JS_UniqueConsignRef, duplicateNumParent.JS_UniqueConsignRef);

			shipment2EntryNum.CE_EntryNum = "6AU123456789119ALDU1N";
			var uniqueNumParent = ForwardingAdditionalRefEntryNumValidationHelper.GetDuplicateRUCParent(shipment2EntryNum);
			AssertNull("Shipment RUC should not match with consol MRUC", uniqueNumParent);
		}

		public void TestGetDuplicateRUCParent_Consol()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "1234";

			var consol1EntryNum = consol1.Numbers.AddNew();
			consol1EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			consol1EntryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var consol2 = otherFactory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "abcd";

			var consol2EntryNum = consol2.Numbers.AddNew();
			consol2EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			consol2EntryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";

			var shipment1 = otherFactory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "ded";

			var shipment1EntryNum = shipment1.Numbers.AddNew();
			shipment1EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			shipment1EntryNum.CE_EntryNum = "6AU123456789119ALDU1N";

			otherFactory.Save();

			var duplicateNumParent = ForwardingAdditionalRefEntryNumValidationHelper.GetDuplicateRUCParent(consol2EntryNum) as ForwardingConsol;
			AssertNotNull(duplicateNumParent);
			AssertEquals("The parent of the duplicate number should be " + consol1.HumanReadableName, consol1.JK_UniqueConsignRef, duplicateNumParent.JK_UniqueConsignRef);

			consol2EntryNum.CE_EntryNum = "6AU123456789119ALDU1N";
			var uniqueNumParent = ForwardingAdditionalRefEntryNumValidationHelper.GetDuplicateRUCParent(consol2EntryNum);
			AssertNull("Consol MRUC should not match with shipment RUC", uniqueNumParent);
		}
	}
}
