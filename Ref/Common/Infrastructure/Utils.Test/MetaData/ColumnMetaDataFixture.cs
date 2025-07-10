using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class ColumnMetaDataFixture
{
	[Test]
	public void Properties_AreSetCorrectly()
	{
		var meta = new ColumnMetaData
		{
			ColumnName = "TestCol",
			DataType = "int",
			ColumnSize = 4,
			NumericPrecision = 10,
			NumericScale = 0,
			IsNullable = true
		};

		Assert.That(meta.ColumnName, Is.EqualTo("TestCol"));
		Assert.That(meta.DataType, Is.EqualTo("int"));
		Assert.That(meta.ColumnSize, Is.EqualTo(4));
		Assert.That(meta.NumericPrecision, Is.EqualTo(10));
		Assert.That(meta.NumericScale, Is.EqualTo(0));
		Assert.That(meta.IsNullable, Is.True);
	}

	[Test]
	public void Equals_ReturnsTrue_ForIdenticalObjects()
	{
		var a = new ColumnMetaData
		{
			ColumnName = "Col",
			DataType = "varchar",
			ColumnSize = 50,
			NumericPrecision = 0,
			NumericScale = 0,
			IsNullable = false
		};
		var b = new ColumnMetaData
		{
			ColumnName = "Col",
			DataType = "varchar",
			ColumnSize = 50,
			NumericPrecision = 0,
			NumericScale = 0,
			IsNullable = false
		};

		Assert.That(a.Equals(b), Is.True);
		Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
	}

	[Test]
	public void Equals_ReturnsFalse_ForDifferentObjects()
	{
		var a = new ColumnMetaData { ColumnName = "Col1", DataType = "int", IsNullable = false };
		var b = new ColumnMetaData { ColumnName = "Col2", DataType = "int", IsNullable = false };

		Assert.That(a.Equals(b), Is.False);
	}

	[Test]
	public void Equals_ReturnsFalse_ForNullOrDifferentType()
	{
		var a = new ColumnMetaData { ColumnName = "Col" };

		Assert.That(a, Is.Not.Null);
		Assert.That(a.Equals("not a ColumnMetaData"), Is.False);
	}
}
