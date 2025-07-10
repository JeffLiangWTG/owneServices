
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	[TestedType(typeof(CusEntryNumberCollection))]
	public class CusEntryNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryNumberCollection(Factory);
		}
	}
}
