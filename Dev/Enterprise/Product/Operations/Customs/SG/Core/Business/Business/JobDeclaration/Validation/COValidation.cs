using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class COValidation : JobDeclarationValidation
	{
		public COValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (!Parent.SG_OutwardTransportMode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ExportDateInfo, "Date of Departure");
			}
		}

		protected override void CheckJE_OH_Manufacturer()
		{
			base.CheckJE_OH_Manufacturer();
			if (SGCertificatesCodeList.IsManufacturerRequired(Parent.Certificate1Type) || SGCertificatesCodeList.IsManufacturerRequired(Parent.Certificate2Type))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ManufacturerInfo, "Manufacturer");
			}

			if (!Parent.JE_OH_Manufacturer.IsEmpty)
			{
				var manufacturer = Parent.Factory.Load<OrgHeader>(Parent.JE_OH_Manufacturer);
				if (manufacturer != null)
				{
					if (!ValidationHelper.HasUEN(manufacturer))
					{
						Parent.JE_OH_ManufacturerInfo.AddMessageError("Manufacturer " + ValidationHelper.UENDoesNotExist);
					}
				}
			}
		}

		protected override void CheckJE_OH_Exporter()
		{
			base.CheckJE_OH_Exporter();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ExporterInfo, "Exporter");
		}

		protected override bool IsInwardCarrierAgentMandatory => TransportModeCodeList.TransportIsSeaOrAir(Parent.JE_TransportMode);
		protected override bool IsOutwardShippingLineForwarderMandatory => TransportModeCodeList.TransportIsSeaOrAir(Parent.SG_OutwardTransportMode);
	}
}
