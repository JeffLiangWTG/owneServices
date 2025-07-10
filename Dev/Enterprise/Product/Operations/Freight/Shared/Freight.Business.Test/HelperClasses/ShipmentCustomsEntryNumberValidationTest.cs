using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestsSubclassesOf(typeof(ShipmentCustomsEntryNumberValidation))]
	public class ShipmentCustomsEntryNumberValidationTest : TestCaseWithFactory
	{
		public virtual void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ShipmentCustomsEntryNumberValidation(null));
			AssertNoExceptionThrown(() => new ShipmentCustomsEntryNumberValidation(new ShipmentCustomsEntryNumberForTest(Factory.New<CommonShipment>())));
		}

		public virtual void TestCheckEntryType()
		{
			ShipmentCustomsEntryNumber importCustomsEntryNumber = GetNewShipmentCustomsEntryNumber();
			importCustomsEntryNumber.EntryType = "CAN";
			AssertNoError(importCustomsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");
			importCustomsEntryNumber.EntryType = "XXX";
			AssertHasError(importCustomsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");

			ShipmentCustomsEntryNumber exportCustomsEntryNumber = GetNewShipmentCustomsEntryNumber(false);
			AssertEquals("Precondition", 2, exportCustomsEntryNumber.EntryType_List.Count);

			exportCustomsEntryNumber.EntryType = "";
			AssertNoErrors(exportCustomsEntryNumber.EntryTypeInfo);

			exportCustomsEntryNumber = GetNewShipmentCustomsEntryNumber(false);
			exportCustomsEntryNumber.EntryType = "CAN";
			AssertHasError(exportCustomsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");
		}

		public virtual void TestCheckEntryTypeForSystemGenerated()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;

			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);
			customsEntryNumber.EntryType = "ZZZ";

			AssertHasError("Expected error as entry type is invalid and not read only", customsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");

			customsEntryNumber.EntryType_ReadOnlyImplementation = () => { return true; };
			customsEntryNumber.Validation.ValidateEntryType();

			Assert("Should be read only", customsEntryNumber.EntryTypeInfo.ReadOnly);
			AssertNoErrors("Expected no error as entry type is read only", customsEntryNumber.EntryTypeInfo);

			customsEntryNumber.EntryType_ReadOnlyImplementation = () => { return false; };
			customsEntryNumber.Validation.ValidateEntryType();

			Assert("Should not be read only", !customsEntryNumber.EntryTypeInfo.ReadOnly);
			AssertHasError("Expected error as entry type is no longer read only", customsEntryNumber.EntryTypeInfo, "Enter a valid Entry Type.");
		}

		public virtual void TestCheckEntryType_EUCustomsEntryTypeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var entryNumber = GetNewShipmentCustomsEntryNumber();
				entryNumber.EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
				AssertHasWarning("Show a warning if an EU specific code is selected for GB", entryNumber.EntryTypeInfo, "This selection is only valid for locations where EU customs rules apply.");

				entryNumber.EntryType = "XXX";
				AssertNoWarning("Without the warning for non-EU code", entryNumber.EntryTypeInfo, "This selection is only valid for locations where EU customs rules apply.");
			}
		}

		public virtual void TestCheckEntryNumber()
		{
			ShipmentCustomsEntryNumber customsEntryNumber = GetNewShipmentCustomsEntryNumber();
			customsEntryNumber.EntryNumber = "KOOMBAYA";
			AssertNoErrors(customsEntryNumber.EntryNumberInfo);
			customsEntryNumber.EntryNumber = "1234567890";
			AssertNoErrors(customsEntryNumber.EntryNumberInfo);
		}

		public virtual void TestCheckIssueDate()
		{
			ShipmentCustomsEntryNumber customsEntryNumber = GetNewShipmentCustomsEntryNumber();
			customsEntryNumber.EntryType = "CUS";
			customsEntryNumber.EntryNumber = "ABC12345";
			customsEntryNumber.IssueDate = new ZDateTime(2011, 1, 1);
			customsEntryNumber.ExpiryDate = new ZDateTime(2012, 1, 1);
			AssertNoMessageError(customsEntryNumber.IssueDateInfo, "The 'Issue Date' must be before the 'Expiry Date'.");
			customsEntryNumber.IssueDate = new ZDateTime(2012, 2, 1);
			AssertHasMessageError(customsEntryNumber.IssueDateInfo, "The 'Issue Date' must be before the 'Expiry Date'.");
		}

		public virtual void TestCheckExpiryDate()
		{
			ShipmentCustomsEntryNumber customsEntryNumber = GetNewShipmentCustomsEntryNumber();
			customsEntryNumber.EntryType = "CUS";
			customsEntryNumber.EntryNumber = "ABC12345";
			customsEntryNumber.IssueDate = new ZDateTime(2011, 1, 1);
			customsEntryNumber.ExpiryDate = new ZDateTime(2012, 1, 1);
			AssertNoMessageError(customsEntryNumber.ExpiryDateInfo, "The 'Expiry Date' must be after the 'Issue Date'.");
			customsEntryNumber.ExpiryDate = new ZDateTime(2010, 12, 12);
			AssertHasMessageError(customsEntryNumber.ExpiryDateInfo, "The 'Expiry Date' must be after the 'Issue Date'.");
		}

		protected virtual ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return GetNewShipmentCustomsEntryNumber(true);
		}

		protected virtual ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber(bool isExport)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = isExport ? "AUSYD" : "USCHI";
			shipment.JS_RL_NKDestination = isExport ? "USCHI" : "AUSYD";

			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;

			ShipmentCustomsEntryNumberForTest customsEntryNumber = new ShipmentCustomsEntryNumberForTest(shipment);

			return customsEntryNumber;
		}
	}
}
