using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class IAddInfoWithSyncPropertySupporterBaseOnly : TestCaseWithFactory
	{
		public void TestDataIsSync()
		{
			CombineAssertions(() =>
			{
				var bizobj = Factory.New<DummyBusinessObjectWithAddInfoWithSyncPropertySupporter>();
				var addInfo = bizobj.AddInfo;
				addInfo.UZ_Int = 10;
				bizobj.Z0_Number = 20;
				addInfo.ColumnsForFastSearchExposed = new SchemaColumn[] { TestAddInfoSchema.UZ_Decimal };
				addInfo.UZ_Decimal = 3234.43m;
				bizobj.Z0_Decimal = 43234.23m;
				bizobj.Z0_Bool = ZBool.True;
				bizobj.Z0_Code = "2SDF4";
				addInfo.UZ_NString = "一二三";
				bizobj.Z0_NVarChar = "一三";
				bizobj.Z0_DateOnly = new ZDate(2021, 6, 25);
				Factory.Save();
				AssertEquals("Z0_VarCharMax", "Decimal=3234.43*Int2=10*TestCode=2SDF4*TestNumber=20*DateValue=2021-06-25 00:00:00.000*NString=一二三*TestNVarChar=一三", bizobj.Z0_VarCharMax);
				AssertEquals("Z0_NVarCharMax", "NString=一二三*TestNVarChar=一三", bizobj.Z0_NVarCharMax);
				AssertHasGenAddOnColumn(bizobj.PK, "UZ_TestCode", "STR", "2SDF4");
				AssertHasGenAddOnColumn(bizobj.PK, "Z0_TestNumber1", "INT", "20");
				AssertHasGenAddOnColumn(bizobj.PK, "UZ_Decimal", "DEC", "3234.43");
				AssertHasGenAddOnColumn(bizobj.PK, "UZ_DateValue", "DAT", "2021-06-25 00:00:00.000");

				addInfo.UZ_Decimal = ZDecimal.Zero;
				addInfo.UZ_Boolean = ZBool.True;
				Factory.Save();
				AssertEquals("Z0_VarCharMax", "Boolean=Y*Int2=10*TestCode=2SDF4*TestNumber=20*DateValue=2021-06-25 00:00:00.000*NString=一二三*TestNVarChar=一三", bizobj.Z0_VarCharMax);
				AssertEquals("Z0_NVarCharMax", "NString=一二三*TestNVarChar=一三", bizobj.Z0_NVarCharMax);
				AssertNull("GenAddOnColumns UZ_Decimal", GetGenAddOnColumnBizObj(bizobj.PK, "UZ_Decimal"));

				var newFactory = new BusinessObjectFactory();
				bizobj = newFactory.Load<DummyBusinessObjectWithAddInfoWithSyncPropertySupporter>(bizobj.PK);
				bizobj.Z0_Code = "3JDS4";
				bizobj.Z0_Number = 30;
				newFactory.Save();
				AssertEquals("Z0_VarCharMax", "Boolean=Y*Int2=10*TestCode=3JDS4*TestNumber=30*DateValue=2021-06-25 00:00:00.000*NString=一二三*TestNVarChar=一三", bizobj.Z0_VarCharMax);
				AssertEquals("Z0_NVarCharMax", "NString=一二三*TestNVarChar=一三", bizobj.Z0_NVarCharMax);
				AssertHasGenAddOnColumn(bizobj.PK, "UZ_TestCode", "STR", "3JDS4");
				AssertHasGenAddOnColumn(bizobj.PK, "Z0_TestNumber1", "INT", "30");
			});
		}

		GenAddOnColumn GetGenAddOnColumnBizObj(ZGuid parentID, ZString name)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentID);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, name);
			return Factory.LoadTop1<GenAddOnColumn>(query);
		}

		void AssertHasGenAddOnColumn(ZGuid parentID, ZString name, ZString type, ZString data)
		{
			AssertGenAddOnColumn(GetGenAddOnColumnBizObj(parentID, name), type, data);
		}

		void AssertGenAddOnColumn(GenAddOnColumn genAddOnColumn, ZString type, ZString data)
		{
			var name = genAddOnColumn.XA_Name;
			AssertEquals(name + " - XA_Type", type, genAddOnColumn.XA_Type);
			AssertEquals(name + " - XA_Data", data, genAddOnColumn.XA_Data);
		}

		public void TestDuplicateAddInfoSyncProperties()
		{
			try
			{
				_ = Factory.New<DummyBusinessObjectWithAddInfoWithSyncPropertySupporterDuplicate>();
				Fail("Should have thrown exception");
			}
			catch (ApplicationException ex)
			{
				AssertEquals("ex.Message", "Can't create a DummyBusinessObjectWithAddInfoWithSyncPropertySupporterDuplicate as its constructor threw an exception.", ex.Message.TrimEnd());
				var innerEx = ex.InnerException;
				AssertType<System.Reflection.TargetInvocationException>(innerEx);
				innerEx = innerEx.InnerException;
				AssertType<ArgumentException>(innerEx);
				AssertContains("innerEx.Message", "An item with the same key has already been added.", innerEx.Message.TrimEnd());
			}
		}
	}

	class DummyBusinessObjectWithAddInfoWithSyncPropertySupporterDuplicate : DummyBusinessObjectWithAddInfoWithSyncPropertySupporter
	{
		public DummyBusinessObjectWithAddInfoWithSyncPropertySupporterDuplicate(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		[AddInfoSyncProperty("Decimal", typeof(Decimal))]
		public override ZInt Z0_AnotherNumber { get => base.Z0_AnotherNumber; set => base.Z0_AnotherNumber = value; }
	}

	[SystemDefinedValues]
	class DummyBusinessObjectWithAddInfoWithSyncPropertySupporter : DummyBusinessObjectWithIAddInfoWithConcurrencyResolverSupporter, IAddInfoWithSyncPropertySupporter, IAddInfoWithConcurrencyResolverSupporter
	{
		public DummyBusinessObjectWithAddInfoWithSyncPropertySupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[IsNAddInfoField]
		[AddInfoSyncProperty("TestNVarChar", typeof(ZString))]
		public override ZString Z0_NVarChar { get => base.Z0_NVarChar; set => base.Z0_NVarChar = value; }

		[AddInfoSyncProperty("Boolean", typeof(ZBool))]
		public override ZBool Z0_Bool { get => base.Z0_Bool; set => base.Z0_Bool = value; }

		[AddInfoSyncProperty("TestCode", typeof(ZString), "UZ_TestCode")]
		public override ZString Z0_Code { get => base.Z0_Code; set => base.Z0_Code = value; }

		[AddInfoSyncProperty("TestNumber", typeof(ZInt), "Z0_TestNumber1")]
		public override ZInt Z0_Number { get => base.Z0_Number; set => base.Z0_Number = value; }

		[AddInfoSyncProperty("Decimal", typeof(ZDecimal), "UZ_Decimal")]
		public override ZDecimal Z0_Decimal { get => base.Z0_Decimal; set => base.Z0_Decimal = value; }

		[AddInfoSyncProperty("DateValue", typeof(ZDateTime), "UZ_DateValue")]
		public override ZDate Z0_DateOnly { get => base.Z0_DateOnly; set => base.Z0_DateOnly = value; }

		IAddInfoWithSyncProperty IAddInfoWithSyncPropertySupporter.AddInfo => AddInfo;

		BaseAddInfo IAddInfoWithConcurrencyResolverSupporter.NewAddInfoBizObj(ZPropertyInfo addInfoProperty)
		{
			return new TestAddInfoWithSyncPropertySupporter(addInfoProperty);
		}
	}

	class TestAddInfoWithSyncPropertySupporter : TestAddInfo
	{
		public TestAddInfoWithSyncPropertySupporter(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{ }

		protected override string GetDBName(string propertyName)
		{
			return TestAddInfoSchema.Constants.UZ_Int == propertyName ? "Int2" : base.GetDBName(propertyName);
		}

		protected override SchemaColumn[] ColumnsForFastSearch => new SchemaColumn[] { TestAddInfoSchema.UZ_Decimal };
	}
}
