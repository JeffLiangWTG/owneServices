using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection))]
	public class CusEntryHeaderCollectionTest : Declaration.Testing.CusEntryHeaderCollectionTest
	{
		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			return declaration;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryHeaderCollection(Declaration, Factory);
		}
	}
}
