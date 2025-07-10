using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DummyCusOutturnCollection))]
	sealed class CusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return DummyCollection;
		}

		public void TestFKSchemaColumnInDependent()
		{
			AssertEquals(CusOutturnSchema.C5_ParentID, DummyCollection.FKSchemaColumnInDependentExposed);
		}

		public void TestDefaultsForNewChild()
		{
			DummyCusOutturn outturn = DummyCollection.AddNew();
			AssertNotNull(Dummy);
			AssertEquals(Dummy, outturn.Parent);
		}

		public void TestParentOnOutturnIsCorrectlySetOnAdd()
		{
			DummyCusOutturn outturn = Factory.New<DummyCusOutturn>();
			DummyCollection.Add(outturn);
			Assert("precondition", !string.IsNullOrEmpty(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Dummy.TableName)));
			AssertEquals(Dummy.PK, outturn.C5_ParentID);
			AssertEquals(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Dummy.TableName), outturn.C5_ParentTableCode);
			AssertEquals(Dummy, outturn.Parent);
		}

		public void TestParentOnOutturnIsCorrectlySetOnAddNew()
		{
			DummyCusOutturn outturn = DummyCollection.AddNew();
			Assert("precondition", !string.IsNullOrEmpty(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Dummy.TableName)));
			AssertEquals(Dummy.PK, outturn.C5_ParentID);
			AssertEquals(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Dummy.TableName), outturn.C5_ParentTableCode);
			AssertEquals(Dummy, outturn.Parent);
		}

		CusOutturnParentDummy Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<CusOutturnParentDummy>();
				}
				return dummy;
			}
		}
		CusOutturnParentDummy dummy;

		DummyCusOutturnCollection DummyCollection
		{
			get
			{
				if (dummyCollection == null)
				{
					dummyCollection = new DummyCusOutturnCollection(Dummy);
				}
				return dummyCollection;
			}
		}
		DummyCusOutturnCollection dummyCollection;

		#region TestHelpers

		class DummyCusOutturn : CusOutturn
		{
			public DummyCusOutturn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override TypeLoaderCollection GetParentLoaders()
			{
				TypeLoaderCollection loaders = base.GetParentLoaders();
				loaders.Add(new TypeLoader(typeof(CusOutturnParentDummy)));
				return loaders;
			}
		}

		class DummyCusOutturnCollection : GenericCusOutturnCollection<DummyCusOutturn, CusOutturnParentDummy>
		{
			public DummyCusOutturnCollection(CusOutturnParentDummy parent) : base(parent)
			{
			}

			public SchemaGuidColumn FKSchemaColumnInDependentExposed
			{
				get
				{
					return FKSchemaColumnInDependent;
				}
			}
		}

		class CusOutturnParentDummy : DummyBusinessObject, IOutturnableLine
		{
			public CusOutturnParentDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IOutturnableLine Members

			ZString IOutturnableLine.UnderbondHumanReadableName
			{
				get { return ZString.Empty; }
			}

			ZInt IOutturnableLine.PackagesManifested
			{
				get { return 0; }
			}

			#endregion

			#region IOutturnableLine Members

			ZString IOutturnableLine.CargoStatus
			{
				get { return "WTO"; }
			}

			#endregion
		}

		#endregion
	}
}
