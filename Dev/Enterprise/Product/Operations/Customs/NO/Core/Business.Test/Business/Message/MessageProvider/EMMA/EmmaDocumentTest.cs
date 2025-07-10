using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaDocument))]
sealed class EmmaDocumentTest : TestCase
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When document is null", () => new EmmaDocument("Doc", null));
		AssertExceptionThrown<ArgumentNullException>("When filename is null", () => new EmmaDocument(null, Mock.Of<IeDoc>()));
		AssertExceptionThrown<ArgumentException>("When filename is empty", () => new EmmaDocument("", Mock.Of<IeDoc>()));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		var documentId = ZGuid.NewZGuid();
		var dateAdded = new ZDateTime(2025, 02, 01);
		var eDocMock = new Mock<IeDoc>();
		eDocMock.Setup(d => d.DocType).Returns(new ZString("INV"));
		eDocMock.Setup(d => d.Description).Returns(new ZString("Invoice"));
		eDocMock.Setup(d => d.UniqueKey).Returns(documentId);
		eDocMock.Setup(d => d.DateAdded).Returns(dateAdded);

		var emmaDocument = new EmmaDocument("SomeFilename", eDocMock.Object);

		AssertEquals("FileName", "SomeFilename", emmaDocument.FileName);
		AssertEquals("DocumentType", "INV", emmaDocument.DocumentType);
		AssertEquals("Description", "Invoice", emmaDocument.Description);
		AssertEquals("Document Id", documentId, emmaDocument.DocumentId);
		AssertEquals("DateAdded", dateAdded, emmaDocument.DateAdded);
		AssertEquals("Document", eDocMock.Object, emmaDocument.Document);
	});
}
