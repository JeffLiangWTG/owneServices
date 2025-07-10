using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
public class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
{
	public void TestSetDefaultsForNewChild()
	{
		var declaration = GetMeANewJobDeclaration();
		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("JI_ValuationCode defaultValue is 1 for import", Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1,
				invoiceLine1.JI_ValuationCode);
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals("JI_ValuationCode defaultValue is empty for export", ZString.Empty, invoiceLine2.JI_ValuationCode);
		});
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override BaseJobDeclaration GetMeANewJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		return declaration;
	}

	protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration)
	{
		return ((JobDeclaration)declaration).CustomsEntryInstructions.FirstOrDefault() ?? ((JobDeclaration)declaration).CustomsEntryInstructions.AddNew();
	}
}
