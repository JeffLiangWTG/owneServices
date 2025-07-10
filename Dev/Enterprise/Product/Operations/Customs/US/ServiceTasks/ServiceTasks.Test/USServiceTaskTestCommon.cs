using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	class USServiceTaskTestCommon : TestCaseWithFactory
	{
		public void AssertCompanyBranchBecomeInactiveDuringProcessing<T>() where T : ServiceProviderImpl, new()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CUS";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_IsActive = true;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BUS";
			branch.GB_IsActive = true;
			Factory.Save();

			var serviceTask = new T();

			ErrorReporter.Clear();

			CombineAssertions(() =>
			{
				serviceTask.RunTask();
				AssertEquals("should no error reported when branch is active both in memory and Db", 0,
					ErrorReporter.TotalErrorCount);

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbBranch SET GB_IsActive = 0, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GETUTCDATE() WHERE GB_PK = '{branch.PK}'");

				serviceTask.RunTask();
				AssertEquals("should no error reported when branch is active in memory but is inactive in Db", 0,
					ErrorReporter.TotalErrorCount);
			});
		}
	}
}
