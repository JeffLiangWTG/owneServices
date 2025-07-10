using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.Types;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public class ReferenceXMLExporter : Customs.Shared.IReferenceXMLExporter
	{
		public void Export(IEnumerable<Guid> dataRecordPks, string xmlTypeName, string filePath)
		{
			var writer = BuildXmlWriter(dataRecordPks, xmlTypeName);
			writer.SaveXml(filePath);
		}

		public bool UploadFile(IEnumerable<Guid> dataRecordPks, string tableName, string emailContacts)
		{
			var memoStream = new MemoryStream();
			var writer = BuildXmlWriter(dataRecordPks, tableName);
			writer.SaveXml(memoStream);

			var serviceProxyLogger = new ServiceLoggerWrapper(new DummyLogger());
			var serverProxy = new ServerProxyHelper(serviceProxyLogger, RefDataRepoRegistry.Instance);

			if (serverProxy.CanInitiliseServerProxy())
			{
				memoStream.Position = 0;
				return serverProxy.GetServerProxy().Upload(memoStream, emailContacts);
			}
			return false;
		}

		IXmlWriter BuildXmlWriter(IEnumerable<Guid> dataRecordPks, string tableName)
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var adoConn = ((IDbConnectionInternals)conn).ADOConnection;
				var dbHelper = new DBHelper(adoConn);
				var valueRetrieval = new ValueRetrieval(dbHelper);
				var mapping = new ReferenceXMLMappingProvider();
				var creator = new XMLDataCreator(valueRetrieval, mapping);
				var storageType = valueRetrieval.GetStorageTypeFromName(tableName);
				var pkColumn = SharedSQLBuilder.GetPKColumn(storageType);
				var xmlType = mapping.GetMappedType(storageType);
				var configuration = new XmlWriterConfiguration();
				configuration.IncludeEntityTypeConfiguration(new AutoSchemaConfiguration());
				var writer = new XmlWriter(configuration);
				var dataSource = FormattableString.Invariant($"CW1 {xmlType.Name}");
				writer.SetDataSource(dataSource);
				writer.SetPublicationTime(ZDateTime.Now.ToDateTime());
				writer.SetUpdateType(UpdateType.Partial);
				foreach (var data in dbHelper.GetRecords(storageType, pkColumn, dataRecordPks))
				{
					var xmlData = creator.Create(data, storageType);
					writer.InvokeGenericMethod(nameof(IXmlWriter.PopulateData), xmlType, new[] { storageType }, xmlData);
				}

				return writer;
			}
		}
	}
}
