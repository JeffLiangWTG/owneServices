using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[TestedType(typeof(CycleNo))]
	sealed class CycleNoTest : RegistryBusinessObjectTemplateTestCase<CycleNo>
	{
		public void TestICodeDescription()
		{
			var coll = new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var cycleNo = coll.AddNew();
			cycleNo.CycleNum = 1;
			cycleNo.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0));
			AssertEquals("1", cycleNo.CycleNumStr);
			AssertEquals("23:40", cycleNo.SubmissionTimeStr);
			AssertEquals("1", ((ICodeDescription)cycleNo).Code);
			AssertEquals("23:40", ((ICodeDescription)cycleNo).Description);
		}

		public void TestValidateCycleNum()
		{
			var coll = new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var cycleNo = coll.AddNew();
			cycleNo.CycleNum = 1;
			var cycleNo2 = coll.AddNew();
			cycleNo2.CycleNum = 1;
			AssertHasError(cycleNo2.CycleNumInfo, string.Format(CycleNo.DuplicateCycleNum, cycleNo2.CycleNum));
		}

		public void TestValidateSubmissionTime()
		{
			var coll = new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var cycleNo = coll.AddNew();
			cycleNo.CycleNum = 1;
			cycleNo.SubmissionTime = ZDateTime.Empty;
			AssertHasError(cycleNo.SubmissionTimeInfo, MandatoryValidation.MustBeEntered + " an AECs Submission Time.");
			cycleNo.SubmissionTime = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0));
			AssertNoError(cycleNo.SubmissionTimeInfo, MandatoryValidation.MustBeEntered + " an AECs Submission Time.");
		}

		public void TestHandleInvalidSubmissioTime()
		{
			var coll = new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var cycleNo = coll.AddNew();
			cycleNo.CycleNum = 1;
			var time = ZDateTime.MinSmallDateTimeValue.Date.Add(new System.TimeSpan(23, 40, 0));
			cycleNo.SubmissionTime = time;
			AssertEquals(time, cycleNo.SubmissionTime);
			cycleNo.SubmissionTime = ZDateTime.Invalid;
			AssertEquals(time, cycleNo.SubmissionTime);
			cycleNo.SubmissionTime = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, cycleNo.SubmissionTime);
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

		protected override BusinessObject GetNewBusinessObject()
		{
			CycleNoCollection cycleNoCollection = new CycleNoCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			CycleNo result = cycleNoCollection.AddNew();
			result.CycleNum = 1;
			return result;
		}

		protected override CycleNo GetBusinessObjectToClone()
		{
			return (CycleNo)GetNewBusinessObject();
		}

		protected override CycleNo GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
