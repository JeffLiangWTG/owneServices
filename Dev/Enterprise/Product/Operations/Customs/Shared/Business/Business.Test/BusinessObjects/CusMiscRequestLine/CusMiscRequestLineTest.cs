using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMiscRequestLine))]
	sealed class CusMiscRequestLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetCusMiscRequestLineForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusMiscRequestLineForTest(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetCusMiscRequestLineForTest(Factory);

		CusMiscRequestLine GetCusMiscRequestLineForTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = "5AC";
			header.CMR_RequestDate = ZDateTime.Today;
			header.CMR_CustomsOffice = "010";
			header.CMR_JobNumber = "1234567890123X";
			header.CMR_GB = Env.CurrentBranch.PK;
			var line = header.RequestLines.AddNew();
			line.CML_EntryType = "EXP";
			line.CML_EntryNumber = "12345";
			return line;
		}
	}
}
