using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;

namespace Enterprise.ContractManagement.Business
{
	public class RatingContractContainerDetention : AutoRatingContractContainerDetention, IRatingContractContainerDetention, IContainerPenaltyMatchResult
	{
		public RatingContractContainerDetention(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public RatingContract Contract => Factory.Load<RatingContract>(RCD_RCT);

		[RelatedBusinessObject(nameof(Contract))]
		public override ZGuid RCD_RCT { get => base.RCD_RCT; set => base.RCD_RCT = value; }

		#region IContainerPenaltyMatchResult

		public ZByte FreeDays => this.RCD_FreeDays;

		public ZString FreeDayType => this.RCD_FreeDayType;

		public ZString PenaltyType => this.RCD_PenaltyType;

		public ZString CreditorType => Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;

		IContainerPenaltyDayExclusion IContainerPenaltyMatchResult.DurationExclusion => DurationExclusion;

		IContainerPenaltyDayExclusion IContainerPenaltyMatchResult.FreeDayExclusion => FreeDayExclusion;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var contract = Factory.NewWithValidTestData<RatingContract>();
			RCD_RCT = contract.PK;
			RCD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			RCD_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			RCD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.CTOAvailable;
		}

#endif
	}
}
