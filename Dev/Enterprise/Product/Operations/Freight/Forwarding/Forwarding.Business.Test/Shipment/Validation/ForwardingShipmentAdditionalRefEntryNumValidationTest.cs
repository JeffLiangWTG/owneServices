using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using AdditionalReferenceNumbersCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentAdditionalRefEntryNumValidationTest : TestCaseWithFactory
	{
		public void TestValidateCON()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;

			var errorMessage = "Only one Carrier Contract Number is allowed to exist.";

			var conNumber1 = shipment.Numbers.AddNew();
			conNumber1.CE_EntryType = AdditionalReferenceNumbersCodes.CON;
			var conNumber2 = shipment.Numbers.AddNew();
			conNumber2.CE_EntryType = AdditionalReferenceNumbersCodes.CON;

			conNumber2.Validation.ValidateCE_EntryNum();

			AssertHasError("Only 1 CON number is allowed", conNumber2.CE_EntryNumInfo, errorMessage);

			shipment.Numbers.RemoveAndDelete(conNumber1);
			conNumber2.Validation.ValidateCE_EntryNum();
			AssertNoError("1 CON number so no error", conNumber2.CE_EntryNumInfo, errorMessage);

			shipment.JS_CarrierContractNumber = "AAAH";
			conNumber2.CE_EntryNum = "BBAA";
			conNumber2.Validation.ValidateCE_EntryNum();

			errorMessage = "Carrier Contract Number is different from Booking > Numbers > CON type.";
			AssertHasError("Numbers should be equal to each other", conNumber2.CE_EntryNumInfo, errorMessage);

			conNumber2.CE_EntryNum = "AAAH";
			conNumber2.Validation.ValidateCE_EntryNum();
			AssertNoError("Numbers are equal", conNumber2.CE_EntryNumInfo, errorMessage);
		}

		public void TestValidateRUCFormat()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var entryNum = shipment.Numbers.AddNew();

			var errorMessage = "The entered RUC code does not match the CPF format: <year, 1><country/region, 2><shipper, 11><decade, 1><reference, 1-20> or the CNPJ format: <year, 1><country/region, 2><shipper, 8><decade, 1><reference, 1-23>";
			entryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum.CE_EntryNum = "AnObviouslyWrongRUC";
			AssertHasMessageError(entryNum.CE_EntryNumInfo, errorMessage);

			entryNum.CE_EntryNum = "6AU123456789119ExtraStuffThatMakesThisNumberWayTooLongButShouldBeTruncated";
			AssertNoMessageError(entryNum.CE_EntryNumInfo, errorMessage);

			entryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";
			AssertNoMessageError(entryNum.CE_EntryNumInfo, errorMessage);

			entryNum.CE_EntryNum = "6AU123456789D0VAHK11N";
			AssertNoMessageError(entryNum.CE_EntryNumInfo, errorMessage);
		}

		public void TestValidateRUCUniqueness()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "thicc";

			var shipment1EntryNum = shipment1.Numbers.AddNew();
			shipment1EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			shipment1EntryNum.CE_EntryNum = "6AU123456789D0VAHK11N";

			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment2EntryNum = shipment2.Numbers.AddNew();
			shipment2EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			shipment2EntryNum.CE_EntryNum = "6AU123456789D0VAHK11N";

			Factory.Save();

			var errorMessage = string.Format("RUC must be unique and cannot be reused. It is already saved against {0}. Apply a suffix if required.", shipment1.HumanReadableName);
			AssertHasMessageError(shipment2EntryNum.CE_EntryNumInfo, errorMessage);

			shipment2EntryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";
			AssertNoMessageError(shipment2EntryNum.CE_EntryNumInfo, errorMessage);
		}

		public void TestWarningForShiLianDanIsUsedInMoreThanOneShipment()
		{
			InsertPackLineTestData("CNSHA", "S00001001", "SLD001");
			InsertEntryNumTestData("CNSHA", "S00001002", "SLD001");

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var cusEntryNumber = InsertEntryNumTestData("CNSHA", "S00001003", "SLD001");
				var expectWarning = "This Shi Lian Dan/Shipping Order Number is already in use on: S00001001, S00001002";
				AssertHasWarning(cusEntryNumber.CE_EntryNumInfo, expectWarning);
			}

			ForwardingPackLine InsertPackLineTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var consol = shipment.Consols.AddNew();
				var container = consol.Containers.AddNew();

				var packline = shipment.OuterPackLines.AddNew();
				packline.SetContainer(container.PK);
				packline.JL_ContainerPackingOrder = 1;
				packline.JL_ExportRefNumber = exportRefNumber;
				return packline;
			}

			CusEntryNumber InsertEntryNumTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var shipmentEntryNum = shipment.Numbers.AddNew();
				shipmentEntryNum.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
				shipmentEntryNum.CE_EntryNum = exportRefNumber;
				return shipmentEntryNum;
			}
		}
	}
}
