using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class RouteSetRatingRoute : RatingRoute<IRoutingSupport>
	{
		public RouteSetRatingRoute(RouteSet parentRouteSet, IRoutingSupport parent)
			: base(parent)
		{
			this.ParentRouteSet = parentRouteSet;
			this.parentConsol = parent as CommonConsol;
		}

		public RouteSet ParentRouteSet { get; }
		readonly CommonConsol parentConsol;

		public CommonConsol ParentConsol => parentConsol;

		public override ILocation Origin
		{
			get { return ParentRouteSet.Origin; }
		}

		public override ILocation Destination
		{
			get { return ParentRouteSet.Destination; }
		}

		public override ILocation GetVia(CostSell costOrSell) => ParentRouteSet.Via;

		public override ILocation GetFirstLoad(CostSell costOrSell) => parentConsol.LoadPort;

		public override ILocation GetLastDischarge(CostSell costOrSell) => parentConsol.DischargePort;

		public override ILocation GetFirstRouteSetLoad(CostSell costOrSell) =>
			parentConsol.Transports.Where(r => r.TransportMode == parent.TransportMode).FirstOrDefault()?.LoadPort;

		public override ILocation GetLastRouteSetDischarge(CostSell costOrSell) =>
			parentConsol.Transports.Where(r => r.TransportMode == parent.TransportMode).LastOrDefault()?.DiscPort;

		public override ZString TransportMode
		{
			get { return ParentRouteSet.TransportMode; }
		}

		public override OrgHeader Carrier
		{
			get { return ParentRouteSet.Carrier; }
		}

		public override Creditors Creditors
		{
			get => creditorsOverride ?? this.GetCreditors();
			set => creditorsOverride = value;
		}
		Creditors creditorsOverride;

		public bool ForceConsolCreditors { get; set; }

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new RouteSetJobDatesProvider(ParentRouteSet, Parent); }
		}

		public override ZInt RouteSetNumber
		{
			get { return ParentRouteSet.RouteSetNumber; }
		}

		public override ZBool SupportsManualRateSelection
		{
			get => supportsManualRateSelectionOverride;
			set => supportsManualRateSelectionOverride = value;
		}
		ZBool supportsManualRateSelectionOverride = true;

		bool IncludeConsolCreditors
		{
			get { return parent.TransportsIncludingRelated.RouteSets.Count == 1 || ForceConsolCreditors; }
		}

		string RouteSetSource
		{
			get { return Res.GetString("77abb2cd-8f40-4152-9f43-19420d846717", "Route Set"); }
		}

		internal List<OrgWithSource[]> LoadingOriginOrgGroups
		{
			get
			{
				var transportLegDepartureLocation = OrgWithSource.NewFrom<OrgAddress>(ParentRouteSet.ReferenceLeg.JW_OA_DepartureLocationInfo);
				if (transportLegDepartureLocation != null)
				{
					transportLegDepartureLocation.PrependSource(RouteSetSource);
				}

				var result = new List<OrgWithSource[]>() { new[] { transportLegDepartureLocation } };

				if (IncludeConsolCreditors && parentConsol != null)
				{
					result.Add(new[]
						{
							OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_DepartureCTOAddressInfo),
							OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_PackDepotAddressInfo),
							OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_DeparturePackCFSTransportAddressInfo)
						});
				}

				return result;
			}
		}

		internal List<OrgWithSource[]> UnloadingDestinationOrgGroups
		{
			get
			{
				var transportLegArrivalLocation = OrgWithSource.NewFrom<OrgAddress>(ParentRouteSet.ReferenceLeg.JW_OA_ArrivalLocationInfo);
				if (transportLegArrivalLocation != null)
				{
					transportLegArrivalLocation.PrependSource(RouteSetSource);
				}

				var result = new List<OrgWithSource[]>() { new[] { transportLegArrivalLocation } };

				if (IncludeConsolCreditors && parentConsol != null)
				{
					var arrCTO = OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_ArrivalCTOAddressInfo);
					var unpackDepot = OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_UnpackDepotAddressInfo);
					var arrCFSTransportSource = new List<string>()
					{
						parentConsol.HumanReadableName,
						parentConsol.JK_OH_ArrivalUnpackCFSTransportInfo.HumanReadableName
					};
					var arrivalUnpack = OrgWithSource.New(parentConsol.ArrivalUnpackCFSTransport, arrCFSTransportSource);

					result.Add(new[] { arrCTO, unpackDepot, arrivalUnpack });
				}

				return result;
			}
		}

		internal OrgWithSource RouteCreditor
		{
			get
			{
				var routeCreditorSource = new List<string>() { RouteSetSource, ParentRouteSet.ReferenceLeg.CreditorPKInfo.HumanReadableName };
				var routeCreditor = ParentRouteSet.ReferenceLeg.Creditor != null ? OrgWithSource.New(ParentRouteSet.ReferenceLeg.Creditor, routeCreditorSource) : null;
				return routeCreditor;
			}
		}

		internal OrgWithSource RouteShippingLine
		{
			get { return OrgWithSource.NewFrom<OrgAddress>(ParentRouteSet.ReferenceLeg.JW_OA_CarrierAddressInfo); }
		}

		bool AddAddressFromParentCreditor
		{
			get
			{
				var orgProxyPK = (GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy).PK;
				return parentConsol != null && parentConsol.CreditorPK != orgProxyPK && IncludeConsolCreditors;
			}
		}

		internal OrgWithSource ParentCreditor
		{
			get
			{
				if (AddAddressFromParentCreditor)
				{
					return OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_CreditorAddressInfo);
				}
				return null;
			}
		}

		internal OrgWithSource ParentCarrierImportCreditor
		{
			get
			{
				if (AddAddressFromParentCreditor)
				{
					return OrgWithSource.NewFrom<OrgAddress>(parentConsol.CarrierImportCreditorAddress?.E2_OA_AddressInfo);
				}
				return null;
			}
		}

		internal OrgWithSource ParentCarrierExportCreditor
		{
			get
			{
				if (AddAddressFromParentCreditor)
				{
					return OrgWithSource.NewFrom<OrgAddress>(parentConsol.CarrierExportCreditorAddress?.E2_OA_AddressInfo);
				}
				return null;
			}
		}

		internal bool IsParentConsolDomesticCoLoad => (parentConsol?.IsDomestic() ?? false) && parentConsol.IsCoLoad;

		internal bool IsParentConsolCrossTradeCoLoad => (parentConsol?.IsCrossTrade() ?? false) && parentConsol.IsCoLoad;

		internal OrgWithSource RouteAgent
		{
			get
			{
				if (IncludeConsolCreditors && parentConsol != null)
				{
					return parentConsol.JobDirection == Directions.Import
						? OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_SendingForwarderAddressInfo)
						: OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_ReceivingForwarderAddressInfo);
				}
				else
				{
					return null;
				}
			}
		}

		internal List<OrgWithSource> RouteForwarders
		{
			get
			{
				var result = new List<OrgWithSource>();
				if (IncludeConsolCreditors && parentConsol != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_SendingForwarderAddressInfo));
					result.Add(OrgWithSource.NewFrom<OrgAddress>(parentConsol.JK_OA_ReceivingForwarderAddressInfo));
				}
				return result;
			}
		}
	}
}
