using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartBomViewCollection))]
	sealed class OrgPartBomViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgPartBomViewCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return BOMView;
		}

		protected override OrgPartBomViewCollection GetCollectionToTest()
		{
			return new OrgPartBomViewCollection(Factory);
		}

		#region Unused functions in collectetion should not be tested

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		#endregion

		public void TestCreateCollection()
		{
			OrgPartBomViewCollection collection = new OrgPartBomViewCollection(Factory);
			SetStructure(collection);
			AssertEquals(7, collection.Count);
		}

		public void TestReload()
		{
			OrgPartBomViewCollection collection = new OrgPartBomViewCollection(Factory);
			SetStructure(collection);

			AssertEquals(7, collection.Count);

			OrgSupplierPart checkPart = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, collection[0].ComponentCode.Trim()));
			AssertEquals(2, checkPart.BillOfMaterials.Count);

			collection.Reload(checkPart.BillOfMaterials[0]);
			AssertEquals(2, collection.Count);
			AssertEquals("G", collection[0].ComponentCode.Trim());
		}

		#region Implementation

		void SetStructure(OrgPartBomViewCollection collection)
		{
			BOM.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			BOM.Component.OP_PartNum = "A";
			BOM.Component.OP_Desc = "A";

			collection.Reload(BOM);
			//							A
			//						/	|	\
			//					B		C		D
			//				/		\
			//			E				F
			//		/		\
			//	G				H
			OrgPartBOM b = CreateNewWithPart(BOM, "B");
			OrgPartBOM c = CreateNewWithPart(BOM, "C");
			OrgPartBOM d = CreateNewWithPart(BOM, "D");
			OrgPartBOM e = CreateNewWithPart(b, "E");
			OrgPartBOM f = CreateNewWithPart(b, "F");
			OrgPartBOM g = CreateNewWithPart(e, "G");
			OrgPartBOM h = CreateNewWithPart(e, "H");
			collection.Load();
		}

		OrgPartBOM CreateNewWithPart(OrgPartBOM parent, string componentPartNum)
		{
			OrgPartBOM result = Factory.New<OrgPartBOM>();
			result.OE_OP_MainProduct = parent.Component.PK;
			result.OE_OP_Component = Factory.New<OrgSupplierPart>().PK;
			result.Component.OP_PartNum = componentPartNum;
			result.Component.OP_Desc = componentPartNum;
			return result;
		}

		OrgPartBomView BOMView
		{
			get { return bomView ?? (bomView = new OrgPartBomView(BOM, 0)); }
		}
		OrgPartBomView bomView;

		OrgPartBOM BOM
		{
			get
			{
				if (fbom == null)
				{
					fbom = Factory.New<OrgPartBOM>();
					OrgSupplierPart part = Factory.New<OrgSupplierPart>();
					OrgSupplierPart subPart = Factory.New<OrgSupplierPart>();
					fbom.OE_OP_MainProduct = part.PK;
					fbom.OE_OP_Component = subPart.PK;
					OrgPartBOM bom1 = CreateNewWithPart(fbom, "Test");
				}
				return fbom;
			}
		}
		OrgPartBOM fbom;

		#endregion
	}
}
