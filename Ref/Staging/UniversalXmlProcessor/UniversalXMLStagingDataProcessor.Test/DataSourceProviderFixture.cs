using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class DataSourceProviderFixture
	{
		[Test]
		public void IsFullUpdate()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
</UniversalReferenceData>";
			var provider = (IDataSourceProvider)new DataSourceProvider(xml);
			Assert.True(provider.IsFullUpdate);
		}

		[Test]
		public void IsAutoSchema()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>Auto</Schema>
</UniversalReferenceData>";
			var provider = (IDataSourceProvider)new DataSourceProvider(xml);
			Assert.True(provider.IsAutoSchema);
		}

		[Test]
		public void IsDeletionType()
		{
			var xml = @"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>Deletion</UpdateType>
</UniversalReferenceData>";
			var provider = (IDataSourceProvider)new DataSourceProvider(xml);
			Assert.True(provider.IsDeletionType);
		}

		[TestCase("<InclusiveEndDate>True</InclusiveEndDate>", true)]
		[TestCase("<InclusiveEndDate>False</InclusiveEndDate>", false)]
		[TestCase("", true)]
		public void EndDateExclusive(string inclusiveEndDateElement, bool value)
		{
			var xml = $@"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	{inclusiveEndDateElement}
</UniversalReferenceData>";
			var provider = (IDataSourceProvider)new DataSourceProvider(xml);
			Assert.AreEqual(value, provider.InclusiveEndDate);
		}

		[TestCase(UpdateType.Partial)]
		[TestCase(UpdateType.Full)]
		[TestCase(UpdateType.Deletion)]
		public void GetUpdateType(UpdateType updateType)
		{
			var xml = $@"<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>{updateType}</UpdateType>
</UniversalReferenceData>";
			var provider = new DataSourceProvider(xml);
			Assert.AreEqual(updateType, provider.GetUpdateType());
		}

		[Test]
		public void Dependency()
		{
			var xml = @"<UniversalReferenceData>
	<Dependency>
		<DependentDataSource DataSource=""Test1"" PublicationTime=""2017-01-01T00:00:00"" />
		<DependentDataSource DataSource=""Test2"" PublicationTime=""2017-02-01T00:00:00"" DependencyType=""Required"" />
	</Dependency >
</UniversalReferenceData>";
			var provider = (IDataSourceProvider)new DataSourceProvider(xml);
			var dependency = provider.Dependencies.ToArray();
			Assert.AreEqual(2, provider.Dependencies.Count());
			Assert.AreEqual("Test1", dependency[0].DataSource);
			Assert.AreEqual(new DateTime(2017, 1, 1), dependency[0].PublicationTime);
			Assert.AreEqual(DependencyType.Preferred, dependency[0].DependencyType);
			Assert.AreEqual("Test2", dependency[1].DataSource);
			Assert.AreEqual(new DateTime(2017, 2, 1), dependency[1].PublicationTime);
			Assert.AreEqual(DependencyType.Required, dependency[1].DependencyType);
		}
	}
}
