using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Freight.Agency.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal class ContainerDetentionMovementRatingAdapter : RatingAdapter<ContainerMovement>, IAutoRatingDescriptionMacroExpander
	{
		public ContainerDetentionMovementRatingAdapter(ContainerMovement movement)
			: base(movement)
		{
			if (movement == null)
			{
				throw new ArgumentNullException(nameof(movement));
			}

			detention = movement.Detention;
			container = GetContainer(movement);
			shipment = container == null ? null : container.Booking;
		}

		ZString ShipmentDestination => shipment == null ? ZString.Empty : shipment.JS_RL_NKDestination;

		ZString ShipmentOrigin => shipment == null ? ZString.Empty : shipment.JS_RL_NKOrigin;

		static BillOfLadingContainer GetContainer(ContainerMovement movement)
		{
			var stock = movement.Stock;

			if (stock == null)
			{
				return null;
			}

			var filter = GetContainerFilter(movement.E9_JV, stock.R6_ContainerNum);
			return movement.Factory.LoadTop1<BillOfLadingContainer>(filter);
		}

		static ZQuery GetContainerFilter(ZGuid voyagePK, string containerNum)
		{
			var origin = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			origin.AddToFilter(JobVoyOriginSchema.JA_JV, voyagePK);

			var sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
			sailing.AddSubQuery(origin, JoinCondition.And);

			var shipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			shipment.AddSubQuery(sailing, JoinCondition.And);

			var container = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
			container.AddSubQuery(shipment, JoinCondition.And);
			container.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNum);

			return container;
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var detentionJobServiceInfo = GetDetentionJobServiceInfo();
				return detentionJobServiceInfo != null ? new JobServicesCollection { detentionJobServiceInfo } : base.JobServices;
			}
		}

		JobServiceInfo GetDetentionJobServiceInfo()
		{
			if (Parent.Stock == null || Parent.Stock.Container == null)
			{
				return null;
			}

			var description = Res.GetString("94f58b4e-3a04-4e18-b7d2-297de0fd4173", "Container Detention");
			var jobInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention, description, null, TimeSpan.FromDays(Parent.E9_DetentionDays), Consignor, 0, JobServiceInfo.Constants.Codes.Day);
			jobInfo.ContainerType = Parent.Stock.Container.PK;
			return jobInfo;
		}

		#region IAutoRating Members

		public override AdapterType AdapterType => AdapterType.ContainerMovement;

		public override ZString OperationalJobCode => Invariant($"{(Parent.Stock == null ? (ZString)"-" : Parent.Stock.R6_ContainerNum)}/{Parent.E9_MovementType}/{Parent.E9_MovementDate.ToShortDateString()}"); // no need to translate

		public override IJobInvoicingSupporter InvoicingSupporter => shipment != null ? shipment.InvoicingSupporter : null;

		public override IJobDatesProvider JobDatesProvider => new DetentionContainerRatingJobDatesProvider(Parent);

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var collection = new ChargeCodeGroupCollection();

				switch (ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType))
				{
					case DetentionInvoiceType.Codes.Export:
						collection.Add(ChargeCodeGroupList.Codes.Origin);
						break;

					case DetentionInvoiceType.Codes.Import:
						collection.Add(ChargeCodeGroupList.Codes.Destination);
						break;
				}

				return collection;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.AgencyDetentionInvoice;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override MergeChargeOptions MergeCharges => MergeChargeOptions.WithinAdapter;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override RateType RateTypeToUse
		{
			get
			{
				switch (ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType))
				{
					case DetentionInvoiceType.Codes.Export:
						return RateType.ShippingExportDetention;

					case DetentionInvoiceType.Codes.Import:
						return RateType.ShippingImportDetention;

					default:
						return 0;
				}
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override AutoRatingStatusInfo StatusInformation => new AutoRatingStatusInfo(true, string.Empty);

		#endregion

		#region IAutoRatingLocations Members

		public override ILocation Destination => Parent.Depot != null && ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType) == DetentionInvoiceType.Codes.Import ? Parent.Depot.EffectiveRelatedPortCode : null;

		public override ILocation Origin => Parent.Depot != null && ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType) == DetentionInvoiceType.Codes.Export ? Parent.Depot.EffectiveRelatedPortCode : null;

		#endregion

		#region IAutoRatingOrganisations Members

		public override OrgHeader Carrier => detention.Principal;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString DeliveryCartageEquipment => ZString.Empty;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString PickupCartageEquipment => ZString.Empty;

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = shipment != null ? shipment.RatingAdapter.DebtorOrgs : new DebtorOrgCollection();
				var localCharges = detention?.InvoicingSupporter?.Job?.LocalCharges;
				if (localCharges != null)
				{
					result[RatingDebtorOrgTypes.LC] = localCharges;
				}

				if (result[RatingDebtorOrgTypes.CNE] == null && ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType) == DetentionInvoiceType.Codes.Import && detention != null)
				{
					result[RatingDebtorOrgTypes.CNE] = detention.Client;
				}

				if (result[RatingDebtorOrgTypes.CNR] == null && ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType) == DetentionInvoiceType.Codes.Export && detention != null)
				{
					result[RatingDebtorOrgTypes.CNR] = detention.Client;
				}

				return result;
			}
		}

		#endregion

		#region IAutoRatingFreightInfo Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override FreightMode FreightMode => FreightMode.SEA | FreightMode.Containerised;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString ContainerMode => Core.Constants.ContainerModes.FCL;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString HousebillReleaseType => ZString.Empty;

		public override Directions JobDirection
		{
			get
			{
				switch (ContainerMovementTypes.GetDetentionCalculation(Parent.E9_MovementType))
				{
					case DetentionInvoiceType.Codes.Import:
						return Directions.Import;
					case DetentionInvoiceType.Codes.Export:
						return Directions.Export;
					default:
						return Directions.Unknown;
				}
			}
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				result.CreateContainerList(includeCommodity: false);

				if (container != null)
				{
					result.AddContainer(container.JC_RC);
				}

				result.Time = new TimeInfo(Parent.E9_DetentionDays, 0, 0);
				return result;
			}
		}

		#endregion

		#region IAutoRatingDescriptionMacroExpander Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Lookup Key.")]
		static class Keys
		{
			public const string Origin = "origin";
			public const string Destionation = "destination";
			public const string DetentionPort = "detentionport";
			public const string Vessel = "vessel";
			public const string Voyage = "voyage";
			public const string ContainerNum = "containernum";
			public const string ContainerType = "containertype";
			public const string DetentionDays = "detentiondays";
			public const string BillNum = "billnum";
		}

		public bool CanExpandMacros => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public string ExpandMacro(string macro)
		{
			switch (macro)
			{
				case Keys.Origin:
					return ShipmentOrigin;
				case Keys.Destionation:
					return ShipmentDestination;
				case Keys.DetentionPort:
					return Parent.DepotPort;
				case Keys.Vessel:
					return shipment == null || shipment.Sailing == null ? string.Empty : shipment.Sailing.JX_JV_NKVessel.ToString();
				case Keys.Voyage:
					return shipment == null || shipment.Sailing == null ? string.Empty : shipment.Sailing.JX_JV_VoyageFlight.ToString();
				case Keys.ContainerNum:
					return container == null ? string.Empty : container.JC_ContainerNum.ToString();
				case Keys.ContainerType:
					return container == null || container.Container == null ? string.Empty : container.Container.RC_Code.ToString();
				case Keys.DetentionDays:
					return Parent.E9_DetentionDays.ToString();
				case Keys.BillNum:
					return Parent.RelatedInfo.BillsOfLading.ToString();
				default:
					return null;
			}
		}

		#endregion

		readonly ContainerDetention detention;
		readonly BillOfLadingContainer container;
		readonly BillOfLading shipment;
	}
}


