using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTypeOverrideCollection : DependentBusinessObjectCollection<AccChargeTypeOverride, AccChargeCode>
	{
		public AccChargeTypeOverrideCollection(AccChargeCode chargeCode) : base(chargeCode)
		{
		}

		#region Retrieve Charge Type

		internal IAccChargeTypeOverride GetChargeType(JobInvoicingConsumerType jobType = null, Directions direction = Directions.Unknown)
		{
			IAccChargeTypeOverride result = null;
			if (jobType != null && direction != Directions.Unknown)
			{
				ZString directionValue = "";
				if (direction == Directions.Import)
				{
					directionValue = Core.Constants.Sales.Mode.Import;
				}
				else if (direction == Directions.Export)
				{
					directionValue = Core.Constants.Sales.Mode.Export;
				}

				var ranker = new ColumnValueRanker();
				ranker.Add(AccChargeTypeOverrideSchema.AN_JobType, (ZString)(jobType != null ? jobType.Code : ""), (ZString)"ALL");
				ranker.Add(AccChargeTypeOverrideSchema.AN_JobDirection, directionValue, (ZString)"ALL");
				result = ranker.GetBestMatch<AccChargeTypeOverride>(Factory, new ZQuery(AccChargeTypeOverrideSchema.AN_AC_ChargeCode, ChargeCode.PK));
			}

			return result ?? ChargeCode;
		}

		#endregion

		#region Indexer + AddNew

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return AccChargeTypeOverrideSchema.AN_AC_ChargeCode; }
		}

		#endregion

		#region Master Charge Code

		public AccChargeCode ChargeCode
		{
			get { return Master; }
		}

		#endregion

		#region BusinessObjectCollectionSnapShotter

		BusinessObjectCollectionSnapshotManager businessObjectCollectionSnapShotter;

		public BusinessObjectCollectionSnapshotManager SnapShotter
		{
			get
			{
				if (businessObjectCollectionSnapShotter == null)
				{
					businessObjectCollectionSnapShotter = new BusinessObjectCollectionSnapshotManager(new AccChargeTypeOverrideCopier(), this);
				}
				return businessObjectCollectionSnapShotter;
			}
		}

		#endregion
	}

	public class AccChargeTypeOverrideCopier : BusinessObjectCollectionCopier
	{
		public AccChargeTypeOverrideCopier()
			: base(
				new[] {
					AccChargeTypeOverrideSchema.AN_ChargeType,
					AccChargeTypeOverrideSchema.AN_InvoiceType,
					AccChargeTypeOverrideSchema.AN_JobType,
					AccChargeTypeOverrideSchema.AN_JobDirection },
				new[] {
					AccChargeTypeOverrideSchema.AN_MarginPercentage })
		{
		}
		static readonly Lazy<AccChargeTypeOverrideCopier> instance = new Lazy<AccChargeTypeOverrideCopier>();
		public static AccChargeTypeOverrideCopier Instance { get { return instance.Value; } }
	}
}
