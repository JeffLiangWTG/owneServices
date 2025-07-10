using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using USEntryChargeTypeList = Enterprise.Registry.Business.Customs.US.EntryChargeTypeList;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest
	{
		public void TestCustomsEntryHeaderMappings()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var entryHeaderMock = CreateCusEntryHeaderMock(declaration);
			var entryHeader = SetupCusEntryHeader(entryHeaderMock.Object);

			var writer = new CustomsEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), CurrentCompanyHelper);
			var entryHeaderData = writer.GetDataObject(entryHeader);
			AssertContents(entryHeaderData);
			AssertNull("Precondition: entryHeaderData.RelatedEntryHeaderCollection", entryHeaderData.RelatedEntryHeaderCollection);
		}

		public void TestCustomsEntryHeaderCusCodeDataMappings()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var usDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				var entryHeader = usDeclaration.CustomsEntryHeaders.AddNew();
				var entryHeaderCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)entryHeader;
				var pscReasonCodesString = "PRC";
				var pscReasonCodesH01 = "H01";
				entryHeaderCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(pscReasonCodesString, out var pscReasonCodesType);
				var pscReasonCodes1 = (CusCodeData)Factory.New(pscReasonCodesType);
				pscReasonCodes1.Parent = entryHeader;
				pscReasonCodes1.CY_Code = pscReasonCodesH01;
				pscReasonCodes1.CY_Data = "12";
				pscReasonCodes1.CY_Order = 1;
				var reconEntryOriginalChargeString = "REC";
				entryHeaderCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(reconEntryOriginalChargeString, out var reconEntryOriginalChargeType);
				var reconEntryOriginalCharge = (CusCodeData)Factory.New(pscReasonCodesType);
				reconEntryOriginalCharge.Parent = entryHeader;
				reconEntryOriginalCharge.CY_Code = "ABC";
				reconEntryOriginalCharge.CY_Data = "43";
				reconEntryOriginalCharge.CY_IsOverridden = ZBool.True;
				reconEntryOriginalCharge.CY_Order = 0;
				var pscReasonCodes2 = (CusCodeData)Factory.New(pscReasonCodesType);
				pscReasonCodes2.Parent = entryHeader;
				pscReasonCodes2.CY_Code = pscReasonCodesH01;
				pscReasonCodes2.CY_Data = "43";
				pscReasonCodes2.CY_Order = 2;
				Factory.SaveForTesting();
				var writer = new CustomsEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), new UniversalDataObjectWriterHelper(usDeclaration.Factory, Core.Constants.CountryCodes.UnitedStates));
				var entryHeaderData = writer.GetDataObject(entryHeader);
				AssertEquals("entryHeaderData.CustomsReferenceCollection.Count", 3, entryHeaderData.CustomsReferenceCollection.Count);
				AssertContents(entryHeaderData.CustomsReferenceCollection[0], GetCodeDescriptionPair(pscReasonCodesString, "PSC Reason Codes"), GetCodeDescriptionPair(pscReasonCodesH01, null), "12", 1, ZBool.False);
				AssertContents(entryHeaderData.CustomsReferenceCollection[1], GetCodeDescriptionPair(pscReasonCodesString, "PSC Reason Codes"), GetCodeDescriptionPair("ABC", null), "43", 0, ZBool.True);
				AssertContents(entryHeaderData.CustomsReferenceCollection[2], GetCodeDescriptionPair(pscReasonCodesString, "PSC Reason Codes"), GetCodeDescriptionPair(pscReasonCodesH01, null), "43", 2, ZBool.False);
			}
		}

		public void TestCustomsEntryHeaderCusAddInfoMappings()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var gbDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<GB.IJobDeclaration>();
				var entryHeader = gbDeclaration.CustomsEntryHeaders.AddNew();
				var entryHeaderCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)entryHeader;
				entryHeaderCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, out var maritimeUCNThatIsHeldType);
				var maritimeUCNThatIsHeld = (CusAddInfo)Factory.New(maritimeUCNThatIsHeldType);
				maritimeUCNThatIsHeld.B7_ParentID = entryHeader.PK;
				maritimeUCNThatIsHeld.B7_ParentTableCode = entryHeader.TablePrefix;
				maritimeUCNThatIsHeld.B7_AddInfoData = MaritimeUcnThatIsHeldSchema.Constants.NW_UCN.Substring(3) + "=12";
				Factory.SaveForTesting();
				var writer = new CustomsEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), new UniversalDataObjectWriterHelper(gbDeclaration.Factory, Core.Constants.CountryCodes.UnitedKingdom));
				var entryHeaderData = writer.GetDataObject(entryHeader);
				AssertEquals("entryHeaderData.AddInfoGroupCollection.Count", 1, entryHeaderData.AddInfoGroupCollection.Count);
				AssertContents(entryHeaderData.AddInfoGroupCollection[0], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, "Maritime UCN That Is Held"), new List<AddInfo>(new[] { new AddInfo() { Key = MaritimeUcnThatIsHeldSchema.Constants.NW_UCN.Substring(3), Value = "12" } }));
			}
		}

		public void TestRelatedEntryHeaderCollection()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = declarationMock.Object;
			var entryHeaderMock = CreateCusEntryHeaderMock(declaration);
			var entryHeader = SetupCusEntryHeaderWithRelatedEntry(entryHeaderMock.Object);

			var writer = new CustomsEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), CurrentCompanyHelper);
			var entryHeaderData = writer.GetDataObject(entryHeader);
			AssertCusEntryHeaderWithRelatedEntry(entryHeaderData);
		}

		public void TestEntryHeaderInstructionLink()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "BLT";
				var entryInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInst1.CEI_Style = "1";
				var entryInst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInst2.CEI_Style = "2";
				var header1 = declaration.ActiveEntryHeaders.AddNew();
				header1.CH_BGMReference = "ENTRY001";
				header1.CH_CEI_Instruction = entryInst1.PK;
				var header2 = declaration.ActiveEntryHeaders.AddNew();
				header2.CH_BGMReference = "ENTRY002";
				header2.CH_CEI_Instruction = entryInst2.PK;
				var header3 = declaration.ActiveEntryHeaders.AddNew();
				header3.CH_BGMReference = "ENTRY003";

				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				var link1 = result.EntryInstructionCollection.FirstOrDefault(x => x.Style.Value == "1").Link.Value;
				var link2 = result.EntryInstructionCollection.FirstOrDefault(x => x.Style.Value == "2").Link.Value;

				CombineAssertions(() =>
				{
					AssertEquals(link1, result.EntryHeaderCollection.FirstOrDefault(x => x.Reference.Value == "ENTRY001").EntryInstructionLink.Value);
					AssertEquals(link2, result.EntryHeaderCollection.FirstOrDefault(x => x.Reference.Value == "ENTRY002").EntryInstructionLink.Value);
					AssertEquals(null, result.EntryHeaderCollection.FirstOrDefault(x => x.Reference.Value == "ENTRY003").EntryInstructionLink);
				});
			}
		}

		Mock<CusEntryHeader> CreateCusEntryHeaderMock(BaseJobDeclaration declaration)
		{
			var headerMock = Factory.NewMoq<CusEntryHeader>();
			var header = headerMock.Object;
			headerMock.Protected().Setup<Registry.Business.Customs.EntryChargeTypeList>("GetEntryChargeTypeList").Returns(Factory.GetCachedValue<USEntryChargeTypeList>());
			Mock<CusEntryHeaderLookups> headerLookups = new Mock<CusEntryHeaderLookups>(header);
			headerLookups.CallBase = true;
			headerLookups.Setup(m => m.MessageStatusList).Returns(new CustomsEntryStatusList());
			headerLookups.Setup(m => m.CH_EntryStatusList).Returns(new CustomsEntryStatusList());

			headerMock.Protected().Setup<CusEntryHeaderLookups>("GetNewLookups").Returns(headerLookups.Object);
			declaration.CustomsEntryHeaders.Add(header);
			return headerMock;
		}

		CusEntryHeader SetupCusEntryHeaderWithRelatedEntry(CusEntryHeader entryHeader)
		{
			SetupCusEntryHeader2(entryHeader);
			var relatedEntryHeaderMock = CreateCusEntryHeaderMock(entryHeader.Declaration);
			var relatedEntryHeader = relatedEntryHeaderMock.Object;
			relatedEntryHeader.CH_CH_PrimeEntry = entryHeader.PK;
			SetupCusEntryHeader3(relatedEntryHeader);
			return entryHeader;
		}

		CusEntryHeader SetupCusEntryHeader(CusEntryHeader entryHeader)
		{
			return SetupCusEntryHeader(entryHeader, new ZDateTime(2011, 2, 3), CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, new ZDateTime(2011, 2, 2), 1404.24m, JobMessageTypeList.Codes.Export, "BDG34332", new ZDate(2011, 2, 7));
		}

		CusEntryHeader SetupCusEntryHeader2(CusEntryHeader entryHeader)
		{
			return SetupCusEntryHeader(entryHeader, new ZDateTime(2011, 2, 2), CustomsEntryStatusList.Codes.ErrorEntrySummaryOriginal, CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, new ZDateTime(2011, 2, 1), 896.24m, JobMessageTypeList.Codes.Import, "BDG98685", new ZDate(2011, 2, 6));
		}

		CusEntryHeader SetupCusEntryHeader3(CusEntryHeader entryHeader)
		{
			return SetupCusEntryHeader(entryHeader, new ZDateTime(2011, 2, 1), CustomsEntryStatusList.Codes.AwaitingEntrySummaryOriginal, CustomsEntryStatusList.Codes.AwaitingElectronicInvoiceOriginal, new ZDateTime(2011, 3, 1), 635.8m, JobMessageTypeList.Codes.Export, "BDG3658", new ZDate(2011, 2, 5));
		}

		CusEntryHeader SetupCusEntryHeaderOnly(CusEntryHeader entryHeader, ZDateTime entryReleaseDate, ZString entryStatus, ZString messageStatus, ZDateTime entrySubmittedDate, ZDecimal totalPaid, ZString messageType, ZString bGMReference, ZDate bondValidToDate)
		{
			entryHeader.CH_EntryReleaseDate = entryReleaseDate;
			entryHeader.CH_EntryStatus = entryStatus;
			entryHeader.CH_Status = messageStatus;
			entryHeader.CH_EntrySubmittedDate = entrySubmittedDate;
			entryHeader.CH_TotalPaid = totalPaid;
			entryHeader.CH_MessageType = messageType;
			entryHeader.CH_BGMReference = bGMReference;
			entryHeader.CH_BondValidToDate = bondValidToDate;
			entryHeader.CH_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			return entryHeader;
		}

		CusEntryHeader SetupCusEntryHeader(CusEntryHeader entryHeader, ZDateTime entryReleaseDate, ZString entryStatus, ZString messageStatus, ZDateTime entrySubmittedDate, ZDecimal totalPaid, ZString messageType, ZString bGMReference, ZDate bondValidToDate)
		{
			entryHeader = SetupCusEntryHeaderOnly(entryHeader, entryReleaseDate, entryStatus, messageStatus, entrySubmittedDate, totalPaid, messageType, bGMReference, bondValidToDate);
			var entryLine = SetupCusEntryLine(entryHeader.MergedLines.AddNew());

			SetupCusEntryHeaderCharges(entryHeader.Charges.AddNew());
			SetupCusEntryHeaderCharges2(entryHeader.Charges.AddNew());

			var cusEntryNumber = SetupCusEntryNumber(entryHeader.Factory);
			cusEntryNumber.Parent = entryHeader;

			var cusEntryNumber2 = SetupCusEntryNumber2(entryHeader.Factory);
			cusEntryNumber2.Parent = entryHeader;

			SetupEntryPayInfo(entryHeader.EntryPayInfos.AddNew(), new ZDateTime(2021, 10, 29), new ZDate(2021, 10, 30), "A10", "BRK", "PEN", true, false, 101m, "Reference1234", "DAI11101354198");
			SetupEntryPayInfo(entryHeader.EntryPayInfos.AddNew(), new ZDateTime(2021, 11, 29), new ZDate(2021, 11, 30), "A20", "CLI", "CLR", false, true, 202m, "Reference5678", "BCD22212465209");
			return entryHeader;
		}

		void SetupCusEntryHeaderCharges(CusEntryHeaderCharges charge, ZDecimal chargeAmount, ZString chargeType)
		{
			charge.C1_ChargeAmount = chargeAmount;
			charge.C1_ChargeType = chargeType;
		}

		void SetupCusEntryHeaderCharges(CusEntryHeaderCharges charge)
		{
			SetupCusEntryHeaderCharges(charge, 236.45m, Core.Constants.USCustoms.FeeCodes.CountervailingDuty);
		}

		void SetupCusEntryHeaderCharges2(CusEntryHeaderCharges charge)
		{
			SetupCusEntryHeaderCharges(charge, 695.78m, Core.Constants.USCustoms.FeeCodes.Cotton);
		}

		void AssertContentsWithLookingAtChildren(UniversalCustoms.EntryHeader entryHeaderData, ZDateTime entryReleaseDate, ICodeDescription entryStatus, ICodeDescription messageStatus, ZDateTime entrySubmittedDate, ZDecimal totalAmountPaid, ICodeDescription type, ZString reference, ZDate bondValidToDate)
		{
			AssertNotNull("Precondition: entryHeaderData", entryHeaderData);

			CombineAssertions(delegate
			{
				AssertEquals("entryHeaderData.EntryReleaseDate", entryReleaseDate, entryHeaderData.EntryReleaseDate);

				AssertNotNull("entryHeaderData.EntryStatus", entryHeaderData.EntryStatus);
				AssertEquals("entryHeaderData.EntryStatus.Code", entryStatus.Code, entryHeaderData.EntryStatus.Code);
				AssertEquals("entryHeaderData.EntryStatus.Description", entryStatus.Description, entryHeaderData.EntryStatus.Description);
				AssertNotNull("entryHeaderData.MessageStatus", entryHeaderData.MessageStatus);
				AssertEquals("entryHeaderData.MessageStatus.Code", messageStatus.Code, entryHeaderData.MessageStatus.Code);
				AssertEquals("entryHeaderData.MessageStatus.Description", messageStatus.Description, entryHeaderData.MessageStatus.Description);
				AssertEquals("entryHeaderData.EntrySubmittedDate", entrySubmittedDate, entryHeaderData.EntrySubmittedDate);
				AssertEquals("entryHeaderData.TotalAmountPaid", totalAmountPaid, entryHeaderData.TotalAmountPaid);
				AssertNotNull("entryHeaderData.Type", entryHeaderData.Type);
				AssertEquals("entryHeaderData.Type.Code", type.Code, entryHeaderData.Type.Code);
				AssertEquals("entryHeaderData.Type.Description", type.Description, entryHeaderData.Type.Description);
				AssertEquals("entryHeaderData.Reference", reference, entryHeaderData.Reference);
				AssertEquals("entryHeaderData.Reference", bondValidToDate, entryHeaderData.BondValidToDate);
				AssertNotNull("Precondition: entryHeaderData.AddInfoCollection", entryHeaderData.AddInfoCollection);
				AddInfoCollectionCreatorTest.AssertContents(entryHeaderData.AddInfoCollection);
			});
		}

		void AssertContents(UniversalCustoms.EntryHeader entryHeaderData, ZDateTime entryReleaseDate, ICodeDescription entryStatus, ICodeDescription messageStatus, ZDateTime entrySubmittedDate, ZDecimal totalAmountPaid, ICodeDescription type, ZString reference, ZDate bondValidToDate)
		{
			AssertContentsWithLookingAtChildren(entryHeaderData, entryReleaseDate, entryStatus, messageStatus, entrySubmittedDate, totalAmountPaid, type, reference, bondValidToDate);
			var entryLineCollection = entryHeaderData.EntryLineCollection;
			AssertNotNull("Precondition: entryHeaderData.EntryLineCollection", entryLineCollection);
			AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryLineCollection.Count);
			AssertContents(entryLineCollection[0]);

			var entryHeaderChargeCollection = entryHeaderData.EntryHeaderChargeCollection;
			AssertNotNull("Precondition: entryHeaderData.EntryHeaderChargeCollection", entryHeaderChargeCollection);
			AssertEquals("entryHeaderData.EntryHeaderChargeCollection.Count", 2, entryHeaderChargeCollection.Count);
			AssertContents(entryHeaderChargeCollection[0]);
			AssertContents2(entryHeaderChargeCollection[1]);

			var entryNumberCollection = entryHeaderData.EntryNumberCollection;
			AssertNotNull("Precondition: entryHeaderData.EntryNumberCollection", entryNumberCollection);
			AssertEquals("entryHeaderData.EntryNumberCollection.Count", 2, entryNumberCollection.Count);
			AssertContents(entryNumberCollection[0]);
			AssertContents2(entryNumberCollection[1]);

			var entryHeaderPaymentInformationCollection = entryHeaderData.PaymentInformationCollection;
			AssertNull("Precondition: entryHeaderData.EntryHeaderPaymentInformationCollection", entryHeaderPaymentInformationCollection);
		}

		void AssertContents(UniversalCustoms.EntryHeader entryHeaderData)
		{
			AssertContents(entryHeaderData, new ZDateTime(2011, 2, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, CustomsEntryStatusList.Descriptions.ClearEntrySummaryOriginal), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, CustomsEntryStatusList.Descriptions.ClearEntrySummaryOriginal), new ZDateTime(2011, 2, 2), 1404.24m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), "BDG34332", new ZDate(2011, 2, 7));
		}

		void AssertContents2(UniversalCustoms.EntryHeader entryHeaderData)
		{
			AssertContents(entryHeaderData, new ZDateTime(2011, 2, 2), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ErrorEntrySummaryOriginal, CustomsEntryStatusList.Descriptions.ErrorEntrySummaryOriginal), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, CustomsEntryStatusList.Descriptions.ClearEntrySummaryOriginal), new ZDateTime(2011, 2, 1), 896.24m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, JobMessageTypeList.Descriptions.Import), "BDG98685", new ZDate(2011, 2, 6));
		}

		void AssertContents3(UniversalCustoms.EntryHeader entryHeaderData)
		{
			AssertContents(entryHeaderData, new ZDateTime(2011, 2, 1), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingEntrySummaryOriginal, CustomsEntryStatusList.Descriptions.AwaitingEntrySummaryOriginal), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingElectronicInvoiceOriginal, CustomsEntryStatusList.Descriptions.AwaitingElectronicInvoiceOriginal), new ZDateTime(2011, 3, 1), 635.8m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), "BDG3658", new ZDate(2011, 2, 5));
		}

		void AssertContents(UniversalCustoms.EntryHeaderCharge entryHeaderChargeData)
		{
			AssertContents(entryHeaderChargeData, 236.45m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, "Countervailing Duty"));
		}

		void AssertContents2(UniversalCustoms.EntryHeaderCharge entryHeaderChargeData)
		{
			AssertContents(entryHeaderChargeData, 695.78m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Cotton, "056 Desc from DB"));
		}

		void AssertContents(UniversalCustoms.EntryHeaderCharge entryHeaderChargeData, ZDecimal amount, ICodeDescription type)
		{
			AssertNotNull("Precondition: entryHeaderChargeData", entryHeaderChargeData);
			CombineAssertions(delegate
			{
				AssertEquals("entryHeaderChargeData.Amount", amount, entryHeaderChargeData.Amount);
				AssertNotNull("entryHeaderChargeData.Type", entryHeaderChargeData.Type);
				AssertEquals("entryHeaderChargeData.Type.Code", type.Code, entryHeaderChargeData.Type.Code);
				AssertEquals("entryHeaderChargeData.Type.Description", type.Description, entryHeaderChargeData.Type.Description);
			});
		}

		void AssertCusEntryHeaderWithRelatedEntry(UniversalCustoms.EntryHeader entryHeaderData)
		{
			AssertContents2(entryHeaderData);
			AssertNotNull("Precondition: entryHeaderData.RelatedEntryHeaderCollection", entryHeaderData.RelatedEntryHeaderCollection);
			AssertEquals("entryHeaderData.RelatedEntryHeaderCollection.Count", 1, entryHeaderData.RelatedEntryHeaderCollection.Count);
			var relatedEntryHeader = entryHeaderData.RelatedEntryHeaderCollection[0];
			AssertContents3(relatedEntryHeader);
			AssertNull("Precondition: relatedEntryHeader.RelatedEntryHeaderCollection", relatedEntryHeader.RelatedEntryHeaderCollection);
		}
	}
}
