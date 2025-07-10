using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC006CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var dataprovider = Mock.Of<ICC006CDataProvider>(m => m.CustomsOfficeOfDestinationActualReferenceNumber == "RefNumber" && m.ArrivalDateAndTimeActual == new DateTime(2022, 4, 1, 12, 34, 56));
		var result = new CC006CMessageInterpreter().Interpret(dataprovider);
		AssertEquals("Arrival advice for NCTS departure received.</br>The movement arrived on 01/04/2022 12:34:56 at office of destination RefNumber.</br>The guarantee for this movement is credited and be used for a new movement.", result);
	}
}
