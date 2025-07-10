using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Common.TypeProvider.Test
{
	class DummyContext : DbContext
	{
		public DummyContext(DbContextOptions<DummyContext> options)
		: base(options)
		{
		}

		public DbSet<DummyForDbContext> Dummies { get; set; }
	}

	class DummyForDbContext
	{

		[Key]
		public Guid ZXY_PK { get; set; }
		public string ZXY_StringProperty { get; set; }
		public int ZXY_IntProperty { get; set; }
	}

	class Dummy
	{
		public Guid ZXY_PK { get; set; }
		public string ZXY_StringProperty { get; set; }
		public int ZXY_IntProperty { get; set; }
		public Guid ZXY_ZZ1_Tariff { get; set; }
		public T GetDefault<T>() => default;
		public static T GetStaticDefault<T>() => default;
		public ICollection<DependentDummy> Dependents { get; }
		public RelatedDummy RefCusTariffType { get; set; }
		public Guid ZXY_ZDB_B { get; set; }
		public DummyB DummyBNavigationProp { get; set; }
	}

	class DummyWithId
	{
		public int ZXY_PortId { get; set; }
		public DateTime ZXY_StartTime { get; set; }
	}

	class DummyWithIdAndPk
	{
		public Guid ZXY_PK { get; set; }
		public int ZXY_PortId { get; set; }
		public DateTime ZXY_StartTime { get; set; }
	}

	class DummyWithIdAndPkAndRvcParentPk
	{
		public Guid RVC_ParentPK { get; set; }
		public Guid ZXY_PK { get; set; }
		public int ZXY_PortId { get; set; }
		public DateTime ZXY_StartTime { get; set; }
	}

	class DummyString
	{
		public Guid ZZY_PK { get; set; }
		public DateTime ZZY_StartDate { get; set; }
	}

	class DependentDummy
	{
		public Guid ZYX_PK { get; set; }
		public Guid ZYX_ZXY { get; set; }
		public Guid ZYX_DataSetPK { get; set; }
		public string ZYX_ZZY_StringDependency { get; set; }
	}

	class RelatedDummy
	{
		public Guid ZZX_PK { get; set; }
		public string ZZX_Code { get; set; }
		public RelatedRelatedDummy RelatedRelatedDummyObj { get; set; }
		public IEnumerable<DependentDummy> DependentDummies { get; set; }
	}

	class RelatedRelatedDummy
	{
		public Guid ZRR_PK { get; set; }
		public string ZRR_Code { get; set; }
	}

	class DummyParentPK
	{
		public Guid RVC_ParentPK { get; set; }
	}

	class DummyB
	{
		public Guid ZDB_PK { get; set; }
		public string ZDB_Code { get; set; }
	}

	[NonPersistentObject]
	class DummyNonPersistentType
	{
	}

	class DummyPersistentType
	{
	}

	class DummyTypeWithIsActiveColumn
	{
		public bool Property_IsActive { get; set; }
	}

	class DummyTypeWithoutIsActiveColumn
	{
	}
}
