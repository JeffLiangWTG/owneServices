using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC060CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var listTypeOfControls = new List<INCTSTypeOfControlsXmlProvider>
			{
				Mock.Of<INCTSTypeOfControlsXmlProvider>(toc =>
					toc.SequenceNumeric == 1 &&
					toc.Type == NCTS5TypeOfControlTypes.Codes.DocumentaryControls &&
					toc.Text == "Text TOC1"),
				Mock.Of<INCTSTypeOfControlsXmlProvider>(toc =>
					toc.SequenceNumeric == 2 &&
					toc.Type == NCTS5TypeOfControlTypes.Codes.IdentificationOfConsignmentAndSeals &&
					toc.Text == "Text TOC2"),
			};
		var listRequestedDocument = new List<INCTSRequestedDocumentXmlProvider>
			{
				Mock.Of<INCTSRequestedDocumentXmlProvider>(rd =>
					rd.SequenceNumeric == 1 &&
					rd.DocumentType == "DT1" &&
					rd.Description == "Doc Type 1"),
				Mock.Of<INCTSRequestedDocumentXmlProvider>(rd =>
					rd.SequenceNumeric == 2 &&
					rd.DocumentType == "DT2" &&
					rd.Description == "Doc Type 2"),
			};

		var interpreter = new CC060CMessageInterpreter();

		var controlDateAndTime = new DateTime(2024, 8, 9, 14, 09, 32);
		var mockCC060C = new Mock<ICC060CDataProvider>();
		mockCC060C.Setup(m => m.ControlNotificationDateAndTime).Returns(controlDateAndTime);
		mockCC060C.Setup(m => m.NotificationType).Returns(NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest);
		mockCC060C.Setup(m => m.TypeOfControls).Returns(listTypeOfControls);
		mockCC060C.Setup(m => m.RequestedDocument).Returns(listRequestedDocument);
		var result = interpreter.Interpret(mockCC060C.Object);
		AssertContains("New Customs Status: Decision to Control Notification</br>Status granted on: 09/08/2024 14:09:32</br>Type of Notification: 1 Additional documents request</br></br>Type of Control 1: 10 Documentary controls Text TOC1</br>Type of Control 2: 41 Identification of consignment and seals Text TOC2</br>Document 1: DT1 Doc Type 1</br>Document 2: DT2 Doc Type 2", result);
		mockCC060C.VerifyAll();
	}
}
