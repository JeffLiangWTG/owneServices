using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class LicensingMessageSendingObjectValidationAbstractTest<TLicensingMessageSendingObject> : TestCaseWithFactory
		where TLicensingMessageSendingObject : LicensingMessageSendingObject
	{
		public void TestCheckShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = "NX201_07";
			var action = (TLicensingMessageSendingObject)Activator.CreateInstance(typeof(TLicensingMessageSendingObject), header);
			action.ShouldSend = true;
			var targetInfo = action.ShouldSendInfo;
			AssertHasErrorContaining(targetInfo, "The message cannot be sent without a valid declaration TW-VAT number. Go to Declarant > Organization > Details > Config > Registration to create a valid TW-VAT number.");
			AssertNoErrorContaining(targetInfo, "There must be at least one Invoice Line linked to the Licensing Message.");

			header.TW1_ControllingMessageType = "NX101";
			action.ValidateShouldSend();
			AssertHasErrorContaining(targetInfo, "There must be at least one Invoice Line linked to the Licensing Message.");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			org.MainAddress.OA_Address1 = "Address 1";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			action.ValidateShouldSend();
			AssertNoErrorContaining(targetInfo, "The message cannot be sent without a valid declaration TW-VAT number. Go to Declarant > Organization > Details > Config > Registration to create a valid TW-VAT number.");

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			action.ValidateShouldSend();
			AssertNoErrorContaining(targetInfo, "There must be at least one Invoice Line linked to the Licensing Message.");
		}

		public virtual void TestCheckAction()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var sendingObj = (TLicensingMessageSendingObject)Activator.CreateInstance(typeof(TLicensingMessageSendingObject), header);
			var targetInfo = sendingObj.ActionInfo;
			AssertNoErrors(targetInfo);
			sendingObj.ShouldSend = true;
			sendingObj.Action = ZString.Empty;
			AssertHasErrorContaining(targetInfo, "Action is required if Send is ticked.");
			sendingObj.Action = "A";
			AssertHasErrorContaining(targetInfo, "Enter a valid Action.");
			sendingObj.Action = NX101ActionCodeList.Codes._9;
			AssertNoErrors(targetInfo);
		}
	}
}
