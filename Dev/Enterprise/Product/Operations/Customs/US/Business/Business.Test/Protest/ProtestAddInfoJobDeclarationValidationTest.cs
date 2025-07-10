using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	sealed class ProtestAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_P_AcceleratedDispositionInd()
		{
			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_AcceleratedDispositionInd = true;
			AssertHasMessageError(Protest.US_P_AcceleratedDispositionIndInfo, ProtestAddInfoJobDeclarationValidation.AcceleratedDispositionOnlyFor514);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			AssertNoMessageError(Protest.US_P_AcceleratedDispositionIndInfo, ProtestAddInfoJobDeclarationValidation.AcceleratedDispositionOnlyFor514);
		}

		public void TestCheckUS_P_ApplicationFurtherReview()
		{
			Protest.TariffActCitation = "";
			Protest.US_P_ApplicationFurtherReview = true;
			AssertHasMessageError(Protest.US_P_ApplicationFurtherReviewInfo, ProtestAddInfoJobDeclarationValidation.ApplicationFurtherReview);

			Protest.US_P_ApplicationFurtherReview = false;
			AssertNoMessageError(Protest.US_P_ApplicationFurtherReviewInfo, ProtestAddInfoJobDeclarationValidation.ApplicationFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_ApplicationFurtherReview = true;
			AssertHasMessageError(Protest.US_P_ApplicationFurtherReviewInfo, ProtestAddInfoJobDeclarationValidation.ApplicationFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationFurtherReview = true;
			AssertNoMessageError(Protest.US_P_ApplicationFurtherReviewInfo, ProtestAddInfoJobDeclarationValidation.ApplicationFurtherReview);
		}

		public void TestCheckUS_P_FAXSentDate()
		{
			Protest.US_P_FaxSentDate = ZDateTime.Empty;
			AssertNoMessageError(Protest.US_P_FaxSentDateInfo, ProtestAddInfoJobDeclarationValidation.FaxSentDateRequired);

			Protest.US_P_FaxSent = true;
			Protest.US_P_FaxSentDate = ZDateTime.Empty;
			AssertHasMessageError(Protest.US_P_FaxSentDateInfo, ProtestAddInfoJobDeclarationValidation.FaxSentDateRequired);

			Protest.US_P_FaxSentDate = ZDateTime.Now;
			AssertNoMessageError(Protest.US_P_FaxSentDateInfo, ProtestAddInfoJobDeclarationValidation.FaxSentDateRequired);
		}

		public void TestCheckUS_P_AddressTeam()
		{
			Protest.US_P_AddressTeam = "XYZ";
			AssertNoMessageError(Protest.US_P_AddressTeamInfo, ProtestAddInfoJobDeclarationValidation.AddressTeamRequired);

			Protest.US_P_AddressTeam = "";
			AssertHasMessageError(Protest.US_P_AddressTeamInfo, ProtestAddInfoJobDeclarationValidation.AddressTeamRequired);
		}

		public void TestCheckUS_P_Assoc514ProtestNo()
		{
			Protest.TariffActCitation = ZString.Empty;
			Protest.US_P_Assoc514ProtestNo = ZString.Empty;
			AssertNoMessageError(Protest.US_P_Assoc514ProtestNoInfo, ProtestAddInfoJobDeclarationValidation.Associated514NumberRequiredWhen181);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_Assoc514ProtestNo = ZString.Empty;
			AssertNoMessageError(Protest.US_P_Assoc514ProtestNoInfo, ProtestAddInfoJobDeclarationValidation.Associated514NumberRequiredWhen181);

			Protest.TariffActCitation = TariffActCitationList.Codes.D_Section181;
			Protest.Declaration.AddInfoValidation.ValidateUS_P_Assoc514ProtestNo();
			AssertHasMessageError(Protest.US_P_Assoc514ProtestNoInfo, ProtestAddInfoJobDeclarationValidation.Associated514NumberRequiredWhen181);

			Protest.US_P_Assoc514ProtestNo = "TEST";
			AssertNoMessageError(Protest.US_P_Assoc514ProtestNoInfo, ProtestAddInfoJobDeclarationValidation.Associated514NumberRequiredWhen181);
			AssertHasMessageError(Protest.US_P_Assoc514ProtestNoInfo, ProtestAddInfoJobDeclarationValidation.Associated514NumberFormat);

			Protest.US_P_Assoc514ProtestNo = "80040065";
			AssertNoMessageError(Protest.US_P_Assoc514ProtestNoInfo, ProtestAddInfoJobDeclarationValidation.Associated514NumberFormat);
		}

		public void TestCheckUS_P_FilingDDPP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			Protest.US_P_FilingDDPP = "";
			AssertHasMessageError(Protest.US_P_FilingDDPPInfo, ProtestAddInfoJobDeclarationValidation.FilingDDPPRequired);

			Protest.US_P_FilingDDPP = "~";
			AssertHasMessageErrorContaining(Protest.US_P_FilingDDPPInfo, ListValidation.InvalidCodeMessageError);

			Protest.US_P_FilingDDPP = "8888";
			AssertNoMessageError(Protest.US_P_FilingDDPPInfo, ProtestAddInfoJobDeclarationValidation.FilingDDPPRequired);
			AssertNoMessageErrorContaining(Protest.US_P_FilingDDPPInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_P_PeriodBaseDate()
		{
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			AssertNoMessageError(Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateRequired);

			Protest.TariffActCitation = TariffActCitationList.Codes.D_Section181;
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			AssertNoMessageError(Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateRequired);

			Protest.TariffActCitation = TariffActCitationList.Codes.B_Section520c;
			var entry = Protest.LinkedEntries.AddNew();
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			AssertHasMessageError("Should be message error, because Linked Entry is not liquidated and Tariff Act Citation is 520",
				Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateRequired);

			entry.US_LE_LiquidationDate = ZDateTime.Today;
			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_PeriodBaseDateQualifier = ProtestPeriodBaseDateQualifierList.Codes.A_Importer;
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			AssertNoMessageError("No message errors, because not a NAFTA Date Qualifier",
				Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateRequired);

			Protest.US_P_PeriodBaseDateQualifier = ProtestPeriodBaseDateQualifierList.Codes.B_CountryOfOrigin;
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			AssertHasMessageError("Should be message errors, because a NAFTA Date Qualifier",
				Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateRequired);

			Protest.US_P_PeriodBaseDate = ZDateTime.Now;
			AssertNoMessageError(Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateRequired);

			Protest.TariffActCitation = TariffActCitationList.Codes.D_Section181;
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			AssertHasMessageError(Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateNotRequired);

			Protest.US_P_PeriodBaseDate = ZDateTime.Empty;
			AssertNoMessageError(Protest.US_P_PeriodBaseDateInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateNotRequired);
		}

		public void TestCheckUS_PeriodBaseQualifier()
		{
			Protest.US_P_PeriodBaseDateQualifier = "X";
			AssertHasMessageError(Protest.US_P_PeriodBaseDateQualifierInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateQualifierList);

			Protest.TariffActCitation = TariffActCitationList.Codes.D_Section181;
			Protest.US_P_PeriodBaseDateQualifier = ZString.Empty;
			AssertNoMessageError("Date Qualifier is not required, because Period Base Date is empty",
				Protest.US_P_PeriodBaseDateQualifierInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateQualifierRequired);

			Protest.US_P_PeriodBaseDateQualifier = ProtestPeriodBaseDateQualifierList.Codes.A_Importer;
			AssertNoMessageError(Protest.US_P_PeriodBaseDateQualifierInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateQualifierList);
			AssertHasMessageError(Protest.US_P_PeriodBaseDateQualifierInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateQualifierNotRequired);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_PeriodBaseDate = ZDateTime.Today;
			Protest.US_P_PeriodBaseDateQualifier = ZString.Empty;
			AssertHasMessageError("Date Qualifier is required, becasue 514 and date is entered",
				Protest.US_P_PeriodBaseDateQualifierInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateQualifierRequired);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_PeriodBaseDate = ZDateTime.Empty;
			var entry = Protest.LinkedEntries.AddNew();
			entry.US_LE_EntryNumber = "XJ51005006";
			Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDateQualifier();
			AssertNoMessageError("Date Qualifier is not required, becasue Period Base Date is empty",
				Protest.US_P_PeriodBaseDateQualifierInfo, ProtestAddInfoJobDeclarationValidation.PeriodBaseDateQualifierRequired);
		}

		public void TestCheckUS_P_ApplicationQuestion1()
		{
			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationQuestion1 = "X";
			var errorText = string.Format(ProtestAddInfoJobDeclarationValidation.ApplicationQuestionList, "1");
			AssertHasMessageError(Protest.US_P_ApplicationQuestion1Info, errorText);
			AssertHasMessageError(Protest.US_P_ApplicationQuestion1Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_ApplicationQuestion1 = FurtherReviewAnswersList.Codes.No;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion1Info, errorText);
			AssertHasMessageError(Protest.US_P_ApplicationQuestion1Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.US_P_ApplicationQuestion1 = ZString.Empty;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion1Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationFurtherReview = true;
			AssertHasMessageError(Protest.US_P_ApplicationQuestion1Info, ProtestAddInfoJobDeclarationValidation.ApplicationQuestionRequired);
			Protest.US_P_ApplicationQuestion1 = FurtherReviewAnswersList.Codes.No;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion1Info, ProtestAddInfoJobDeclarationValidation.ApplicationQuestionRequired);
		}

		public void TestCheckUS_P_ApplicationQuestion2()
		{
			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationQuestion2 = "X";
			var errorText = string.Format(ProtestAddInfoJobDeclarationValidation.ApplicationQuestionList, "2");
			AssertHasMessageError(Protest.US_P_ApplicationQuestion2Info, errorText);
			AssertHasMessageError(Protest.US_P_ApplicationQuestion2Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_ApplicationQuestion2 = FurtherReviewAnswersList.Codes.No;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion2Info, errorText);
			AssertHasMessageError(Protest.US_P_ApplicationQuestion2Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.US_P_ApplicationQuestion2 = ZString.Empty;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion2Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationFurtherReview = true;
			AssertHasMessageError(Protest.US_P_ApplicationQuestion2Info, ProtestAddInfoJobDeclarationValidation.ApplicationQuestionRequired);
			Protest.US_P_ApplicationQuestion2 = FurtherReviewAnswersList.Codes.No;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion2Info, ProtestAddInfoJobDeclarationValidation.ApplicationQuestionRequired);
		}

		public void TestCheckUS_P_ApplicationQuestion3()
		{
			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationQuestion3 = "X";
			var errorText = string.Format(ProtestAddInfoJobDeclarationValidation.ApplicationQuestionList, "3");
			AssertHasMessageError(Protest.US_P_ApplicationQuestion3Info, errorText);
			AssertHasMessageError(Protest.US_P_ApplicationQuestion3Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_ApplicationQuestion3 = FurtherReviewAnswersList.Codes.No;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion3Info, errorText);
			AssertHasMessageError(Protest.US_P_ApplicationQuestion3Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.US_P_ApplicationQuestion3 = ZString.Empty;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion3Info, ProtestAddInfoJobDeclarationValidation.OnlyFor514WithFurtherReview);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_ApplicationFurtherReview = true;
			AssertHasMessageError(Protest.US_P_ApplicationQuestion3Info, ProtestAddInfoJobDeclarationValidation.ApplicationQuestionRequired);
			Protest.US_P_ApplicationQuestion3 = FurtherReviewAnswersList.Codes.No;
			AssertNoMessageError(Protest.US_P_ApplicationQuestion3Info, ProtestAddInfoJobDeclarationValidation.ApplicationQuestionRequired);
		}

		public void TestCheckUS_P_InternalAdviceNo()
		{
			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			Protest.US_P_InternalAdviceNo = "X";
			AssertNoMessageError(Protest.US_P_InternalAdviceNoInfo, ProtestAddInfoJobDeclarationValidation.OnlyFor514Protest);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_InternalAdviceNo = "X";
			AssertHasMessageError(Protest.US_P_InternalAdviceNoInfo, ProtestAddInfoJobDeclarationValidation.OnlyFor514Protest);
		}

		public void TestCheckUS_P_TestSummonsNo()
		{
			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_TestSummonsNo = "X";
			AssertHasMessageError(Protest.US_P_TestSummonsNoInfo, ProtestAddInfoJobDeclarationValidation.OnlyFor514Protest);

			Protest.TariffActCitation = TariffActCitationList.Codes.A_Section514;
			AssertNoMessageError(Protest.US_P_TestSummonsNoInfo, ProtestAddInfoJobDeclarationValidation.OnlyFor514Protest);
			AssertHasMessageError(Protest.US_P_TestSummonsNoInfo, ProtestAddInfoJobDeclarationValidation.SummonNoFormat);

			Protest.US_P_TestSummonsNo = "123456789";
			AssertNoMessageError(Protest.US_P_TestSummonsNoInfo, ProtestAddInfoJobDeclarationValidation.SummonNoFormat);
		}

		public void TestCheckUS_P_ProtestantType()
		{
			Protest.US_P_ProtestantType = "";
			AssertHasMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.EmptyProtestantType);

			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ImporterConsignee;
			AssertNoMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.EmptyProtestantType);

			Protest.US_P_ProtestantType = "Q";
			AssertHasMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.InvalidProtestantType);

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ForeignExporterProducer;
			AssertHasMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.ProtestantTypeFIsInvalid);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "CLABB";
			Protest.Protestant.OrganisationPK = organisation.PK;

			Protest.Declaration.AddInfoValidation.ValidateUS_P_ProtestantType();
			AssertNoMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.ProtestantTypeFIsInvalid);

			Protest.TariffActCitation = TariffActCitationList.Codes.D_Section181;
			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ForeignExporterProducer;
			AssertHasMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.ProtestantTypeFIsInvalid);

			organisation.OH_RL_NKClosestPort = "MXABB";
			Protest.Declaration.AddInfoValidation.ValidateUS_P_ProtestantType();
			AssertNoMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.ProtestantTypeFIsInvalid);

			var message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ProtestInitialFiling);
			message.EM_MessageText = "B011101SV9PJ                                               112009               P10            B00156367   X2                                                   P11N        N        N        0PR                                               P1588881                        201109012                                       P30 I118888-12345                                                               P600001 00300756                                                                P70PROTEST DECISION FOR TEST                                                    P71SOME GOODS FOR TEST                                                          P900001JUSTIFICATION NOTES SHOULD BE SAVED AND CHANGED                          Y  1101SV9PJ00008";
			message.EM_MessageNum = "112009";
			Protest.Messages.Add(message);

			message = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			message.EM_MessageText = "B015501125PL                                               112009               P01550111150027B11O004724     Protest filing accepted error free                Y  5501125PL00001";
			message.EM_MessageNum = "112009";
			Protest.Messages.Add(message);

			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ForeignExporterProducer;
			AssertHasMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.ProtestantTypeIsDifferent);

			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ImporterConsignee;
			AssertNoMessageError(Protest.US_P_ProtestantTypeInfo, ProtestAddInfoJobDeclarationValidation.ProtestantTypeIsDifferent);
		}

		public void TestCheckUS_P_RefundCOPartyType()
		{
			Protest.US_P_RefundCOPartyType = "";
			AssertNoMessageError(Protest.US_P_RefundCOPartyTypeInfo, ProtestAddInfoJobDeclarationValidation.RefundPartyTypeList);

			Protest.US_P_RefundCOPartyType = "X";
			AssertHasMessageError(Protest.US_P_RefundCOPartyTypeInfo, ProtestAddInfoJobDeclarationValidation.RefundPartyTypeList);

			Protest.US_P_RefundCOPartyType = ProtestRefundCOPartyTypeList.Codes.Attorney;
			AssertNoMessageError(Protest.US_P_RefundCOPartyTypeInfo, ProtestAddInfoJobDeclarationValidation.RefundPartyTypeList);
		}

		public void TestUS_P_ProtestedDecision()
		{
			Protest.US_P_ProtestedDecision = "Test";
			AssertNoMessageError(Protest.US_P_ProtestedDecisionInfo, ProtestAddInfoJobDeclarationValidation.ProtestedDecisionRequired);

			Protest.US_P_ProtestedDecision = "";
			AssertHasMessageError(Protest.US_P_ProtestedDecisionInfo, ProtestAddInfoJobDeclarationValidation.ProtestedDecisionRequired);
		}

		public void TestCheckUS_P_LeadProtestNo()
		{
			Protest.US_P_LeadProtestNo = "XXX";
			AssertHasMessageError(Protest.US_P_LeadProtestNoInfo, ProtestAddInfoJobDeclarationValidation.LeadProtestNoFormat);

			Protest.US_P_LeadProtestNo = "123456789123";
			AssertNoMessageError(Protest.US_P_LeadProtestNoInfo, ProtestAddInfoJobDeclarationValidation.LeadProtestNoFormat);
		}

		public void TestUS_P_MerchandiseDesc()
		{
			Protest.US_P_MerchandiseDesc = "Test";
			AssertNoMessageError(Protest.US_P_MerchandiseDescInfo, ProtestAddInfoJobDeclarationValidation.MerchandiseDescRequired);

			Protest.US_P_MerchandiseDesc = "";
			AssertHasMessageError(Protest.US_P_MerchandiseDescInfo, ProtestAddInfoJobDeclarationValidation.MerchandiseDescRequired);
		}

		public void TestCheckUS_P_SubstituteDDPP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			Protest.US_P_SubstituteDDPP = "~";
			AssertHasMessageErrorContaining(Protest.US_P_SubstituteDDPPInfo, ListValidation.InvalidCodeMessageError);

			Protest.US_P_SubstituteDDPP = "8888";
			AssertNoMessageErrorContaining(Protest.US_P_SubstituteDDPPInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestUS_P_HardCopySent_SampleSent()
		{
			var message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ProtestInitialFiling);
			message.EM_MessageText = "B011101SV9PJ                                               112009               P10            B00156367   X2                                                   P11N        N        N        0PR                                               P1588881                        201109012                                       P30 I118888-12345                                                               P600001 00300756                                                                P70PROTEST DECISION FOR TEST                                                    P71SOME GOODS FOR TEST                                                          P900001JUSTIFICATION NOTES SHOULD BE SAVED AND CHANGED                          Y  1101SV9PJ00008";
			message.EM_MessageNum = "112009";
			Protest.Messages.Add(message);

			var responseMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			responseMessage.EM_MessageText = "B015501125PL                                               112009               P01550111150027B11O004724     Protest filing accepted error free                Y  5501125PL00001";
			responseMessage.EM_MessageNum = "112009";
			Protest.Messages.Add(responseMessage);

			Protest.US_P_HardCopySent = true;
			Protest.US_P_SampleSent = true;
			AssertNoMessageError("If Hard Copy indicator was not sent in initial transaction, we can send 'S' in amendment",
				Protest.US_P_HardCopySentInfo, ProtestAddInfoJobDeclarationValidation.IndicatorIsDifferent);
			AssertNoMessageError("If Sample Sent indicator was not transmitted in initial transaction, we can send 'S' in amendment",
				Protest.US_P_SampleSentInfo, ProtestAddInfoJobDeclarationValidation.IndicatorIsDifferent);

			protest = null;
			Protest.US_P_HardCopySent = true;
			Protest.US_P_SampleSent = true;
			message = CreateMessage(EDIMessage.Direction.Transmit, ApplicationIdentifierCodeList.Codes.ProtestInitialFiling);
			message.EM_MessageText = "B011101SV9PJ                                               112008               P10            B00156367   X2                                                   P11S        S        N        0PR                                               P1588881                        201109012                                       P30 I118888-12345                                                               P600001 00300756                                                                P70PROTEST DECISION FOR TEST                                                    P71SOME GOODS FOR TEST                                                          P900001JUSTIFICATION NOTES SHOULD BE SAVED AND CHANGED                          Y  1101SV9PJ00008";
			message.EM_MessageNum = "112008";
			Protest.Messages.Add(message);

			responseMessage = CreateMessage(EDIMessage.Direction.Receive, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
			responseMessage.EM_MessageText = "B015501125PL                                               112008               P01550111150027B11O004724     Protest filing accepted error free                Y  5501125PL00001";
			responseMessage.EM_MessageNum = "112008";
			Protest.Messages.Add(responseMessage);

			Protest.US_P_HardCopySent = false;
			Protest.US_P_SampleSent = false;
			AssertHasMessageError("Once 'S' has been sent in initial transaction for Hard Copy indicator, it cannot be changed to 'N'",
				Protest.US_P_HardCopySentInfo, ProtestAddInfoJobDeclarationValidation.IndicatorIsDifferent);
			AssertHasMessageError("Once 'S' has been sent in initial transaction for Sample Sent indicator, it cannot be changed to 'N'",
				Protest.US_P_SampleSentInfo, ProtestAddInfoJobDeclarationValidation.IndicatorIsDifferent);
		}

		Protest protest;
		Protest Protest => protest ?? (protest = new Protest(Factory.New<JobDeclaration>()));

		MQEDIMessage CreateMessage(string receiveTransmit, string applicationIdentifier)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = receiveTransmit;
			result.EM_MessageType = applicationIdentifier;
			return result;
		}
	}
}
