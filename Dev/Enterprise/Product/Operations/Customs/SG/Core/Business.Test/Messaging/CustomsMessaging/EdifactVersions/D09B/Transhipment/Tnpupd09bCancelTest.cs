using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Tnpupd09bCancelTest : TestCaseWithFactory
	{
		public void TestMessageSubType()
		{
			AssertEquals(CUSDECEDIMessage.Cancellation, MessageBuilder.MessageSubType);
		}

		[ExpectNoExceptions()]
		public void TestGenerateBGM()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("BGM+915+<<MSGNO PLACEHOLDER>>+1'", msg.BGM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCST()
		{
			DataProvider.CargoPackingType = CargoPackingTypeCodeList.Codes.PackingType3;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("CST+++CNL'", msg.CST.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMEASegments()
		{
			DataProvider.IsExport = false;
			DataProvider.TotalOuterPack = 100;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.TotalGrossWeight = 111;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("", msg.MEA.ToString(new UNOASGCharacterSet()));
			DataProvider.IsExport = true;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Air;
			ResetMessageBuilder();
			msg = MessageBuilder.CusdecMessage;
			AssertEquals("", msg.MEA.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateEQDAndSELSegments()
		{
			ContainersTestClass[] containers = new ContainersTestClass[1];
			containers[0] = new ContainersTestClass();
			containers[0].ContainerNumber = "101";
			containers[0].ContainerType = "FCL";
			containers[0].ContainerSize = 13;
			containers[0].ContainerWeight = 913;
			containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
			DataProvider.Containers = containers;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("", msg.EQD.ToString(new UNOASGCharacterSet()));
			AssertEquals("", msg.SEL.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateFTX()
		{
			DataProvider.TradersRemarksForMessage = new ZString[] { "This is one string remark" };
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Amend For Test";
			DataProvider.AdditionalMessageInfo.CancellationCode = ReasonForCancellationCodeList.Codes.C10;
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("FTX+AES+++C10'", msg.FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup1()
		{
			DataProvider.DeclarantId = "ORGSG00001";
			LicencesAndDocumentsTestClass[] documents = new LicencesAndDocumentsTestClass[2];
			documents[0] = new LicencesAndDocumentsTestClass();
			documents[0].LicenceNumber = "LIC123-3456-1";
			documents[1] = new LicencesAndDocumentsTestClass();
			documents[1].LicenceNumber = "LIC123-3456-2";
			DataProvider.LicencesAndDocuments = documents;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GTR;
			DataProvider.PermitNoToUpdateOrCancel = "PERM001/1-2";
			ZString[] additionals = new ZString[1];
			additionals[0] = new ZString("ADD12034");
			DataProvider.AdditionalRecipients = additionals;
			DataProvider.ReplacementPermitNumber = "PERM001/1-4";
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("RFF+MS:ORGS.ORGSG00001'RFF+MR:ADD12034'RFF+ABT:PERM001/1-2'RFF+AAE:PERM001/1-4'", msg.Group1.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup4()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SHO;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.IsExport = false;
			DataProvider.IsImport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.HasLiquorOrTobacco = true;
			DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.InwardTransportIdentifier = "IV2232324";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardVesselNationality = "SG";
			DataProvider.OutwardTransportIdentifier = "OV010101";
			DataProvider.OutwardJourneyIdentifier = "V555555";
			DataProvider.OutwardVesselType = "FR";
			DataProvider.TowingVesselName = "TW00101";
			DataProvider.TowingVesselType = "";
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("", msg.Group4.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup5()
		{
			AttachmentsTestClass[] attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass();
			attachments[0].FileName = "ATTDOC_0001";
			attachments[0].DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			attachments[1] = new AttachmentsTestClass();
			attachments[1].FileName = "ATTDOC_0002";
			attachments[1].DocType = SupportingDocumentTypeCodeList.Codes.DocType002;
			SGCUSDECTestClass.ImplementsAdditionalMessageInformation addInfo = (SGCUSDECTestClass.ImplementsAdditionalMessageInformation)DataProvider.AdditionalMessageInformation;
			addInfo.SupportingDocuments = attachments;
			DataProvider.InwardMasterBill = "77777";
			DataProvider.OutwardMasterBill = "5555";
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("SG5 should have attachments only", "DOC+916+001::ATTDOC_0001'DOC+916+002::ATTDOC_0002'", msg.Group5.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup6()
		{
			var agent = new AgentInfoTestClass();
			agent.Name = "Declarer";
			agent.Code = "DEC0012SA";
			agent.Phone = "322223";
			DataProvider.Declarant = agent;
			var msg = MessageBuilder.CusdecMessage;
			AssertMultilineEquals("TNPUPD Cancel - should only be declarant SG6", "NAD+DT'CTA+IC+DEC0012SA:DECLARER'COM+322223:TE'", msg.Group6.ToString(new UNOASGCharacterSet()), '\'');
		}

		public void TestGenerateSegmentGroup10()
		{
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("TNPUPD Cancel - should not have SG10", "", msg.Group10.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateInvoiceLineDetailSegments_SegmentGroup32()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			InitItem(items[0]);
			DataProvider.Items = items;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.TCE;
			DataProvider.IsImport = true;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.OutwardMasterBill = "MAW1";
			DataProvider.InwardMasterBill = "MAW2";
			DataProvider.IsCASCProductCodeNeeded = true;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("", msg.Group32.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSegmentGroup51()
		{
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GTR;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.TotalCustomsValue = 230;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("", msg.Group51.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageCNT()
		{
			DataProvider.NumberOfRequestsForUpdate = 3;
			var msg = MessageBuilder.CusdecMessage;
			AssertEquals("CNT+6:3'", msg.CNT.ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		TNPDECTestClass DataProvider
		{
			get
			{
				if (fDataProvider == null)
				{
					fDataProvider = new TNPDECTestClass();
				}

				return fDataProvider;
			}
		}

		TNPDECTestClass fDataProvider;
		Tnpupd09bCancel MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new Tnpupd09bCancel(DataProvider);
				}

				return fMessageBuilder;
			}
		}

		Tnpupd09bCancel fMessageBuilder;
		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}

		void InitItem(ItemsTestClass item)
		{
			item.BrandName = "Sonic";
			item.CountryOfOriginCode = "AU";
			item.CurrentLotNumber = "7";
			item.DutyAmount = 12;
			item.DutyUnitRate = 7;
			item.DGIndicator = "Y";
			item.UnitDutiableQuantity = 2;
			item.UnitDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.E_SDNPIndicator = "Y";
			item.ExciseAmount = 5;
			item.GoodsDescription = "Misc things";
			item.GSTPayable = 111;
			item.HSCode = "1000012";
			item.HSQuantity = 5;
			item.HSQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.InvoiceNumber = "INV100023";
			item.IsBasedOnRates = true;
			item.IsDangerous = true;
			item.IsLiquor = true;
			item.IsMotorVehicle = true;
			item.PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRI;
			item.IsTobacco = true;
			item.ModelDescription = "srft123fg";
			item.PackingUnitType = "CON";
			item.PackInmostQuantity = 10;
			item.PackInnerQuantity = 8;
			item.PackInQuantity = 9;
			item.PackOuterQuantity = 12;
			item.PercentageOfAlcohol = 40;
			item.PercentageOfAlcoholUnitType = "LAL";
			item.PreviousLotNumber = "3";
			item.SerialNumber = "SN01010101001";
			item.TotalDutiableQuantity = 13;
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			item.UnitDutiableQuantity = 17;
			ZString[] threeCASCCodes = new ZString[3];
			threeCASCCodes[0] = "CS11111";
			threeCASCCodes[1] = "CS22222";
			threeCASCCodes[2] = "CS33333";
			item.DateOfFirstRegistration = new ZDate(2005, 11, 23);
			item.EngineCapacity = 1600;
			item.EngineCapacityUnit = EngineCapacityCodeList.Codes.CC;
			item.RegistrationNumberSG = "SG12345";
			//item.CASCCodes1 = threeCASCCodes;
		}
		#endregion
	}
}
