using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class TransportLegImportHelper
	{
		public static void ReadTransportLegs(IXmlImportLogger logger, Shipment dataObject, UniversalObjectFactory factory, ITransportParentCommon transportParent, WhsWarehouse warehouse, bool onlyIncludeOutbound = false)
		{
			var collection = GetValidTransportLegs(dataObject?.TransportLegCollection, warehouse, onlyIncludeOutbound);
			var legReader = ObjectFactory.Get<ITransportLegCollectionReader>("ITransportLegCollectionReaderForTransit",
				collection, logger, factory, transportParent, new Func<TransportLeg, bool>(leg => false));
			legReader.ReadIntoCollection();
		}

		static DataObjectList<TransportLeg> GetValidTransportLegs(DataObjectList<TransportLeg> legCollection, WhsWarehouse warehouse, bool onlyIncludeOutbound)
		{
			var result = new DataObjectList<TransportLeg>();

			if (legCollection != null)
			{
				var homePortList = legCollection.Where(l => l.IsLoadingPort(warehouse.RelatedCompanyBranch.GB_RL_NKHomePort) || (!onlyIncludeOutbound && l.IsDischargingPort(warehouse.RelatedCompanyBranch.GB_RL_NKHomePort)));
				if (homePortList.Any())
				{ result.AddRange(homePortList); }
				else
				{
					var extraPorts = warehouse.RelatedCompanyBranch.ExtraPorts.Cast<MasterFiles.Business.GlbBranchExtraPorts>().OrderBy(p => p.GY_RL_NKAdditionalBranchRelatedPort);

					foreach (var extraPort in extraPorts)
					{
						var extraPortList = legCollection.Where(l => l.IsLoadingPort(extraPort.GY_RL_NKAdditionalBranchRelatedPort) || (!onlyIncludeOutbound && l.IsDischargingPort(extraPort.GY_RL_NKAdditionalBranchRelatedPort)));
						if (extraPortList.Any())
						{
							result.AddRange(extraPortList);
							break;
						}
					}
				}
			}
			return result;
		}

		public static List<ITransport> GetValidTransportLegs(ITransport[] legCollection, WhsWarehouse warehouse, bool onlyIncludeOutbound)
		{
			var result = new List<ITransport>();

			if (legCollection != null)
			{
				var homePortList = legCollection.Where(l => l.JW_RL_NKLoadPort == warehouse.RelatedCompanyBranch.GB_RL_NKHomePort || (!onlyIncludeOutbound && l.JW_RL_NKDiscPort == warehouse.RelatedCompanyBranch.GB_RL_NKHomePort));
				if (homePortList.Any())
				{ result.AddRange(homePortList); }
				else
				{
					var extraPorts = warehouse.RelatedCompanyBranch.ExtraPorts.Cast<MasterFiles.Business.GlbBranchExtraPorts>().OrderBy(p => p.GY_RL_NKAdditionalBranchRelatedPort);

					foreach (var extraPort in extraPorts)
					{
						var extraPortList = legCollection.Where(l => l.JW_RL_NKLoadPort == extraPort.GY_RL_NKAdditionalBranchRelatedPort || (!onlyIncludeOutbound && l.JW_RL_NKDiscPort == extraPort.GY_RL_NKAdditionalBranchRelatedPort));
						if (extraPortList.Any())
						{
							result.AddRange(extraPortList);
							break;
						}
					}
				}
			}
			return result;
		}

		static bool IsLoadingPort(this TransportLeg leg, ZString portCode)
		{
			return leg.PortOfLoading != null && leg.PortOfLoading.Code.HasValue && leg.PortOfLoading.Code.Value == portCode;
		}
		static bool IsDischargingPort(this TransportLeg leg, ZString portCode)
		{
			return leg.PortOfDischarge != null && leg.PortOfDischarge.Code.HasValue && leg.PortOfDischarge.Code.Value == portCode;
		}

		public static void DeleteExistingTransportLegs(UniversalObjectFactory factory, ZGuid parentPK)
		{
			var query = new ZQuery();
			query.AddToFilter(JobConsolTransportSchema.JW_ParentGUID, parentPK);

			var transportLegs = factory.RowFactory.Load(JobConsolTransportSchema.Constants.TableName, query);
			foreach (var leg in transportLegs)
			{
				var row = (IColumnIndexer)leg;
				row.Delete();
			}
		}
	}
}
