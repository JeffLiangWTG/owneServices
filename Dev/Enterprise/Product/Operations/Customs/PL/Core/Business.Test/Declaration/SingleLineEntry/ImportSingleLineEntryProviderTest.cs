using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportSingleLineEntryProviderTest : TestCaseWithFactory
{
	public void TestGetSingleLineEntry() => AssertType<ImportSingleLineEntry>(new ImportSingleLineEntryProvider().GetSingleLineEntry(GetDeclaration()));

	public void TestDefaultCPCCode_Import() =>
		AssertEquals("1000", new ImportSingleLineEntryProvider().DefaultCPCCode(GetDeclaration(MessageTypeList.Codes.Export)));

	public void TestDefaultCPCCode_Export() =>
		AssertEquals("4000", new ImportSingleLineEntryProvider().DefaultCPCCode(GetDeclaration(MessageTypeList.Codes.Import)));

	public void TestDefaultCPCCode_EmptyMessageType()
	{
		var declaration = GetDeclaration();
		declaration.JE_MessageType = ZString.Empty;
		AssertEquals(ZString.Empty, new ImportSingleLineEntryProvider().DefaultCPCCode(declaration));
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
