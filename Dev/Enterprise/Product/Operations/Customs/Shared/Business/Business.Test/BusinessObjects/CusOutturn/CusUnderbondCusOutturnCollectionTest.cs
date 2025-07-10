using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusUnderbondCusOutturnCollectionForTest))]
	sealed class CusUnderbondCusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypeOFElements()
		{
			AssertEquals("TypeOfElements", typeof(CusOutturn), Collection.TypeOfElements);
		}

		public void TestFKSchemaColumnInDependent()
		{
			AssertEquals("FKSchemaColumnInDependent", Enterprise.ZArchitecture.Schema.CusOutturnSchema.C5_C4_Underbond, Collection.FKSchemaColumnInDependent);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Collection;
		}

		public void TestOutturnHeaderCollectionCopied()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			var underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
			underbond.C4_C6 = header.PK;
			CusOutturn outturnLine = underbond.Outturns.AddNew();
			AssertEquals("Header should default when new item added", header.PK, outturnLine.C5_C6);
		}

		CusUnderbondCusOutturnCollectionForTest fCollection;
		new CusUnderbondCusOutturnCollectionForTest Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = new CusUnderbondCusOutturnCollectionForTest(Underbond);
				}
				return fCollection;
			}
		}

		CusUnderbond fUnderbond;
		CusUnderbond Underbond
		{
			get
			{
				if (fUnderbond == null)
				{
					fUnderbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
				}
				return fUnderbond;
			}
		}

		class CusUnderbondCusOutturnCollectionForTest : CusUnderbondCusOutturnCollection
		{
			public CusUnderbondCusOutturnCollectionForTest(CusUnderbond underbond) : base(underbond)
			{
			}

			internal new SchemaGuidColumn FKSchemaColumnInDependent => base.FKSchemaColumnInDependent;
		}
	}
}
