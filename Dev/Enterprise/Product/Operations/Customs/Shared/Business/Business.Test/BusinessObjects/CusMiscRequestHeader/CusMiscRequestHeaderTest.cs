using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMiscRequestHeader))]
	public class CusMiscRequestHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetCusMiscRequestHeaderForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusMiscRequestHeaderForTest(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetCusMiscRequestHeaderForTest();

		CusMiscRequestHeader GetCusMiscRequestHeaderForTest(BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = Factory;
			}
			var header = factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = "5AC";
			header.CMR_RequestDate = ZDateTime.Today;
			header.CMR_CustomsOffice = "010";
			header.CMR_JobNumber = "1234567890123X";
			header.CMR_GB = Env.CurrentBranch.PK;
			return header;
		}
	}
}
