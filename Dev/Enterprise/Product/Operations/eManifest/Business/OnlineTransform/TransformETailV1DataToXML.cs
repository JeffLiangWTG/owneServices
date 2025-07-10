using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	public class TransformETailV1DataToXML
	{
		public TransformETailV1DataToXML()
		{
			ShipmentPKsForTransform = new List<Guid>();
		}

		public void Run(Action<string> logAction, CancellationToken token)
		{
			if (ShouldRun)
			{
				GetShipmentPKsForTransform();
				if (ShipmentPKsForTransform.Count > 0)
				{
					var branch = GlbBranch.GetOneActiveBranchPerCompany().FirstOrDefault();
					if (branch != null)
					{
						using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
						{
							for (var i = 0; i < ShipmentPKsForTransform.Count; i += BatchSize)
							{
								var shipmentPKBatch = ShipmentPKsForTransform.Skip(i).Take(BatchSize);
								var shipmentQuery = new ZQuery(JobShipmentSchema.PK, shipmentPKBatch);
								var shipmentBatch = Factory.Load<ForwardingShipment>(shipmentQuery);

								ProcessShipmentBatch(shipmentBatch, logAction, token);
							}
						}
					}
				}
			}
		}

		public bool ShouldRun => DbObjectCreator.TableExists(Db.Connection, "SupplierBookingLine") && Db.Connection.Exists("FROM dbo.SupplierBookingLine WHERE DL_JS_ApprovedShipment IS NOT NULL");

		void ProcessShipmentBatch(IEnumerable<ForwardingShipment> shipmentBatch, Action<string> logAction, CancellationToken token)
		{
			Db.Connection.RunInTransaction(() =>
			{
				ExportETailV1DataAsXMLToEDocs(shipmentBatch, token);
				MarkExistingETailV1DataAsExported(shipmentBatch);
				Factory.Save();
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ExportETailV1DataAsXMLToEDocs(IEnumerable<ForwardingShipment> shipmentBatch, CancellationToken token)
		{
			using (var command = Db.Connection.Command(ExportXMLSQL))
			{
				command.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, new Guid());
				foreach (var shipment in shipmentBatch)
				{
					var xmlBytes = Array.Empty<byte>();
					var zipFileName = shipment.JS_UniqueConsignRef + (NoResString)" - eTail V1 Data.xml.gz";

					token.ThrowIfCancellationRequested();
					command.SetParameterValue("@ShipmentPK", shipment.PK.ToGuid());

					using (var reader = command.ExecuteReader())
					using (var stream = new MemoryStream())
					{
						if (reader.Read())
						{
							var buffer = new byte[8040];
							long bytesRead;
							long offset = 0;

							while ((bytesRead = reader.GetBytes(0, offset, buffer, 0, buffer.Length)) > 0)
							{
								offset = offset + bytesRead;
								stream.Write(buffer, 0, (int)bytesRead);
							}

							xmlBytes = stream.ToArray();
						}
					}

					if (xmlBytes.Length > 0)
					{
						shipment.DocManagerInfo.MasterFactory = Factory;
						_ = shipment.DocManagerInfo.AddFileOrDocument(
							xmlBytes,
							zipFileName,
							Core.Constants.RefDocTypes.ETailV1Data,
							visibleCompanyPK: Env.CurrentCompanyPK,
							visibleBranchPK: Env.CurrentBranchPK,
							visibleDepartmentPK: Env.CurrentDepartmentPK);
					}
				}
			}
		}

		const string ExportXMLSQL = @"
DECLARE @Xml VARCHAR(MAX) =
(
	SELECT *
	FROM dbo.SupplierBookingHeader
	LEFT JOIN dbo.SupplierBookingLine ON DL_DH_BookingHeader = DH_PK
	WHERE DL_JS_ApprovedShipment = @ShipmentPK
	FOR XML AUTO
)

SELECT COMPRESS(@Xml)
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void MarkExistingETailV1DataAsExported(IEnumerable<ForwardingShipment> shipmentBatch)
		{
			using (var command = Db.Connection.Command(MarkAsExportedSQL))
			{
				command.AddTableValuedParameter("@ShipmentPKs", TVPHelper.TVP_uniqueidentifier, shipmentBatch.Select(x => x.PK.ToGuid()));
				command.ExecuteNonQuery();
			}
		}

		const string MarkAsExportedSQL = @"
UPDATE dbo.SupplierBookingLine
SET DL_JS_ApprovedShipment = NULL
WHERE DL_JS_ApprovedShipment IN (SELECT Value FROM @ShipmentPKs)
";

		#region Factory

		DocumentFactory Factory => fFactory ?? (fFactory = InitialiseDocumentFactory());
		DocumentFactory fFactory;

		DocumentFactory InitialiseDocumentFactory()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var businessObjectFactory = new BusinessObjectFactory(Db.Connection);
			businessObjectFactory.NameForDebugging = "Local BusinessObjectFactory for ETail V1 Transform";

			var factory = (DocumentFactory)documentFactoryProvider.GetFactory(businessObjectFactory);
			factory.NameForDebugging = "ETail V1 Transform Main DocumentFactory : BusinessObjectFactory";

			return factory;
		}

		#endregion

		#region Shipments

		const int BatchSize = 100;

		List<Guid> ShipmentPKsForTransform { get; }

		void GetShipmentPKsForTransform()
		{
			var query = $@"SELECT DISTINCT DL_JS_ApprovedShipment 
						FROM dbo.SupplierBookingLine 
						WHERE DL_JS_ApprovedShipment IS NOT NULL";

			Db.Connection.ExecuteReader(query, record => ShipmentPKsForTransform.Add(record.GetGuid(0)));
		}

		#endregion
	}
}
