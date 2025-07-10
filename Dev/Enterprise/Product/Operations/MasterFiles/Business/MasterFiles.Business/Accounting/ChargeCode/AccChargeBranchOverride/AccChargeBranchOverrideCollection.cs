using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeBranchOverrideCollection : ActiveBusinessObjectCollection<AccChargeBranchOverride>
	{
		public AccChargeBranchOverrideCollection(AccChargeCode master) : base(master.Factory)
		{
			Master = master;
		}

		readonly AccChargeCode Master;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();

			filter.AddToFilter(AccChargeBranchOverrideSchema.YA_AC_ChargeCode, Master.PK);

			return filter;
		}

		protected override void SetRelationshipDefaultsForElementCore(AccChargeBranchOverride newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);

			newElement.YA_AC_ChargeCode = Master.PK;
		}

		public AccChargeBranchOverride GetChargeBranchOverride(JobInvoicingConsumerType jobType, ZString direction, ZString transportMode)
		{
			if (jobType == null || direction.IsEmpty)
			{
				return null;
			}

			ColumnValueRanker ranker = new ColumnValueRanker();
			ranker.Add(AccChargeBranchOverrideSchema.YA_JobType, (ZString)(jobType != null ? jobType.Code : ""), (ZString)AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All);
			ranker.Add(AccChargeBranchOverrideSchema.YA_Direction, direction, (ZString)AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All, ZString.Empty);
			ranker.Add(AccChargeBranchOverrideSchema.YA_TransportMode, transportMode, (ZString)Constants.FreightShipmentDirection.Code.All, ZString.Empty);

			return ranker.GetBestMatch<AccChargeBranchOverride>(Factory, CreateRelationshipFilter());
		}
	}
}
