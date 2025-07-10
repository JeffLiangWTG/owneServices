using System.Drawing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentImageCollectionRegistryDataType))]
	sealed class ComplianceDocumentImageCollectionRegistryDataTypeTest : FallbackMergedRegistryBusinessObjectCollectionDataTypeTestCase
	{
		#region Implementation

		protected override IRegistryDataType GetNewDataType()
		{
			return new ComplianceDocumentImageCollectionRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default).DataType;
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceDocumentImageCollectionRegistryItemEditor"; }
		}

		public override void TestGetSetValidValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				base.TestGetSetValidValues();
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ComplianceDocumentImageCollection();
			var image = collection.AddNew();
			image.Country = Core.Constants.CountryCodes.Taiwan;
			image.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			image.Remark = "Remark";
			image.Image = new Bitmap(1, 1);
			image.ImagePkForTest = new ZGuid("AC45E085-F03C-4C01-9703-1A8F9BF85C66");

			var image1 = collection.AddNew();
			image1.Country = Core.Constants.CountryCodes.Taiwan;
			image1.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			image1.Remark = "Remark1";
			image1.Image = new Bitmap(2, 2);
			image1.ImagePkForTest = new ZGuid("A9EC2830-2DEF-422B-AF2E-3EF9C7F88D6B");

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,73,
				0,109,0,97,0,103,0,101,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,
				0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,
				0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,
				0,110,0,99,0,101,0,34,0,62,0,60,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,67,0,111,
				0,117,0,110,0,116,0,114,0,121,0,62,0,84,0,87,0,60,0,47,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,60,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,83,0,117,0,98,0,84,
				0,121,0,112,0,101,0,62,0,78,0,84,0,67,0,60,0,47,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,83,0,117,0,98,0,84,0,121,0,112,0,101,0,62,0,60,0,82,0,101,0,109,0,97,0,114,
				0,107,0,62,0,82,0,101,0,109,0,97,0,114,0,107,0,60,0,47,0,82,0,101,0,109,0,97,0,114,0,107,0,62,0,60,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,97,0,99,0,52,0,53,0,101,0,48,0,56,
				0,53,0,45,0,102,0,48,0,51,0,99,0,45,0,52,0,99,0,48,0,49,0,45,0,57,0,55,0,48,0,51,0,45,0,49,0,97,0,56,0,102,0,57,0,98,0,102,0,56,0,53,0,99,0,54,0,54,0,60,0,47,0,73,0,109,
				0,97,0,103,0,101,0,80,0,75,0,62,0,60,0,47,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,73,0,109,0,97,0,103,0,101,0,62,0,60,
				0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,
				0,84,0,87,0,60,0,47,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,60,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,83,0,117,0,98,0,84,0,121,0,112,0,101,0,62,0,78,0,84,
				0,73,0,60,0,47,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,83,0,117,0,98,0,84,0,121,0,112,0,101,0,62,0,60,0,82,0,101,0,109,0,97,0,114,0,107,0,62,0,82,0,101,0,109,0,97,
				0,114,0,107,0,49,0,60,0,47,0,82,0,101,0,109,0,97,0,114,0,107,0,62,0,60,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,97,0,57,0,101,0,99,0,50,0,56,0,51,0,48,0,45,0,50,0,100,0,101,
				0,102,0,45,0,52,0,50,0,50,0,98,0,45,0,97,0,102,0,50,0,101,0,45,0,51,0,101,0,102,0,57,0,99,0,55,0,102,0,56,0,56,0,100,0,54,0,98,0,60,0,47,0,73,0,109,0,97,0,103,0,101,0,80,0,75,
				0,62,0,60,0,47,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,
				0,121,0,79,0,102,0,67,0,111,0,109,0,112,0,108,0,105,0,97,0,110,0,99,0,101,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,73,0,109,0,97,0,103,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
