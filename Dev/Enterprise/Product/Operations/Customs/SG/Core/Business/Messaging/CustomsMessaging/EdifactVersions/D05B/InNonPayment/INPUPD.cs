using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class INPUPD : INPDEC
	{
		public INPUPD(IINPUPD customsDec)
			: base(customsDec)
		{
		}

		protected new IINPUPD CustomsDec
		{
			get { return (IINPUPD)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.INPUPD; }
		}

		public override string MessageSubType
		{
			get { return CUSDECEDIMessage.Amendment; }
		}

		protected override MessageFunctionCodeList MessageFunctionCode
		{
			get { return MessageFunctionCodeList.Request; }
		}

		protected override void GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cST = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cST.CustomsIdentityCodes1.CustomsGoodsIdentifier = CustomsDec.CargoPackingType;
			cST.CustomsIdentityCodes2.CustomsGoodsIdentifier = SGConstants.UpdateIndicators.AME;
		}

		protected override void GenerateFTX(FTXSegmentMessageSection ftxSection)
		{
			base.GenerateFTX(ftxSection);

			if (CustomsDec.AdditionalMessageInformation != null)
			{
				if (CustomsDec.AdditionalMessageInformation.ExtendingTemporaryImportPeriod)
				{
					var fTX2 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					fTX2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.PeriodOfTime;
					fTX2.TextLiteral.FreeText1 = CodeForExtensionOfPermitValidityCodeList.Codes.Y;

					if (!CustomsDec.AdditionalMessageInformation.ReasonForExtendingTemporaryImportPeriod.IsEmpty)
					{
						var fTX3 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
						fTX3.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.TypeOfTransactionReason;
						TextSplitElegantly remark2 = new TextSplitElegantly(70, 4);
						remark2.Text = CustomsDec.AdditionalMessageInformation.ReasonForExtendingTemporaryImportPeriod;
						fTX3.TextLiteral.FreeText1 = remark2[0];
						fTX3.TextLiteral.FreeText2 = remark2[1];
						fTX3.TextLiteral.FreeText3 = remark2[2];
						fTX3.TextLiteral.FreeText4 = remark2[3];
					}
				}

				if (!CustomsDec.AdditionalMessageInformation.ReasonForAmending.IsEmpty)
				{
					var fTX2 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					fTX2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalAttributeInformation;
					TextSplitElegantly remark1 = new TextSplitElegantly(70, 4);
					remark1.Text = CustomsDec.AdditionalMessageInformation.ReasonForAmending;
					fTX2.TextLiteral.FreeText1 = remark1[0];
					fTX2.TextLiteral.FreeText2 = remark1[1];
					fTX2.TextLiteral.FreeText3 = remark1[2];
					fTX2.TextLiteral.FreeText4 = remark1[3];
				}
			}
		}

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateSegmentGroup1(sg1Section);
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT  *
			PopulateReplacementPermitNoSegment(sg1Section); // AAE
		}

		protected override void GenerateCNT(CUSDECMessage cusdecEdifactMsg)
		{
			base.GenerateCNT(cusdecEdifactMsg);
			var cNT = cusdecEdifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsEntries;
			cNT.Control.ControlTotalQuantity = CustomsDec.NumberOfRequestsForUpdate.ToString();
		}
	}
}
