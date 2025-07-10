using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DangerousGoodsManifestPortCollectionDataType))]
	internal class DangerousGoodsManifestPortCollectionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DangerousGoodsManifestPortCollectionDataType>
	{
		#region Implementation
		protected override string ExpectedEditorName
		{
			get
			{
				return "DangerousGoodsManifestPortRegistryItemEditor";
			}
		}

		protected override DangerousGoodsManifestPortCollectionDataType GetNewDataType()
		{
			return new DangerousGoodsManifestPortCollectionDataType(new DangerousGoodsManifestPortCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new DangerousGoodsManifestPortCollection();
			var port = collection.AddNew();
			port.Port = "AUMEL";
			port.PrincipalPK = new ZGuid("07928095-5BC7-4B1E-835C-12797F398CA0");
			port.SenderID = "SenderID_123";
			port.Enabled = true;

			var xmlValue = @"<ArrayOfDangerousGoodsManifestPort xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<DangerousGoodsManifestPort>
		<Port>AUMEL</Port>
		<PrincipalPK>07928095-5BC7-4B1E-835C-12797F398CA0</PrincipalPK>
		<SenderID>SenderID_123</SenderID>
		<Enabled>true</Enabled>
	</DangerousGoodsManifestPort>
</ArrayOfDangerousGoodsManifestPort>";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, xmlValue), };
		}
		#endregion
	}
}
