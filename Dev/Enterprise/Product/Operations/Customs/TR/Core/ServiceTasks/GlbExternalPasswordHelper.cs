using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ServiceTasks
{
	public static class GlbExternalPasswordHelper
	{
		public static string CheckAnyStaffHasCertificate()
		{
			var staffHasCertificate = CertificateRequirementChecker.ExistsStaffWithCertificate(PasswordTypesList.Codes.TRK);
			return staffHasCertificate ? string.Empty : (NoResString)"There is no Certificate configured in Turkey.";
		}
	}
}
