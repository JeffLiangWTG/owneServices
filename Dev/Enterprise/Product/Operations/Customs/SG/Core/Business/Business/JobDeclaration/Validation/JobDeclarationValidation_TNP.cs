using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationValidation_TNP : CUSDECValidation
	{
		public JobDeclarationValidation_TNP(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override ICodeDescriptionPairList MessageSubTypeList
		{
			get { return Parent.Lookups.TNPMessageSubTypeList; }
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.REM && Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BRE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportModeInfo, "Inward Transport Mode");
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			if (Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.REM
				|| Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.BRE
				|| Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.IGM)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo, "Importer");
			}
		}

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			if ((!Parent.JE_HouseBill.IsEmpty || !Parent.SG_OutwardHAWB.IsEmpty) && (Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.TTF) && (Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.TTI))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ForwarderInfo, "Forwarder");
			}
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			if (Parent.JE_OH_Consignee.IsEmpty)
			{
				var forStorageorSeastores = Parent.PlaceOfStorage != null || Parent.IsSeaStore;
				if (!forStorageorSeastores)
				{
					if (Parent.AddInfoValidation.IsStrategic)
					{
						Parent.JE_OH_ConsigneeInfo.AddMessageError("Consignee is required when the goods being declared are strategic, unless the goods are sea stores or the goods are meant for storage");
					}
					else if (Parent.JE_MessageSubType == DeclarationTypeCodeList.Codes.IGM)
					{
						Parent.JE_OH_ConsigneeInfo.AddMessageError("Consignee is required for IGM declaration, unless the goods are sea stores or the goods are meant for storage");
					}
				}
			}
		}

		protected override bool IsInwardCarrierAgentMandatory => Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.REM && Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.BRE
																&& TransportModeCodeList.TransportIsSeaOrAir(Parent.JE_TransportMode);

		protected override bool IsOutwardShippingLineForwarderMandatory => Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.REM && Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.BRE
																		&& TransportModeCodeList.TransportIsSeaOrAir(Parent.SG_OutwardTransportMode) && !(Parent.PlaceOfStorage?.IsFTZ() ?? false);
	}
}
