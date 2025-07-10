using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.CertificateOfOrigin.Base
{
	sealed class CertificateOfOriginDataObjectWriterTests : TestCase
	{
		readonly TestCertificateOfOriginDataObjectWriter writer = new TestCertificateOfOriginDataObjectWriter();
		const string Type = nameof(Type);

		public void Test_CreateAddInfo_ZDateTime()
		{
			var dateTime = ZDateTime.UtcNow;
			var expectedResult = dateTime.ToISO8601String();
			var result = writer.TestCreateAddInfo(Type, dateTime);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateAddInfo_Bool()
		{
			var boolean = true;
			var expectedResult = boolean.ToString().ToLower();
			var result = writer.TestCreateAddInfo(Type, boolean);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateAddInfo_ZBool()
		{
			var boolean = ZBool.True;
			var expectedResult = true.ToString().ToLower();
			var result = writer.TestCreateAddInfo(Type, boolean);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateAddInfo_String()
		{
			var expectedResult = "This Is A String";
			var result = writer.TestCreateAddInfo(Type, expectedResult);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateContext_ZDateTime()
		{
			var dateTime = ZDateTime.UtcNow;
			var expectedResult = dateTime.ToISO8601String();
			var result = writer.TestCreateContext(Type, dateTime);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateContext_Bool()
		{
			var boolean = true;
			var expectedResult = boolean.ToString().ToLower();
			var result = writer.TestCreateContext(Type, boolean);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateContext_ZBool()
		{
			var boolean = ZBool.True;
			var expectedResult = true.ToString().ToLower();
			var result = writer.TestCreateContext(Type, boolean);

			AssertEquals(expectedResult, result.Value);
		}

		public void Test_CreateContext_String()
		{
			var expectedResult = "This Is A String";
			var result = writer.TestCreateContext(Type, expectedResult);

			AssertEquals(expectedResult, result.Value);
		}
	}

	sealed class TestCertificateOfOriginDataObjectWriter : CertificateOfOriginDataObjectWriter<TestCertificateOfOriginDocDataObject, TestCertificateOfOriginLineItemDocDataObject>
	{
		protected override string DocumentType => throw new System.NotImplementedException();

		public TestCertificateOfOriginDataObjectWriter()
			: base(Mock.Of<IDataWritingManager>(), Mock.Of<IDocument>(), ShipmentDocumentNames.AANZFTACertificateOfOrigin)
		{
		}

		public AddInfo TestCreateAddInfo(string type, object value)
		{
			return CreateAddInfo(type, value);
		}

		public Context TestCreateContext(string type, object value)
		{
			return CreateContext(type, value);
		}
	}

	sealed class TestCertificateOfOriginDocDataObject : CertificateOfOriginDocDataObject<TestCertificateOfOriginLineItemDocDataObject>
	{
		public TestCertificateOfOriginDocDataObject(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}
	}

	sealed class TestCertificateOfOriginLineItemDocDataObject : CertificateOfOriginLineItemDocDataObject
	{
		public TestCertificateOfOriginLineItemDocDataObject(object id) : base(id)
		{
		}
	}
}
