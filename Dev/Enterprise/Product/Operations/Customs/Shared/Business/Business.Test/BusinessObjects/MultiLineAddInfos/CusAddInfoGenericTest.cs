using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.Business.MultiLineAddInfos.Testing.UnknownCusAddInfoTest;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestedType(typeof(CusAddInfo<AddInfoWithTypeCode>))]
	sealed class CusAddInfoGenericTest : CusAddInfoTest<CusAddInfo<AddInfoWithTypeCode>>
	{
		public void TestIAddInfoManagerMembers()
		{
			var info = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			IAddInfoManager manager = info;
			AssertEquals(info.Data, manager.AddInfo);
		}

		public void TestClone()
		{
			CusAddInfo<AddInfoWithTypeCode> info = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			info.B7_ParentID = ZGuid.NewZGuid();
			info.B7_ParentTableCode = "B7";
			info.Data.UZ_String = "Fred";

			CusAddInfo<AddInfoWithTypeCode> clonedInfo = info.Clone();
			AssertNotEquals("Cloned BO PK should be different", info.PK, clonedInfo.PK);
			AssertEquals("clonedInfo.B7_ParentID", ZGuid.Empty, clonedInfo.B7_ParentID);
			AssertEquals("clonedInfo.B7_ParentTableCode", ZString.Empty, clonedInfo.B7_ParentTableCode);
			AssertEquals("clonedInfo.Data.UZ_String", "Fred", clonedInfo.Data.UZ_String);
		}

		public void TestParentIsSetOnLoaded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = (BaseJobDeclaration)Factory.New<US.IJobDeclaration>();
				var declarationCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)declaration;
				Type itDocType = null;
				declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITDoc, out itDocType);
				var itDoc = (CusAddInfo)Factory.New(itDocType);
				itDoc.Parent = declaration;
				itDoc.B7_AddInfoData = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=AREA1234";
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var itDocInDiffFactory = newFactory.Load<CusAddInfo>(itDoc.PK);
				var parent = itDocInDiffFactory.Parent;
				AssertNotNull("Parent should be set from B7_ParentTableCode and B7_ParentID", parent);
				AssertEquals("parent should be the correct declaration", declaration.PK, parent.PK);
			}
		}

		public void TestDefaultingOfTypeFromAttribute()
		{
			CusAddInfo<AddInfoWithTypeCode> info = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			AssertEquals("info.B7_Type", CusAddInfoTypeAttribute.Codes.TypeCodeForTesting, info.B7_Type);
		}

		public void TestSerialisationToB7_AddInfoData()
		{
			CusAddInfo<AddInfoWithTypeCode> info = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			info.Data.UZ_String = "MyTestString";
			info.B7_ParentTableCode = "JE";
			info.B7_ParentID = ZGuid.NewZGuid();
			info.B7_Type = "PGA";
			Factory.Save();
			AssertEquals("info.B7_AddInfoColumn", "String=MyTestString", info.B7_AddInfoData);
			info.Data.UZ_String = "";
			Factory.Save();
			AssertEquals("info.B7_AddInfoColumn", "", info.B7_AddInfoData);
		}

		[ExpectNoExceptions]
		public void TestWrongConstructorBlows()
		{
			NUnit.Framework.Assert.That(delegate
			{
				Factory.New<CusAddInfo<AddInfoWithWrongConstructor>>();
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentException)));
		}

		[ExpectNoExceptions]
		public void TestMissingTypeCodeAttributeBlows()
		{
			NUnit.Framework.Assert.That(delegate
			{
				Factory.New<CusAddInfo<TestAddInfo>>();
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentException)));
		}

		public void TestNoDBHitOnStmDocDataOverrideWhenDelete()
		{
			var factory = Factory.CreateNewFactory();
			var cusAddInfo = factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			cusAddInfo.B7_ParentTableCode = "JE";
			cusAddInfo.B7_ParentID = ZGuid.NewZGuid();
			cusAddInfo.B7_Type = "PGA";
			factory.Save();
			cusAddInfo.Delete();
			var expectedDBHits = new Dictionary<string, int> { { "StmDocDataOverride", 0 } };
			AssertDbHits(expectedDBHits, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			BusinessObject bizObj = base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			((IBusinessObjectInternals)bizObj).Row[CusAddInfo.Schema.B7_Type] = "";
			return bizObj;
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[CusAddInfo.Schema.B7_Type] = (ZString)"_._";
				return result;
			}
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, CusAddInfo<AddInfoWithTypeCode> bizObj)
		{
			factory.Load<JobDeclarationWithCusAddInfoTypeSupporter>(bizObj.B7_ParentID);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var sql = @"
IF (OBJECT_ID('Constraint_B7_Type') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.CusAddInfo NOCHECK CONSTRAINT Constraint_B7_Type
END";
			TestConnection.ExecuteNonQuery(sql);

			var declaration = factory.New<JobDeclarationWithCusAddInfoTypeSupporter>();
			var result = factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			result.B7_ParentID = declaration.PK;
			result.B7_ParentTableCode = declaration.TablePrefix;
			return result;
		}

		public void TestB7ParentIDAndParentTableCode()
		{
			var atf = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();
			atf.B7_ParentID = ZGuid.NewZGuid();
			atf.B7_ParentTableCode = "JE";
			atf.B7_Type = "PGA";
			Factory.Save();

			AssertExceptionThrown<ZSaveException>(() =>
		   {
			   atf.OnLoaded();
			   atf.B7_ParentID = ZGuid.Empty;
			   Factory.Save();
		   });

			AssertExceptionThrown<ZSaveException>(() =>
			{
				atf.OnLoaded();
				atf.B7_ParentID = ZGuid.Missing;
				Factory.Save();
			});
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("ZGuid.Missing is saved to database in column B7_ParentID", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertExceptionThrown<ZSaveException>(() =>
			{
				atf.OnLoaded();
				atf.B7_ParentID = ZGuid.Invalid;
				Factory.Save();
			});
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("ZGuid.Invalid is saved to database in column B7_ParentID", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertExceptionThrown<ZSaveException>(() =>
			{
				atf.OnLoaded();
				atf.B7_ParentID = ZGuid.NewZGuid();
				atf.B7_ParentTableCode = ZString.Empty;
				Factory.Save();
			});
		}

		public void TestB7_AddInfoSet()
		{
			var bo = Factory.New<CusAddInfo<AddInfoWithTypeCode>>();

			AssertEquals(ZString.Empty, bo.B7_AddInfoData);
			AssertEquals(ZString.Empty, bo.B7_NAddInfoData);

			bo.B7_AddInfoData = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.B7_AddInfoData);
			AssertEquals(ZString.Empty, bo.B7_NAddInfoData);
		}

		public void TestB7_AddInfoSet_WithBaseAddInfo()
		{
			var bo = Factory.New<CusAddInfoWithBaseAddInfoForTesting>();

			AssertEquals(ZString.Empty, bo.B7_AddInfoData);
			AssertEquals(ZString.Empty, bo.B7_NAddInfoData);

			bo.B7_AddInfoData = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.B7_AddInfoData);
			AssertEquals("NString=def", bo.B7_NAddInfoData);
		}

		public void TestB7_AddInfoSet_WithAddInfoWrapper()
		{
			var bo = Factory.New<CusAddInfoWithAddInfoWrapperForTesting>();

			AssertEquals(ZString.Empty, bo.B7_AddInfoData);
			AssertEquals(ZString.Empty, bo.B7_NAddInfoData);
			AssertEquals(ZString.Empty, bo.B7_String);
			AssertEquals(ZString.Empty, bo.B7_NString);

			bo.B7_AddInfoData = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.B7_AddInfoData);
			AssertEquals("", bo.B7_NAddInfoData);
			AssertEquals("abc", bo.B7_String);
			AssertEquals("", bo.B7_NString);

			bo.B7_NAddInfoData = "String=ghi*NString=jkl";

			AssertEquals("String=abc*NString=def", bo.B7_AddInfoData);
			AssertEquals("String=ghi*NString=jkl", bo.B7_NAddInfoData);
			AssertEquals("abc", bo.B7_String);
			AssertEquals("jkl", bo.B7_NString);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var bizObj = (CusAddInfo<AddInfoWithTypeCode>)base.GetBusinessObjectForFetchForLoad();
			bizObj.B7_ParentTableCode = CusAddInfoSchema.Constants.Prefix;
			bizObj.B7_Type = "PGA";
			return bizObj;
		}
	}

	sealed class CusAddInfoWithBaseAddInfoForTesting : CusAddInfo<AddInfoWithTypeCode>, IAddInfoManager, INAddInfoSupporter
	{
		public CusAddInfoWithBaseAddInfoForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public TestAddInfo AddInfo => addInfo ?? (addInfo = new TestAddInfo(this));
		TestAddInfo addInfo;

		IAddInfo IAddInfoManager.AddInfo => AddInfo;
		public ZPropertyInfoString NAddInfoProperty => B7_NAddInfoDataInfo as ZPropertyInfoString;
	}

	sealed class CusAddInfoWithAddInfoWrapperForTesting : CusAddInfo<AddInfoWithTypeCode>, IAddInfoManagerWithSchema
	{
		public CusAddInfoWithAddInfoWrapperForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			addInfo = new AddInfoWrapper<CusAddInfoWithAddInfoWrapperForTesting>(
				this,
				Schema.B7_AddInfoData,
				() => AddInfoNamesMapping,
				Schema.B7_NAddInfoData,
				() => NAddInfoNamesMapping
			);
		}

		public ZString B7_String
		{
			get => B7_StringData.Value;
			set
			{
				SetNonPersistentPropertyValue(B7_StringInfo, ref B7_StringData.Value, value);
			}
		}
		public ZPropertyInfo B7_StringInfo => GetZPropertyInfo(nameof(B7_String));
		AddInfoPropertyData<ZString> B7_StringData => b7_String ?? (b7_String = new AddInfoPropertyData<ZString>(nameof(B7_String)));
		AddInfoPropertyData<ZString> b7_String;

		public ZString B7_NString
		{
			get => B7_NStringData.Value;
			set
			{
				SetNonPersistentPropertyValue(B7_NStringInfo, ref B7_NStringData.Value, value);
			}
		}
		public ZPropertyInfo B7_NStringInfo => GetZPropertyInfo(nameof(B7_NString));
		AddInfoPropertyData<ZString> B7_NStringData => b7_NString ?? (b7_NString = new AddInfoPropertyData<ZString>(nameof(B7_NString)));
		AddInfoPropertyData<ZString> b7_NString;

		IDictionary<string, IAddInfoPropertyData> AddInfoNamesMapping => addInfoNamesMapping ?? (addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "String", B7_StringData } });
		IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping;

		IDictionary<string, IAddInfoPropertyData> NAddInfoNamesMapping => nAddInfoNamesMapping ?? (nAddInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "NString", B7_NStringData } });
		IDictionary<string, IAddInfoPropertyData> nAddInfoNamesMapping;

		IAddInfo IAddInfoManager.AddInfo => addInfo;
		readonly IAddInfo addInfo;

		ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => TWJobDeclarationSchema.Instance;
	}
}
