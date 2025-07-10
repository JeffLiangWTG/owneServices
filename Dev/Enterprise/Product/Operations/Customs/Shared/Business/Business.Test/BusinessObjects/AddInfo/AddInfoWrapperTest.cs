using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class AddInfoWrapperTest : TestCaseWithFactory
	{
		public void TestGetKey()
		{
			var declaration = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			IAddInfo addInfo = declaration.addInfo;
			AssertEquals("AgentsReference", addInfo.GetKey(JobDeclarationSchema.Constants.JE_AgentsReference));
		}

		public void TestGetKeys()
		{
			var declaration = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			IAddInfo addInfo = declaration.addInfo;
			var keys = addInfo.GetKeys();
			var carrier = keys["CarrierCode"];
			AssertEquals("JE_CarrierCode", carrier.PropertyName);
		}

		public void TestSetValue()
		{
			var declaration = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			IAddInfo addInfo = declaration.addInfo;
			AssertEquals("", declaration.JE_CarrierCode);
			addInfo.SetValue(JobDeclarationSchema.Constants.JE_CarrierCode, "ABC");
			AssertEquals("ABC", declaration.JE_CarrierCode);
			addInfo.SetValue(JobDeclarationSchema.Constants.JE_CarrierCode, "ABCDEF");
			AssertEquals("ABCD", declaration.JE_CarrierCode);
			AssertEquals(0m, declaration.JE_TotalVolume);
			addInfo.SetValue(JobDeclarationSchema.Constants.JE_TotalVolume, "123.45");
			AssertEquals(123.45m, declaration.JE_TotalVolume);
		}

		public void TestGetEffectiveValue()
		{
			var declaration = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			declaration.JE_CarrierCode = "ABC";
			IAddInfo addInfo = declaration.addInfo;
			AssertEquals("ABC", addInfo.GetEffectiveValue(JobDeclarationSchema.Constants.JE_CarrierCode));
		}

		public void TestUpdateAddInfoFromString_WithoutRowUpdate() => AssertUpdateAddInfoFromString(false);
		public void TestUpdateAddInfoFromString_WithRowUpdate() => AssertUpdateAddInfoFromString(true);

		void AssertUpdateAddInfoFromString(bool withRowUpdate)
		{
			var stringToSplit = "FieldA1=value1*FieldN1=价值1*FieldA2=value2*FieldN2=价值2";
			var expectedAddInfo = "FieldA1=value1*FieldA2=value2";
			var expectedNAddInfo = "FieldN1=价值1*FieldN2=价值2";

			var bo = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			if (withRowUpdate)
			{
				((IColumnIndexer)((INeedRow)bo).Row)["JE_AddInfo"] = expectedAddInfo;
				((IColumnIndexer)((INeedRow)bo).Row)["JE_NAddInfo"] = expectedNAddInfo;
			}

			IAddInfo wrapper = bo.addInfo;
			wrapper.UpdateAddInfoFromString(stringToSplit);
			CombineAssertions(() =>
			{
				AssertEquals("JE_AddInfo", expectedAddInfo, bo.JE_AddInfo);
				AssertEquals("JE_NAddInfo", expectedNAddInfo, bo.JE_NAddInfo);
				AssertEquals("JE_FieldA1", "value1", bo.JE_FieldA1);
				AssertEquals("JE_FieldA2", "value2", bo.JE_FieldA2);
				AssertEquals("JE_FieldN1", "价值1", bo.JE_FieldN1);
				AssertEquals("JE_FieldN2", "价值2", bo.JE_FieldN2);
			});
		}

		public void TestUpdateRelatedPropertyInfo()
		{
			var expectedAddInfo = "AgentRef=value3*FieldA1=value1*FieldA2=value2";
			var expectedNAddInfo = "FieldN1=价值1*FieldN2=价值2*GoodsDesc=价值3";
			var declaration = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			declaration.JE_FieldA1 = "value1";
			declaration.JE_FieldA2 = "value2";
			declaration.JE_AgentsReference = "value3";
			declaration.JE_FieldN1 = "价值1";
			declaration.JE_FieldN2 = "价值2";
			declaration.JE_GoodsDescription = "价值3";
			CombineAssertions(() =>
			{
				AssertEquals("Pre: JE_AddInfo", ZString.Empty, declaration.JE_AddInfo);
				AssertEquals("Pre: JE_NAddInfo", ZString.Empty, declaration.JE_NAddInfo);

				IAddInfo addInfo = declaration.addInfo;
				addInfo.UpdateRelatedPropertyInfo();
				AssertEquals("Post: JE_AddInfo", expectedAddInfo, declaration.JE_AddInfo);
				AssertEquals("Post: JE_NAddInfo", expectedNAddInfo, declaration.JE_NAddInfo);
			});
		}

		public void TestParent()
		{
			var declaration = Factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			IAddInfo addInfo = declaration.addInfo;
			AssertSame(declaration, addInfo.Parent);
		}

		public void TestReload()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration = factory.New<JobDeclarationWithIAddInfoManagerWithSchema>();
			factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration2 = factory2.Load<JobDeclarationWithIAddInfoManagerWithSchema>(declaration.PK);
			declaration2.JE_FieldA1 = "value1";
			declaration2.JE_FieldA2 = "value2";
			declaration2.JE_AgentsReference = "value3";
			declaration2.JE_FieldN1 = "价值1";
			declaration2.JE_FieldN2 = "价值2";
			declaration2.JE_GoodsDescription = "价值3";
			factory2.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Pre: JE_AddInfo", ZString.Empty, declaration.JE_AddInfo);
				AssertEquals("Pre: JE_NAddInfo", ZString.Empty, declaration.JE_NAddInfo);
				AssertEquals("Pre: JE_FieldA1", ZString.Empty, declaration.JE_FieldA1);
				AssertEquals("Pre: JE_FieldA2", ZString.Empty, declaration.JE_FieldA2);
				AssertEquals("Pre: JE_AgentsReference", ZString.Empty, declaration.JE_AgentsReference);
				AssertEquals("Pre: JE_FieldN1", ZString.Empty, declaration.JE_FieldN1);
				AssertEquals("Pre: JE_FieldN2", ZString.Empty, declaration.JE_FieldN2);
				AssertEquals("Pre: JE_GoodsDescription", ZString.Empty, declaration.JE_GoodsDescription);
				declaration.Reload();
				var expectedAddInfo = "AgentRef=value3*FieldA1=value1*FieldA2=value2";
				var expectedNAddInfo = "FieldN1=价值1*FieldN2=价值2*GoodsDesc=价值3";
				AssertEquals("Post: JE_AddInfo", expectedAddInfo, declaration.JE_AddInfo);
				AssertEquals("Post: JE_NAddInfo", expectedNAddInfo, declaration.JE_NAddInfo);
				AssertEquals("Post: JE_FieldA1", "value1", declaration.JE_FieldA1);
				AssertEquals("Post: JE_FieldA2", "value2", declaration.JE_FieldA2);
				AssertEquals("Post: JE_AgentsReference", "value3", declaration.JE_AgentsReference);
				AssertEquals("Post: JE_FieldN1", "价值1", declaration.JE_FieldN1);
				AssertEquals("Post: JE_FieldN2", "价值2", declaration.JE_FieldN2);
				AssertEquals("Post: JE_GoodsDescription", "价值3", declaration.JE_GoodsDescription);
			});
		}

		public void TestReportWhenParentUseINAddInfoSupporter()
		{
			var bizObj = Factory.New<JobDeclarationTestClass>();
			var addInfoWrapper = bizObj.addInfo;
			AssertEquals(
				"Enterprise.Customs.Business.Testing.AddInfoWrapperTest+JobDeclarationTestClass has implement INAddInfoSupporter, it should not be used in conjuction with AddInfoWrapper`1 as INAddInfoSupporter will cause _AddInfo and _NAddInfo to be combine; see AddInfoParser.ConcatAddInfoStrings usage.",
				ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		class JobDeclarationTestClass : JobDeclarationWithIAddInfoManagerWithSchema, INAddInfoSupporter
		{
			public JobDeclarationTestClass(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			ZPropertyInfoString INAddInfoSupporter.NAddInfoProperty => JE_NAddInfoInfo as ZPropertyInfoString;
		}

		internal class JobDeclarationWithIAddInfoManagerWithSchema : BaseJobDeclaration, IAddInfoManagerWithSchema, IAddInfoWithSyncPropertySupporter, IAddInfoWithSyncProperty
		{
			public JobDeclarationWithIAddInfoManagerWithSchema(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
				addInfo = new AddInfoWrapper<JobDeclarationWithIAddInfoManagerWithSchema>(this, Schema.JE_AddInfo, () => AddInfoNamesMapping, Schema.JE_NAddInfo, () => NAddInfoNamesMapping);
			}

			public readonly AddInfoWrapper<JobDeclarationWithIAddInfoManagerWithSchema> addInfo;

			public override void OnLoaded()
			{
				base.OnLoaded();
				AddInfoParser.Deserialise(JE_AddInfo, AddInfoNamesMapping, true);
				AddInfoParser.Deserialise(JE_NAddInfo, NAddInfoNamesMapping, true);
			}

			protected override void OnFactorySaving()
			{
				if (HasChangesNotIncludingChildren)
				{
					using (GetValidationSuspender())
					{
						addInfo.UpdateRelatedPropertyInfo();
					}
				}
				base.OnFactorySaving();
			}

			public override void OnSaved(bool saveSucceeded)
			{
				if (saveSucceeded)
				{
					AddInfoNamesMapping.Values.ForEach(x => x.OriginalValue = x.Value);
					NAddInfoNamesMapping.Values.ForEach(x => x.OriginalValue = x.Value);
				}
				base.OnSaved(saveSucceeded);
			}

			[AddInfoSyncProperty("AgentRef", typeof(ZString))]
			public override ZString JE_AgentsReference { get => base.JE_AgentsReference; set => base.JE_AgentsReference = value; }

			[IsNAddInfoField]
			[AddInfoSyncProperty("GoodsDesc", typeof(ZString))]
			public override ZString JE_GoodsDescription { get => base.JE_GoodsDescription; set => base.JE_GoodsDescription = value; }

			public virtual ZString JE_FieldA1
			{
				get => JE_FieldA1Data.Value;
				set
				{
					SetNonPersistentPropertyValue(JE_FieldA1Info, ref JE_FieldA1Data.Value, value);
				}
			}
			public ZPropertyInfo JE_FieldA1Info => GetZPropertyInfo(nameof(JE_FieldA1));
			AddInfoPropertyData<ZString> JE_FieldA1Data => fieldA1Data ?? (fieldA1Data = new AddInfoPropertyData<ZString>(nameof(JE_FieldA1)));
			AddInfoPropertyData<ZString> fieldA1Data;

			public virtual ZString JE_FieldA2
			{
				get => JE_FieldA2Data.Value;
				set
				{
					SetNonPersistentPropertyValue(JE_FieldA2Info, ref JE_FieldA2Data.Value, value);
				}
			}
			public ZPropertyInfo JE_FieldA2Info => GetZPropertyInfo(nameof(JE_FieldA2));
			AddInfoPropertyData<ZString> JE_FieldA2Data => fieldA2Data ?? (fieldA2Data = new AddInfoPropertyData<ZString>(nameof(JE_FieldA2)));
			AddInfoPropertyData<ZString> fieldA2Data;

			public IDictionary<string, IAddInfoPropertyData> AddInfoNamesMapping
			{
				get
				{
					return addInfoNamesMapping ?? (addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
					{
						{ "FieldA1", JE_FieldA1Data },
						{ "FieldA2", JE_FieldA2Data },
					});
				}
			}
			IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping;

			public virtual ZString JE_FieldN1
			{
				get => JE_FieldN1Data.Value;
				set
				{
					SetNonPersistentPropertyValue(JE_FieldN1Info, ref JE_FieldN1Data.Value, value);
				}
			}
			public ZPropertyInfo JE_FieldN1Info => GetZPropertyInfo(nameof(JE_FieldN1));
			AddInfoPropertyData<ZString> JE_FieldN1Data => fieldN1Data ?? (fieldN1Data = new AddInfoPropertyData<ZString>(nameof(JE_FieldN1)));
			AddInfoPropertyData<ZString> fieldN1Data;

			public virtual ZString JE_FieldN2
			{
				get => JE_FieldN2Data.Value;
				set
				{
					SetNonPersistentPropertyValue(JE_FieldN2Info, ref JE_FieldN2Data.Value, value);
				}
			}
			public ZPropertyInfo JE_FieldN2Info => GetZPropertyInfo(nameof(JE_FieldN2));
			AddInfoPropertyData<ZString> JE_FieldN2Data => fieldN2Data ?? (fieldN2Data = new AddInfoPropertyData<ZString>(nameof(JE_FieldN2)));
			AddInfoPropertyData<ZString> fieldN2Data;

			public IDictionary<string, IAddInfoPropertyData> NAddInfoNamesMapping
			{
				get
				{
					return nAddInfoNamesMapping ?? (nAddInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData>
					{
						{ "FieldN1", JE_FieldN1Data },
						{ "FieldN2", JE_FieldN2Data },
					});
				}
			}
			IDictionary<string, IAddInfoPropertyData> nAddInfoNamesMapping;

			IAddInfo IAddInfoManager.AddInfo => addInfo;

			ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => JobDeclarationSchema.Instance;

			IAddInfoWithSyncProperty IAddInfoWithSyncPropertySupporter.AddInfo => this;

			void IAddInfoWithSyncProperty.EnableSynchronization() { }

			IZType IAddInfoWithSyncProperty.GetAddInfoValue(IZType data, Type addInfoValueType)
				=> (IZType)Activator.CreateInstance(addInfoValueType, data);

			ZPropertyInfo IAddInfoWithSyncProperty.AddInfoProperty => JE_AddInfoInfo;
		}
	}
}
