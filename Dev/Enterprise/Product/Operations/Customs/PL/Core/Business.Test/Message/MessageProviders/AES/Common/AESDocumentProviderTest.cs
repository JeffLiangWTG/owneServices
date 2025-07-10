using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

public class AESDocumentProviderTest : DataProviderTestCase<AESDocumentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusSupportingInfo", "Value cannot be null.\r\nParameter name: cusSupportingInfo",
			() => new AESDocumentProvider(null, false));
	}

	public void TestType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Type is not empty", "ABC", GetProvider().Type);
			cusSupportingInfo.CSI_Code = ZString.Empty;
			AssertNull("Type is empty", GetProvider().Type);
		});
	}

	public void TestDescription()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Description is not empty", "123", GetProvider().Description);
			cusSupportingInfo.CSI_Description = ZString.Empty;
			AssertNull("Type is empty", GetProvider().Description);
			AssertEquals("Description as reference number", "321", GetProvider(true).Description);
		});
	}

	protected override AESDocumentProvider GetProvider() => GetProvider(false);

	protected override void SetUp()
	{
		base.SetUp();
		cusSupportingInfo = Factory.New<CusSupportingInfo>();
		cusSupportingInfo.CSI_Code = "ABC";
		cusSupportingInfo.CSI_Description = "123";
		cusSupportingInfo.CSI_ReferenceNumber = "321";
	}

	protected virtual AESDocumentProvider GetProvider(bool referenceNumberAsDescription) =>
		new AESDocumentProvider(cusSupportingInfo, referenceNumberAsDescription);

	protected CusSupportingInfo cusSupportingInfo;
}
