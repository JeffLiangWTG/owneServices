using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransportContractDocumentsTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(this.transportContractDocument.ID, NUnit.Framework.Is.EqualTo("HH111111").Using(CustomComparers.TypeComparison));
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "69517920011";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			IConsignment consignment = new Consignment(entryHeader);
			var transportContractDocument = consignment.TransportContractDocuments.Single(x => x.TypeCode == "741");
			NUnit.Framework.Assert.That(transportContractDocument.ID, NUnit.Framework.Is.EqualTo("695-17920011").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(transportContractDocument.TypeCode, NUnit.Framework.Is.EqualTo("703").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeconsolidator()
		{
			NUnit.Framework.Assert.That(transportContractDocument.Deconsolidator, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_HouseBill = "HH111111";
			transportContractDocument = new Consignment(entryHeader).TransportContractDocuments.Single(x => x.ID == Declaration.JE_HouseBill);
		}

		CusEntryHeader entryHeader;
		JobDeclaration Declaration => entryHeader.Declaration;
		ITransportContractDocument transportContractDocument;
	}
}
