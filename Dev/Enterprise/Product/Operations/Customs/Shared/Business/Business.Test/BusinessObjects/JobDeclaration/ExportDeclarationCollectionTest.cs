using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ExportDeclarationCollection))]
	class ExportDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportDeclarationCollection(Factory, Declaration);
		}

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;
		#endregion
	}
}
