using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	[TestedType(typeof(NZDocsMAFCSContainer))]
	public class NZDocsMAFCSContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZDocsMAFCSContainer(Factory);
		}
	}
}
