using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class CusEntryHeaderValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public virtual void TestValidatePackage()
		{
		}

		#region Implementation
		public virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
		#endregion
	}
}
