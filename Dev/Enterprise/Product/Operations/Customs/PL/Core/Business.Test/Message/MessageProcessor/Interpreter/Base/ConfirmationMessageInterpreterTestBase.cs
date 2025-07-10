using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(ConfirmationMessageInterpreter<>))]
public abstract class ConfirmationMessageInterpreterTestBase<TLinkedObject> : MessageInterpreterTest<IConfirmation>
	where TLinkedObject : EnterpriseBusinessObject
{
	public void TestInterpret()
	{
		dataProviderMock.Setup(x => x.ExternalSystemID).Returns("ExternalSystemID");
		dataProviderMock.Setup(x => x.ReferenceToExternalSystemID).Returns("ReferenceToExternalSystemID");

		var expectedInterpretation = "UPP - Official Confirmation of Submission</br>IdentyfikatorPoswiadczenia: ExternalSystemID</br>idDokumentuSEAP: ReferenceToExternalSystemID";

		var interpretation = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestLineDescription()
	{
		AssertHasLine("Description", "UPP - Official Confirmation of Submission");
	}

	public void TestExternalSystemIdOrSeapDocumentID() => CombineAssertions(() =>
	{
		dataProviderMock.Setup(x => x.ExternalSystemID).Returns("ExternalSystemID");
		dataProviderMock.Setup(x => x.ReferenceToExternalSystemID).Returns("ReferenceToExternalSystemID");
		AssertHasLine("ExternalSystemID present", "IdentyfikatorPoswiadczenia: ExternalSystemID");
		AssertHasLine("ReferenceToExternalSystemID present", "idDokumentuSEAP: ReferenceToExternalSystemID");
	});

	protected override void SetUp()
	{
		base.SetUp();

		var attachedObject = CreateAttachedObject();
		interpreter = new ConfirmationMessageInterpreter<TLinkedObject>(attachedObject);
	}

	protected abstract TLinkedObject CreateAttachedObject();

	ConfirmationMessageInterpreter<TLinkedObject> interpreter;

	void AssertHasLine(string description, string expected) =>
		AssertEquals(description, true, interpreter.Interpret(dataProviderMock.Object).Contains(expected));
}
