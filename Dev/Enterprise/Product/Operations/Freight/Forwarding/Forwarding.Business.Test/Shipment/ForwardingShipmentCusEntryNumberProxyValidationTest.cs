using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCusEntryNumberProxyValidation))]
	sealed class ForwardingShipmentCusEntryNumberProxyValidationTest : ForwardingShipmentCustomsEntryNumberValidationTest
	{
		public override void TestCheckEntryType()
		{
			base.TestCheckEntryType();

			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			cusEntryNumberProxy.EntryType = "XXX";
			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateEntryType();
			AssertHasError(cusEntryNumberProxy.EntryTypeInfo, "Enter a valid Entry Type.");

			cusEntryNumberProxy.CusEntryNumber.CE_ParentTable = "OtherTable";
			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateEntryType();
			AssertNoError(cusEntryNumberProxy.EntryTypeInfo, "Enter a valid Entry Type.");
		}

		public override void TestCheckEntryNumber()
		{
			base.TestCheckEntryNumber();

			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			cusEntryNumberProxy.EntryType = "CAN";
			cusEntryNumberProxy.EntryNumber = "12345";
			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateEntryNumber();
			AssertHasMessageError(cusEntryNumberProxy.EntryNumberInfo, "CAN must be 9 characters.");

			cusEntryNumberProxy.CusEntryNumber.CE_ParentTable = "OtherTable";
			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateEntryNumber();
			AssertNoMessageError(cusEntryNumberProxy.EntryNumberInfo, "CAN must be 9 characters.");
		}

		public void TestCheckEntryNumber_MandatoryValidation()
		{
			var cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			cusEntryNumberProxy.EntryType = "PMT";
			cusEntryNumberProxy.CusEntryNumber.CE_EntryIsSystemGenerated = true;

			Assert("Precondition: Read-only", cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			cusEntryNumberProxy.Validation.ValidateEntryNumber();
			AssertNoWarning(cusEntryNumberProxy.EntryNumberInfo, "You have not entered an Entry Number.");

			cusEntryNumberProxy.CusEntryNumber.CE_EntryIsSystemGenerated = false;

			Assert("Precondition: Read/Write", !cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			cusEntryNumberProxy.Validation.ValidateEntryNumber();
			AssertHasWarning(cusEntryNumberProxy.EntryNumberInfo, "You have not entered an Entry Number.");
		}

		public override void TestCheckIssueDate()
		{
			base.TestCheckIssueDate();

			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			cusEntryNumberProxy.IssueDate = new ZDateTime(2011, 1, 1);
			cusEntryNumberProxy.ExpiryDate = new ZDateTime(2010, 12, 12);
			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateIssueDate();
			AssertHasMessageError(cusEntryNumberProxy.IssueDateInfo, "The 'Issue Date' must be before the 'Expiry Date'.");

			cusEntryNumberProxy.CusEntryNumber.CE_ParentTable = "OtherTable";
			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateIssueDate();
			AssertNoMessageError(cusEntryNumberProxy.IssueDateInfo, "The 'Issue Date' must be before the 'Expiry Date'.");
		}

		public override void TestCheckExpiryDate()
		{
			base.TestCheckExpiryDate();

			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			cusEntryNumberProxy.IssueDate = new ZDateTime(2011, 1, 1);
			cusEntryNumberProxy.ExpiryDate = new ZDateTime(2010, 12, 12);
			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateExpiryDate();
			AssertHasMessageError(cusEntryNumberProxy.ExpiryDateInfo, "The 'Expiry Date' must be after the 'Issue Date'.");

			cusEntryNumberProxy.CusEntryNumber.CE_ParentTable = "OtherTable";
			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsParentShipment);
			cusEntryNumberProxy.Validation.ValidateExpiryDate();
			AssertNoMessageError(cusEntryNumberProxy.ExpiryDateInfo, "The 'Expiry Date' must be after the 'Issue Date'.");
		}

		protected override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_ParentID = shipment.PK;
			cusEntryNumber.CE_ParentTable = shipment.TableName;

			return new ForwardingShipmentCusEntryNumberProxy(shipment, cusEntryNumber);
		}
	}
}
