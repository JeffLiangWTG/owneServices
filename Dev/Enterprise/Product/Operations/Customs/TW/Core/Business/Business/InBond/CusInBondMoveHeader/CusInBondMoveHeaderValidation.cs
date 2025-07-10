using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveHeaderValidation : Customs.Business.CusInBondMoveHeaderValidation
	{
		public CusInBondMoveHeaderValidation(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveHeader Parent => (CusInBondMoveHeader)base.Parent;

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_InBondEntryTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_InBondEntryTypeInfo, Parent.Lookups.EntryTypeList);
		}

		protected override void CheckBM_ExportTransportMode()
		{
			base.CheckBM_ExportTransportMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_ExportTransportModeInfo, Parent.Lookups.TranshipmentTransportCodeList);
		}

		protected override void CheckBM_PlaceOfLoading()
		{
			base.CheckBM_PlaceOfLoading();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_PlaceOfLoadingInfo, Parent.Lookups.FacilityCollection);
		}

		protected override void CheckBM_ForeignDestPortKCode()
		{
			base.CheckBM_ForeignDestPortKCode();
			var targetInfo = Parent.BM_ForeignDestPortKCodeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.FacilityCollection);
			CheckBothBM_ForeignDestPortKCodeAndBM_RL_NKForeignDestPort(targetInfo);
		}

		protected override void CheckBM_RL_NKForeignDestPort()
		{
			base.CheckBM_RL_NKForeignDestPort();
			var targetInfo = Parent.BM_RL_NKForeignDestPortInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.DestinationPorts);
			CheckBothBM_ForeignDestPortKCodeAndBM_RL_NKForeignDestPort(targetInfo);
		}

		void CheckBothBM_ForeignDestPortKCodeAndBM_RL_NKForeignDestPort(ZPropertyInfo info)
		{
			if (Parent.BM_ForeignDestPortKCode.IsEmpty && Parent.BM_RL_NKForeignDestPort.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
			else if (!Parent.BM_ForeignDestPortKCode.IsEmpty && !Parent.BM_RL_NKForeignDestPort.IsEmpty)
			{
				info.AddMessageError(ValidationConstants.CusInBondMoveHeader.DestinationAndDestinationUnBothNotEmpty);
			}
		}

		protected override void CheckBM_TransportAtDeparture()
		{
			var vesselREG = Parent.BM_TransportAtDeparture;
			var targetInfo = Parent.BM_TransportAtDepartureInfo;
			if (!vesselREG.IsEmpty)
			{
				if (vesselREG.Length != 6)
				{
					targetInfo.AddMessageError(ValidationConstants.CusInBondMoveHeader.TW_ExportVesselREGLength);
				}

				if (!vesselREG.IsLettersAndNumbersOnlyOrEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.CusInBondMoveHeader.OnlyLettersAndNumbersForExportVesselREG);
				}

				EnglishCharactersValidation.ErrorIfNotWesternEuropean(targetInfo);
			}
		}
	}
}
