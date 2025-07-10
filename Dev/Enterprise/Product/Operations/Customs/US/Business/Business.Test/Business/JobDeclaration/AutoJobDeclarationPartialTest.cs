using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class AutoJobDeclarationPartialTest : TestCaseWithFactory
	{
		public void TestShowManufacturerIDInAddressList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, declaration.JE_OA_InvoicerAddress_ZAddress);
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, declaration.JE_OA_ManufacturerAddress_ZAddress);
		}

		public void TestCreatedAddrssOranisationPKOnDeclarant()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			Factory.Save();
			AssertEquals("Stephen.Day Company Code came through the address Wonderfull Day", ZString.Empty, declaration.IORName);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "SDAY";
			org1.OH_FullName = "Stephen Day";
			var orgAddress1 = org1.Addresses.AddNewMainAddress();
			orgAddress1.OA_Code = "WD";
			orgAddress1.OA_Address1 = "Wonderful Day";

			declaration.JE_OA_DeclarantAddress = orgAddress1.PK;
			Factory.Save();
			AssertEquals("Stephen.Day Company Code came through the address Wonderfull Day", org1.OH_FullName, declaration.IORName);
		}

		[TestDate(2014, 12, 16, 12, 30, 35)]
		public void TestHandleJobDeclarationAddInfoConcurrencyIssue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_ConsolACE = true;
			declaration.JE_CustomsCommencedDate = new ZDateTime(2014, 11, 1);
			declaration.US_BondAmount = 10m;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			declaration.US_BondAmount2 = 200m;
			declaration.US_BondProducerAccNo = "123";
			declaration.US_ConsolACE = false;
			declaration.JE_CustomsCommencedDate = new ZDateTime(2014, 10, 10);
			declarationInDiffFactory.US_BondAmount2 = 150m;
			declarationInDiffFactory.US_BondProducerAccNo = "123";
			declarationInDiffFactory.JE_CustomsCommencedDate = new ZDateTime(2014, 12, 10);
			declarationInDiffFactory.US_ClaimPort = "2709";
			Factory.RefreshEnabled = false;
			Factory.Save();
			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				newFactory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertConcurrencyValue(declaration, false, new ZDateTime(2014, 10, 10), 10m, 200m, "");
			AssertConcurrencyValue(declarationInDiffFactory, false, new ZDateTime(2014, 10, 10), 10m, 200m, "2709");
			var message = @"Another user (CargoWise Support @ 16 Dec 2014 12:30:00) has changed these fields.
Bond Amount 2: Yours: '150', Theirs: '200'
Consol ACE: Yours: 'Y', Theirs: 'N'";
			AssertHasWarningContaining(declarationInDiffFactory.JE_MessageTypeInfo, message);
			AssertHasWarningContaining(declarationInDiffFactory.US_BondAmount2Info, @"Another user (CargoWise Support @ 16 Dec 2014 12:30:00) has changed this field.
Yours: '150', Theirs: '200'");
			AssertHasWarningContaining(declarationInDiffFactory.JE_CustomsCommencedDateInfo, @"Another user (CargoWise Support @ 16 Dec 2014 12:30:00) has changed this field.
Yours: '10/12/2014 12:00:00 AM', Theirs: '10/10/2014 12:00:00 AM'");
			AssertHasWarningContaining(declarationInDiffFactory.US_ConsolACEInfo, @"Another user (CargoWise Support @ 16 Dec 2014 12:30:00) has changed this field.
Yours: 'Y', Theirs: 'N'");
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
Declaration B00001000 (CargoWise Support @ 16 Dec 2014 12:30:00)
	Add Info
	Customs Commenced Date", handler.ReportInformationMessage.Trim());

			newFactory.Save();
			AssertConcurrencyValue(declaration, false, new ZDateTime(2014, 10, 10), 10m, 200m, "");
			AssertConcurrencyValue(declarationInDiffFactory, false, new ZDateTime(2014, 10, 10), 10m, 200m, "2709");
			declaration.JE_CustomsCommencedDate = new ZDateTime(2014, 12, 12);
			declaration.US_ClaimPort = "2710";
			handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertConcurrencyValue(declaration, false, new ZDateTime(2014, 12, 12), 10m, 200m, "2709");
			AssertConcurrencyValue(declarationInDiffFactory, false, new ZDateTime(2014, 10, 10), 10m, 200m, "2709");
			message = @"Another user (CargoWise Support @ 16 Dec 2014 12:30:00) has changed these fields.
Claim Port: Yours: '2710', Theirs: '2709'";
			AssertHasWarningContaining(declaration.JE_MessageTypeInfo, message);
			AssertHasWarningContaining(declaration.US_ClaimPortInfo, @"Another user (CargoWise Support @ 16 Dec 2014 12:30:00) has changed this field.
Yours: '2710', Theirs: '2709'");
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
Declaration B00001000 (CargoWise Support @ 16 Dec 2014 12:30:00)
	Add Info", handler.ReportInformationMessage.Trim());
		}

		void AssertConcurrencyValue(JobDeclaration declaration, ZBool consolACE, ZDateTime customsCommencedDate, ZDecimal bondAmount, ZDecimal bondAmount2, ZString claimPort)
		{
			AssertEquals("declaration.JE_CustomsCommencedDate", customsCommencedDate, declaration.JE_CustomsCommencedDate);
			AssertEquals("declaration.US_BondAmount", bondAmount, declaration.US_BondAmount);
			AssertEquals("declaration.US_BondAmount2", bondAmount2, declaration.US_BondAmount2);
			AssertEquals("declaration.US_ClaimPort", claimPort, declaration.US_ClaimPort);
		}
	}
}
