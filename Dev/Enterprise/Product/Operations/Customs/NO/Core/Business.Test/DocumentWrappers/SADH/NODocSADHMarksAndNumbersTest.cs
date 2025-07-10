using System;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHMarksAndNumbers))]
sealed class NODocSADHMarksAndNumbersTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper() => NODocSADHMarksAndNumbers.New("TestValue", Factory);

	public void TestConstructor()
	{
		_ = AssertArgumentExceptionThrown<ArgumentException>("value", () => NODocSADHMarksAndNumbers.New(null, Factory));
	}

	public void TestWrapper()
	{
		var wrapper = new NODocSADHMarksAndNumbers("Test", Factory);
		AssertEquals("Test", wrapper.Value);
	}
}
