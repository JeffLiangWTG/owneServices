using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using static System.FormattableString;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMasterCarrierCode();
			ValidatePlaceOfExit();
			ValidateEstimatedTimeOfLoading();
			ValidatePlaceOfEntry();
			ValidateTSS_Vessel();
			ValidateTSS_VoyageFlight();
			ValidateTSS_RadioCallSign();
			ValidateTSS_CargoCarrierPK();
			ValidateTSS_DateOfDeparture();
		}

		public void ValidateMasterCarrierCode()
		{
			ValidateCalculatedProperty(Parent.MasterCarrierCodeInfo);
		}

		protected void CheckMasterCarrierCode()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.MasterCarrierCodeInfo);
			if (!Parent.MasterCarrierCode.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.MasterCarrierCodeInfo.AddMessageError("Master Carrier Code is alpha numeric only.");
			}

			var masterCarrierCode = Parent.MasterCarrierCode;
			if (!masterCarrierCode.IsEmpty)
			{
				if (Parent.IsSea)
				{
					var zzCarrier = Parent.GetMasterZZCarrier(masterCarrierCode, false);
					if (Parent.AMA_ManifestType.Equals(nameof(ManifestDocumentType.COH)) && Parent.AMA_AgentType.Equals(Core.Constants.AgentType.CoLoad))
					{
						if (masterCarrierCode.Length != 8)
						{
							Parent.MasterCarrierCodeInfo.AddWarning("Sub-Master Carrier Code for COH Manifest, should have 8 characters.");
						}
						if (zzCarrier == null)
						{
							Parent.MasterCarrierCodeInfo.AddMessageError("Sub-Master Carrier Code is not in the list of known ZA cargo carrier codes.");
						}
					}
					else
					{
						if (zzCarrier == null)
						{
							Parent.MasterCarrierCodeInfo.AddMessageError("Master Carrier Code is not in the list of known ZA carrier codes.");
						}
					}
				}
			}
		}

		protected override void CheckAMA_RadioCallSign()
		{
			base.CheckAMA_RadioCallSign();
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RadioCallSignInfo);
			}
		}

		protected override ZString CarrierType => Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER;

		protected override bool ShouldValidateCCCCodeOfCarrier => Parent.AMA_CarrierCode.IsEmpty;

		protected override bool ShouldValidateRadioCallSignOfVessel => Parent.AMA_RadioCallSign.IsEmpty;

		protected override bool ShouldValidateZZCarrierLinkedToZZVessel => false;

		protected override ZString GetMasterBOLHumanReadable(ZString agentType)
		{
			return agentType == Core.Constants.AgentType.CoLoad
				? "Parent Bill"
				: "Master Bill";
		}

		protected override void CheckAMA_MasterBill()
		{
			base.CheckAMA_MasterBill();
			var header = Parent;
			if (header != null)
			{
				var manifestType = header.AMA_ManifestType;
				if ((Parent.IsAir && manifestType == nameof(ManifestDocumentType.FWB)) || (Parent.IsSea && manifestType == nameof(ManifestDocumentType.COM)))
				{
					var manifestNumberlabel = Parent.MasterBillLabel.Caption;
					if (Parent.AMA_MasterBill.IsEmpty)
					{
						Parent.AMA_MasterBillInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.ManifestNumberIsRequired(manifestNumberlabel));
					}
					else
					{
						var bills = Parent.Bills;
						var billCount = bills.Count;
						if (billCount == 0)
						{
							Parent.AMA_MasterBillInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.BillIsRequiredForManifestType(manifestNumberlabel, manifestType));
						}
						else if (billCount > 1)
						{
							Parent.AMA_MasterBillInfo.AddMessageError(ASYCUDA.Business.ValidationConstants.OnlyOneBillIsAllowed(manifestType));
						}
					}
				}
				if (manifestType == nameof(ManifestDocumentType.ALH)
				|| manifestType == nameof(ManifestDocumentType.COH)
				|| manifestType == nameof(ManifestDocumentType.HAB))
				{
					var manifestNumber = Parent.AMA_MasterBill;
					if (Parent.Bills.Cast<AsycudaBill>().Any(x => x.ABL_BillNumber == manifestNumber))
					{
						Parent.AMA_MasterBillInfo.AddWarning("Master Transport Document Number cannot be the same as a Bill Number when the Manifest Type is ALH, COH or HAB.");
					}
				}
			}
		}

		protected override void CheckAMA_TransportMode()
		{
			base.CheckAMA_TransportMode();
			if (Parent.Lookups.ManifestTypes.Count == 0)
			{
				Parent.AMA_TransportModeInfo.AddError("This transport mode is not supported for ZA.");
			}
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_OA_DeconsolidateAddress()
		{
			base.CheckAMA_OA_DeconsolidateAddress();
			var manifestType = Parent.AMA_ManifestType;
			var nature = Parent.AMA_Nature;
			var containers = Parent.Containers.Cast<AsycudaContainer>().ToArray();
			var fullContainerLoadPresent = containers.Any(x => x.ACN_EmptyFullIndicator == "FCL");
			if (IsDepotOfUnpackAndTerminalOfDischargeMandatory(nature)
				&& (manifestType == nameof(ManifestDocumentType.ALH)
					|| manifestType == nameof(ManifestDocumentType.COH)
					|| manifestType == nameof(ManifestDocumentType.HAB)))
			{
				if (Parent.AMA_OA_DeconsolidateAddress.IsEmpty && Parent.IsSea && Parent.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised && !fullContainerLoadPresent)
				{
					Parent.AMA_OA_DeconsolidateAddressInfo.AddMessageError(Invariant($"For Import Manifests of Type '{manifestType}' Deconsolidation address is Mandatory."));
				}
				else if (Parent.AMA_OA_DeconsolidateAddress.IsEmpty)
				{
					Parent.AMA_OA_DeconsolidateAddressInfo.AddWarning(Invariant($"For Import Manifests of Type '{manifestType}' Deconsolidation address is Mandatory."));
				}
				else if (Parent.DepotCode.IsEmpty)
				{
					Parent.AMA_OA_DeconsolidateAddressInfo.AddMessageError("Organisation must have a code of type 'CPD' loaded.");
				}
			}
		}

		static ZBool IsDepotOfUnpackAndTerminalOfDischargeMandatory(ZString nature)
		{
			return nature == ShipmentTypeList.Codes.Import23
					|| nature == ShipmentTypeList.Codes.Transit24
					|| nature == ShipmentTypeList.Codes.Transhipment28;
		}

		protected override void CheckAMA_OA_DischargeTerminalAddress()
		{
			base.CheckAMA_OA_DischargeTerminalAddress();
			if (Parent.IsDischargeTerminalEnabled && !Parent.AMA_OA_DischargeTerminalAddress.IsEmpty && Parent.TerminalCode.IsEmpty)
			{
				Parent.AMA_OA_DischargeTerminalAddressInfo.AddMessageError("Organisation must have a code of type 'CPT' loaded.");
			}
		}

		protected override void CheckAMA_AgentType()
		{
			base.CheckAMA_AgentType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_AgentTypeInfo);

			var nature = Parent.AMA_Nature;
			var manifestType = Parent.AMA_ManifestType;
			if (nature == ShipmentTypeList.Codes.Import23
				|| nature == ShipmentTypeList.Codes.Transhipment28
				|| nature == ShipmentTypeList.Codes.Transit24)
			{
				if (manifestType == nameof(ManifestDocumentType.COH))
				{
					if (Parent.AMA_AgentType != Core.Constants.AgentType.CoLoad)
					{
						Parent.AMA_AgentTypeInfo.AddWarning(Res.GetString("E465E3BD-BCB5-4E3C-AC8D-D51DBD1B9D92", "For Import, Transit & Transhipment Manifests where the Manifest Type is COH, the Agent Type must be CLD."));
					}
				}
				else if (manifestType == nameof(ManifestDocumentType.ALM)
						|| manifestType == nameof(ManifestDocumentType.ALH)
						|| manifestType == nameof(ManifestDocumentType.BBB))
				{
					if (Parent.AMA_AgentType != Core.Constants.AgentType.Agent)
					{
						Parent.AMA_AgentTypeInfo.AddWarning(Res.GetString("EB9144C9-DDC4-46FF-8EE4-CEA4DB6BB341", "For Import, Transit & Transhipment Manifests where the Manifest Type is ALM, ALH or BBB the Agent Type must be AGT."));
					}
				}
			}
			else if (nature == ShipmentTypeList.Codes.Export22)
			{
				if (manifestType == nameof(ManifestDocumentType.COM)
					|| manifestType == nameof(ManifestDocumentType.BBB))
				{
					if (Parent.AMA_AgentType != Core.Constants.AgentType.Agent)
					{
						Parent.AMA_AgentTypeInfo.AddWarning(Res.GetString("FD1A84BB-F5ED-4BDC-95F1-F04889019EC6", "For Export Manifests where the Manifest Type is COM or BBB the Agent Type must be AGT."));
					}
				}
			}
		}

		protected override void CheckAMA_ManifestType()
		{
			base.CheckAMA_ManifestType();

			if (!Parent.IsAir)
			{
				var manifestType = Parent.AMA_ManifestType;
				if (manifestType == nameof(ManifestDocumentType.COH)
					|| manifestType == nameof(ManifestDocumentType.COM)
					|| manifestType == nameof(ManifestDocumentType.ECL)
					|| manifestType == nameof(ManifestDocumentType.ALM)
					|| manifestType == nameof(ManifestDocumentType.ALH))
				{
					if (Parent.Containers.Count == 0)
					{
						Parent.AMA_ManifestTypeInfo.AddMessageError("At least one container must be captured for Manifest Type " + manifestType);
					}
				}
			}
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();
			var nature = Parent.AMA_Nature;
			if (!nature.IsEmpty)
			{
				if (Parent.AMA_ManifestType == nameof(ManifestDocumentType.RFM) && nature == ShipmentTypeList.Codes.Transhipment28)
				{
					Parent.AMA_NatureInfo.AddMessageError("Manifest Nature cannot be TSS – Transhipment (28) for Road Manifests.");
				}
			}
		}

		protected override void CheckAMA_NatureCore()
		{
			var manifestType = Parent.AMA_ManifestType;
			if (manifestType == nameof(ManifestDocumentType.RFM))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
			}
			else
			{
				base.CheckAMA_NatureCore();
			}
		}

		protected override void CheckAMA_CarrierCode()
		{
			base.CheckAMA_CarrierCode();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CarrierCodeInfo);
			var carrierCode = Parent.AMA_CarrierCode;
			if (!carrierCode.IsEmpty)
			{
				var header = Parent;
				if (header.IsSea)
				{
					var zzCarrier = header.GetMasterZZCarrier(carrierCode, true);
					if (zzCarrier == null)
					{
						Parent.AMA_CarrierCodeInfo.AddMessageError("Carrier Code is not in the list of known ZA carrier codes.");
					}
				}
			}
		}
	}
}
