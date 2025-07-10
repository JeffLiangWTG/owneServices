using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[CodeProperty(RateTransportZonesSchema.Constants.TZ_ZoneName), DescriptionProperty(RateTransportZonesSchema.Constants.TZ_ZoneName)]
	[DebuggerDisplay("{TZ_ZoneName}")]
	public class RateTransportZone : AutoRateTransportZones, IRatingZone, IRateTransportZone
	{
		public RateTransportZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IRateTransportZone

		IRateTransportZoneItemCollection IRateTransportZone.Items
		{
			get { return Items; }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Items.DeleteAll();
			base.Delete();
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				var query = new ZQuery(RateEntrySchema.TI_TZ_OriginZone, SQLComparisonOperator.Equal, PK);
				query.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_TZ_DestinationZone, PK);

				var entry = Factory.LoadTop1<RateEntry>(query);

				if (entry != null)
				{
					reasonForNotAbleToDelete = GetRateEntryReferencesToZoneMessage(entry);
					return false;
				}

				query = new ZQuery(RateLineItemsSchema.TM_TZ_DomesticZone, SQLComparisonOperator.Equal, PK);
				var rateLineItem = Factory.LoadTop1<RateLineItem>(query);

				if (rateLineItem != null)
				{
					reasonForNotAbleToDelete = GetRateLineItemReferencesToZoneMessage(rateLineItem);
					return false;
				}

				reasonForNotAbleToDelete = (NoResString)string.Empty;
				return true;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return reasonForNotAbleToDelete; }
		}
		MultilingualString reasonForNotAbleToDelete;

		MultilingualString GetRateEntryReferencesToZoneMessage(RateEntry entry)
		{
			return ResString.GetMultilingualString("710ffbe8-0716-4034-a635-1109255350ed",
				"Transport zone {0} is used by {4}'s ({5}) Rate Entry with Location: {1}, Start date: {2} and End Date: {3}.",
				TZ_ZoneName,
				entry.TI_OriginLRC.IsEmpty ? Res.GetString("27b2dbee-09a4-428f-9e1f-e0554b313dc2", "Empty") : (string)entry.TI_OriginLRC,
				entry.TI_RateStartDate.ToString("dd/MM/yyyy"),
				entry.TI_RateEndDate.IsEmpty ? Res.GetString("27b2dbee-09a4-428f-9e1f-e0554b313dc2", "Empty") : entry.TI_RateEndDate.ToString("dd/MM/yyyy"),
				entry.Parent.HumanReadableName,
				entry.Organisation);
		}

		MultilingualString GetRateLineItemReferencesToZoneMessage(RateLineItem rateLineItem)
		{
			var entry = rateLineItem.Parent.Parent;
			return ResString.GetMultilingualString("7c089e7f-609f-4dfd-8b07-ce3dd975bd46", "Transport zone {0} is used by CTZ calculator in {4}'s ({5}) Rate Entry with Location: {1}, Start date: {2} and End Date: {3}.",
				TZ_ZoneName,
				entry.TI_OriginLRC.IsEmpty ? Res.GetString("27b2dbee-09a4-428f-9e1f-e0554b313dc2", "Empty") : (string)entry.TI_OriginLRC,
				entry.TI_RateStartDate.ToString("dd/MM/yyyy"),
				entry.TI_RateEndDate.IsEmpty ? Res.GetString("27b2dbee-09a4-428f-9e1f-e0554b313dc2", "Empty") : entry.TI_RateEndDate.ToString("dd/MM/yyyy"),
				entry.Parent.HumanReadableName,
				entry.Organisation);
		}

		#endregion

		#region IRatingZone

		ZString IRatingZone.ZoneCode
		{
			get { return TZ_ZoneName; }
		}

		ZGuid IRatingZone.RelatedOrgPK
		{
			get { return TransportProvider != null ? TransportProvider.TP_OH_RelatedParty : ZGuid.Empty; }
		}

		#endregion

		#region Related Business Objects

		#region Transport Provider

		public RateTransportProvider TransportProvider
		{
			get { return Factory.Load<RateTransportProvider>(TZ_TP); }
		}

		#endregion

		#region Zone Items

		[ChildEditable(true)]
		public RateTransportZoneItemCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new RateTransportZoneItemCollection(this);
					RegisterEditableChildObject(items);
				}
				return items;
			}
		}
		RateTransportZoneItemCollection items;

		#endregion

		#endregion

		#region Properties

		[RelatedBusinessObject("TransportProvider")]
		public override ZGuid TZ_TP
		{
			get { return base.TZ_TP; }
			set
			{
				base.TZ_TP = value;
				Items.MarkAsNeedingValidation();
			}
		}

		public bool TZ_IsActive_ReadOnly
		{
			get { return !TransportProvider.TP_IsActive; }
		}

		IRateTransportProvider IRateTransportZone.TransportProvider => TransportProvider;

		#endregion

		public static RateTransportZone GetOperationZone(BusinessObjectFactory factory, IOrgHeader owner, ILocation location, string countryCode, ZDecimal? distance, ZString postCode, ZString citySuburb)
		{
			var zoneSet = RateTransportZoneHelper.GetMostApplicableZoneSet(location, owner, RatingConstants.RatingZoneTypes.Operations, Core.Constants.RateMode.ALL, factory);

			return distance.HasValue
				? RateTransportZoneHelper.GetZoneForDistance(zoneSet, countryCode, distance.Value)
				: RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, countryCode, postCode, citySuburb);
		}
	}
}

