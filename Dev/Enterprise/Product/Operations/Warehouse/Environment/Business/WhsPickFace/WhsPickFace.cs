using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public sealed class WhsPickFace : AutoWhsPickFace, IWhsPickFace, ILocationConsumer
	{
		public WhsPickFace(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WF_ReplenishMaximum = 1;
			WF_ReplenishmentMultiple = 1;
		}

		#endregion

		#region WF_OH_Client

		[List("Lookups.Clients")]
		public override ZGuid WF_OH_Client
		{
			get { return base.WF_OH_Client; }
			set { base.WF_OH_Client = value; }
		}

		#endregion

		#region Location

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(LocationWhsGuid); }
		}

		#endregion

		#region Location

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WF_WL); }
		}

		#endregion

		#region LocationWhsGuid

		[List("Lookups.Warehouses")]
		public ZGuid LocationWhsGuid
		{
			get { return Location?.WLV_WW_Whs ?? locationWhsGuid; }
			set
			{
				var location = Location;
				if (location != null && location.WLV_WW_Whs != value || locationWhsGuid != value)
				{
					locationWhsGuid = value;
					locationString = ZString.Empty;
					WF_WL = ZGuid.Empty;

					// Show error if there was an issue
					if (!IsValidationSuspended)
					{
						Validation.ValidateWarehouse();
					}
				}
				LocationWhsGuidInfo.RefreshBinding();
			}
		}

		ZGuid locationWhsGuid;

		public ZPropertyInfo LocationWhsGuidInfo
		{
			get { return GetZPropertyInfo(nameof(LocationWhsGuid)); }
		}

		#endregion

		#region WF_WL

		[RelatedBusinessObject("Location")]
		public override ZGuid WF_WL
		{
			get { return base.WF_WL; }
			set { base.WF_WL = value; }
		}

		#endregion

		#region LocationString

		[ReadOnly(false)]
		[List("Lookups.Locations")]
		[MaxLength(36)]
		public ZString LocationString
		{
			get { return Location?.WLV_LocationString_UserFriendly ?? locationString; }
			set
			{
				var location = Location;
				if (location != null && location.WLV_LocationString != value || locationString != value || !WF_WL.IsValid)
				{
					//prevalidation
					CheckMaximumLength(LocationStringInfo, value);

					locationString = value;
					WF_WL = WhsLocation.FindLocationPK(Factory, value, LocationWhsGuid);

					// Show error if there was an issue
					if (!IsValidationSuspended)
					{
						Validation.ValidateLocationString();
					}
					LocationStringInfo.RefreshBinding();
				}
			}
		}

		ZString locationString;

		public ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(nameof(LocationString)); }
		}

		#endregion

		#region ILocationConsumer Member

		ZGuid ILocationConsumer.LocationPK
		{
			get { return WF_WL; }
		}

		ZGuid ILocationConsumer.WarehousePK
		{
			get { return LocationWhsGuid; }
		}

		ZString ILocationConsumer.LocationTypeForMessages
		{
			get { return Res.GetString("287742cf-f72a-49ff-a97a-a9eff7614daf", "Pickface"); }
		}

		ZString ILocationConsumer.LocationTitle { get; set; }

		#endregion

		#endregion

		#region Committed Picks

		public WhsPickFaceCommittedStockViewCollection CommittedPicks
		{
			get
			{
				var committedPicks = GetCommittedPicks();
				var comparer = (IComparer<ISupportPickPriority>)ObjectFactory.Get<ISupportObjectPickPriorityComparer>();
				committedPicks.ApplySort(comparer);
				return committedPicks;
			}
		}

		WhsPickFaceCommittedStockViewCollection GetCommittedPicks()
		{
			var query = new ZQuery(WhsPickFaceCommittedStockViewSchema.WCP_WF_PickFace, PK);
			var collection = new WhsPickFaceCommittedStockViewCollection(Factory, query);
			return collection;
		}

		#endregion

		#region Awaiting Picks

		public WhsPickFaceAwaitingReplenishmentViewCollection AwaitingPicks
		{
			get
			{
				var awaitingPicks = GetAwaitingPicks();
				var comparer = (IComparer<ISupportPickPriority>)ObjectFactory.Get<ISupportObjectPickPriorityComparer>();
				awaitingPicks.ApplySort(comparer);
				return awaitingPicks;
			}
		}

		WhsPickFaceAwaitingReplenishmentViewCollection GetAwaitingPicks()
		{
			var awaitingPicksSQL = $@"
SELECT
	WWP_WP
FROM
	dbo.WhsPickFace
	JOIN dbo.WhsPickFaceAwaitingReplenishmentView ON WF_OH_Client = WWP_OH_Client AND WF_OP = WWP_OP
	JOIN dbo.WhsLocationView ON WLV_PK = WF_WL AND WLV_WW_Whs = WWP_WW_Whs
WHERE
	WF_PK = @pickFacePK
	AND WWP_QuantityRequired > 0
";
			var awaitingPicksQuery = new ZDBOnlyQuery(typeof(WhsPickFaceAwaitingReplenishmentView));
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@pickFacePK", this.PK, WhsPickFaceSchema.PK));
			awaitingPicksQuery.AddFilterAndZSQLParameterCollection($"WWP_WP IN ({awaitingPicksSQL})", sqlParams);
			awaitingPicksQuery.AddToFilter(WhsPickFaceAwaitingReplenishmentViewSchema.WWP_OP, WF_OP);
			awaitingPicksQuery.AddToFilter(WhsPickFaceAwaitingReplenishmentViewSchema.WWP_OH_Client, WF_OH_Client);
			var awaitingPicksCollection = new WhsPickFaceAwaitingReplenishmentViewCollection(Factory, awaitingPicksQuery);
			return awaitingPicksCollection;
		}

		#endregion

	}
}
