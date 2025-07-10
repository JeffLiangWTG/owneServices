using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.ctypes;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC056CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var rejectionDescriptionCodeList = new CC056CRejectionDescriptions();
		var rejectionCodeDescriptionList = new CC056CRejectionCodeDescriptions();
		var errorCodeDescriptionList = new CC056CErrorCodeDescriptions();

		foreach (var businessRejectionType in rejectionDescriptionCodeList.GetAllCodes())
		{ 
			foreach (var rejectionCode in rejectionCodeDescriptionList.GetAllCodes())
			{
				foreach (var functionalErrorCode in errorCodeDescriptionList.GetAllCodes())
				{
					AssertInterpret(businessRejectionType, rejectionCode, functionalErrorCode, rejectionDescriptionCodeList.GetDescriptionFromCode(businessRejectionType), rejectionCodeDescriptionList.GetDescriptionFromCode(rejectionCode), errorCodeDescriptionList.GetDescriptionFromCode(functionalErrorCode));
				}
			}
		}
	}

	void AssertInterpret(string businessRejectionType, string rejectionCode, string functionalErrorCode, string businessRejectionTypeDescription, string rejectionCodeDescription, string functionalErrorCodeDescription)
	{
		var interpreter = new CC056CMessageInterpreter();

		var functionalError1 = new FunctionalErrorType04();
		functionalError1.ErrorPointer = "EP1";
		functionalError1.ErrorCode = functionalErrorCode;
		functionalError1.ErrorReason = "bad type one";
		functionalError1.OriginalAttributeValue = "11";

		var functionalError = new List<NCTSFunctionalErrorProvider>
			{
				NCTSFunctionalErrorProvider.New(functionalError1),
			};

		var controlDateAndTime = new DateTime(2022, 4, 1, 12, 34, 56);
		var mockCC056C = new Mock<ICC056CDataProvider>();
		mockCC056C.Setup(m => m.BusinessRejectionType).Returns(businessRejectionType);
		mockCC056C.Setup(m => m.RejectionDateAndTime).Returns(controlDateAndTime);
		mockCC056C.Setup(m => m.RejectionCode).Returns(rejectionCode);
		mockCC056C.Setup(m => m.RejectionReason).Returns("ABC");
		mockCC056C.Setup(m => m.FunctionalErrors).Returns(functionalError);
		var result = interpreter.Interpret(mockCC056C.Object);
		AssertEquals($"Business Rejection Type = {businessRejectionType}, RejectionCode = {rejectionCode}, FunctionalErrorCode = {functionalErrorCode}", $"Declaration received an error for type {businessRejectionType} on 01-04-2022 12:34:56 ({businessRejectionTypeDescription})</br>Reason: {rejectionCode} ABC ({rejectionCodeDescription})</br>Functional error code: {functionalErrorCode} ({functionalErrorCodeDescription})</br>Reason: bad type one</br>Attribute: EP1</br>Element in declaration contains now the value: 11</br>", result);
	}
}
