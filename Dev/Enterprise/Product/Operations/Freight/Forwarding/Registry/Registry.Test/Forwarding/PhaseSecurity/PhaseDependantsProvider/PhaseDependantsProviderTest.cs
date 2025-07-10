using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public class PhaseDependantsProviderTest : PhaseDependantsProviderTestCase
	{
		public void TestPropertyDependantNestedClass()
		{
			IPhaseDependant dependant = new PhaseDependantsProvider.PropertyDependant("hello", "world");
			AssertEquals(PhaseConstants.DependantType.Property, dependant.DependantType);
			AssertEquals("hello", dependant.Name);
			AssertEquals("world", dependant.Description);
		}

		public void TestTypeDependantNestedClass()
		{
			IPhaseDependant dependant = new PhaseDependantsProvider.TypeDependant("hello", "world");
			AssertEquals(PhaseConstants.DependantType.TypeName, dependant.DependantType);
			AssertEquals("hello", dependant.Name);
			AssertEquals("world", dependant.Description);
		}

		public void TestGetIZTypeProperties()
		{
			PhaseDependantsProviderForTesting provider = new PhaseDependantsProviderForTesting();
			IEnumerable<IPhaseDependant> properties = provider.GetIZTypeProperties();

			AssertContainsExactElementsInAnyOrder("Only writeable IZType properties returned", new ZString[]
			{
				"Z0_Guid",
				"Z0_AddInfo",
				"NonPersistentProperty",
				"Z0_AnotherDate",
				"Z0_AnotherDecimal",
				"Z0_AnotherNumber",
				"Z0_BitFalse",
				"Z0_BitFiltered",
				"Z0_BitTrue",
				"Z0_Bool",
				"Z0_Byte",
				"Z0_Code",
				"Z0_Date",
				"Z0_DateOnly",
				"Z0_DateTimeOffset",
				"Z0_Decimal",
				"Z0_Description",
				"Z0_FK_Code",
				"Z0_Geography",
				"Z0_IsSystem",
				"Z0_Long",
				"Z0_Money",
				"Z0_NAddInfo",
				"Z0_Number",
				"Z0_NVarChar",
				"Z0_NVarCharMax",
				"Z0_Short",
				"Z0_SmallDateTime",
				"Z0_SparseBit",
				"Z0_SparseByte",
				"Z0_SparseChar",
				"Z0_SparseDate",
				"Z0_SparseDateTime",
				"Z0_SparseDateTimeOffset",
				"Z0_SparseDecimal",
				"Z0_SparseGuid",
				"Z0_SparseLong",
				"Z0_SparseMoney",
				"Z0_SparseNumber",
				"Z0_SparseNVarChar",
				"Z0_SparseShort",
				"Z0_SparseSmallDateTime",
				"Z0_SparseTime",
				"Z0_SparseVarBinaryMax",
				"Z0_SparseVarChar",
				"Z0_SparseXml",
				"Z0_Time",
				"Z0_VarBinaryMax",
				"Z0_VarBinaryMax_HTML",
				"Z0_VarCharMax",
				"Z0_Xml"
			},
			properties.Select(x => x.Name));

			AssertEquals(true, properties.All(property => property.DependantType == PhaseConstants.DependantType.Property));
		}

		#region Implementation

		protected override PhaseDependantsProvider GetDependantsProvider()
		{
			return new PhaseDependantsProviderForTesting();
		}

		protected override Type GetParentType()
		{
			return typeof(ParentClassForTesting);
		}

		class PhaseDependantsProviderForTesting : PhaseDependantsProvider
		{
			protected override Type ParentType
			{
				get { return typeof(ParentClassForTesting); }
			}
		}

		public class ParentClassForTesting : DummyBusinessObject
		{
			public ParentClassForTesting(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public string PlainString { get; set; }
			public bool PlainBool { get; set; }
		}

		#endregion
	}
}
