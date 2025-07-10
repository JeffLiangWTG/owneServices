using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSDEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.Edifact.Utilities;
namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class TNPUPD : TNPDEC
	{
		public TNPUPD(ITNPUPD customsDec)
			: base(customsDec)
		{
		}

		protected new ITNPUPD CustomsDec
		{
			get { return (ITNPUPD)sgCusdec; }
		}

		public override string MessageType
		{
			get { return CommonAccessReferenceCodeList.Codes.TNPUPD; }
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
				if (!CustomsDec.AdditionalMessageInformation.ReasonForAmending.IsEmpty)
				{
					var fTX1 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					fTX1.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalAttributeInformation;
					TextSplitElegantly textLiteral = new TextSplitElegantly(70, 4);
					textLiteral.Text = CustomsDec.AdditionalMessageInformation.ReasonForAmending;
					fTX1.TextLiteral.FreeText1 = textLiteral[0];
					fTX1.TextLiteral.FreeText2 = textLiteral[1];
					fTX1.TextLiteral.FreeText3 = textLiteral[2];
					fTX1.TextLiteral.FreeText4 = textLiteral[3];
				}

				if (CustomsDec.AdditionalMessageInformation != null && CustomsDec.AdditionalMessageInformation.ExtendingTemporaryImportPeriod)
				{
					var fTX2 = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
					fTX2.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.PeriodOfTime;
					fTX2.TextLiteral.FreeText1 = CodeForExtensionOfPermitValidityCodeList.Codes.Y;
				}
			}
		}

		protected override void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			base.GenerateSegmentGroup1(sg1Section);  // MS, DM, ACE, MR (Note 1, 2, 4 & 7)
			PopulatePermitNoToUpdateOrCancelSegment(sg1Section); //ABT (Note 5)
			if (CustomsDec.DeclarationType == DeclarationTypeCodeList.Codes.GTR)
			{
				PopulateSupplyIndicatorSegment(sg1Section); // TN (Note 3)
			}
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
