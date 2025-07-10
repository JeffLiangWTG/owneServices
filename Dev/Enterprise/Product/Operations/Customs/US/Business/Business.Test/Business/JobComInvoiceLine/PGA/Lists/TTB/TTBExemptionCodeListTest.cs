using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class TTBExemptionCodeListTest : TestCaseWithFactory
	{
		public void TestGetListForPermit()
		{
			var list = new TTBExemptionCodeList();
			var list1 = TTBExemptionCodeList.GetListForPermit(Factory, ZString.Empty);
			var list2 = TTBExemptionCodeList.GetListForPermit(Factory, ZString.Empty);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(list.Count, list1.Count);
			foreach (ICodeDescription pair in list2)
			{
				AssertEquals(pair.Code, pair.Description, list1.GetDescriptionFromCode(pair.Code));
			}

			list1 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.Beverage);
			list2 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.Beverage);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(4, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX1,
				TTBExemptionCodeList.Codes.TTBEX2,
				TTBExemptionCodeList.Codes.TTBEX14,
				TTBExemptionCodeList.Codes.TTBEX15
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.DistilledSpirits);
			list2 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.DistilledSpirits);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(4, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX1,
				TTBExemptionCodeList.Codes.TTBEX5,
				TTBExemptionCodeList.Codes.TTBEX14,
				TTBExemptionCodeList.Codes.TTBEX15
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.Tobacco);
			list2 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.Tobacco);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(4, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX1,
				TTBExemptionCodeList.Codes.TTBEX6,
				TTBExemptionCodeList.Codes.TTBEX13,
				TTBExemptionCodeList.Codes.TTBEX15
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.Wine);
			list2 = TTBExemptionCodeList.GetListForPermit(Factory, TTBProgramCodeList.Codes.Wine);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(5, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX1,
				TTBExemptionCodeList.Codes.TTBEX3,
				TTBExemptionCodeList.Codes.TTBEX4,
				TTBExemptionCodeList.Codes.TTBEX14,
				TTBExemptionCodeList.Codes.TTBEX15
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}
		}

		public void TestGetListForCOLA()
		{
			var list = new TTBExemptionCodeList();
			var list1 = TTBExemptionCodeList.GetListForCOLA(Factory, ZString.Empty);
			var list2 = TTBExemptionCodeList.GetListForCOLA(Factory, ZString.Empty);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(0, list1.Count);
			list1 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.Beverage);
			list2 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.Beverage);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(5, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX2,
				TTBExemptionCodeList.Codes.TTBEX7,
				TTBExemptionCodeList.Codes.TTBEX8,
				TTBExemptionCodeList.Codes.TTBEX11,
				TTBExemptionCodeList.Codes.TTBEX12
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.DistilledSpirits);
			list2 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.DistilledSpirits);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(4, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX5,
				TTBExemptionCodeList.Codes.TTBEX7,
				TTBExemptionCodeList.Codes.TTBEX9,
				TTBExemptionCodeList.Codes.TTBEX12
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			list1 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.Tobacco);
			list2 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.Tobacco);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(0, list1.Count);

			list1 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.Wine);
			list2 = TTBExemptionCodeList.GetListForCOLA(Factory, TTBProgramCodeList.Codes.Wine);
			AssertEquals("Cached List", list1, list2);
			AssertEquals(5, list1.Count);
			foreach (var code in new[]
			{
				TTBExemptionCodeList.Codes.TTBEX3,
				TTBExemptionCodeList.Codes.TTBEX4,
				TTBExemptionCodeList.Codes.TTBEX7,
				TTBExemptionCodeList.Codes.TTBEX10,
				TTBExemptionCodeList.Codes.TTBEX12
			})
			{
				AssertEquals(code, list.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}
		}
	}
}
