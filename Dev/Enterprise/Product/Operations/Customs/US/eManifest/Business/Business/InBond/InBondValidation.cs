using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class InBondValidation : Customs.Business.CusInBondMoveHeaderValidation
	{
		public InBondValidation(InBond parent)
			: base(parent)
		{
		}

		new InBond Parent
		{
			get { return (InBond)base.Parent; }
		}

		#region CheckBM_DestinationPortDCode

		protected override void CheckBM_DestinationPortCode()
		{
			base.CheckBM_DestinationPortCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_DestinationPortCodeInfo);
		}

		#endregion

		#region CheckBM_ExportDate

		protected override void CheckBM_ExportDate()
		{
			base.CheckBM_ExportDate();
			if (Parent.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ExportDateInfo);
			}
		}

		#endregion

		#region CheckBM_ForeignDestPortKCode

		protected override void CheckBM_ForeignDestPortKCode()
		{
			base.CheckBM_ForeignDestPortKCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_ForeignDestPortKCodeInfo);
			if (Parent.IsExport && !Parent.BM_ForeignDestPortKCodeInfo.HasNotifications())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ForeignDestPortKCodeInfo);
			}
		}

		#endregion

		#region CheckBM_InBondCarrierID

		protected override void CheckBM_InBondCarrierID()
		{
			base.CheckBM_InBondCarrierID();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_InBondCarrierIDInfo);
		}

		#endregion

		#region CheckBM_InBondEntryType

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_InBondEntryTypeInfo);
		}

		#endregion

		#region CheckBM_PedimentoNumber

		protected override void CheckBM_PedimentoNumber()
		{
			base.CheckBM_PedimentoNumber();
			const string mexico = "201";
			if (Parent.IsExport && Parent.BM_ForeignDestPortKCode.StartsWith(mexico))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_PedimentoNumberInfo);
			}

			if (!Parent.BM_PedimentoNumber.IsEmpty && !PedimentoNumberValidator.IsValidPedimentoNumber(Parent.BM_PedimentoNumber))
			{
				Parent.BM_PedimentoNumberInfo.AddMessageError(PedimentoNumberValidator.PedimentoNumberRightFormat);
			}
		}

		#endregion

		#region CheckBM_RL_NKDestinationPort

		protected override void CheckBM_RL_NKDestinationPort()
		{
			base.CheckBM_RL_NKDestinationPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_RL_NKDestinationPortInfo);
		}

		#endregion

		#region CheckBM_RL_NKForeignDestPort

		protected override void CheckBM_RL_NKForeignDestPort()
		{
			base.CheckBM_RL_NKForeignDestPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_RL_NKForeignDestPortInfo);
		}

		#endregion

		#region CheckBM_OnwardCarrier

		protected override void CheckBM_OnwardCarrier()
		{
			base.CheckBM_OnwardCarrier();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_OnwardCarrierInfo);
		}

		#endregion
	}
}
