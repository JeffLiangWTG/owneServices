using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public void ValidateTR_GM_PresentationCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.TR_GM_PresentationCustomsOfficeInfo);
		}

		protected void CheckTR_GM_PresentationCustomsOffice()
		{
			if (!TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Parent.AMA_TransportMode, Parent.AMA_ManifestType))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TR_GM_PresentationCustomsOfficeInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransportType();
			ValidateTR_GM_PresentationCustomsOffice();
			ValidateTIRNumber();
			ValidateManifestToOpenDuplicates();
		}

		public void ValidateTransportType()
		{
			ValidateCalculatedProperty(Parent.TransportTypeInfo);
		}

		protected void CheckTransportType()
		{
			if (!Parent.IsOnlyForSeaAndGrupaj)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TransportTypeInfo);
			}
		}

		protected override void CheckAMA_VoyageCore()
		{
			if (ManifestTypesRequiringVoyageAndConveyance(Parent.AMA_ManifestType))
			{
				base.CheckAMA_VoyageCore();
			}
		}

		protected override void CheckAMA_RN_NKConveyanceNationalityCore()
		{
			var header = Parent;
			if ((header.IsSea || header.IsAir) && header.AMA_ManifestType != TRManifestTypes.Codes.GRUPAJ)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
			}
		}

		protected override void CheckAMA_VesselName()
		{
			base.CheckAMA_VesselName();
			var header = Parent;

			if (header != null && TRManifestTypes.IsManifestTypesRelatedToSea(header.AMA_TransportMode, header.AMA_ManifestType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(header.AMA_VesselNameInfo);
				ListValidation.WarnIfInvalidCode(Parent.AMA_VesselNameInfo, ResString.GetMultilingualString("73AAF9AD-ACE5-43DD-99C9-05EEB53FEB22", "The Vessel Name entered does not exist in the reference file."));
			}
			else if (header != null && header.IsAir && header.AMA_ManifestType == TRManifestTypes.Codes.HAVITH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(header.AMA_VesselNameInfo);
			}
		}

		protected override void CheckAMA_LloydsNumber()
		{
			base.CheckAMA_LloydsNumber();
			var header = Parent;
			if (header != null && (TRManifestTypes.IsManifestTypesRelatedToSea(header.AMA_TransportMode, header.AMA_ManifestType) || TRManifestTypes.IsManifestTypesRelatedToAir(header.AMA_TransportMode, header.AMA_ManifestType)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(header.AMA_LloydsNumberInfo);
			}
		}

		protected override void CheckAMA_DateAtCustomsOffice()
		{
			base.CheckAMA_DateAtCustomsOffice();
			var header = Parent;
			if (header != null && TRManifestTypes.IsManifestTypesRelatedToSea(header.AMA_TransportMode, header.AMA_ManifestType) && !TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(Parent.AMA_TransportMode, Parent.AMA_ManifestType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(header.AMA_DateAtCustomsOfficeInfo, header.DateCustomsOfficeLabel.Caption);
			}
		}

		internal bool ManifestTypesRequiringVoyageAndConveyance(ZString manifestType)
		{
			return manifestType == TRManifestTypes.Codes.CIKONC
				|| manifestType == TRManifestTypes.Codes.DENIHR
				|| manifestType == TRManifestTypes.Codes.DENITH
				|| manifestType == TRManifestTypes.Codes.HAVIHR
				|| manifestType == TRManifestTypes.Codes.HAVITH
				|| manifestType == TRManifestTypes.Codes.VARONC
				|| manifestType == TRManifestTypes.Codes.EMANIF;
		}

		public void ValidateTIRNumber()
		{
			ValidateCalculatedProperty(Parent.TIRNumberInfo);
		}

		protected void CheckTIRNumber()
		{
			if (!Parent.IsShippingLine)
			{
				if (Parent.AMA_ManifestType == TRManifestTypes.Codes.ATAIHR || Parent.AMA_ManifestType == TRManifestTypes.Codes.ATAITH)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.TIRNumberInfo);
				}
			}
		}

		readonly ZString sameRegistrationNoErrorMessage = ResString.GetMultilingualString("2AAF234C-D97E-472A-84C2-EBD92A9656D1", "Duplicate registration number");
		readonly ZString sameBillNoErrorMessage = ResString.GetMultilingualString("E5F4BECE-8D55-4EA0-AD3A-A0F9B2EA4549", "Duplicate bill number");
		readonly ZString sameBillLineNoErrorMessage = ResString.GetMultilingualString("F3D26F47-107C-4047-BDBE-FC96F9F8D994", "Duplicate bill line number");

		public void ValidateManifestToOpenDuplicates()
		{
			var items = Parent.ManifestsToOpenList.Cast<ManifestToOpen>().ToList();
			foreach (var m2o in items)
			{
				m2o.RemoveRowMessageError(sameRegistrationNoErrorMessage);
				m2o.RemoveRowMessageError(sameBillNoErrorMessage);
				m2o.RemoveRowMessageError(sameBillLineNoErrorMessage);

				if (items.Any(x => x.PK != m2o.PK && x.CSI_SubType == m2o.CSI_SubType && x.CSI_ReferenceNumber2 == m2o.CSI_ReferenceNumber2 && x.CSI_ReferenceNumber == m2o.CSI_ReferenceNumber && x.CSI_LineNo == m2o.CSI_LineNo))
				{
					if (m2o.CSI_SubType == SubTypeListForManifestToOpen.Codes.Manifestlevel)
					{
						m2o.AddRowMessageError(sameRegistrationNoErrorMessage);
					}
					else if (m2o.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlevel)
					{
						m2o.AddRowMessageError(sameBillNoErrorMessage);
					}
					else if (m2o.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlinelevel)
					{
						m2o.AddRowMessageError(sameBillLineNoErrorMessage);
					}
				}
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			if (Parent.AMA_ManifestType != TRManifestTypes.Codes.GRUPAJ)
			{
				base.CheckAMA_OA_CarrierMandatory();
			}
		}

		protected override void CheckAMA_MasterBill()
		{
			base.CheckAMA_MasterBill();
			var parent = Parent;

			if (parent.IsImport && (parent.IsAir || parent.IsSea) && parent.AMA_ManifestType != TRManifestTypes.Codes.VARONC && parent.AMA_ManifestType != TRManifestTypes.Codes.GRUPAJ)
			{
				var otherHeaderWithTheSameMasterBill = AsycudaHelper.LoadManifestHeadersForMasterBill(parent.Factory, parent.AMA_MasterBill, parent.AMA_TransportMode, parent.PK, parent.AMA_ApplicationCode);
				if (otherHeaderWithTheSameMasterBill != null)
				{
					var masterBillInfo = parent.AMA_MasterBillInfo;
					if (parent.IsAir)
					{
						masterBillInfo.AddError(Res.GetString("7A572656-C01F-46D8-BFB5-0A61BFF4DBB3", "There is another Global Manifest which is registered with this airway bill number."));
					}
					else if (parent.IsSea)
					{
						masterBillInfo.AddMessageError(Res.GetString("E89747A1-E3EC-448D-AF86-F8A15C7FFE5D", "There is another Global Manifest which is registered with this BOL number. Please check it to prevent an error."));
					}
				}
			}
		}
	}
}
