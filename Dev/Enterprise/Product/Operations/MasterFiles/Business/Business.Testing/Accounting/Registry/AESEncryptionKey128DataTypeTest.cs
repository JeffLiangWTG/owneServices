using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AESEncryptionKey128DataType))]
	sealed class AESEncryptionKey128DataTypeTest : RegistryDataTypeTestCase<AESEncryptionKey128DataType>
	{
		public void TestValidation()
		{
			var dataType = GetNewDataType();
			var registryItem = new AESEncryptionKey128RegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
			var samples = new List<(bool valid, string value)>()
			{
				(true, "01234567890123456789012345678901"),
				(true, "ABCDEFABCDEFABCDEFABCDEFABCDEFAB"),
				(true, "abcdefabcdefabcdefabcdefabcdefab"),
				(true, "012345678901234DEFABCDEFABCDEFAB"),
				(false, "0123456789012345678901234567890"),
				(false, "012345678901234567890123456789012"),
				(false, "012345678901234567890123456789012"),
				(false, "G12345678901234DEFABCDEFABCDEFAB"),
			};

			void action(string value)
			{
				registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
				dataType.Validate(registryItem, value, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			}

			foreach (var (valid, value) in samples)
			{
				if (valid)
				{
					AssertNoExceptionThrown(() => action(value));
				}
				else
				{
					AssertExceptionThrown<RegistryValidationException>(() => action(value));
				}
			}
		}

		#region Implementation

		protected override AESEncryptionKey128DataType GetNewDataType()
		{
			return new AESEncryptionKey128DataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", new byte[] {
					65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
					65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
					65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
					65, 0, 65, 0 }),
				new ValidSampleAndBinaryValueInDB("11111111111111111111111111111111", new byte[] {
					49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0,
					49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0,
					49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0, 49, 0,
					49, 0, 49, 0 })
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		#endregion
	}
}
