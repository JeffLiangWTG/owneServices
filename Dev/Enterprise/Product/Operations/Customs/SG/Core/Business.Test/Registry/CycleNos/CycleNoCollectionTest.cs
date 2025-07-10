using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[TestedType(typeof(CycleNoCollection))]
	sealed class CycleNoCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CycleNoCollection>
	{
		public void TestICodeDescriptionPairList()
		{
			var coll = new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var cycleNo = coll.AddNew();
			cycleNo.CycleNum = 1;
			cycleNo.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0));
			AssertEquals(true, ((ICodeDescriptionPairList)coll).ContainsCode(1));
			AssertEquals("23:40", ((ICodeDescriptionPairList)coll).GetDescriptionFromCode("1"));
		}

		public void TestGetDefaultCollection()
		{
			var collection = new CycleNoCollection().GetDefaultCollection;
			AssertEquals("Has 32 default cycle numbers", 32, collection.Count);
			var cycleNum = 1;
			AssertCycleNo(collection[0], cycleNum, ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0)));
			var submissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(0, 25, 0));
			for (int i = 1; i < 32; i++)
			{
				AssertCycleNo(collection[i], i + 1, submissionTime);
				submissionTime = submissionTime.AddMinutes(45);
			}
		}

		void AssertCycleNo(CycleNo cycle, ZInt cycleNum, ZDateTime submissionTime)
		{
			AssertEquals("cycle.CycleNum", cycleNum, cycle.CycleNum);
			AssertEquals("cycle.SubmissionTime", submissionTime, cycle.SubmissionTime);
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override CycleNoCollection GetCollectionToTest()
		{
			return new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CycleNo();
		}
	}
}
