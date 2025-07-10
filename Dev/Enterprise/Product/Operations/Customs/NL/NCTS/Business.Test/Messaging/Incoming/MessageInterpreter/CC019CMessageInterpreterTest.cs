using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC019CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC019CMessageInterpreter();
		var discrepanciesNotificationText = "TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText line1</br>" +
		"TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText line2</br>" +
		"TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText line3</br>" +
		"TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText TestDiscrepanciesNotificationText line4";
		var mockCC019C = Mock.Of<ICC019CDataProvider>(provider =>
			provider.DiscrepanciesNotificationDate == new DateTime(2024, 8, 1) &&
			provider.DiscrepanciesNotificationText == discrepanciesNotificationText &&
			provider.Guarantor == Mock.Of<INCTSPartyWithAddressProvider>(guarantor =>
				guarantor.Id == "TestGuarantorID" &&
				guarantor.Name == "TestGuarantorName" &&
				guarantor.Address == Mock.Of<INCTSAddressProvider>(address =>
					address.StreetAndNumber == "TestGuarantorStreetAndNumber" &&
					address.City == "TestGuarantorCity" &&
					address.Postcode == "TestGuarantorPostalCode" &&
					address.Country == "TestGuarantorCountry"
				)
			)
		);
		var result = interpreter.Interpret(mockCC019C);
		AssertEquals($"Discrepancies for NCTS departure received at: 01/08/2024</br>{discrepanciesNotificationText.Substring(0, 512)}</br>The guarantor for this declaration is :</br>TestGuarantorID</br>TestGuarantorName</br>TestGuarantorStreetAndNumber</br>TestGuarantorPostalCode TestGuarantorCity</br>TestGuarantorCountry", result);
	}
}
