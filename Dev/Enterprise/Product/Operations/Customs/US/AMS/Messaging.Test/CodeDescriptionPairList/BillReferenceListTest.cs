using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class BillReferenceListTest : TestCaseWithFactory
	{
		public void TestGetCachedValue()
		{
			var list1 = BillReferenceList.GetCachedValue(Factory);
			var list2 = BillReferenceList.GetCachedValue(Factory);
			AssertEquals("Should be cached", true, object.ReferenceEquals(list1, list2));
			AssertEquals(44, list1.Count);
			AssertEquals(BillReferenceList.Codes._2K, BillReferenceList.Descriptions._2K, list1.GetDescriptionFromCode(BillReferenceList.Codes._2K));
			AssertEquals(BillReferenceList.Codes.BL, BillReferenceList.Descriptions.BL, list1.GetDescriptionFromCode(BillReferenceList.Codes.BL));
			AssertEquals(BillReferenceList.Codes.BM, BillReferenceList.Descriptions.BM, list1.GetDescriptionFromCode(BillReferenceList.Codes.BM));
			AssertEquals(BillReferenceList.Codes.BN, BillReferenceList.Descriptions.BN, list1.GetDescriptionFromCode(BillReferenceList.Codes.BN));
			AssertEquals(BillReferenceList.Codes.CG, BillReferenceList.Descriptions.CG, list1.GetDescriptionFromCode(BillReferenceList.Codes.CG));
			AssertEquals(BillReferenceList.Codes.CN, BillReferenceList.Descriptions.CN, list1.GetDescriptionFromCode(BillReferenceList.Codes.CN));
			AssertEquals(BillReferenceList.Codes.CO, BillReferenceList.Descriptions.CO, list1.GetDescriptionFromCode(BillReferenceList.Codes.CO));
			AssertEquals(BillReferenceList.Codes.CR, BillReferenceList.Descriptions.CR, list1.GetDescriptionFromCode(BillReferenceList.Codes.CR));
			AssertEquals(BillReferenceList.Codes.CSK, BillReferenceList.Descriptions.CSK, list1.GetDescriptionFromCode(BillReferenceList.Codes.CSK));
			AssertEquals(BillReferenceList.Codes.CUB, BillReferenceList.Descriptions.CUB, list1.GetDescriptionFromCode(BillReferenceList.Codes.CUB));
			AssertEquals(BillReferenceList.Codes.CX, BillReferenceList.Descriptions.CX, list1.GetDescriptionFromCode(BillReferenceList.Codes.CX));
			AssertEquals(BillReferenceList.Codes.ED, BillReferenceList.Descriptions.ED, list1.GetDescriptionFromCode(BillReferenceList.Codes.ED));
			AssertEquals(BillReferenceList.Codes.FEN, BillReferenceList.Descriptions.FEN, list1.GetDescriptionFromCode(BillReferenceList.Codes.FEN));
			AssertEquals(BillReferenceList.Codes.FN, BillReferenceList.Descriptions.FN, list1.GetDescriptionFromCode(BillReferenceList.Codes.FN));
			AssertEquals(BillReferenceList.Codes.FP, BillReferenceList.Descriptions.FP, list1.GetDescriptionFromCode(BillReferenceList.Codes.FP));
			AssertEquals(BillReferenceList.Codes.GB, BillReferenceList.Descriptions.GB, list1.GetDescriptionFromCode(BillReferenceList.Codes.GB));
			AssertEquals(BillReferenceList.Codes.GR, BillReferenceList.Descriptions.GR, list1.GetDescriptionFromCode(BillReferenceList.Codes.GR));
			AssertEquals(BillReferenceList.Codes.HS, BillReferenceList.Descriptions.HS, list1.GetDescriptionFromCode(BillReferenceList.Codes.HS));
			AssertEquals(BillReferenceList.Codes.IN, BillReferenceList.Descriptions.IN, list1.GetDescriptionFromCode(BillReferenceList.Codes.IN));
			AssertEquals(BillReferenceList.Codes.LT, BillReferenceList.Descriptions.LT, list1.GetDescriptionFromCode(BillReferenceList.Codes.LT));
			AssertEquals(BillReferenceList.Codes.MA, BillReferenceList.Descriptions.MA, list1.GetDescriptionFromCode(BillReferenceList.Codes.MA));
			AssertEquals(BillReferenceList.Codes.MB, BillReferenceList.Descriptions.MB, list1.GetDescriptionFromCode(BillReferenceList.Codes.MB));
			AssertEquals(BillReferenceList.Codes.OB, BillReferenceList.Descriptions.OB, list1.GetDescriptionFromCode(BillReferenceList.Codes.OB));
			AssertEquals(BillReferenceList.Codes.OL, BillReferenceList.Descriptions.OL, list1.GetDescriptionFromCode(BillReferenceList.Codes.OL));
			AssertEquals(BillReferenceList.Codes.OM, BillReferenceList.Descriptions.OM, list1.GetDescriptionFromCode(BillReferenceList.Codes.OM));
			AssertEquals(BillReferenceList.Codes.OW, BillReferenceList.Descriptions.OW, list1.GetDescriptionFromCode(BillReferenceList.Codes.OW));
			AssertEquals(BillReferenceList.Codes.PK, BillReferenceList.Descriptions.PK, list1.GetDescriptionFromCode(BillReferenceList.Codes.PK));
			AssertEquals(BillReferenceList.Codes.PN, BillReferenceList.Descriptions.PN, list1.GetDescriptionFromCode(BillReferenceList.Codes.PN));
			AssertEquals(BillReferenceList.Codes.PO, BillReferenceList.Descriptions.PO, list1.GetDescriptionFromCode(BillReferenceList.Codes.PO));
			AssertEquals(BillReferenceList.Codes.RC, BillReferenceList.Descriptions.RC, list1.GetDescriptionFromCode(BillReferenceList.Codes.RC));
			AssertEquals(BillReferenceList.Codes.S7, BillReferenceList.Descriptions.S7, list1.GetDescriptionFromCode(BillReferenceList.Codes.S7));
			AssertEquals(BillReferenceList.Codes.SI, BillReferenceList.Descriptions.SI, list1.GetDescriptionFromCode(BillReferenceList.Codes.SI));
			AssertEquals(BillReferenceList.Codes.SO, BillReferenceList.Descriptions.SO, list1.GetDescriptionFromCode(BillReferenceList.Codes.SO));
			AssertEquals(BillReferenceList.Codes.ST, BillReferenceList.Descriptions.ST, list1.GetDescriptionFromCode(BillReferenceList.Codes.ST));
			AssertEquals(BillReferenceList.Codes.SW, BillReferenceList.Descriptions.SW, list1.GetDescriptionFromCode(BillReferenceList.Codes.SW));
			AssertEquals(BillReferenceList.Codes.ULC, BillReferenceList.Descriptions.ULC, list1.GetDescriptionFromCode(BillReferenceList.Codes.ULC));
			AssertEquals(BillReferenceList.Codes.UT, BillReferenceList.Descriptions.UT, list1.GetDescriptionFromCode(BillReferenceList.Codes.UT));
			AssertEquals(BillReferenceList.Codes.VA, BillReferenceList.Descriptions.VA, list1.GetDescriptionFromCode(BillReferenceList.Codes.VA));
			AssertEquals(BillReferenceList.Codes.WU, BillReferenceList.Descriptions.WU, list1.GetDescriptionFromCode(BillReferenceList.Codes.WU));
			AssertEquals(BillReferenceList.Codes.WY, BillReferenceList.Descriptions.WY, list1.GetDescriptionFromCode(BillReferenceList.Codes.WY));
			AssertEquals(BillReferenceList.Codes.XC, BillReferenceList.Descriptions.XC, list1.GetDescriptionFromCode(BillReferenceList.Codes.XC));
			AssertEquals(BillReferenceList.Codes.XP, BillReferenceList.Descriptions.XP, list1.GetDescriptionFromCode(BillReferenceList.Codes.XP));
			AssertEquals(BillReferenceList.Codes.ZE, BillReferenceList.Descriptions.ZE, list1.GetDescriptionFromCode(BillReferenceList.Codes.ZE));
			AssertEquals(BillReferenceList.Codes.ZZ, BillReferenceList.Descriptions.ZZ, list1.GetDescriptionFromCode(BillReferenceList.Codes.ZZ));
		}

		public void TestGetACEM1InBondList()
		{
			var list = BillReferenceList.GetCachedACEM1InBondList(Factory);
			AssertEquals(37, list.Count);
			AssertEquals(BillReferenceList.Codes._2K, BillReferenceList.Descriptions._2K, list.GetDescriptionFromCode(BillReferenceList.Codes._2K));
			AssertEquals(BillReferenceList.Codes.BL, BillReferenceList.Descriptions.BL, list.GetDescriptionFromCode(BillReferenceList.Codes.BL));
			AssertEquals(BillReferenceList.Codes.BM, BillReferenceList.Descriptions.BM, list.GetDescriptionFromCode(BillReferenceList.Codes.BM));
			AssertEquals(BillReferenceList.Codes.BN, BillReferenceList.Descriptions.BN, list.GetDescriptionFromCode(BillReferenceList.Codes.BN));
			AssertEquals(BillReferenceList.Codes.CG, BillReferenceList.Descriptions.CG, list.GetDescriptionFromCode(BillReferenceList.Codes.CG));
			AssertEquals(BillReferenceList.Codes.CN, BillReferenceList.Descriptions.CN, list.GetDescriptionFromCode(BillReferenceList.Codes.CN));
			AssertEquals(BillReferenceList.Codes.CO, BillReferenceList.Descriptions.CO, list.GetDescriptionFromCode(BillReferenceList.Codes.CO));
			AssertEquals(BillReferenceList.Codes.CR, BillReferenceList.Descriptions.CR, list.GetDescriptionFromCode(BillReferenceList.Codes.CR));
			AssertEquals(BillReferenceList.Codes.CUB, BillReferenceList.Descriptions.CUB, list.GetDescriptionFromCode(BillReferenceList.Codes.CUB));
			AssertEquals(BillReferenceList.Codes.CX, BillReferenceList.Descriptions.CX, list.GetDescriptionFromCode(BillReferenceList.Codes.CX));
			AssertEquals(BillReferenceList.Codes.ED, BillReferenceList.Descriptions.ED, list.GetDescriptionFromCode(BillReferenceList.Codes.ED));
			AssertEquals(BillReferenceList.Codes.FEN, BillReferenceList.Descriptions.FEN, list.GetDescriptionFromCode(BillReferenceList.Codes.FEN));
			AssertEquals(BillReferenceList.Codes.FN, BillReferenceList.Descriptions.FN, list.GetDescriptionFromCode(BillReferenceList.Codes.FN));
			AssertEquals(BillReferenceList.Codes.FP, BillReferenceList.Descriptions.FP, list.GetDescriptionFromCode(BillReferenceList.Codes.FP));
			AssertEquals(BillReferenceList.Codes.GB, BillReferenceList.Descriptions.GB, list.GetDescriptionFromCode(BillReferenceList.Codes.GB));
			AssertEquals(BillReferenceList.Codes.GR, BillReferenceList.Descriptions.GR, list.GetDescriptionFromCode(BillReferenceList.Codes.GR));
			AssertEquals(BillReferenceList.Codes.HS, BillReferenceList.Descriptions.HS, list.GetDescriptionFromCode(BillReferenceList.Codes.HS));
			AssertEquals(BillReferenceList.Codes.IN, BillReferenceList.Descriptions.IN, list.GetDescriptionFromCode(BillReferenceList.Codes.IN));
			AssertEquals(BillReferenceList.Codes.LT, BillReferenceList.Descriptions.LT, list.GetDescriptionFromCode(BillReferenceList.Codes.LT));
			AssertEquals(BillReferenceList.Codes.MA, BillReferenceList.Descriptions.MA, list.GetDescriptionFromCode(BillReferenceList.Codes.MA));
			AssertEquals(BillReferenceList.Codes.MB, BillReferenceList.Descriptions.MB, list.GetDescriptionFromCode(BillReferenceList.Codes.MB));
			AssertEquals(BillReferenceList.Codes.OM, BillReferenceList.Descriptions.OM, list.GetDescriptionFromCode(BillReferenceList.Codes.OM));
			AssertEquals(BillReferenceList.Codes.OW, BillReferenceList.Descriptions.OW, list.GetDescriptionFromCode(BillReferenceList.Codes.OW));
			AssertEquals(BillReferenceList.Codes.PK, BillReferenceList.Descriptions.PK, list.GetDescriptionFromCode(BillReferenceList.Codes.PK));
			AssertEquals(BillReferenceList.Codes.PN, BillReferenceList.Descriptions.PN, list.GetDescriptionFromCode(BillReferenceList.Codes.PN));
			AssertEquals(BillReferenceList.Codes.PO, BillReferenceList.Descriptions.PO, list.GetDescriptionFromCode(BillReferenceList.Codes.PO));
			AssertEquals(BillReferenceList.Codes.SI, BillReferenceList.Descriptions.SI, list.GetDescriptionFromCode(BillReferenceList.Codes.SI));
			AssertEquals(BillReferenceList.Codes.SO, BillReferenceList.Descriptions.SO, list.GetDescriptionFromCode(BillReferenceList.Codes.SO));
			AssertEquals(BillReferenceList.Codes.ST, BillReferenceList.Descriptions.ST, list.GetDescriptionFromCode(BillReferenceList.Codes.ST));
			AssertEquals(BillReferenceList.Codes.SW, BillReferenceList.Descriptions.SW, list.GetDescriptionFromCode(BillReferenceList.Codes.SW));
			AssertEquals(BillReferenceList.Codes.VA, BillReferenceList.Descriptions.VA, list.GetDescriptionFromCode(BillReferenceList.Codes.VA));
			AssertEquals(BillReferenceList.Codes.WU, BillReferenceList.Descriptions.WU, list.GetDescriptionFromCode(BillReferenceList.Codes.WU));
			AssertEquals(BillReferenceList.Codes.WY, BillReferenceList.Descriptions.WY, list.GetDescriptionFromCode(BillReferenceList.Codes.WY));
			AssertEquals(BillReferenceList.Codes.XC, BillReferenceList.Descriptions.XC, list.GetDescriptionFromCode(BillReferenceList.Codes.XC));
			AssertEquals(BillReferenceList.Codes.XP, BillReferenceList.Descriptions.XP, list.GetDescriptionFromCode(BillReferenceList.Codes.XP));
			AssertEquals(BillReferenceList.Codes.ZE, BillReferenceList.Descriptions.ZE, list.GetDescriptionFromCode(BillReferenceList.Codes.ZE));
			AssertEquals(BillReferenceList.Codes.ZZ, BillReferenceList.Descriptions.ZZ, list.GetDescriptionFromCode(BillReferenceList.Codes.ZZ));
		}
	}
}
