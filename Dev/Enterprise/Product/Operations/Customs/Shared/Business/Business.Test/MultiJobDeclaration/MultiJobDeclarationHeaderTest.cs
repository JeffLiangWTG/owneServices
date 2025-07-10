using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(MultiJobDeclarationHeader))]
	public class MultiJobDeclarationHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeclarationIsNotSaved()
		{
			MultiJobDeclarationHeader header = new MultiJobDeclarationHeader(Factory);
			AssertNotNull(header.Declaration);
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertNull(factory2.Load<BaseJobDeclaration>(header.Declaration.PK));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MultiJobDeclarationHeader(Factory);
		}
	}
}
