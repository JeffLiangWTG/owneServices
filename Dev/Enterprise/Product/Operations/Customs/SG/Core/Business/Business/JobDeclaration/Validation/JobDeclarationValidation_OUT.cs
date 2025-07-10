using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationValidation_OUT : CUSDECValidation
	{
		public JobDeclarationValidation_OUT(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override ICodeDescriptionPairList MessageSubTypeList
		{
			get { return Parent.Lookups.OUTMessageSubTypeList; }
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (!Parent.JE_TransportMode.IsEmpty && Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.DRT)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_TransportModeInfo, "Inward Transport Mode.\r\nFor Out declaration, Inward Transport is only required where declaration Type = DRT (when there is inter-gateway movement)");
			}
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			Parent.AddInfoValidation.ValidateSG_IsSeaStore();
		}

		protected override void CheckJE_OH_Forwarder()
		{
			CheckForwarder();
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			if (Parent.JE_OH_Consignee.IsEmpty)
			{
				var placeOfStorage = Parent.PlaceOfStorage;
				var placeOfRelease = Parent.PlaceOfRelease;
				if (!Parent.IsSeaStore && placeOfStorage == null)
				{
					if (Parent.AddInfoValidation.IsStrategic)
					{
						Parent.JE_OH_ConsigneeInfo.AddMessageError("Consignee is required when the goods being declared are strategic, unless the declaration is for sea stores");
					}
					else if (placeOfRelease.IsLicencedPremise())
					{
						Parent.JE_OH_ConsigneeInfo.AddMessageError("Consignee is required when the goods being released from licensed premises, unless the declaration is for sea stores");
					}
				}
			}
		}

		protected override void CheckJE_OH_Exporter()
		{
			base.CheckJE_OH_Exporter();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ExporterInfo, "Exporter");
		}

		protected override bool IsInwardCarrierAgentMandatory => Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.DRT && TransportModeCodeList.TransportIsSeaOrAir(Parent.JE_TransportMode);
		protected override bool IsOutwardShippingLineForwarderMandatory => TransportModeCodeList.TransportIsSeaOrAir(Parent.SG_OutwardTransportMode);
	}
}
