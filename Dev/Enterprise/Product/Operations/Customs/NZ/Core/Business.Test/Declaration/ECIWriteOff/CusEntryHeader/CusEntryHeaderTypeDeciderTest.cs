using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	public class CusEntryHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDeciderWithOldReference()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals(typeof(CusEntryHeader), declaration.CusEntryHeader.GetType());
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(typeof(CusEntryHeader), declaration2.CusEntryHeader.GetType());

			ZString manifestReference = NumberFountains.OldECIManifestReferencePrefix + "01010101";
			declaration.JE_DeclarationReference = manifestReference + "-1";
			declaration.CusEntryHeader.CH_IsActive = false;
			Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(manifestEntryHeader);
			manifestEntryHeader.CH_JE = declaration.PK;
			manifestEntryHeader.CH_BGMReference = manifestReference;
			Factory.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declaration3 = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals(typeof(Manifesting.CusEntryHeader), declaration3.CusEntryHeader.GetType());
		}

		public void TestTypeDeciderWithNewReference()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals(typeof(CusEntryHeader), declaration.CusEntryHeader.GetType());
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(typeof(CusEntryHeader), declaration2.CusEntryHeader.GetType());

			ZString manifestReference = NumberFountains.ECIManifestReferencePrefix + "01010101";
			declaration.JE_DeclarationReference = manifestReference + "-1";
			declaration.CusEntryHeader.CH_IsActive = false;
			Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(manifestEntryHeader);
			manifestEntryHeader.CH_JE = declaration.PK;
			manifestEntryHeader.CH_BGMReference = manifestReference;
			Factory.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declaration3 = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals(typeof(Manifesting.CusEntryHeader), declaration3.CusEntryHeader.GetType());
		}
	}
}
