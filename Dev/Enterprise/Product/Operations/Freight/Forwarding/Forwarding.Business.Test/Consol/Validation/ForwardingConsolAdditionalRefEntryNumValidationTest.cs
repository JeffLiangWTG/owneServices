using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolAdditionalRefEntryNumValidationTest : TestCaseWithFactory
	{
		public void TestValidateBookingReferenceDuplicate()
		{
			var errorMessage = "This Reference Number is a duplicate of this consol's Carrier Booking Reference.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_BookingReference = "123";
			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			entryNum.CE_EntryNum = "Not123";
			AssertNoError(entryNum.CE_EntryNumInfo, errorMessage);

			entryNum.CE_EntryNum = "123";
			AssertHasError(entryNum.CE_EntryNumInfo, errorMessage);
		}

		public void TestValidateMRUCFormat()
		{
			var consol = Factory.New<ForwardingConsol>();
			var entryNum = consol.Numbers.AddNew();

			var errorMessage = "The entered MRUC code does not match the CPF format: <year, 1><country/region, 2><shipper, 11><decade, 1><reference, 1-20> or the CNPJ format: <year, 1><country/region, 2><shipper, 8><decade, 1><reference, 1-23>";
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

		public void TestValidateMRUCUniqueness()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "thicc";

			var consol1EntryNum = consol1.Numbers.AddNew();
			consol1EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			consol1EntryNum.CE_EntryNum = "6AU123456789D0VAHK11N";

			var consol2 = Factory.New<ForwardingConsol>();
			var consol2EntryNum = consol2.Numbers.AddNew();
			consol2EntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			consol2EntryNum.CE_EntryNum = "6AU123456789D0VAHK11N";

			Factory.Save();

			var errorMessage = string.Format("MRUC must be unique and cannot be reused. It is already saved against {0}.", consol1.HumanReadableName);
			AssertHasMessageError(consol2EntryNum.CE_EntryNumInfo, errorMessage);

			consol2EntryNum.CE_EntryNum = "6AU123456789119D0VAHK11N";
			AssertNoMessageError(consol2EntryNum.CE_EntryNumInfo, errorMessage);
		}

		public void TestValidateSystemOnlyEntryType()
		{
			const string expectedErrorSuffix = " is reserved for system use and cannot be manually entered.";

			var consol = Factory.New<ForwardingConsol>();

			var entryTypesForSystemOnly = new ZString[]
			{
				ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference,
			};

			foreach (var entryType in entryTypesForSystemOnly)
			{
				var entryNum = consol.Numbers.AddNew();
				entryNum.CE_EntryType = entryType;
				AssertHasError(entryNum.CE_EntryTypeInfo, entryType + expectedErrorSuffix);

				entryNum.CE_EntryIsSystemGenerated = true;
				entryNum.Validation.ValidateCE_EntryType();
				AssertNoError(entryNum.CE_EntryTypeInfo, entryType + expectedErrorSuffix);

				Factory.Save();

				var reloadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				var reloadedEntryNum = reloadedConsol.Numbers.Cast<CusEntryNumber>().First(x => x.CE_EntryType == entryType);
				AssertEquals(entryType, reloadedEntryNum.CE_EntryType);

				reloadedEntryNum.Validation.ValidateCE_EntryType();
				AssertNoError(reloadedEntryNum.CE_EntryTypeInfo, entryType + expectedErrorSuffix);
			}
		}

		public void TestWarningAddedToCarrierContractNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;

			entryNum.Validation.ValidateCE_EntryType();
			AssertHasRowWarning(
				"Carrier Contract Numbers should always have a warning.",
				entryNum,
				"There is a dedicated field, Carrier Contract No. available on Details > Pre-Allocation. Please use this field for rating, carrier bookings, and allocations. Reference type 'CON' is no longer used for such purposes.");
		}
	}
}
