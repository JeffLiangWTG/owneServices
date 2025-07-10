using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class DataSourceProvider : IDataSourceProvider
	{
		public DataSourceProvider(string xmlContent)
		{
			Argument.NotNullOrEmpty(xmlContent, nameof(xmlContent));

			xml = XElement.Parse(xmlContent);
		}

		readonly XElement xml;

		public string AppName
		{
			get
			{
				if (string.IsNullOrEmpty(appName))
				{
					appName = xml.Element(XmlProducerAppName)?.Value;
				}
				return appName;
			}
		}
		string appName;

		public string SubSource
		{
			get
			{
				if (string.IsNullOrEmpty(subSource))
				{
					subSource = xml.Element(SubSourceName)?.Value;
				}
				return subSource;
			}
		}
		string subSource;

		public bool IsFullUpdate
		{
			get
			{
				if (!isFullUpdate.HasValue)
				{
					isFullUpdate = FullUpdateTypeValue.Equals(xml.Element(UpdateTypeElementName)?.Value, StringComparison.OrdinalIgnoreCase);
				}
				return isFullUpdate.Value;
			}
		}
		bool? isFullUpdate;

		public bool IsDeletionType
		{
			get
			{
				if (!isDeletionType.HasValue)
				{
					isDeletionType = DeletionUpdateTypeValue.Equals(xml.Element(UpdateTypeElementName)?.Value, StringComparison.OrdinalIgnoreCase);
				}
				return isDeletionType.Value;
			}
		}
		bool? isDeletionType;

		public bool InclusiveEndDate
		{
			get
			{
				if (!inclusiveEndDate.HasValue)
				{
					inclusiveEndDate = bool.Parse(xml.Element(InclusiveEndDateValue)?.Value ?? "True");
				}
				return inclusiveEndDate.Value;
			}
		}
		bool? inclusiveEndDate;

		public bool IsAutoSchema
		{
			get
			{
				if (!isAutoSchema.HasValue)
				{
					isAutoSchema = AutoSchemaValue.Equals(xml.Element(SchemaElementName)?.Value, StringComparison.OrdinalIgnoreCase);
				}
				return isAutoSchema.Value;
			}
		}

		bool? isAutoSchema;

		public IEnumerable<Dependency> Dependencies
		{
			get
			{
				if (dependencies == null)
				{
					var dependencyXml = xml.Element(DependencyName);
					dependencies = dependencyXml is null ? new Dependency[0] : dependencyXml.Descendants().Select(x =>
					  new Dependency
					  (
						  x.Attribute(nameof(Dependency.DataSource)).Value,
						  DateTime.Parse(x.Attribute(nameof(Dependency.PublicationTime)).Value, CultureInfo.InvariantCulture),
						  GetDependencyType(x.Attribute(nameof(Dependency.DependencyType))?.Value)
					  )).ToArray();
				}
				return dependencies;
			}
		}
		Dependency[] dependencies;

		static DependencyType GetDependencyType(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				return (DependencyType)Enum.Parse(typeof(DependencyType), value);
			}
			return DependencyType.Preferred;
		}

		public UpdateType GetUpdateType()
		{
			if (IsFullUpdate)
			{
				return UpdateType.Full;
			}
			if (IsDeletionType)
			{
				return UpdateType.Deletion;
			}
			return UpdateType.Partial;
		}

		const string FullUpdateTypeValue = "FULL";
		const string DeletionUpdateTypeValue = "Deletion";
		const string UpdateTypeElementName = "UpdateType";
		const string InclusiveEndDateValue = "InclusiveEndDate";
		const string XmlProducerAppName = "AppName";
		const string SubSourceName = "DataSource";
		const string SchemaElementName = "Schema";
		const string AutoSchemaValue = "Auto";
		const string DependencyName = "Dependency";
	}
}
