using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.ServiceTasks.Rim;

namespace Enterprise.Telematics.ServiceTasks.Test.Rim
{
	class RimDataRecordNumberStrategyTest : TestCaseWithFactory
	{
		public void TestNumberFountain()
		{
			// Arrange
			CombineAssertions(
				() =>
				{
					Test("ASD", 20, 0);
					Test("DSA", 20, 20);
				});

			void Test(string companyCode, int iterations, int indexOffset)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				company.GC_Code = companyCode;
				branch.GB_Code = companyCode;
				branch.GB_GC = company.PK;
				Factory.Save();

				using (Env.Instance.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var numberStrategy = new RimDataRecordNumberStrategy();
					var expectedCodes = Enumerable.Range(1, iterations)
						.Select(i => $"WTG-{companyCode}-{(indexOffset + i):D10}")
						.ToArray();

					// Act
					var result = expectedCodes
						.Select(i => numberStrategy.GetNextFormatted(Factory))
						.ToArray();

					// Assert
					AssertArrayEqualsByElements(expectedCodes, result);
				}
			}
		}
	}
}
