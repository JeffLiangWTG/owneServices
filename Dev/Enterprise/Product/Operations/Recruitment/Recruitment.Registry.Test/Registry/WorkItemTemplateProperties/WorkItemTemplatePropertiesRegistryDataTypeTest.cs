using CargoWise.Types;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Registry
{
	[TestedType(typeof(WorkItemTemplatePropertiesRegistryDataType))]
	sealed class WorkItemTemplatePropertiesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<WorkItemTemplatePropertiesRegistryDataType>
	{
		protected override WorkItemTemplatePropertiesRegistryDataType GetNewDataType() => new WorkItemTemplatePropertiesRegistryDataType();

		protected override string ExpectedEditorName => "WorkItemTemplatePropertiesRegistryItemEditor";

		const string xml1 = @"<?xml version=""1.0"" encoding=""utf-16""?>
		<ArrayOfWorkItemTemplateProperties xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
			<WorkItemTemplateProperties>
				<FriendlyName>friendly_name1</FriendlyName>
				<WKI_PK>B01DF7D0-9E86-4D68-B0B9-F63738E8862B</WKI_PK>
			</WorkItemTemplateProperties>
		</ArrayOfWorkItemTemplateProperties>";

		const string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
		<ArrayOfWorkItemTemplateProperties xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
			<WorkItemTemplateProperties>
				<FriendlyName>friendly_name2</FriendlyName>
				<WKI_PK>3139C719-9E1C-470E-8E08-0D182C1E5B95</WKI_PK>
			</WorkItemTemplateProperties>
		</ArrayOfWorkItemTemplateProperties>";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new WorkItemTemplatePropertiesCollection();
			_ = collection1.Add("friendly_name1", new ZGuid("B01DF7D0-9E86-4D68-B0B9-F63738E8862B"));

			var collection2 = new WorkItemTemplatePropertiesCollection();
			_ = collection2.Add("friendly_name2", new ZGuid("3139C719-9E1C-470E-8E08-0D182C1E5B95"));

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, xml1),
				new ValidSampleAndBinaryValueInDB(collection2, xml2),
			};
		}
	}
}
