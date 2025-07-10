using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(USInBondMoveHeaderDocumentSupporter))]
	class USInBondMoveHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProviders()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB234234";
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB636985";
			Factory.Save();

			var inbondMoveHeader = (IDocumentSupportable)Factory.Load<USInBondMoveHeader>(moveHeader1.PK);
			var providers = inbondMoveHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusInBondHeaderDocumentSupporter.CBPForm7512DataContextValue), null);
			AssertEquals(1, providers.Length);
			var printedBizObj = (CBP7512Document)providers[0].ParentBusinessObject;
			AssertEquals("INB234234", printedBizObj.FormattedEntryNumber);

			inbondMoveHeader = Factory.Load<USInBondMoveHeader>(moveHeader2.PK);
			providers = inbondMoveHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusInBondHeaderDocumentSupporter.CBPForm7512DataContextValue), null);
			AssertEquals(1, providers.Length);
			printedBizObj = (CBP7512Document)providers[0].ParentBusinessObject;
			AssertEquals("INB636985", printedBizObj.FormattedEntryNumber);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MWB123";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MWB456";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			Factory.Save();

			return Factory.Load<USInBondMoveHeader>(moveHeader.PK);
		}
	}
}
