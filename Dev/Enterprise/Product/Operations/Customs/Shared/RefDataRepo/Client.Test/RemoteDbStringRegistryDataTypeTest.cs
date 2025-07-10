using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[TestedType(typeof(RemoteDbStringRegistryDataType))]
	class RemoteDbStringRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return true; }
		}
		protected override StringRegistryDataType GetNewDataType()
		{
			return new RemoteDbStringRegistryDataType();
		}

		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}

		public void TestValidateCore()
		{
			var testDataType = (RemoteDbStringRegistryDataType)GetNewDataType();
			AssertValidateServiceUri(testDataType);
			AssertValidateRemoteDatabaseName(testDataType);
		}

		void AssertValidateServiceUri(RemoteDbStringRegistryDataType testRegistryDataType)
		{
			bool hasValidationException = false;
			try
			{
				RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RefDbTableNameResolver.DefaultSingleRefDbName);
				testRegistryDataType.Validate(RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri, "123", Guid.Empty, Guid.Empty, Guid.Empty);
			}
			catch (RegistryValidationException)
			{
				hasValidationException = true;
			}
			AssertEquals("There should be validation when changing RemoteDatabaseServiceUri without changing SingleRefDatabaseName.", hasValidationException, hasValidationException);
		}

		void AssertValidateRemoteDatabaseName(RemoteDbStringRegistryDataType testRegistryDataType)
		{
			bool hasValidationException = false;
			try
			{
				RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
				testRegistryDataType.Validate(RemoteDatabaseRegistry.Instance.SingleRefDatabaseName, RefDbTableNameResolver.DefaultSingleRefDbName, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			catch (RegistryValidationException)
			{
				hasValidationException = true;
			}
			AssertEquals("There should be validation when changing SingleRefDatabaseName while RemoteDatabaseServiceUri is not default.", hasValidationException, hasValidationException);
		}
	}
}
