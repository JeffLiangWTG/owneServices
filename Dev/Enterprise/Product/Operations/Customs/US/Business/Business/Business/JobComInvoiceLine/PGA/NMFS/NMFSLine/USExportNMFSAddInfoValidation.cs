using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USExportNMFSAddInfoValidation : USNMFSLineAddInfoValidation
	{
		public USExportNMFSAddInfoValidation(USNMFSLineAddInfo parent)
			: base(parent)
		{
		}

		new USNMFSLineAddInfo Parent
		{
			get { return (USNMFSLineAddInfo)base.Parent; }
		}

		protected override void CheckUS_ProgramType()
		{
			base.CheckUS_ProgramType();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProgramTypeInfo, Parent.Lookups.NMFSPrograms);
			if (IsNMFSIndicatorDeclared && Parent.US_ProgramType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProgramTypeInfo);
			}

			ValidateUS_DocumentType();
			ValidateUS_IFTPPermitNumber();
			ValidateUS_ProcessingType();
			ValidateUS_DISDocumentID();
			ValidateUS_Quantity();
			ValidateUS_UnitOfMeasure();
			ValidateUS_CatchDocument();
			ValidateUS_ReExportNumber();

			var firstHarvestingDetail = Parent.Parent.HarvestingDetails.OfType<NMFSHarvestingDetail>().FirstOrDefault();
			if (firstHarvestingDetail != null)
			{
				firstHarvestingDetail.AddInfoValidation.ValidateUS_HarvestedCountry();
				firstHarvestingDetail.AddInfoValidation.ValidateUS_VesselCountry();
				firstHarvestingDetail.AddInfoValidation.ValidateUS_OceanAreaOfCatch();
			}
		}

		protected override void CheckUS_ProcessingType()
		{
			base.CheckUS_ProcessingType();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProcessingTypeInfo, Parent.Lookups.ProcessingTypeList);
			if (IsNMFSIndicatorDeclared && IsHMSProgram)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProcessingTypeInfo);
			}
		}

		protected override void CheckUS_DocumentType()
		{
			base.CheckUS_DocumentType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DocumentTypeInfo, Parent.Lookups.DocumentTypeList);

			if (IsNMFSIndicatorDeclared && (IsHMSProgram || ISAMLRProgram))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DocumentTypeInfo);
			}
		}

		protected override void CheckUS_DISDocumentID()
		{
			base.CheckUS_DISDocumentID();

			if (IsNMFSIndicatorDeclared)
			{
				if (Parent.US_EBCDNumber.IsEmpty && IsHMSProgram)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DISDocumentIDInfo);
				}
				else if (!Parent.US_EBCDNumber.IsEmpty && !Parent.US_DISDocumentID.IsEmpty)
				{
					Parent.US_DISDocumentIDInfo.AddMessageError(EitherDISDocumentIDOrEBCDRequired);
				}
			}

			ValidateUS_EBCDNumber();
		}

		protected override void CheckUS_EBCDNumber()
		{
			base.CheckUS_EBCDNumber();

			if (IsNMFSIndicatorDeclared)
			{
				if (!Parent.US_DISDocumentID.IsEmpty && !Parent.US_EBCDNumber.IsEmpty)
				{
					Parent.US_EBCDNumberInfo.AddMessageError(EitherDISDocumentIDOrEBCDRequired);
				}
			}

			ValidateUS_DISDocumentID();
		}
		internal const string EitherDISDocumentIDOrEBCDRequired = "Either DIS Document ID or eBCD Number is required. If both entered, only eBCD Number will be used in AES message.";

		protected override void CheckUS_IFTPPermitNumber()
		{
			base.CheckUS_IFTPPermitNumber();

			if (IsNMFSIndicatorDeclared && (IsHMSProgram || ISAMLRProgram))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IFTPPermitNumberInfo);
			}
		}

		protected override void CheckUS_Quantity()
		{
			base.CheckUS_Quantity();

			if (IsNMFSIndicatorDeclared && ISAMLRProgram)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_QuantityInfo);

				var quantityInKG = ZDecimal.Zero;
				if (Core.Constants.Weight.ContainsCode(Parent.US_UnitOfMeasure))
				{
					quantityInKG = ((ZDecimal)Core.Constants.Weight.Convert(Parent.US_Quantity, Parent.US_UnitOfMeasure, Core.Constants.Weight.Kilograms)).Round(0);
				}

				if (quantityInKG > 999999999999999m)
				{
					Parent.US_QuantityInfo.AddMessageError(QuantityInKGIsTooLarge);
				}
			}
		}
		internal const string QuantityInKGIsTooLarge = "The total weight value in KG should be less than 999,999,999,999,999. You can press F5 to run unit conversion. Please adjust value accordingly, otherwise space will be sent in AES message.";

		protected override void CheckUS_UnitOfMeasure()
		{
			base.CheckUS_UnitOfMeasure();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_UnitOfMeasureInfo, Parent.Lookups.WeightUQList);
			if (IsNMFSIndicatorDeclared && ISAMLRProgram)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UnitOfMeasureInfo);
			}
		}

		protected override void CheckUS_CatchDocument()
		{
			base.CheckUS_CatchDocument();

			if (IsNMFSIndicatorDeclared && ISAMLRProgram)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CatchDocumentInfo);
			}
		}

		protected override void CheckUS_ReExportNumber()
		{
			base.CheckUS_ReExportNumber();

			if (IsNMFSIndicatorDeclared && IsHMSProgram)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ReExportNumberInfo);
			}
		}

		ZBool IsNMFSIndicatorDeclared
		{
			get
			{
				var invoiceLine = Parent.Parent != null ? Parent.Parent.InvoiceLine : null;
				return invoiceLine != null && OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSHMSInd);
			}
		}

		ZBool IsHMSProgram
		{
			get { return Parent.US_ProgramType == NMFSProgramCodeList.Codes.HMS; }
		}

		ZBool ISAMLRProgram
		{
			get { return Parent.US_ProgramType == NMFSProgramCodeList.Codes.AMR; }
		}
	}
}
