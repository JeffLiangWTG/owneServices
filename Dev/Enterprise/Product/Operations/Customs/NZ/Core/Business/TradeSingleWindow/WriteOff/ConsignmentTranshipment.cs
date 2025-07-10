using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class ConsignmentTranshipment : Consignment, IMovementStatus
	{
		internal ConsignmentTranshipment(WriteOffResponseTranshipmentRequest response, string id, string status, ZString movementStatus)
			: base(response, id, status, movementStatus)
		{
			this.response = Argument.NotNull(response, nameof(response));
			this.movementStatus = movementStatus;
			underbondMovement = response.TranshipmentRequest;
		}

		readonly WriteOffResponseTranshipmentRequest response;
		readonly ZString movementStatus;
		readonly TranshipmentRequest underbondMovement;

		protected override ZString CustomsStatusCore
		{
			get { return underbondMovement?.C4_Status ?? ZString.Empty; }
			set
			{
				if (!movementStatus.IsEmpty)
				{
					UpdateTranshipmentMovementStatusIfRequired(underbondMovement, response.ResponsibleGovernmentAgency, movementStatus);
				}
			}
		}

		protected override string JobNumberCore => ZString.Empty;

		public override string MovementStatusDescription
		{
			get
			{
				string result = string.Empty;
				if (!movementStatus.IsEmpty)
				{
					result = movementStatus + "-" + response.LowValueConsignmentStatusList.GetDescriptionFromCode(movementStatus);
				}
				return result;
			}
		}

		#region Write-Off Status

		TSWStatus TSWStatus => new TSWStatus(this);

		protected override ZString MessageStatusCore
		{
			get => underbondMovement?.C4_MessageStatus ?? ZString.Empty;
			set
			{
				if (underbondMovement != null)
				{
					underbondMovement.C4_MessageStatus = value;
				}
			}
		}

		protected override ZString HouseBillCore => ZString.Empty;

		#region IMovementStatus Members

		TranshipmentRequest IMovementStatus.UnderbondMovement => underbondMovement;

		ZString IMovementStatus.CustomsStatus => underbondMovement?.C4_Status ?? ZString.Empty;

		ZString IMovementStatus.GoodsClearanceStatus => Status;

		ZString IMovementStatus.CombinedStatus => underbondMovement?.C4_Status ?? ZString.Empty;

		ZString IMovementStatus.Agency => Response.ResponsibleGovernmentAgency;

		#endregion

		void UpdateTranshipmentMovementStatusIfRequired(TranshipmentRequest underbondMovement, string agency, ZString movementStatus)
		{
			if (underbondMovement != null)
			{
				if (underbondMovement.C4_Status.Length > 2)
				{
					underbondMovement.C4_Status = ZString.Empty;
				}

				if (agency == ResponsibleGovernmentAgencyList.Codes.TSW)
				{
					underbondMovement.C4_Status = EnterpriseStatus;
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO)
				{
					var customsStatus = underbondMovement.C4_Status.SubstringSafe(0, 1);
					underbondMovement.C4_Status = TSWStatus.CalculateCombinedMovementStatus(customsStatus, movementStatus);
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.NZCS)
				{
					var bioStatus = underbondMovement.C4_Status.SubstringSafe(1, 1);
					underbondMovement.C4_Status = TSWStatus.CalculateCombinedMovementStatus(movementStatus, bioStatus);
				}
			}
		}

		#endregion
	}
}

