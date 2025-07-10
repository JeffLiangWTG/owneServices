using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class TrackingMAWBHeader : ExportAWBHeader, IMessagingSupport
	{
		public new abstract class Schema : ExportAWBHeader.Schema
		{
			public const string NumberOfPieces = "NumberOfPieces";
			public const string ActualWeight = "ActualWeight";
			public const string ActualWeightUnit = "ActualWeightUnit";
			public const string AWBMessagingStatusDescription = "AWBMessagingStatusDescription";
		}

		public TrackingMAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		[ChildEditable(true)]
		public ExportAWBHeaderDependentCollection WebChildBills
		{
			get
			{
				if (IsInDatabase)
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					TrackingMAWBHeader newMAWB = newFactory.Load<TrackingMAWBHeader>(PK);
					if (newMAWB != null)
					{
						return newMAWB.ChildBills;
					}
				}
				return ChildBills;
			}
		}

		public TrackingMAWBHeaderCollection HAWBs
		{
			get
			{
				if (hawbs == null)
				{
					hawbs = new TrackingMAWBHeaderCollection(Factory);
					hawbs.AddRange(ChildBills);
				}

				return hawbs;
			}
		}
		TrackingMAWBHeaderCollection hawbs;

		#endregion

		#region FromNumber

		public static TrackingMAWBHeader FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid pK, TrackingSiteUser siteUser)
		{
			TrackingMAWBHeader result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				var filter = new ZQuery(ExportAWBHeaderSchema.PK, pK);
				// will not filter by organization for now
				//if (!SiteUser.IsShipmentQuickViewUser) // We use it to enable direct view of SHipment Detalils by Housebill Number
				//{
				//    Filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingMAWBHeader>());
				//}
				filter.IgnoreActiveFilter = true;
				result = (TrackingMAWBHeader)factory.LoadTop1(typeof(TrackingMAWBHeader), filter);
				if (result != null)
				{
					result.SiteUser = siteUser;
				}
			}
			return result;
		}

		#endregion FromNumber

		#region SiteUser

		public TrackingSiteUser SiteUser { get; set; }

		#endregion

		#region IMessagingSupport Members

		public IEnumerable EDIMessages
		{
			get { return Messages; }
		}

		public ZString ParentReferenceNumber
		{
			get { return EH_WayBillNumber; }
		}

		#endregion

		#region Properties

		public ZString NumberOfPieces
		{
			get { return AWBRateLines[0].ER_NoOfPiecesOrRCP; }
			set { AWBRateLines[0].ER_NoOfPiecesOrRCP = value; }
		}

		public ZPropertyInfo NumberOfPiecesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NumberOfPieces, x => AWBRateLines[0].ER_NoOfPiecesOrRCPInfo); }
		}

		public ZDecimal ActualWeight
		{
			get { return AWBRateLines[0].ER_GrossWeight; }
			set { AWBRateLines[0].ER_GrossWeight = value; }
		}

		public ZPropertyInfo ActualWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ActualWeight, x => AWBRateLines[0].ER_GrossWeightInfo); }
		}

		public ZString ActualWeightUnit
		{
			get { return AWBRateLines[0].ER_WeightInLBsOrKGs; }
			set { AWBRateLines[0].ER_WeightInLBsOrKGs = value; }
		}

		public ZPropertyInfo ActualWeightUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ActualWeightUnit, x => AWBRateLines[0].ER_WeightInLBsOrKGsInfo); }
		}

		public CodeDescriptionPairList RateUQList
		{
			get { return AWBRateLines[0].RateUQList; }
		}

		public ZPropertyInfo AWBMessagingStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.AWBMessagingStatusDescription); }
		}

		#endregion
	}
}
