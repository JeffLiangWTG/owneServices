using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class SingleLineEntryProviderTest : TestCaseWithFactory
{
	public void TestGetSingleLineEntry() => AssertType<SingleLineEntry>(new SingleLineEntryProvider().GetSingleLineEntry(GetDeclaration()));

	public void TestDefaultCPCCode_Import() =>
		AssertEquals("1000", new SingleLineEntryProvider().DefaultCPCCode(GetDeclaration(MessageTypeList.Codes.Export)));

	public void TestDefaultCPCCode_Export() =>
		AssertEquals("4000", new SingleLineEntryProvider().DefaultCPCCode(GetDeclaration(MessageTypeList.Codes.Import)));

	public void TestDefaultCPCCode_EmptyMessageType()
	{
		var declaration = GetDeclaration();
		declaration.JE_MessageType = ZString.Empty;
		AssertEquals(ZString.Empty, new SingleLineEntryProvider().DefaultCPCCode(declaration));
	}

	JobDeclaration GetDeclaration(string messageType = null)
	{
		var declaration = Factory.New<JobDeclaration>();
		if (messageType != null)
		{
			declaration.JE_MessageType = messageType;
		}

		return declaration;
	}
}
