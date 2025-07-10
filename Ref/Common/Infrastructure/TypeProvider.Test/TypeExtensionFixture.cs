using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.TypeProvider.Test
{
	[TestFixture]
	class TypeExtensionFixture
	{
		[Test]
		public void GetPKPropertyInfo()
		{
			var dummy = new Dummy { ZXY_PK = Guid.NewGuid() };
			Assert.AreEqual(dummy.ZXY_PK, typeof(Dummy).GetPKPropertyInfo().GetValue(dummy));
			Assert.AreEqual(nameof(dummy.ZXY_PK), typeof(Dummy).GetPKPropertyName());

			var dummyPK = new DummyParentPK { RVC_ParentPK = Guid.NewGuid() };
			Assert.AreEqual(dummyPK.RVC_ParentPK, typeof(DummyParentPK).GetPKPropertyInfo().GetValue(dummyPK));
			Assert.AreEqual(nameof(dummyPK.RVC_ParentPK), typeof(DummyParentPK).GetPKPropertyName());

			var dummyWithId = new DummyWithId { ZXY_PortId = 1 };
			Assert.AreEqual(dummyWithId.ZXY_PortId, typeof(DummyWithId).GetPKPropertyInfo().GetValue(dummyWithId));
			Assert.AreEqual(nameof(dummyWithId.ZXY_PortId), typeof(DummyWithId).GetPKPropertyName());

			var dummyWithIdAndPk = new DummyWithIdAndPk { ZXY_PK = Guid.NewGuid(), ZXY_PortId = 1 };
			Assert.AreEqual(dummyWithIdAndPk.ZXY_PK, typeof(DummyWithIdAndPk).GetPKPropertyInfo().GetValue(dummyWithIdAndPk));
			Assert.AreEqual(nameof(dummyWithIdAndPk.ZXY_PK), typeof(DummyWithIdAndPk).GetPKPropertyName());

			var dummyWithIdAndPkAndRvcParentPk = new DummyWithIdAndPkAndRvcParentPk { RVC_ParentPK = Guid.NewGuid(), ZXY_PK = Guid.NewGuid(), ZXY_PortId = 1 };
			Assert.AreEqual(dummyWithIdAndPkAndRvcParentPk.RVC_ParentPK, typeof(DummyWithIdAndPkAndRvcParentPk).GetPKPropertyInfo().GetValue(dummyWithIdAndPkAndRvcParentPk));
			Assert.AreEqual(nameof(dummyWithIdAndPkAndRvcParentPk.RVC_ParentPK), typeof(DummyWithIdAndPkAndRvcParentPk).GetPKPropertyName());
		}

		[Test]
		public void GetDataSetPKPropertyInfo()
		{
			var dataSetPK = Guid.NewGuid();
			var dummy = new Dummy { ZXY_PK = Guid.NewGuid(), ZXY_ZZ1_Tariff = dataSetPK };
			Assert.AreEqual(nameof(dummy.ZXY_ZZ1_Tariff), typeof(Dummy).GetDataSetPKPropertyInfo()?.Name);
			Assert.AreEqual(dataSetPK, typeof(Dummy).GetDataSetPKPropertyInfo().GetValue(dummy));

			var dependentDummy = new DependentDummy { ZYX_PK = Guid.NewGuid(), ZYX_DataSetPK = dataSetPK };
			Assert.AreEqual(nameof(dependentDummy.ZYX_DataSetPK), typeof(DependentDummy).GetDataSetPKPropertyInfo()?.Name);
			Assert.AreEqual(dataSetPK, typeof(DependentDummy).GetDataSetPKPropertyInfo().GetValue(dependentDummy));
		}

		[Test]
		public void GetFKPropertyInfo()
		{
			Assert.AreEqual(nameof(DependentDummy.ZYX_ZXY), typeof(DependentDummy).GetFKPropertyInfo(typeof(Dummy)).Name);
			Assert.AreEqual(nameof(DependentDummy.ZYX_ZXY), typeof(DependentDummy).GetFKPropertyInfo("ZXY").Name);
		}

		[Test]
		public void GetFKPropertyInfoIncludingStrings()
		{
			Assert.AreEqual(nameof(DependentDummy.ZYX_ZXY), typeof(DependentDummy).GetNKorFKPropertyInfo(typeof(Dummy), false).Name);
			Assert.AreEqual(nameof(DependentDummy.ZYX_ZZY_StringDependency), typeof(DependentDummy).GetNKorFKPropertyInfo(typeof(DummyString), false).Name);
			Assert.AreEqual(nameof(DummyParentPK.RVC_ParentPK), typeof(DummyParentPK).GetNKorFKPropertyInfo(typeof(DummyString), true).Name);
			Assert.IsNull(typeof(DummyParentPK).GetNKorFKPropertyInfo(typeof(DummyString), false));
		}

		[Test]
		public void GetCollectionNavigationPropertyInfo()
		{
			Assert.AreEqual(nameof(Dummy.Dependents), typeof(Dummy).GetCollectionNavigationPropertyInfo(typeof(DependentDummy)).Name);
		}

		[Test]
		public void GetCollectionNavigationPropertyInfos()
		{
			var relatedProperties = typeof(Dummy).GetCollectionNavigationPropertyInfos();
			Assert.AreEqual(1, relatedProperties.Count());
			Assert.AreEqual(typeof(ICollection<DependentDummy>), relatedProperties.FirstOrDefault().PropertyType);
		}

		[Test]
		public void GetElementTypeOfCollection()
		{
			Assert.AreEqual(typeof(DependentDummy), typeof(ICollection<DependentDummy>).GetElementTypeOfCollection());
		}

		[Test]
		public void GetTablePrefix()
		{
			Assert.AreEqual("ZXY", typeof(Dummy).GetTablePrefix());
			Assert.AreEqual("RVC", typeof(DummyParentPK).GetTablePrefix());
		}

		[TestCase(false)]
		[TestCase(true)]
		public void IsRelatedTo(bool includeParentPK)
		{
			Assert.AreEqual(false, typeof(Dummy).IsRelatedTo(typeof(Dummy), includeParentPK));
			Assert.AreEqual(false, typeof(Dummy).IsRelatedTo(typeof(DependentDummy), includeParentPK));
			Assert.AreEqual(true, typeof(DependentDummy).IsRelatedTo(typeof(Dummy), includeParentPK));
			Assert.AreEqual(includeParentPK, typeof(DummyParentPK).IsRelatedTo(typeof(Dummy), includeParentPK));
		}

		[Test]
		public void IsExpirableType()
		{
			Assert.False(typeof(Dummy).IsExpirableType());
			Assert.True(typeof(DummyString).IsExpirableType());
		}

		[Test]
		public void TestGetNavigationPropertyInfo()
		{
			var propInfo = typeof(Dummy).GetNavigationPropertyInfo(typeof(DummyB));
			Assert.IsNotNull(propInfo);
			Assert.AreEqual("DummyBNavigationProp", propInfo.Name);

			Assert.IsNull(typeof(RelatedDummy).GetNavigationPropertyInfo(typeof(DummyB)));
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsSystemType(Type type)
		{
			var result = type.IsSystemType();
			if (type.Namespace == "System")
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsBoolType(Type type)
		{
			var result = type.IsBoolType();
			if (type == typeof(bool))
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsStringType(Type type)
		{
			var result = type.IsStringType();
			if (type == typeof(string))
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsIntegerType(Type type)
		{
			var result = type.IsIntegerType();
			if (type == typeof(int) || type == typeof(long))
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsNumericType(Type type)
		{
			var result = type.IsNumericType();
			if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsDateTime(Type type)
		{
			var result = type.IsDateTime();
			if (type == typeof(DateTime))
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[TestCaseSource(nameof(AllTypes))]
		public void IsNullable(Type type)
		{
			var result = type.IsNullable();
			if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
			{
				Assert.True(result);
			}
			else
			{
				Assert.False(result);
			}
		}

		[Test]
		public void TestIsNonPersistentType()
		{
			Assert.That(typeof(DummyNonPersistentType).IsNonPersistent());
			Assert.That(!typeof(DummyPersistentType).IsNonPersistent());
		}

		static Type[] AllTypes = new[] {
			typeof(byte),
			typeof(byte[]),
			typeof(string),
			typeof(bool),
			typeof(bool?),
			typeof(int),
			typeof(int?),
			typeof(long),
			typeof(long?),
			typeof(decimal),
			typeof(decimal?),
			typeof(float),
			typeof(float?),
			typeof(double),
			typeof(double?),
			typeof(DateTime),
			typeof(DateTime?),
			typeof(Guid),
			typeof(Guid?),
			typeof(Dummy),
			typeof(DependentDummy)
		};

		[TestCaseSource(nameof(ChangeValues))]
		public void ChangeType(string value, Type targetType, object expectedValue)
		{
			Assert.AreEqual(expectedValue, TypeExtension.ChangeType(value, targetType));
		}

		[Test]
		public void ChangeType_Geometry()
		{
			var wellKnowText = "Point (10 20)";
			var result = (Geometry)TypeExtension.ChangeType(wellKnowText, typeof(Geometry));
			Assert.That(result.ToText(), Is.EqualTo(wellKnowText).IgnoreCase);
		}

		[Test]
		public void TestContainsIsActiveColumn()
		{
			Assert.That(typeof(DummyTypeWithIsActiveColumn).ContainsIsActiveColumn, Is.True);
			Assert.That(typeof(DummyTypeWithoutIsActiveColumn).ContainsIsActiveColumn, Is.False);
		}

		[Test]
		public void GetTypeFromBaseType()
		{
			var result = typeof(Dummy).GetTypeFromBaseType("DummyWithId");
			Assert.That(result, Is.EqualTo(typeof(DummyWithId)));
		}

		static object[] ChangeValues =
		{
			new object[] { "true", typeof(bool), true },
			new object[] { "1", typeof(bool), true },
			new object[] { "false", typeof(bool), false },
			new object[] { "0", typeof(bool), false },
			new object[] { "4", typeof(int), 4 },
			new object[] { "3.4", typeof(decimal), 3.4 },
			new object[] { "A7D23F6D-8274-4E51-8256-D5E56B14850F", typeof(Guid), new Guid("A7D23F6D-8274-4E51-8256-D5E56B14850F") },
			new object[] { "1900-01-01 00:00:00", typeof(DateTime), new DateTime(1900, 01, 01) },
			new object[] { Convert.ToBase64String(Encoding.Unicode.GetBytes("This is a test1")), typeof(byte[]), Encoding.Unicode.GetBytes("This is a test1") },
			new object[] { "", typeof(int?), null },
			new object[] { "", typeof(bool?), null },
			new object[] { "", typeof(DateTime?), null },
			new object[] { null, typeof(DateTime?), null },
			new object[] { "", typeof(DateTimeOffset?), null },
			new object[] { null, typeof(DateTimeOffset?), null }
		};
	}
}
