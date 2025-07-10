using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Processors
{
	public class AESCollectionProcessor : IUniversalShipmentProcessor
	{
		readonly ForwardingConsol consol;

		public AESCollectionProcessor(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public void Process(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment)
		{
			var consolHeader = consol.AWBHeader as ConsolExportAWBHeader;
			if (consolHeader == null || universalShipment?.CarrierDocumentsOverride?.AWBHeader == null)
			{
				return;
			}

			// Set AESExportCollection for Shipment node(ForwardingConsol)
			universalShipment.CarrierDocumentsOverride.AWBHeader.SetAESExportCollection(() => GetAESCollection(consolHeader));

			var fhlShipments = consol?.Shipments?.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
			if (universalShipment.SubShipmentCollection?.Any() == true && fhlShipments?.Any() == true)
			{
				foreach (var subshipment in universalShipment.SubShipmentCollection)
				{
					// Find mapping forwardingShipment of subshipment
					var forwardingShipment = fhlShipments.FirstOrDefault(x =>
						string.Equals(x.JS_UniqueConsignRef, subshipment.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key));

					if (forwardingShipment != null)
					{
						var shipmentHeader = forwardingShipment.AWBHeader;
						if (shipmentHeader != null)
						{
							// Set AESExportCollection for SubShipment node(ForwardingShipment)
							subshipment.CarrierDocumentsOverride.AWBHeader.SetAESExportCollection(() => GetAESCollection(shipmentHeader));
						}
					}
				}
			}
		}

		static List<CodeDescriptionPair35Char> GetAESCollection(ExportAWBHeader exportAWBHeader)
		{
			if (exportAWBHeader == null)
			{
				return null;
			}

			var aesCollection = new List<CodeDescriptionPair35Char>();
			var ociExportStatementCodeTranslations = new Dictionary<string, string>
			{
				{ "PRF", "AES" },
				{ "PDU", "PDF" },
				{ "DWN", "AED" },
				{ "LOW", (NoResString)"AES NOEEI EXC" },
			};

			var isCurrentCountryInUsaAndTerritories = Core.Constants.CountryCodes.UsaAndTerritoriesList.Any(x => x == GlbBranch.CurrentBranch.Country.Code);
			if (isCurrentCountryInUsaAndTerritories)
			{
				var exportStatements = IEnumerableExtensions.DistinctBy(exportAWBHeader.ExportStatements_CargoIMP.AsReadOnly(), x => new { x.Code, x.Statement }).ToList();

				foreach (var exportStatementSetting in exportStatements)
				{
					if (ociExportStatementCodeTranslations.ContainsKey(exportStatementSetting.Code))
					{
						var aesExport = $"{ociExportStatementCodeTranslations[exportStatementSetting.Code]} {exportStatementSetting.Statement.Trim()}";
						aesCollection.Add(new CodeDescriptionPair35Char { Code = aesExport });
					}
				}
			}

			return aesCollection;
		}
	}
}
