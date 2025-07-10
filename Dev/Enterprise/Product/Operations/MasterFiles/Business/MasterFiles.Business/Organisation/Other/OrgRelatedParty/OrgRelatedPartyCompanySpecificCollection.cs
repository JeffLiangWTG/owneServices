using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRelatedPartyCompanySpecificCollection : OrgRelatedPartyDependentCollection
	{
		public OrgRelatedPartyCompanySpecificCollection(OrgHeader parentOrganisation, BusinessObjectFactory factory)
			: base(parentOrganisation, factory)
		{
		}

		#region Get Related Party

		public OrgRelatedParty GetRelatedParty(ZString partyType, ZString direction)
		{
			return GetRelatedParty(ZGuid.Empty, partyType, direction, ZString.Empty, ZString.Empty);
		}

		public OrgRelatedParty GetRelatedParty(ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(ZGuid.Empty, partyType, direction, transportMode, containerMode);
		}

		public OrgRelatedParty GetRelatedParty(ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			return GetRelatedParty(ZGuid.Empty, partyType, direction, transportMode, containerMode, location);
		}

		public OrgRelatedParty GetRelatedParty(ZGuid? orgAddressPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(orgAddressPK, partyType, direction, transportMode, containerMode, ZString.Empty);
		}

		public OrgRelatedParty GetRelatedParty(ZGuid? orgAddressPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			return GetRelatedParty(orgAddressPK, partyType, direction, transportMode, containerMode, location, true);
		}

		OrgRelatedParty GetRelatedParty(ZGuid? orgAddressPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location, bool allowFallback)
		{
			// If this query changes for any reason then $\Glow should be updated. At this time the path is:
			// $\Glow\DotNet\Business\Organisation\Business.Organisation.Service\RelatedPartyCalculator.cs

			ZQuery filter = new ZQuery(OrgRelatedPartySchema.PR_PartyType, partyType);

			if (!orgAddressPK.HasValue || !orgAddressPK.Value.IsEmpty)
			{
				filter.AddToFilter(OrgRelatedPartySchema.PR_OA, orgAddressPK);
			}

			ZQuery companyFilter = new ZQuery(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

			if (!ShouldBeCompanySpecific(partyType))
			{
				companyFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);
			}

			filter.AddToFilter(companyFilter);

			ZQuery directionFilter = new ZQuery(OrgRelatedPartySchema.PR_FreightDirection, direction);
			if (direction == RelatedPartyDirectionList.Codes.Delivery || direction == RelatedPartyDirectionList.Codes.Pickup)
			{
				directionFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			}

			filter.AddToFilter(directionFilter);

			if (!transportMode.IsEmpty)
			{
				transportMode = RefineTransportModeForTwoModeTransportModes(direction, transportMode);

				var transportModeFilter = new ZQuery(OrgRelatedPartySchema.PR_FreightTransportMode, transportMode);

				if (allowFallback)
				{
					var containerModeFilter = new ZQuery(OrgRelatedPartySchema.PR_FreightContainerMode, ZString.Empty);
					if (!containerMode.IsEmpty)
					{
						containerModeFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_FreightContainerMode, containerMode);
					}
					transportModeFilter.AddToFilter(containerModeFilter, JoinCondition.And);
				}
				else
				{
					transportModeFilter.AddToFilter(JoinCondition.And, OrgRelatedPartySchema.PR_FreightContainerMode, containerMode);
				}

				ZQuery modeFilter = new ZQuery(transportModeFilter);

				if (allowFallback)
				{
					if (transportMode != Constants.TransportModes.All)
					{
						var transportModeAllfilter = new ZQuery(OrgRelatedPartySchema.PR_FreightTransportMode, Constants.TransportModes.All);
						modeFilter.AddToFilter(transportModeAllfilter, JoinCondition.Or);
					}
				}

				filter.AddToFilter(modeFilter);
			}

			ZQuery locationFilter = new ZQuery();
			locationFilter.AddToFilter(OrgRelatedPartySchema.PR_Location, location);

			if (!location.IsEmpty && allowFallback)
			{
				locationFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_Location, location.Left(2));
				locationFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_Location, ZString.Empty);
			}

			filter.AddToFilter(locationFilter);

			OrgRelatedParty[] possibleValues = (OrgRelatedParty[])Find(filter);

			var list = possibleValues.ToList();
			list.Sort(new RelatedPartyComparer());
			return list.Count > 0 ? list[0] : null;
		}

		static ZString RefineTransportModeForTwoModeTransportModes(ZString direction, ZString transportMode)
		{
			if (transportMode.EqualsIgnoringCase(Core.Constants.TransportModes.AirSea) && direction == RelatedPartyDirectionList.Codes.Pickup)
			{
				transportMode = Core.Constants.TransportModes.Air;
			}
			else if (transportMode.EqualsIgnoringCase(Core.Constants.TransportModes.AirSea) && direction == RelatedPartyDirectionList.Codes.Delivery)
			{
				transportMode = Core.Constants.TransportModes.Sea;
			}
			else if (transportMode.EqualsIgnoringCase(Core.Constants.TransportModes.SeaAir) && direction == RelatedPartyDirectionList.Codes.Pickup)
			{
				transportMode = Core.Constants.TransportModes.Sea;
			}
			else if (transportMode.EqualsIgnoringCase(Core.Constants.TransportModes.SeaAir) && direction == RelatedPartyDirectionList.Codes.Delivery)
			{
				transportMode = Core.Constants.TransportModes.Air;
			}

			return transportMode;
		}

		class RelatedPartyComparer : IComparer<OrgRelatedParty>, IComparer
		{
			public int Compare(object x, object y)
			{
				return Compare((OrgRelatedParty)x, (OrgRelatedParty)y);
			}

			public int Compare(OrgRelatedParty x, OrgRelatedParty y)
			{
				return Score(y) - Score(x);
			}

			[WTG.StaticAnalysis.Annotation.CodeAlive("Developer friendly enum")]
			[Flags]
			enum Scores
			{
				Direction = 1,
				TransportModeAll = 2,
				TransportMode = 4,
				ContainerMode = 8,
				Country = 16,
				Port = 32,
				Company = 64
			}

			int Score(OrgRelatedParty org)
			{
				int result = 0;

				if (!org.PR_GC.IsEmpty)
				{
					result |= (int)Scores.Company;
				}

				if (org.PR_Location.Length == 5)
				{
					result |= (int)Scores.Port;
				}

				if (org.PR_Location.Length == 2)
				{
					result |= (int)Scores.Country;
				}

				if (!org.PR_FreightTransportMode.IsEmpty)
				{
					if (!org.PR_FreightContainerMode.IsEmpty)
					{
						result |= (int)Scores.ContainerMode;
					}
					else if (org.PR_FreightTransportMode != Core.Constants.TransportModes.All)
					{
						result |= (int)Scores.TransportMode;
					}
					else
					{
						result |= (int)Scores.TransportModeAll;
					}
				}

				if (org.PR_FreightDirection != ZString.Empty && org.PR_FreightDirection != RelatedPartyDirectionList.Codes.PickupAndDelivery)
				{
					result |= (int)Scores.Direction;
				}

				return result;
			}
		}

		#endregion

		#region SetRelatedParty

		public void SetRelatedParty(ZGuid forAddressPK, OrgHeader relatedParty, ZString partyType, ZString direction, ZString freightMode, ZString containerMode, ZString location)
		{
			if (relatedParty != null)
			{
				OrgRelatedParty relatedPartyRecord = GetRelatedParty(forAddressPK, partyType, direction, freightMode, containerMode, location, false);

				if (relatedPartyRecord == null)
				{
					relatedPartyRecord = AddNew();
					relatedPartyRecord.PR_PartyType = partyType;
					relatedPartyRecord.PR_FreightDirection = direction;
					relatedPartyRecord.PR_FreightTransportMode = freightMode;
					relatedPartyRecord.PR_FreightContainerMode = containerMode;
					relatedPartyRecord.PR_Location = location;
					relatedPartyRecord.PR_OA = forAddressPK;
				}

				relatedPartyRecord.PR_GC = relatedPartyRecord.IsCompanySpecific ? GlbCompany.CurrentCompany.PK : ZGuid.Empty;
				relatedPartyRecord.PR_OH_RelatedParty = relatedParty.PK;
			}
		}

		public void SetRelatedParty(OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			SetRelatedParty(ZGuid.Empty, relatedParty, partyType, direction, transportMode, containerMode, location);
		}

		public void SetRelatedParty(ZGuid forAddressPK, OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			SetRelatedParty(forAddressPK, relatedParty, partyType, direction, transportMode, containerMode, ZString.Empty);
		}

		public void SetRelatedParty(OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			SetRelatedParty(ZGuid.Empty, relatedParty, partyType, direction, transportMode, containerMode);
		}

		public void SetRelatedParty(OrgHeader relatedParty, ZString partyType, ZString direction)
		{
			SetRelatedParty(ZGuid.Empty, relatedParty, partyType, direction, ZString.Empty, ZString.Empty);
		}

		#endregion

		#region Remove Related Party

		public void RemoveRelatedParty(ZString partyType, ZString direction)
		{
			RemoveRelatedParty(partyType, direction, ZString.Empty, ZString.Empty);
		}

		public void RemoveRelatedParty(ZString partyType, ZString direction, ZString freightMode, ZString containerMode)
		{
			OrgRelatedParty partyToDelete = GetRelatedParty(partyType, direction, freightMode, containerMode);
			if (partyToDelete != null)
			{
				RemoveAndDelete(partyToDelete);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (!((OrgRelatedParty)elementToDelete).IsAlrightToAddOrDelete)
			{
				return;
			}

			base.RemoveAndDelete(elementToDelete);
		}

		#endregion

		public static bool ShouldBeCompanySpecific(ZString partyType)
		{
			return partyType == RelatedPartyTypeList.Codes.APSettlementGroup ||
					partyType == RelatedPartyTypeList.Codes.ARSettlementGroup;
		}

		#region Relationship Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (Master != null)
			{
				ZQuery companyFilter = new ZQuery(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
				companyFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_GC, null);
				result.AddToFilter(companyFilter);
			}
			return result;
		}

		#endregion

		#region Refresh Org

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Master.RefreshRelatedPartyProxyFields();
		}

		#endregion
	}
}
