using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class AMSBillMatchingComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var amsHeader1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			AssertEquals(SubApplicationCodeList.Codes.AMS, amsHeader1.BH_ApplicationCode);
			var amsBill1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			amsBill1.B0_BH = amsHeader1.PK;
			var amsMoveHeader1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			amsMoveHeader1.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			amsMoveHeader1.BM_BH = amsHeader1.PK;
			var amsMoveDetail1 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveDetail>();
			amsMoveDetail1.B9_B0 = amsBill1.PK;
			amsMoveDetail1.B9_BM = amsMoveHeader1.PK;
			var amsBOL1 = (IBaseBillOfLading)amsMoveDetail1;
			var amsHeader2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			AssertEquals(SubApplicationCodeList.Codes.AMS, amsHeader2.BH_ApplicationCode);
			var amsBill2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			amsBill2.B0_BH = amsHeader2.PK;
			var amsMoveHeader2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			amsMoveHeader2.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			amsMoveHeader2.BM_BH = amsHeader2.PK;
			var amsMoveDetail2 = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveDetail>();
			amsMoveDetail2.B9_B0 = amsBill2.PK;
			amsMoveDetail2.B9_BM = amsMoveHeader2.PK;
			var amsBOL2 = (IBaseBillOfLading)amsMoveDetail2;
			var inbHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			AssertEquals(SubApplicationCodeList.Codes.MasterInBond, inbHeader.BH_ApplicationCode);
			var inbBill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			inbBill.B0_BH = inbHeader.PK;
			var inbMoveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			inbMoveHeader.BM_BH = inbHeader.PK;
			var inbMoveDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			inbMoveDetail.B9_B0 = inbBill.PK;
			inbMoveDetail.B9_BM = inbMoveHeader.PK;
			var inbBOL = (IBaseBillOfLading)inbMoveDetail;
			var comparer = new AMSBillMatchingComparer();
			AssertEquals(0, comparer.Compare(null, null));
			AssertEquals(0, comparer.Compare(amsBOL1, amsBOL1));
			AssertEquals(1, comparer.Compare(amsBOL1, null));
			AssertEquals(-1, comparer.Compare(null, amsBOL1));
			AssertEquals(-1, comparer.Compare(amsBOL1, inbBOL));
			AssertEquals(1, comparer.Compare(inbBOL, amsBOL1));
			AssertEquals(0, comparer.Compare(inbBOL, inbBOL));
			AssertEquals(-1, comparer.Compare(amsBOL1, amsBOL2));
			AssertEquals(1, comparer.Compare(amsBOL2, amsBOL1));
			amsMoveHeader1.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			AssertEquals(0, comparer.Compare(amsBOL2, amsBOL1));
			amsMoveHeader1.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			AssertEquals(-1, comparer.Compare(amsBOL2, amsBOL1));
			AssertEquals(1, comparer.Compare(amsBOL1, amsBOL2));
			amsMoveHeader2.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			AssertEquals(-1, comparer.Compare(amsBOL1, amsBOL2));
			AssertEquals(1, comparer.Compare(amsBOL2, amsBOL1));
			var list = new SortedList<IBaseBillOfLading, IManifestMessageAttachee>(comparer);
			list.Add(inbBOL, inbBOL.MessageAttachee);
			list.Add(amsBOL2, amsBOL2.MessageAttachee);
			list.Add(amsBOL1, amsBOL1.MessageAttachee);
			AssertEquals(amsBOL1, list.Keys[0]);
			AssertEquals(amsBOL2, list.Keys[1]);
			AssertEquals(inbBOL, list.Keys[2]);
		}
	}
}
