using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC045CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC045CMessageInterpreter();
		var mockCC045C = Mock.Of<ICC045CDataProvider>(provider =>
			provider.WriteOffDate == new DateTime(2024, 8, 1) &&
			provider.Guarantor == Mock.Of<INCTSPartyWithAddressProvider>(party =>
				party.Id == "123456789" &&
				party.Name == "Guarantor Name" &&
				party.Address == Mock.Of<INCTSAddressProvider>(address =>
					address.StreetAndNumber == "StreetAndNumber" &&
					address.Postcode == "1234 AB" &&
					address.City == "City" &&
					address.Country == "NL"
				)
			)
		);
		var result = interpreter.Interpret(mockCC045C);
		AssertEquals("Write-Off notification for NCTS departure received at 01/08/2024</br>The guarantor for this declaration is 123456789 Guarantor Name</br>StreetAndNumber</br>1234 AB City</br>NL", result);
	}
}
