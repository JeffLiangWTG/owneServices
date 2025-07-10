using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortManifestPortCollectionDataType))]
	class PortManifestPortCollectionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PortManifestPortCollectionDataType>
	{
		#region Implementation
		protected override string ExpectedEditorName
		{
			get
			{
				return "PortManifestPortRegistryItemEditor";
			}
		}

		protected override PortManifestPortCollectionDataType GetNewDataType()
		{
			return new PortManifestPortCollectionDataType(new PortManifestPortCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new PortManifestPortCollection();
			var port = collection.AddNew();
			port.Port = "NZLYT";
			port.PrincipalPK = new ZGuid("07928095-5BC7-4B1E-835C-12797F398CA0");
			port.SenderID = "SenderID_123";
			port.Enabled = true;

			var xmlValue = @"<ArrayOfPortManifestPort xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<PortManifestPort>
		<Port>NZLYT</Port>
		<PrincipalPK>07928095-5BC7-4B1E-835C-12797F398CA0</PrincipalPK>
		<SenderID>SenderID_123</SenderID>
		<Enabled>true</Enabled>
	</PortManifestPort>
</ArrayOfPortManifestPort>";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, xmlValue), };
		}
		#endregion
	}
}
