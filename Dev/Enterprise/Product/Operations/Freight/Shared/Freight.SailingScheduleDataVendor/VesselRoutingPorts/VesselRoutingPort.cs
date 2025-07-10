using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in AU-NZ and DE sailing schedule imports")]
	public class VesselRoutingPort : AutoViewVesselRoutingPorts
	{
		public VesselRoutingPort(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public VesselRoutingPort Load(VoyageOrigin origin, ZString lineOperator)
			{
				return Load(origin, lineOperator.IsEmpty ? null : new List<ZString> { lineOperator });
			}

			public VesselRoutingPort Load(VoyageOrigin origin, List<ZString> lineOperators)
			{
				Voyage = origin.Voyage;
				return origin.Voyage == null ? null : Load(origin.Voyage.Vessel, origin.Voyage.JV_VoyageFlight, origin.JA_RL_NKPortOfLoading, lineOperators);
			}

			public VesselRoutingPort Load(VoyageDestination destination, ZString lineOperator)
			{
				return Load(destination, lineOperator.IsEmpty ? null : new List<ZString> { lineOperator });
			}

			public VesselRoutingPort Load(VoyageDestination destination, List<ZString> lineOperators)
			{
				Voyage = destination.Voyage;
				return destination.Voyage == null ? null : Load(destination.Voyage.Vessel, destination.Voyage.JV_VoyageFlight, destination.JB_RL_NKPortOfDischarge, lineOperators);
			}

			public VesselRoutingPort Load(RefVessel vessel, ZString voyage, ZString portCode, List<ZString> lineOperators)
			{
				VesselRoutingPort result = null;

				if (vessel != null && !voyage.IsEmpty && !portCode.IsEmpty)
				{
					var query = GetQuery(voyage, portCode, vessel.RV_LloydsNumber);
					if (lineOperators != null && lineOperators.Any())
					{
						query.AddToFilter(ViewVesselRoutingPortsSchema.E7_LineOperator, lineOperators);
					}
					result = Factory.LoadTop1<VesselRoutingPort>(query);
				}

				return result;
			}

			ZQuery GetQuery(ZString voyage, ZString portCode, ZString lloydsNumber)
			{
				var voyageQuery = new ZQuery();
				voyageQuery.AddToFilter(ViewVesselRoutingPortsSchema.E7_VoyageIn, voyage);
				voyageQuery.AddToFilter(JoinCondition.Or, ViewVesselRoutingPortsSchema.E7_VoyageOut, voyage);

				var query = new ZQuery();
				query.AddToFilter(JoinCondition.And, ViewVesselRoutingPortsSchema.E7_RL_NKPortCode, portCode);
				query.AddToFilter(ViewVesselRoutingPortsSchema.E7_LloydsNumber, lloydsNumber);
				query.AddToFilter(voyageQuery, JoinCondition.And);

				query.OrderBy = ViewVesselRoutingPortsSchema.E7_ETA.Name + " DESC";

				return query;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(VesselRoutingPort);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in Load")]
			JobVoyage Voyage { get; set; }
		}

		#endregion
	}
}
