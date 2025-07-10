using System.Globalization;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(BaseEDIMessage))]
public abstract class BaseEdiMessageTest<T> : EDIMessageTest
	where T : BaseEDIMessage
{
	public void TestMessageDefaults() => AssertEquals(ApplicationCode, Message.EM_ApplicationCode);

	[UseSnapshotProtection]
	public void TestMessageReferenceNumber()
	{
		const int maxValueForPLMessageControlNumber = 9999999;
		var plMessageControlNumberFountain = Environment.Env.NumberFountains.PLMessageControlNumber(ApplicationCode, ZDateTime.Now.ToString("yy", CultureInfo.InvariantCulture));

		plMessageControlNumberFountain.SetNext(Factory, maxValueForPLMessageControlNumber);
		Message.AssignMessageNumber();
		AssertEquals("EM_MessageNum value must consistently come from from PLMessageControlNumber numbers fountain", ExpectedMessageNumPrefix + "9999999", Message.EM_MessageNum);
	}

	public virtual void TestMessageIdentificationPlaceholder()
	{
		Message.EM_MessageText = $"<Msg>{BaseEDIMessage.PLMessageNumberPlaceHolder}</Msg>";
		CertificateHelper.SetUpTestCertificate();
		Factory.Save();

		var expectedMessageNumber = Message.EM_MessageNum;
		AssertStartsWith("Message Identification placeholder should be replaced", $"<Msg>{expectedMessageNumber}", Message.EM_MessageText);
	}

	protected abstract string ApplicationCode { get; }

	protected virtual string ExpectedMessageNumPrefix => ZDateTime.Now.ToString("yy", CultureInfo.InvariantCulture);

	public void TestClearMessageNumberOnFailureToSave()
	{
		AssertEquals(true, Message.ClearMessageNumberOnFailureToSave);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Message = Factory.New<T>();
	}

	protected T Message { get; private set; }
}
