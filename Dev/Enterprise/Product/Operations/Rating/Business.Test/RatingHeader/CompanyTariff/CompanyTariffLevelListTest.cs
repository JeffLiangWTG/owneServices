using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Rating.Business.Test
{
	public class CompanyTariffLevelListTest : TestCaseWithFactory
	{
		public void TestGetCompanyTariffLevelOverrideList_WhenCurrentCompanyReturnNull_ShouldReportError()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var companyTariffLevelOverrideList = new CompanyTariffLevelList(new BusinessObjectFactory { NameForDebugging = "Factory to load environment company" }).CompanyTariffLevelOverrideList;

				AssertEquals("Current environment company PK is 00000000-0000-0000-0000-000000000000\r\nCurrent environment branch PK is 00000000-0000-0000-0000-000000000000\r\nCurrent environment user context is ", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}
	}
}
