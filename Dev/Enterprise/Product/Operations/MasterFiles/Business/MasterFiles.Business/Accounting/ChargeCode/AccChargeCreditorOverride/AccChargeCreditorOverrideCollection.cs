using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCreditorOverrideCollection : ActiveBusinessObjectCollection<AccChargeCreditorOverride>
	{
		public AccChargeCreditorOverrideCollection(AccChargeCode master) : base(master.Factory)
		{
			Master = master;
		}

		readonly AccChargeCode Master;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();

			filter.AddToFilter(AccChargeCreditorOverrideSchema.ACC_AC_ChargeCode, Master.PK);

			return filter;
		}

		protected override void SetRelationshipDefaultsForElementCore(AccChargeCreditorOverride newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);

			newElement.ACC_AC_ChargeCode = Master.PK;
		}

		public ZGuid? GetCreditorOverride(IJobInvoicingSupporter jobInvoicingSupporter, CostSell costSell, JobInvoicingConsumerType jobType, string direction, string transportMode, ZGuid? department, bool overseasAgentIsApplicable, ZGuid? overseasCreditor)
		{
			if (jobType != null && jobInvoicingSupporter != null)
			{
				var paymentTerm = jobInvoicingSupporter.GetPaymentTermForCreditorOverride(costSell);
				return GetCreditorOverride(jobType, direction, transportMode, paymentTerm, department, overseasAgentIsApplicable, overseasCreditor);
			}

			return null;
		}

		public ZGuid? GetCreditorOverride(IJobInvoicingSupporter jobInvoicingSupporter, CostSell costSell, JobInvoicingConsumerType jobType, Directions? direction, string transportMode, ZGuid? department, bool overseasAgentIsApplicable, ZGuid? overseasCreditor)
			=> GetCreditorOverride(jobInvoicingSupporter, costSell, jobType, GetDirectionString(direction), transportMode, department, overseasAgentIsApplicable, overseasCreditor);

		public ZGuid? GetCreditorOverride(JobInvoicingConsumerType jobType, string direction, string transportMode, string paymentTerm, ZGuid? department, bool overseasAgentIsApplicable, ZGuid? overseasCreditor)
		{
			var creditorOverride = GetCreditorOverride(jobType, direction, transportMode, paymentTerm, department);
			if (creditorOverride != null)
			{
				if (!creditorOverride.ACC_OH_Creditor.IsEmpty)
				{
					return creditorOverride.ACC_OH_Creditor;
				}
				else if (AccChargeCreditorOverride.IsJobTypeShipmentOrConsol(jobType.Code)
					&& creditorOverride.ACC_CreditorRole == DocAddressTypes.Codes.OverseasAgent
					&& overseasAgentIsApplicable)
				{
					return overseasCreditor;
				}
			}

			return null;
		}

		public ZGuid? GetCreditorOverride(JobInvoicingConsumerType jobType, Directions? direction, string transportMode, string paymentTerm, ZGuid? department, bool overseasAgentIsApplicable, ZGuid? overseasCreditor)
			=> GetCreditorOverride(jobType, GetDirectionString(direction), transportMode, paymentTerm, department, overseasAgentIsApplicable, overseasCreditor);

		public AccChargeCreditorOverride GetCreditorOverride(JobInvoicingConsumerType jobType, string direction, string transportMode, string paymentTerm = null, ZGuid? department = null)
		{
			if (jobType == null || string.IsNullOrEmpty(direction))
			{
				return null;
			}
			ColumnValueRanker ranker = new ColumnValueRanker();
			ranker.Add(AccChargeCreditorOverrideSchema.ACC_JobType, (ZString)jobType.Code, (ZString)AccChargeCreditorOverrideLookups.JobTypeAdditionalCodes.All);
			ranker.Add(AccChargeCreditorOverrideSchema.ACC_Direction, (ZString)direction, (ZString)AccChargeCreditorOverrideLookups.TransportModeAdditionalCodes.All, ZString.Empty);
			ranker.Add(AccChargeCreditorOverrideSchema.ACC_TransportMode, (ZString)transportMode, (ZString)Core.Constants.FreightShipmentDirection.Code.All, ZString.Empty);
			ranker.Add(AccChargeCreditorOverrideSchema.ACC_PaymentTerm, (ZString)paymentTerm, (ZString)AccChargeCreditorOverrideLookups.PaymentTermAdditionalCodes.All, ZString.Empty);
			ranker.Add(AccChargeCreditorOverrideSchema.ACC_GE_Department, department ?? ZGuid.Empty, ZGuid.Empty);

			return ranker.GetBestMatch<AccChargeCreditorOverride>(Factory, CreateRelationshipFilter());
		}

		ZString GetDirectionString(Directions? directionEnum)
		{
			switch (directionEnum)
			{
				case Directions.Import:
					return Core.Constants.FreightShipmentDirection.Code.Import;
				case Directions.Export:
					return Core.Constants.FreightShipmentDirection.Code.Export;
				case Directions.Domestic:
					return Core.Constants.FreightShipmentDirection.Code.Domestic;
				default:
					return Core.Constants.FreightShipmentDirection.Code.Other;
			}
		}

		#region BusinessObjectCollectionSnapShotter

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		BusinessObjectCollectionSnapshotManager businessObjectCollectionSnapShotter;

		public BusinessObjectCollectionSnapshotManager SnapShotter
		{
			get
			{
				if (businessObjectCollectionSnapShotter == null)
				{
					businessObjectCollectionSnapShotter = new BusinessObjectCollectionSnapshotManager(new AccChargeCreditorOverrideCopier(), this);
				}
				return businessObjectCollectionSnapShotter;
			}
		}

		#endregion

		public class AccChargeCreditorOverrideCopier : BusinessObjectCollectionCopier
		{
			public AccChargeCreditorOverrideCopier()
				: base(
					new CargoWise.Schema.SchemaColumn[] {
						AccChargeCreditorOverrideSchema.ACC_JobType,
						AccChargeCreditorOverrideSchema.ACC_Direction,
						AccChargeCreditorOverrideSchema.ACC_TransportMode,
						AccChargeCreditorOverrideSchema.ACC_PaymentTerm,
						AccChargeCreditorOverrideSchema.ACC_GE_Department,
						AccChargeCreditorOverrideSchema.ACC_DefaultingRule,
					},
					new CargoWise.Schema.SchemaColumn[] {
						AccChargeCreditorOverrideSchema.ACC_CreditorRole,
						AccChargeCreditorOverrideSchema.ACC_OH_Creditor
					})
			{
			}
		}
	}
}
