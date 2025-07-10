using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public static class CreditorsPrioritizer
	{
		static string MostInterestingTransport
		{
			get { return Res.GetString("ca04e8a5-a38e-40d1-8f6b-e4df4ed47275", "Most Interesting"); }
		}

		static string PickupAgent
		{
			get { return Res.GetString("c1bfaff6-b6d3-4c73-98af-681125f7f583", "Pickup Agent"); }
		}

		public static Creditors GetCreditors(this CommonConsol consol)
		{
			if (!consol.CreditorPK.IsEmpty
				&& consol.CreditorPK == GlbBranch.CurrentBranch.GB_OH_OrgProxy
				&& consol.IsGatewayBillingEnabled())
			{
				return Creditors.New(OrgWithSource.NewFrom<OrgAddress>(consol.JK_OA_CreditorAddressInfo));
			}

			return GetCreditors(new[] { consol }, ((IImportExport)consol).JobDirection);
		}

		public static Creditors GetCreditors(this CommonShipment shipment)
		{
			var consols = new List<CommonConsol>();
			consols.AddRange(new[] { shipment.GetFirstOrCorrectConsol() }.Concat(shipment.Consols.Cast<CommonConsol>()).Where(x => x != null).Distinct());

			var consolProviders = GetCreditors(consols, ((IImportExport)shipment).JobDirection);

			var shipmentProviders = new Creditors();

			if (((IImportExport)shipment).JobDirection == Directions.Import)
			{
				shipmentProviders.Add(ChargeCodeGroupList.Codes.CustomsDuty, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_ImportBrokerInfo)));
				shipmentProviders.Add(ChargeCodeGroupList.Codes.Brokerage, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_ImportBrokerInfo)));
			}

			if (((IImportExport)shipment).JobDirection == Directions.Export)
			{
				shipmentProviders.Add(ChargeCodeGroupList.Codes.CustomsDuty, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_ExportBrokerInfo)));
				shipmentProviders.Add(ChargeCodeGroupList.Codes.OriginBrokerage, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_ExportBrokerInfo)));
			}

			if (shipment.JS_IsBooking && !shipment.JS_IsForwardRegistered)
			{
				shipmentProviders.Add(ChargeCodeGroupList.Codes.Freight, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgAddress>(shipment.JS_OA_BookedShippingLineAddressInfo)));
				shipmentProviders.Add(ChargeCodeGroupList.Codes.Insurance, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgAddress>(shipment.JS_OA_BookedShippingLineAddressInfo)));
			}

			var maxPriorityOnConsol = 7;

			var deliveryCartageCoSource = new List<string>
			{
				shipment.HumanReadableName,
				shipment.DocsAndCartage.HumanReadableName,
				shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.HumanReadableName
			};

			OrgWithSource deliveryCartageRelatedPartyWithSource = null;
			var deliveryCartageCoWithSource = OrgWithSource.New(shipment.DocsAndCartage.DeliveryCartageCo, deliveryCartageCoSource);

			if (deliveryCartageCoWithSource != null
				&& shipment.ConsigneeDeliveryAddress?.E2_OA_Address_ZAddress?.OrgAddress is OrgAddress deliverTo
				&& !string.IsNullOrWhiteSpace(deliverTo.ClosestPort))
			{
				var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
				{
					TransportMode = shipment.JS_TransportMode,
					ContainerMode = shipment.JS_PackingMode,
					CreditorType = DefaultCreditorHelper.CreditorType.DeliveryTransport,
					UNLOCO = deliverTo.ClosestPort
				};

				var creditor = DefaultCreditorHelper.GetCreditorOrgHeaderFromOrgRelatedParties(deliveryCartageCoWithSource.Org, orgRelatedPartyFilter, shipment.Factory);

				if (creditor != null)
				{
					var deliveryRelatedPartySource = new List<string>
					{
						shipment.HumanReadableName,
						shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.HumanReadableName,
						Res.GetString("66080b4b-3882-4ca0-8a66-bded031cb467", "Delivery Transport Related Party")
					};

					deliveryCartageRelatedPartyWithSource = OrgWithSource.New(creditor, deliveryRelatedPartySource);
				}
			}

			var controllingAgentAddress = shipment.DocAddresses.FindByDocAddressType(DocAddressType.ControllingAgent);

			var controllingAgentSource = new List<string>
			{
				shipment.HumanReadableName,
				Res.GetString("85915b29-003e-456d-ab30-06e34abdd025", "Controlling Agent")
			};

			OrgWithSource pickupCartageRelatedPartyWithSource = null;
			var pickupCartageCoWithSource = OrgWithSource.NewFrom<OrgAddress>(shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);

			if (pickupCartageCoWithSource != null
				&& shipment.ConsignorPickupAddress?.E2_OA_Address_ZAddress?.OrgAddress is OrgAddress pickupFrom
				&& !string.IsNullOrWhiteSpace(pickupFrom.ClosestPort))
			{
				pickupCartageCoWithSource.PrependSource(shipment.HumanReadableName);

				var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
				{
					TransportMode = shipment.JS_TransportMode,
					ContainerMode = shipment.JS_PackingMode,
					CreditorType = DefaultCreditorHelper.CreditorType.PickupTransport,
					UNLOCO = pickupFrom.ClosestPort
				};

				var creditor = DefaultCreditorHelper.GetCreditorOrgHeaderFromOrgRelatedParties(pickupCartageCoWithSource.Org, orgRelatedPartyFilter, shipment.Factory);

				if (creditor != null)
				{
					var pickupRelatedPartySource = new List<string>
					{
						shipment.HumanReadableName,
						shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo.HumanReadableName,
						Res.GetString("be60a7eb-4bb1-4e17-9914-2b7c7194536f", "Pickup Transport Related Party")
					};

					pickupCartageRelatedPartyWithSource = OrgWithSource.New(creditor, pickupRelatedPartySource);
				}
			}

			var pickupAgentSource = new List<string>
			{
				shipment.HumanReadableName,
				PickupAgent
			};

			for (int i = 1; i < maxPriorityOnConsol; i++)
			{
				foreach (var group in shipmentChargeCodeGroups)
				{
					shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.New(controllingAgentAddress != null ? controllingAgentAddress.Organisation : null, controllingAgentSource) } });

					switch (group)
					{
						case ChargeCodeGroupList.Codes.Origin:
						case ChargeCodeGroupList.Codes.Loading:
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, pickupCartageRelatedPartyWithSource } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, pickupCartageCoWithSource } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_ExportBrokerInfo) } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.New(shipment.PickupAgent, pickupAgentSource) } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.NewFrom<OrgAddress>(shipment.JS_OA_ExportReceivingDepotInfo) } });
							break;

						case ChargeCodeGroupList.Codes.Destination:
						case ChargeCodeGroupList.Codes.Unloading:
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, deliveryCartageRelatedPartyWithSource } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, deliveryCartageCoWithSource } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_ImportBrokerInfo) } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.NewFrom<OrgHeader>(shipment.JS_OH_DeliveryAgentInfo) } });
							shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.NewFrom<OrgAddress>(shipment.JS_OA_ImportReleaseDepotInfo) } });
							break;
					}

					if (shipment.JS_IsBooking)
					{
						shipmentProviders[group].Merge(new OrgPrioritizedList { { i, OrgWithSource.NewFrom<OrgAddress>(shipment.JS_OA_BookedShippingLineAddressInfo) } });
					}
				}
			}

			shipmentProviders.Merge(consolProviders);
			return shipmentProviders;
		}

		static Creditors GetCreditors(IEnumerable<CommonConsol> consols, Directions direction)
		{
			var result = new Creditors();

			foreach (var group in consolChargeCodeGroups)
			{
				int i = 1;

				var ctoCfsProviderGroups = GetCtoCfsProviderGroups(group);

				var list = new ProvidersList(consols);
				list.AddFromConsols(ref i, x =>
				{
					var creditorOnRouteSource = new List<string>()
					{
						x.HumanReadableName,
						x.Transports.HumanReadableName,
						MostInterestingTransport,
						x.Transports.MostInterestingTransport.CreditorPKInfo.HumanReadableName
					};

					return OrgWithSource.New(x.Transports.MostInterestingTransport.Creditor, creditorOnRouteSource);
				});

				list.AddExportImportCreditor(ref i);

				list.AddFromConsols(ref i, GetConsolCreditor);

				switch (direction)
				{
					case Directions.Import:
					case Directions.Export:
						list.AddAgentCarrierAndOtherProviders(ref i, ctoCfsProviderGroups, group, direction);
						break;

					case Directions.Domestic:

						foreach (var tpGroup in ctoCfsProviderGroups)
						{
							list.AddFromConsols(ref i, tpGroup);
						}

						list.AddFromConsols(ref i, x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ShippingLineAddressInfo));
						break;

					case Directions.CrossTrade:

						var listOfFuncs = new List<Func<CommonConsol, OrgWithSource>>();

						listOfFuncs.Add(x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ShippingLineAddressInfo));
						listOfFuncs.Add(x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_SendingForwarderAddressInfo));
						listOfFuncs.Add(x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ReceivingForwarderAddressInfo));

						listOfFuncs.AddRange(ctoCfsProviderGroups.SelectMany(x => x));

						list.AddFromConsols(ref i, listOfFuncs.ToArray());

						break;
				}

				result.Add(group, list);
			}

			return result;
		}

		static OrgWithSource GetConsolCreditor(CommonConsol consol)
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;

			if (orgProxy != null && consol.Creditor != null && consol.Creditor.PK != orgProxy.PK)
			{
				return OrgWithSource.NewFrom<OrgAddress>(consol.JK_OA_CreditorAddressInfo);
			}

			return null;
		}

		static OrgWithSource GetConsolExportImportCreditor(CommonConsol consol)
		{
			var isExport = consol.IsExport();
			var isImport = consol.IsImport();
			var isCrossTrade = consol.IsCrossTrade();
			var isDomestic = consol.IsDomestic();

			var applicableTransport = isExport || isImport || isCrossTrade || isDomestic;

			if (consol.IsCoLoad && applicableTransport)
			{
				OrgHeader creditorAddressOrg = null;
				var orgSource = string.Empty;

				if (isExport || isDomestic)
				{
					creditorAddressOrg = consol.CarrierExportCreditor;
					orgSource = Res.GetString("71188a8e-a9c5-48bc-9cc6-337197f3e742", "Carrier Export Creditor");
				}
				else
				{
					creditorAddressOrg = consol.CarrierImportCreditor;
					orgSource = Res.GetString("c16b0ab2-df45-4e98-9f87-0c66af2060e2", "Carrier Import Creditor");
				}

				var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;

				if (orgProxy != null && creditorAddressOrg != null && creditorAddressOrg.PK != orgProxy.PK)
				{
					var creditorAddressSource = new List<string>
					{
						consol.HumanReadableName,
						orgSource
					};

					return OrgWithSource.New(creditorAddressOrg, creditorAddressSource);
				}
			}

			return null;
		}

		static List<Func<CommonConsol, OrgWithSource>[]> GetCtoCfsProviderGroups(string group)
		{
			var result = new List<Func<CommonConsol, OrgWithSource>[]>();

			switch (group)
			{
				case ChargeCodeGroupList.Codes.Loading:
				case ChargeCodeGroupList.Codes.Origin:
					Func<CommonConsol, OrgWithSource> depCTOOnRoute = x =>
					{
						var orgWithSource = OrgWithSource.NewFrom<OrgAddress>(x.Transports.MostInterestingTransport.JW_OA_DepartureLocationInfo);

						if (orgWithSource != null)
						{
							orgWithSource.PrependSource(x.HumanReadableName, x.Transports.HumanReadableName, MostInterestingTransport);
						}

						return orgWithSource;
					};

					Func<CommonConsol, OrgWithSource> depCTO = x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_DepartureCTOAddressInfo);

					Func<CommonConsol, OrgWithSource> depCFS = x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_PackDepotAddressInfo);

					Func<CommonConsol, OrgWithSource> depCFSTransport = x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_DeparturePackCFSTransportAddressInfo);

					result.Add(new[] { depCTOOnRoute });
					result.Add(new[] { depCTO, depCFS, depCFSTransport });
					return result;

				case ChargeCodeGroupList.Codes.Unloading:
				case ChargeCodeGroupList.Codes.Destination:
					Func<CommonConsol, OrgWithSource> arrCTOOnRoute = x =>
					{
						var orgWithSource = OrgWithSource.NewFrom<OrgAddress>(x.Transports.MostInterestingTransport.JW_OA_ArrivalLocationInfo);

						if (orgWithSource != null)
						{
							orgWithSource.PrependSource(x.HumanReadableName, x.Transports.HumanReadableName, MostInterestingTransport);
						}

						return orgWithSource;
					};
					Func<CommonConsol, OrgWithSource> arrCTO = x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ArrivalCTOAddressInfo);

					Func<CommonConsol, OrgWithSource> arrCFS = x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_UnpackDepotAddressInfo);

					Func<CommonConsol, OrgWithSource> arrCFSTransport = x =>
					{
						var unpackCFSSource = new List<string>()
						{
							x.HumanReadableName,
							x.JK_OH_ArrivalUnpackCFSTransportInfo.HumanReadableName
						};

						return OrgWithSource.New(x.ArrivalUnpackCFSTransport, unpackCFSSource);
					};

					result.Add(new[] { arrCTOOnRoute });
					result.Add(new[] { arrCTO, arrCFS, arrCFSTransport });
					return result;
			}

			return result;
		}

		public static Creditors GetCreditors(this RouteSetRatingRoute ratingRoute)
		{
			var result = new Creditors();

			foreach (var group in consolChargeCodeGroups)
			{
				int i = 1;

				var ratingRouteCtoCfsProviderGroups = GetRatingRouteCtoCfsProviderGroups(ratingRoute, group);

				var list = new OrgPrioritizedList();

				AddWithIncreasedPriority(ref i, list, ratingRoute.RouteCreditor);

				if (ratingRoute.IsParentConsolCrossTradeCoLoad)
				{
					AddWithIncreasedPriority(ref i, list, ratingRoute.ParentCarrierImportCreditor);
				}

				if (ratingRoute.IsParentConsolDomesticCoLoad)
				{
					AddWithIncreasedPriority(ref i, list, ratingRoute.ParentCarrierExportCreditor);
				}

				AddWithIncreasedPriority(ref i, list, ratingRoute.ParentCreditor);

				var direction = ((IImportExport)ratingRoute.Parent).JobDirection;

				switch (direction)
				{
					case Directions.Import:
					case Directions.Export:
						AddAgentCarrierAndOtherProviders(ref i, list, ratingRouteCtoCfsProviderGroups, group, direction, ratingRoute);
						break;

					case Directions.Domestic:

						foreach (var tpGroup in ratingRouteCtoCfsProviderGroups)
						{
							AddWithIncreasedPriority(ref i, list, tpGroup);
						}

						AddWithIncreasedPriority(ref i, list, ratingRoute.RouteShippingLine);
						break;

					case Directions.CrossTrade:

						var listOfOrgs = new List<OrgWithSource>();

						listOfOrgs.Add(ratingRoute.RouteShippingLine);
						listOfOrgs.AddRange(ratingRoute.RouteForwarders);
						listOfOrgs.AddRange(ratingRouteCtoCfsProviderGroups.SelectMany(x => x));

						AddWithIncreasedPriority(ref i, list, listOfOrgs.ToArray());
						break;
				}

				result.Add(group, list);
			}

			return result;
		}

		static void AddWithIncreasedPriority(ref int priority, OrgPrioritizedList list, params OrgWithSource[] items)
		{
			int i = priority++;
			if (items != null)
			{
				foreach (var item in items)
				{
					list.Add(i, item);
				}
			}
		}

		static List<OrgWithSource[]> GetRatingRouteCtoCfsProviderGroups(RouteSetRatingRoute routeSet, string group)
		{
			var result = new List<OrgWithSource[]>();

			switch (group)
			{
				case ChargeCodeGroupList.Codes.Loading:
				case ChargeCodeGroupList.Codes.Origin:
					result.AddRange(routeSet.LoadingOriginOrgGroups);
					return result;

				case ChargeCodeGroupList.Codes.Unloading:
				case ChargeCodeGroupList.Codes.Destination:
					result.AddRange(routeSet.UnloadingDestinationOrgGroups);
					return result;
			}

			return result;
		}

		static void AddAgentCarrierAndOtherProviders(ref int priority, OrgPrioritizedList listToFill, List<OrgWithSource[]> otherProviderGroups, string group, Directions direction, RouteSetRatingRoute ratingRoute)
		{
			var additionalProviders1 = new List<OrgWithSource[]>();
			additionalProviders1.AddRange(otherProviderGroups);
			additionalProviders1.Add(new[] { ratingRoute.RouteShippingLine });
			additionalProviders1.Add(new[] { ratingRoute.RouteAgent });

			var additionalProviders2 = new List<OrgWithSource[]>();
			additionalProviders2.Add(new[] { ratingRoute.RouteAgent });
			additionalProviders2.Add(new[] { ratingRoute.RouteShippingLine });
			additionalProviders2.AddRange(otherProviderGroups);

			var consol = ratingRoute.Parent as CommonConsol;

			if (consol != null)
			{
				for (int i = 0; i < additionalProviders1.Count; i++)
				{
					if (consol.JK_PrepaidCollect == Constants.PaymentType.Collect || consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid)
					{
						bool chargedAtOurSide;

						if (direction == Directions.Import)
						{
							chargedAtOurSide = consol.JK_PrepaidCollect == Constants.PaymentType.Collect;
						}
						else
						{
							chargedAtOurSide = consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid;
						}

						if (group == ChargeCodeGroupList.Codes.Destination || group == ChargeCodeGroupList.Codes.Unloading)
						{
							chargedAtOurSide = !chargedAtOurSide;
						}

						var relevantGroup = chargedAtOurSide ? additionalProviders1[i] : additionalProviders2[i];

						listToFill.Add(priority, relevantGroup);
					}

					priority++;
				}
			}
		}

		class ProvidersList : OrgPrioritizedList
		{
			public ProvidersList(IEnumerable<CommonConsol> consols)
			{
				this.consols.UnionWith(consols);
			}

			readonly HashSet<CommonConsol> consols = new HashSet<CommonConsol>();

			public void AddAgentCarrierAndOtherProviders(ref int priority, List<Func<CommonConsol, OrgWithSource>[]> otherProviderGroups, string group, Directions direction)
			{
				Func<CommonConsol, OrgWithSource> agent = x => direction == Directions.Import
					? OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_SendingForwarderAddressInfo)
					: OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ReceivingForwarderAddressInfo);

				var providerAdditionFuncs1 = new List<Func<CommonConsol, OrgWithSource>[]>();
				providerAdditionFuncs1.AddRange(otherProviderGroups);
				providerAdditionFuncs1.Add(new Func<CommonConsol, OrgWithSource>[] { x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ShippingLineAddressInfo) });
				providerAdditionFuncs1.Add(new[] { agent });

				var providerAdditionFuncs2 = new List<Func<CommonConsol, OrgWithSource>[]>
											{
												new[] { agent },
												new Func<CommonConsol, OrgWithSource>[] { x => OrgWithSource.NewFrom<OrgAddress>(x.JK_OA_ShippingLineAddressInfo) }
											};

				providerAdditionFuncs2.AddRange(otherProviderGroups);

				for (int i = 0; i < providerAdditionFuncs1.Count; i++)
				{
					foreach (var consol in consols)
					{
						if (consol.JK_PrepaidCollect == Constants.PaymentType.Collect || consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid)
						{
							bool chargedAtOurSide;

							if (direction == Directions.Import)
							{
								chargedAtOurSide = consol.JK_PrepaidCollect == Constants.PaymentType.Collect;
							}
							else
							{
								chargedAtOurSide = consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid;
							}

							if (group == ChargeCodeGroupList.Codes.Destination || group == ChargeCodeGroupList.Codes.Unloading)
							{
								chargedAtOurSide = !chargedAtOurSide;
							}

							var relevantGroup = chargedAtOurSide ? providerAdditionFuncs1[i] : providerAdditionFuncs2[i];

							foreach (var providerGetter in relevantGroup)
							{
								Add(priority, providerGetter(consol));
							}
						}
					}

					priority++;
				}
			}

			public void AddFromConsols(ref int priority, params Func<CommonConsol, OrgWithSource>[] additionFuncs)
			{
				int i = priority++;

				if (additionFuncs != null)
				{
					foreach (var addition in additionFuncs)
					{
						if (addition != null)
						{
							AddRange(i, consols.Select(addition));
						}
					}
				}
			}

			public void AddExportImportCreditor(ref int priority)
			{
				foreach (var consol in consols)
				{
					var orgWithSource = GetConsolExportImportCreditor(consol);

					if (orgWithSource != null)
					{
						Add(priority++, orgWithSource);
					}
				}
			}
		}

		static IEnumerable<string> shipmentChargeCodeGroups
		{
			get
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
				};
			}
		}

		static IEnumerable<string> consolChargeCodeGroups
		{
			get
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.Insurance,
				};
			}
		}
	}
}
