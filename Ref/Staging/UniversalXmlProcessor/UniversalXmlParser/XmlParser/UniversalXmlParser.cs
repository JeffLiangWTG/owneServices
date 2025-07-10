using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public class UniversalXmlParser : IUniversalXmlParser
	{
		#region Member Variables

		readonly IUniversalXmlSchemaHandler schemaHandler;
		readonly IStagingRepositoryWrapper stagingRepositoryWrapper;
		readonly ISourceDataWriter sourceDataWriter;
		readonly IFileTrace fileTrace;
		readonly string fileSourceAppName;

		XDocument schemaXDocument;
		int lineNumber;
		readonly UniversalXmlWrapper universalXmlWrapper;

		#endregion

		#region Constructor

		public UniversalXmlParser(
			IUniversalXmlSchemaHandler universalXmlSchemaHandler,
			IStagingRepositoryWrapper stagingRepositoryWrapper,
			ISourceDataWriter universalXmlSourceDataWriter,
			IFileTrace xmlFileTrace,
			string appName = null)
		{
			Argument.NotNull(universalXmlSchemaHandler, nameof(universalXmlSchemaHandler));
			Argument.NotNull(stagingRepositoryWrapper, nameof(stagingRepositoryWrapper));
			Argument.NotNull(universalXmlSourceDataWriter, nameof(universalXmlSourceDataWriter));
			Argument.NotNull(xmlFileTrace, nameof(xmlFileTrace));

			schemaHandler = universalXmlSchemaHandler;
			this.stagingRepositoryWrapper = stagingRepositoryWrapper;
			sourceDataWriter = universalXmlSourceDataWriter;
			fileTrace = xmlFileTrace;
			fileSourceAppName = appName;
			universalXmlWrapper = new UniversalXmlWrapper();
		}

		#endregion

		#region Properties

		public XDocument SchemaXDocument => schemaXDocument;
		public IList<EntityTypeMetaData> EntityTypes { get; private set; }

		#endregion

		#region Methods

		public async Task ParseAsync(Stream stream, bool forceParsing)
		{
			Argument.NotNull(stream, nameof(stream));
			try
			{
				var lastSuccessLineNumber = await fileTrace.GetLastSuccessLineNumberAsync();

				var hasValidatedXml = false;
				var sourceId = Guid.Empty;

				var excludeElements = new string[] { "PublicationTime", "AppName", "AppProgramArgs" };
				var fileHash = XmlHash.HashStream(stream, excludeElements);

				using (var streamReader = new StreamReader(stream, Encoding.UTF8))
				using (var xmlReader = XmlReader.Create(streamReader))
				{
					var xmlInfo = (IXmlLineInfo)xmlReader;
					xmlReader.MoveToContent();
					if (string.Compare(xmlReader.Name, Constants.UniversalXml.RootElement, StringComparison.OrdinalIgnoreCase) != 0)
					{
						const string message = "The root element of the provided xml is not " + Constants.UniversalXml.RootElement + ".";
						throw new RefDataProcessingException(message, ErrorCodes.RootElementNotValid);
					}

					while (!xmlReader.EOF)
					{
						do
						{
							lineNumber = xmlInfo.LineNumber;
							if (lineNumber <= lastSuccessLineNumber)
							{
								xmlReader.Read();
								continue;
							}

							await fileTrace.SaveLastSuccessLineNumberAsync(lineNumber);

							if ((xmlReader.Depth != 1) || (xmlReader.NodeType != XmlNodeType.Element))
							{
								xmlReader.Read();
								continue;
							}

							switch (xmlReader.Name)
							{
								case Constants.UniversalXmlMetadata.DataSource:
									universalXmlWrapper.DataSource = xmlReader.ReadElementContentAsString();
									if (!string.IsNullOrEmpty(universalXmlWrapper.DataSource))
									{
										Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.SubSource)}:{universalXmlWrapper.DataSource}");
									}
									break;
								case Constants.UniversalXmlMetadata.AppName:
									universalXmlWrapper.AppName = xmlReader.ReadElementContentAsString();
									if (!string.IsNullOrEmpty(universalXmlWrapper.AppName))
									{
										Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.SourceDataAppName)}:{universalXmlWrapper.AppName}");
									}
									break;
								case Constants.UniversalXmlMetadata.PublicationTime:
									var success = DateTime.TryParse(xmlReader.ReadElementContentAsString(), out var publicationTime);
									if (success)
									{
										universalXmlWrapper.PublicationTime = publicationTime;
									}
									break;
								case Constants.UniversalXmlMetadata.UpdateType:
									universalXmlWrapper.UpdateType = xmlReader.ReadElementContentAsString();
									break;
								case Constants.UniversalXmlMetadata.InclusiveEndDate:
									universalXmlWrapper.InclusiveEndDate = xmlReader.ReadElementContentAsString();
									break;
								case Constants.UniversalXmlMetadata.Metadata:
									universalXmlWrapper.Metadata = xmlReader.ReadOuterXml();
									break;
								case Constants.UniversalXml.Dependency:
									universalXmlWrapper.DependencyXml = xmlReader.ReadOuterXml();
									break;
								case Constants.UniversalXml.SchemaElement:
									universalXmlWrapper.SchemaXml = xmlReader.ReadOuterXml();
									universalXmlWrapper.PopulateXmlSchemaEntityRelationshipMapList();
									break;
								default:
									var isDataElement = stagingRepositoryWrapper.IsDataElement(xmlReader.Name);
									if (!isDataElement)
									{
										universalXmlWrapper.AddUnknownElements(xmlReader.ReadOuterXml());
									}
									else
									{
										if (!hasValidatedXml)
										{
											if (!ValidateXmlAndTryToSetDUPFlag(forceParsing, ref sourceId, fileHash))
											{
												return;
											}
											AddMetadataToXml();
											hasValidatedXml = true;
										}

										await ParseXmlContentAsync(xmlReader, sourceId);
									}
									break;
							}
						} while (xmlReader.IsStartElement());
						xmlReader.Read();
					}

					if (!hasValidatedXml)
					{
						if (!ValidateXmlAndTryToSetDUPFlag(forceParsing, ref sourceId, fileHash))
						{
							return;
						}
						Console.WriteLine("The provided XML doesn't have valid data");
						sourceDataWriter.SetFlagErrorOnRootLevel();
					}

					await stagingRepositoryWrapper.ExecuteBulkInsertAsync();

					if (hasValidatedXml)
					{
						sourceDataWriter.SetFlagAllRecordsAreProcessed();
					}
				}
			}
			catch
			{
				sourceDataWriter.SetFlagErrorOnRootLevel();
				throw;
			}
			finally
			{
				fileTrace.Remove();
			}
		}

		bool ValidateXmlAndTryToSetDUPFlag(bool forceParsing, ref Guid sourceId, string fileHash)
		{
			if (string.IsNullOrEmpty(universalXmlWrapper.DataSource))
			{
				throw new RefDataProcessingException($"The provided XML has errors in MetaData: {Constants.UniversalXmlMetadata.DataSource} element is missing.", ErrorCodes.IncorrectXmlMetaData);
			}

			var dataSource = universalXmlWrapper.DataSource;

			sourceId = sourceDataWriter.AddOrUpdateSourceDataAndGetPK(dataSource, universalXmlWrapper.PublicationTime ?? DateTime.UtcNow, fileHash);

			if (!universalXmlWrapper.IsValid(out string errorMessage) || universalXmlWrapper.HasErrors())
			{
				var loggedErrorMessage = string.IsNullOrEmpty(errorMessage) ? universalXmlWrapper.ErrorMessage : errorMessage;
				sourceDataWriter.CreateDataProcessingInformationError(loggedErrorMessage);
				Task.Run(stagingRepositoryWrapper.ExecuteBulkInsertAsync).Wait();
				sourceDataWriter.SetFlagErrorOnRootLevel();
				throw new RefDataProcessingException($"The provided XML has errors in MetaData: {loggedErrorMessage}", ErrorCodes.IncorrectXmlMetaData);
			}

			var isDuplicate = sourceDataWriter.CheckDuplicateExists(dataSource, fileHash);
			if (isDuplicate)
			{
				Console.WriteLine($"The provided XML is a duplicate for {FlagHelper.GetFlag(UXMLProducerHelper.SubSource)}:{dataSource} with hash {fileHash}");
				sourceDataWriter.SetFlagDuplicated();
			}

			return forceParsing || !isDuplicate;
		}

		void AddMetadataToXml()
		{
			sourceDataWriter.AddContent(Constants.UniversalXmlMetadata.DataSource, universalXmlWrapper.DataSource);
			sourceDataWriter.AddContent(Constants.UniversalXmlMetadata.PublicationTime, universalXmlWrapper.PublicationTime.Value.ToString("s"));

			AddExtraDataToSource();
		}

		void AddExtraDataToSource()
		{
			Argument.NotNull(sourceDataWriter, nameof(sourceDataWriter));
			Argument.NotNull(universalXmlWrapper, nameof(universalXmlWrapper));

			var appName = string.IsNullOrEmpty(universalXmlWrapper.AppName) ? fileSourceAppName : universalXmlWrapper.AppName;
			if (!string.IsNullOrEmpty(appName))
			{
				sourceDataWriter.AddContent(Constants.UniversalXmlMetadata.AppName, appName);
			}
			if (!string.IsNullOrEmpty(universalXmlWrapper.UpdateType))
			{
				sourceDataWriter.AddContent(Constants.UniversalXmlMetadata.UpdateType, universalXmlWrapper.UpdateType);
			}
			if (!string.IsNullOrEmpty(universalXmlWrapper.InclusiveEndDate))
			{
				sourceDataWriter.AddContent(Constants.UniversalXmlMetadata.InclusiveEndDate, universalXmlWrapper.InclusiveEndDate);
			}
			if (!string.IsNullOrEmpty(universalXmlWrapper.Metadata))
			{
				sourceDataWriter.AddXmlContent(universalXmlWrapper.Metadata);
			}
			if (!string.IsNullOrEmpty(universalXmlWrapper.DependencyXml))
			{
				sourceDataWriter.AddXmlContent(universalXmlWrapper.DependencyXml);
			}

			var schemaXml = universalXmlWrapper.SchemaXml;
			sourceDataWriter.AddXmlContent(schemaXml);

			EntityTypes = schemaHandler.ParseSchemaXmlToEntityTypes(schemaXml, out schemaXDocument);
			stagingRepositoryWrapper.SetEntityTypeMetaData(EntityTypes);

			if (!string.IsNullOrEmpty(universalXmlWrapper.ErrorXml))
			{
				sourceDataWriter.AddXmlContent(universalXmlWrapper.ErrorXml);
			}
			foreach (var unknownElement in universalXmlWrapper.UnknownElements)
			{
				sourceDataWriter.AddXmlContent(unknownElement);
			}
		}

		async Task<bool> ParseXmlContentAsync(XmlReader xmlReader, Guid sourceId)
		{
			var entityTypeName = xmlReader.Name;
			var xmlContent = xmlReader.ReadOuterXml();

			stagingRepositoryWrapper.SchemaRelationshipMapList = universalXmlWrapper.XmlSchemaEntityRelationshipMapList;
			var result = stagingRepositoryWrapper.CreateRecord(entityTypeName, xmlContent, lineNumber, sourceId);
			if (string.IsNullOrEmpty(result))
			{
				if (stagingRepositoryWrapper.CheckBulkInsert)
				{
					await stagingRepositoryWrapper.ExecuteBulkInsertAsync();
				}
			}

			return true;
		}

		#endregion
	}
}
