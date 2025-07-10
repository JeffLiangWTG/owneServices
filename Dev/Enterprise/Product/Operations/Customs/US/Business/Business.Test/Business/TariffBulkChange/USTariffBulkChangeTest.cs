using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USTariffBulkChange))]
	sealed class USTariffBulkChangeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateForSchB()
		{
			CreateSimpleProduct("Test Export Product 1", ClassificationTypeList.Codes.SHB, "6909191000", "");
			CreateSimpleProduct("Test Export Product 2", ClassificationTypeList.Codes.SHB, "4016996000", "");

			CreateSimpleProductWithClass("Test Export Product 3", CusClassification.ClassificationType.EXP, ClassificationTypeList.Codes.SHB, "2922292700", "LOOK1");
			CreateSimpleProductWithClass("Test Export Product 4", CusClassification.ClassificationType.EXP, ClassificationTypeList.Codes.SHB, "1703900000", "LOOK2");

			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "1703900000";
			classification.CC_LookupCode = "LOOKUP Not selected";
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			Factory.Save();

			USTariffBulkChange tariffChanger = new USTariffBulkChange(Factory);
			tariffChanger.HTSTariffFlag = false;
			tariffChanger.SHBTariffFlag = true;
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "6909191000", NewTariffNum = "6909191001" });
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "2922292700", NewTariffNum = "2922292701" });
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "1703900000", NewTariffNum = "1703900001" });
			AssertEquals(3, tariffChanger.Tariffs.Count);
			tariffChanger.Update();

			newFactory = new BusinessObjectFactory();
			AssertEquals(1, tariffChanger.ProductsChanged);
			AssertEquals(3, tariffChanger.ClassificationsChanged);
			AssertUpdateResults("Test Export Product 1", "6909191001", "");
			AssertUpdateResults("Test Export Product 2", "4016996000", "");
			AssertClassificationsUpdateResults("LOOK1", "2922292701");
			AssertClassificationsUpdateResults("LOOK2", "1703900001");
			AssertClassificationsUpdateResults("LOOKUP Not selected", "1703900001");
		}

		public void TestUpdateChildrenForHTI()
		{
			CreateSimpleProduct("Test Product 1", ClassificationTypeList.Codes.HTI, "6203434030", "");

			OrgSupplierPart part = CreateSimpleProduct("Test Product 2", ClassificationTypeList.Codes.HTI, "", "");
			CusClassPartPivot child1 = part.PivotsForBinding[0].Children.AddNew();
			child1.CI_TariffNum = "9801001010";
			child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;

			CusClassPartPivot child2 = part.PivotsForBinding[0].Children.AddNew();
			child2.CI_TariffNum = "6203434030";
			child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			CusClassPartPivot child3 = part.PivotsForBinding[0].Children.AddNew();
			child3.CI_TariffNum = "5001000001";
			child3.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			Factory.Save();

			USTariffBulkChange tariffChanger = new USTariffBulkChange(Factory);
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "6203434030", NewTariffNum = "6203434031" });
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "9801001010", NewTariffNum = "9801001011" });
			tariffChanger.Update();

			newFactory = new BusinessObjectFactory();
			AssertEquals(2, tariffChanger.ProductsChanged);
			AssertEquals(0, tariffChanger.ClassificationsChanged);
			AssertUpdateResults("Test Product 1", "6203434031", "");
			OrgSupplierPart product = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test Product 2"));
			AssertEquals(1, product.PivotsForBinding.Count);
			AssertEquals(3, product.PivotsForBinding[0].Children.Count);
			AssertNotNull(product.PivotsForBinding[0].Children.Find(x => x.CI_TariffNum == "9801001011"));
			AssertNotNull(product.PivotsForBinding[0].Children.Find(x => x.CI_TariffNum == "6203434031"));
			AssertNotNull(product.PivotsForBinding[0].Children.Find(x => x.CI_TariffNum == "5001000001"));
		}

		public void TestUpdateProduct_SameTariffForHTSAndSHB_FlagHTS()
		{
			//Prepare test data
			OrgSupplierPart part = CreateSimpleProduct("Test Product 1", ClassificationTypeList.Codes.HTI, "6203434030", "");
			CusClassPartPivot child1 = part.PivotsForBinding[0].Children.AddNew();
			child1.CI_TariffNum = "9801001010";
			child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;

			CusClassPartPivot child2 = part.PivotsForBinding[0].Children.AddNew();
			child2.CI_TariffNum = "6203434030";
			child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			CusClassPartPivot child3 = part.PivotsForBinding[0].Children.AddNew();
			child3.CI_TariffNum = "5001000001";
			child3.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			CreatePivot(part.PK, ClassificationTypeList.Codes.HTE, "6203434030", "");
			CreatePivot(part.PK, ClassificationTypeList.Codes.SHB, "6203434030", "");

			OrgSupplierPart part2 = CreateSimpleProduct("Test Product 2", ClassificationTypeList.Codes.HTI, "6203434030", "");
			CreatePivot(part2.PK, ClassificationTypeList.Codes.SHB, "6203434030", "");
			CreatePivot(part2.PK, ClassificationTypeList.Codes.HTE, "6203434030", "");
			Factory.Save();

			//Prepare to update
			USTariffBulkChange tariffChanger = new USTariffBulkChange(Factory);
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "6203434030", NewTariffNum = "6203434031" });
			AssertEquals("HTS tariffs will be updated", true, tariffChanger.HTSTariffFlag);
			tariffChanger.Update();

			newFactory = new BusinessObjectFactory();
			AssertEquals(2, tariffChanger.ProductsChanged);
			AssertEquals(0, tariffChanger.ClassificationsChanged);

			//Product 1
			OrgSupplierPart loadedProduct = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test Product 1"));
			CusClassPartPivot loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTI);
			AssertEquals("HTI pivot should be updated", "6203434031", loadedPivot.CI_TariffNum);

			CusClassPartPivot loadedChildPivot = GetChildPivot(loadedPivot.Children, "6203434031");
			AssertEquals("This child should be updated", "6203434031", loadedChildPivot.CI_TariffNum);

			loadedChildPivot = GetChildPivot(loadedPivot.Children, "9801001010");
			AssertEquals("Other children should not be updated", "9801001010", loadedChildPivot.CI_TariffNum);

			loadedChildPivot = GetChildPivot(loadedPivot.Children, "5001000001");
			AssertEquals("Other children should not be updated", "5001000001", loadedChildPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTE);
			AssertEquals("HTE pivot should be updated, because this is a part of HTS", "6203434031", loadedPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.SHB);
			AssertEquals("SHB pivot should not be updated, even if Tariff Number the same", "6203434030", loadedPivot.CI_TariffNum);

			////Product 2
			loadedProduct = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test Product 2"));
			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTI);
			AssertEquals("HTI pivot should be updated", "6203434031", loadedPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTE);
			AssertEquals("HTE pivot should be updated, because this is a part of HTS", "6203434031", loadedPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.SHB);
			AssertEquals("SHB pivot should not be updated, even if Tariff Number the same", "6203434030", loadedPivot.CI_TariffNum);
		}

		public void TestUpdateProduct_SameTariffForHTSAndSHB_FlagSHB()
		{
			//Prepare test data
			OrgSupplierPart part = CreateSimpleProduct("Test Product 1", ClassificationTypeList.Codes.HTI, "2208700030", "");
			CusClassPartPivot child1 = part.PivotsForBinding[0].Children.AddNew();
			child1.CI_TariffNum = "2208700030";
			child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;

			CusClassPartPivot child2 = part.PivotsForBinding[0].Children.AddNew();
			child2.CI_TariffNum = "6203434030";
			child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;

			CreatePivot(part.PK, ClassificationTypeList.Codes.HTE, "2208700030", "");
			CreatePivot(part.PK, ClassificationTypeList.Codes.SHB, "2208700030", "");

			OrgSupplierPart part2 = CreateSimpleProduct("Test Product 2", ClassificationTypeList.Codes.HTI, "2208700030", "");
			CreatePivot(part2.PK, ClassificationTypeList.Codes.SHB, "2208700030", "");
			CreatePivot(part2.PK, ClassificationTypeList.Codes.HTE, "2208700030", "");
			Factory.Save();

			//Prepare to update
			USTariffBulkChange tariffChanger = new USTariffBulkChange(Factory);
			tariffChanger.HTSTariffFlag = false;
			tariffChanger.SHBTariffFlag = true;
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "2208700030", NewTariffNum = "2208700031" });
			AssertEquals("SHB tariffs will be updated", true, tariffChanger.SHBTariffFlag);
			AssertEquals(false, tariffChanger.HTSTariffFlag);
			tariffChanger.Update();

			newFactory = new BusinessObjectFactory();
			AssertEquals(2, tariffChanger.ProductsChanged);
			AssertEquals(0, tariffChanger.ClassificationsChanged);

			//Product 1
			OrgSupplierPart loadedProduct = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test Product 1"));
			CusClassPartPivot loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTI);
			AssertEquals("HTI pivot should not be updated", "2208700030", loadedPivot.CI_TariffNum);

			CusClassPartPivot loadedChildPivot = GetChildPivot(loadedPivot.Children, "6203434030");
			AssertEquals("This child should not be updated", "6203434030", loadedChildPivot.CI_TariffNum);

			loadedChildPivot = GetChildPivot(loadedPivot.Children, "2208700030");
			AssertEquals("This child should not be updated", "2208700030", loadedChildPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTE);
			AssertEquals("HTE pivot should not be updated", "2208700030", loadedPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.SHB);
			AssertEquals("SHB pivot should be updated, because flag is SHB", "2208700031", loadedPivot.CI_TariffNum);

			////Product 2
			loadedProduct = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test Product 2"));
			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTI);
			AssertEquals("HTI pivot should not be updated", "2208700030", loadedPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.HTE);
			AssertEquals("HTE pivot should not be updated", "2208700030", loadedPivot.CI_TariffNum);

			loadedPivot = GetPivot(loadedProduct.PivotsForBinding, ClassificationTypeList.Codes.SHB);
			AssertEquals("SHB pivot should be updated", "2208700031", loadedPivot.CI_TariffNum);
		}

		CusClassPartPivot GetPivot(CusClassPartPivotCollection pivots, ZString childType)
		{
			List<Customs.Business.BaseCusClassPartPivot> loadedPivots = new List<Customs.Business.BaseCusClassPartPivot>(pivots.Find(x => x.CI_ChildType == childType));
			return (CusClassPartPivot)loadedPivots[0];
		}

		CusClassPartPivot GetChildPivot(CusClassPartPivotCollection pivots, ZString tariffNum)
		{
			List<Customs.Business.BaseCusClassPartPivot> loadedPivots = new List<Customs.Business.BaseCusClassPartPivot>(pivots.Find(x => x.CI_TariffNum == tariffNum));
			return (CusClassPartPivot)loadedPivots[0];
		}

		public void TestUpdateForHTS()
		{
			CreateSimpleProduct("Test Product 1", ClassificationTypeList.Codes.HTI, "6003203000", "99101220");
			CreateSimpleProduct("Test Product 2", ClassificationTypeList.Codes.HTI, "8704320020", "");
			CreateSimpleProduct("Test Product 3", ClassificationTypeList.Codes.HTE, "6003203000", "");
			CreateSimpleProductWithClass("Test Product 4", CusClassification.ClassificationType.IMP, ClassificationTypeList.Codes.HTI, "2922292700", "LOOK1");
			CreateSimpleProductWithClass("Test Product 5", CusClassification.ClassificationType.IMP, ClassificationTypeList.Codes.HTE, "0121323122", "LOOK2");

			OrgSupplierPart product = CreateSimpleProduct("Test Product 6", ClassificationTypeList.Codes.HTE, "6003203000", "");
			CreatePivot(product.PK, ClassificationTypeList.Codes.HTI, "6003203000", "99101220");
			CreatePivot(product.PK, ClassificationTypeList.Codes.HTI, "6003203000", "");

			product = CreateSimpleProductWithClass("Test Product 7", CusClassification.ClassificationType.IMP, ClassificationTypeList.Codes.HTI, "0121323122", "LOOK3");
			CusClassification classification_2 = Factory.New<CusClassification>();
			classification_2.CC_TariffNum = "8540810000";
			classification_2.CC_LookupCode = "LOOK4";
			classification_2.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			CusClassPartPivot importPivot_2 = product.PivotsForBinding.AddNew();
			importPivot_2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			importPivot_2.CI_CC = classification_2.PK;
			Factory.Save();

			USTariffBulkChange tariffChanger = new USTariffBulkChange(Factory);
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "6003203000", NewTariffNum = "0712903000" });
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "99101220", NewTariffNum = "99030225" });
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "0121323122", NewTariffNum = "0601108500" });
			tariffChanger.Tariffs.Add(new TariffToChange(tariffChanger) { OldTariffNum = "8540810000", NewTariffNum = "2904101000" });
			AssertEquals(4, tariffChanger.Tariffs.Count);
			tariffChanger.Update();

			newFactory = new BusinessObjectFactory();
			AssertEquals(3, tariffChanger.ProductsChanged);
			AssertEquals(3, tariffChanger.ClassificationsChanged);

			AssertUpdateResults("Test Product 1", "0712903000", "99030225");
			AssertUpdateResults("Test Product 2", "8704320020", "");
			AssertUpdateResults("Test Product 3", "0712903000", "");
			AssertClassificationsUpdateResults("LOOK1", "2922292700");
			AssertClassificationsUpdateResults("LOOK2", "0601108500");
			AssertClassificationsUpdateResults("LOOK3", "0601108500");
			AssertClassificationsUpdateResults("LOOK4", "2904101000");

			product = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "Test Product 6"));
			AssertEquals(3, product.PivotsForBinding.Count);
			product.PivotsForBinding.Sort(CusClassPartPivotSchema.CI_SupplementalTariff.Name, System.ComponentModel.ListSortDirection.Descending);
			AssertEquals("0712903000", product.PivotsForBinding[0].CI_TariffNum);
			AssertEquals("99030225", product.PivotsForBinding[0].CI_SupplementalTariff);
			AssertEquals("0712903000", product.PivotsForBinding[1].CI_TariffNum);
			AssertEquals("0712903000", product.PivotsForBinding[2].CI_TariffNum);
		}
		BusinessObjectFactory newFactory;

		void AssertUpdateResults(string num, string tariff, string supTariff)
		{
			OrgSupplierPart product = newFactory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, num));
			AssertEquals(tariff, product.PivotsForBinding[0].CI_TariffNum);
			AssertEquals(supTariff, product.PivotsForBinding[0].CI_SupplementalTariff);
		}

		void AssertClassificationsUpdateResults(string num, string tariff)
		{
			CusClassification classification = newFactory.LoadTop1<CusClassification>(new ZQuery(CusClassificationSchema.CC_LookupCode, num));
			AssertEquals("Test Product 4", tariff, classification.CC_TariffNum);
		}

		OrgSupplierPart CreateSimpleProduct(string num, string type, string tariff, string supTariff)
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = num;
			CreatePivot(product.PK, type, tariff, supTariff);
			return product;
		}

		void CreatePivot(ZGuid productPK, string type, string tariff, string supTariff)
		{
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = productPK;
			pivot.CI_ChildType = type;
			pivot.CI_TariffNum = tariff;
			pivot.CI_SupplementalTariff = supTariff;
		}

		OrgSupplierPart CreateSimpleProductWithClass(string num, string classificType, string type, string tariff, string lookupCode)
		{
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = num;
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = tariff;
			classification.CC_LookupCode = lookupCode;
			classification.CC_ClassificationType = classificType;
			CusClassPartPivot importPivot = product.PivotsForBinding.AddNew();
			importPivot.CI_CC = classification.PK;
			importPivot.CI_ChildType = type;
			return product;
		}
	}
}
