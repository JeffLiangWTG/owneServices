using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobScheduleChange.Loader))]
	sealed class JobScheduleChangeLoaderTest : LoaderTestCase
	{
		public void TestLoadOrCreate()
		{
			JobScheduleChange createdScheduleChange = Loader.LoadOrCreate(Voyage, ScheduleDateTypes.Codes.ETA);
			AssertEquals("IsInDatabase", false, createdScheduleChange.IsInDatabase);
			AssertEquals("E7_ParentID", Voyage.PK, createdScheduleChange.E7_ParentID);
			AssertEquals("E7_ParentTableCode", JobVoyageSchema.Constants.Prefix, createdScheduleChange.E7_ParentTableCode);
			AssertEquals("E7_DateType", ScheduleDateTypes.Codes.ETA, createdScheduleChange.E7_DateType);
			AssertEquals("E7_GS_NKChangedBy", GlbStaff.CurrentUser.GS_Code, createdScheduleChange.E7_GS_NKChangedBy);

			JobScheduleChange loadedScheduleChange = Loader.LoadOrCreate(Voyage, ScheduleDateTypes.Codes.ETA);
			AssertEquals("LoadOrCreate should load the existing schedule change", createdScheduleChange.PK, loadedScheduleChange.PK);
		}

		public void TestLoad()
		{
			ZGuid parentID = ZGuid.NewZGuid();

			JobScheduleChange scheduleChange = Factory.New<JobScheduleChange>();
			scheduleChange.E7_ParentID = parentID;
			scheduleChange.E7_DateType = ScheduleDateTypes.Codes.ETA;
			scheduleChange.E7_GS_NKChangedBy = GlbStaff.CurrentUser.GS_Code;

			JobScheduleChange decoyScheduleChange1 = Factory.New<JobScheduleChange>();
			decoyScheduleChange1.E7_ParentID = ZGuid.NewZGuid();
			decoyScheduleChange1.E7_DateType = ScheduleDateTypes.Codes.ETA;
			decoyScheduleChange1.E7_GS_NKChangedBy = GlbStaff.CurrentUser.GS_Code;

			JobScheduleChange decoyScheduleChange2 = Factory.New<JobScheduleChange>();
			decoyScheduleChange2.E7_ParentID = parentID;
			decoyScheduleChange2.E7_DateType = ScheduleDateTypes.Codes.FCLAvailable;
			decoyScheduleChange2.E7_GS_NKChangedBy = GlbStaff.CurrentUser.GS_Code;

			JobScheduleChange decoyScheduleChange3 = Factory.New<JobScheduleChange>();
			decoyScheduleChange3.E7_ParentID = parentID;
			decoyScheduleChange3.E7_DateType = ScheduleDateTypes.Codes.ETA;
			decoyScheduleChange3.E7_GS_NKChangedBy = "DCY";

			JobScheduleChange loadedScheduleChange = Loader.Load(scheduleChange.E7_ParentID, scheduleChange.E7_DateType);
			AssertEquals("Should load the correct schedule", scheduleChange, loadedScheduleChange);
		}

		#region Implementation

		JobScheduleChange.Loader Loader
		{
			get { return loader ?? (loader = new JobScheduleChange.Loader(Factory)); }
		}
		JobScheduleChange.Loader loader;

		JobVoyage Voyage
		{
			get { return voyage ?? (voyage = Factory.New<JobVoyage>()); }
		}
		JobVoyage voyage;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new JobScheduleChange.Loader(Factory);
		}

		#endregion
	}
}
