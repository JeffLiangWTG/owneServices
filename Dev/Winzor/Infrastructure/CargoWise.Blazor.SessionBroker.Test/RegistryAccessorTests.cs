using System;
using System.Text;
using CargoWise.Blazor.SessionBroker.Helpers;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

public class RegistryAccessorTests : TestWithDatabase
{
	[Test]
	public void DeserializeDefaultDBSchemaVersionFromBinary()
	{
		var obj = ConfigWithOIDCSettingsHelper.DBSchemaVersionBinaryValue();
		var dbSchemaVersion = obj is not DBNull ? Encoding.Unicode.GetString(obj as byte[]) : string.Empty;
		Assert.That(dbSchemaVersion, Is.Not.Null);
		Assert.That(dbSchemaVersion, Is.EqualTo(string.Empty));
	}

	[Test]
	public void DeserializeDBSchemaVersionFromBinary()
	{
		var obj = ConfigWithOIDCSettingsHelper.DBSchemaVersionBinaryValue("0x3700380032003100");
		var dbSchemaVersion = obj is not DBNull ? Encoding.Unicode.GetString(obj as byte[]) : string.Empty;
		Assert.That(dbSchemaVersion, Is.Not.Null);
		Assert.That(dbSchemaVersion, Is.EqualTo("7821"));
	}

	[Test]
	public void GetDefaultDBSchemaVersion()
	{
		var dbAccessorMock = ConfigWithOIDCSettingsHelper.DatabaseAccessorWithDBSchemaVersionMock();
		var registryAccessor = new RegistryAccessor(dbAccessorMock.Object);
		var dbSchemaVersion = registryAccessor.GetDBSchemaVersion();
		Assert.That(dbSchemaVersion, Is.Not.Null);
		Assert.That(dbSchemaVersion, Is.EqualTo(string.Empty));
	}

	[Test]
	public void GetDBSchemaVersion()
	{
		var dbAccessorMock = ConfigWithOIDCSettingsHelper.DatabaseAccessorWithDBSchemaVersionMock("0x3700380032003100");
		var registryAccessor = new RegistryAccessor(dbAccessorMock.Object);
		var dbSchemaVersion = registryAccessor.GetDBSchemaVersion();
		Assert.That(dbSchemaVersion, Is.Not.Null);
		Assert.That(dbSchemaVersion, Is.EqualTo("7821"));
	}
}
