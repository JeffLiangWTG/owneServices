using System;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Models;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public interface IDummyStorage : IDataSetStorage
	{
		Guid D1_PK { get; }
		string D1_Property1 { get; }
		int D1_Property2 { get; }
		DateTime D1_Property3 { get; }
	}

	public interface IUserDummyStorage : IDataSetStorage
	{
		Guid D1_PK { get; }
		string D1_Property1 { get; }
		int D1_Property2 { get; }
		DateTime D1_Property3 { get; }
		bool D1_IsSystem { get; set; }
		bool D1_IsActive { get; set; }
	}

	public interface IDummyStorageWithSqlGeographyColumn : IDataSetStorage
	{
		Guid D1_PK { get; }
		string D1_Property1 { get; }
		int D1_Property2 { get; }
		DateTime D1_Property3 { get; }
		string D1_GeographyProperty { get; }
	}

	public class DummyStorageWithSqlGeographyColumn
	{
		public string D1_Property1 { get; set; }
		public int D1_Property2 { get; set; }
		public DateTime D1_Property3 { get; set; }
		public string D1_GeographyProperty { get; set; }
		public bool D1_IsActive { get; set; }
	}

	public class DummyStorage
	{
		public string D1_Property1 { get; set; }
		public int D1_Property2 { get; set; }
		public DateTime D1_Property3 { get; set; }
		public bool D1_IsActive { get; set; }
	}

	public class UserDummyDependentStorage : RefDataSet
	{
	}
}
