using System.Collections.Generic;
using System.IO;
using CargoWise.Customs.TR.MessageDefinitions.Manifest.YeniOzetBeyan;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class TRManifestMessageGeneratorTest : TRBaseMessageGeneratorAbstractTest<TRManifestMessage>
	{
		public void TestMessageInterpretation()
		{
			Header.Messages.Clear();
			AssertEquals(0, Header.Messages.Count);
			Header.AMA_ManifestType = "ATAİTH";

			var message = ((ITRCustomsMessageGenerator)Generator).GenerateMessage();
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
				AssertEquals(TRMessageTypes.Codes.TRO, message.EM_MessageType);
				AssertEquals(TRCustomsDataRegistry.Instance.IsTRTestingSystem, message.EM_IsTestMessage);
				AssertEquals("20201224104", message.EM_MessageOwner);
				AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("ULU-MAN0000442|20201224104", message.EM_ApplicationReference);
				AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals(Header.PK, message.EM_LinkUniqueID);
			});

			CombineAssertions(() =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("Global Manifest Message for job MAN0000442 sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Manifest Type'", message.EM_MessageInterpretation.Contains("<td>Manifest Type:</td><td>ATAİTH</td>"));
				Assert("EM_MessageInterpretation should contains 'Job Number'", message.EM_MessageInterpretation.Contains("<td>Job Number:</td><td>ULU-MAN0000442</td>"));
				Assert("EM_MessageInterpretation should contains 'Customs Office'", message.EM_MessageInterpretation.Contains("<td>Customs Office:</td><td>067777</td"));
				Assert("EM_MessageInterpretation should contains 'Load Port'", message.EM_MessageInterpretation.Contains("<td>Load Port:</td><td>ESACE</td>"));
				Assert("EM_MessageInterpretation should contains 'Customs Discharge'", message.EM_MessageInterpretation.Contains("<td>Customs Discharge:</td><td>TRMRA-009</td>"));
				Assert("EM_MessageInterpretation should contains 'Vessel'", message.EM_MessageInterpretation.Contains("<td>Vessel:</td><td>VESSEL NO</td>"));
				Assert("EM_MessageInterpretation should contains 'Voyage'", message.EM_MessageInterpretation.Contains("<td>Voyage:</td><td>VOYAGE</td>"));
			});

			AssertMessageCanBeDeseriaized<OzetBeyanBilgisi>(message.EM_FormattedMessageText);
		}

		void AssertMessageCanBeDeseriaized<T>(string messageContent)
		{
#if NETFRAMEWORK
			messageContent = messageContent.Substring(messageContent.IndexOf("<q1:OzetBeyanBilgisi"), messageContent.IndexOf("</Gelen>") - messageContent.IndexOf("<q1:OzetBeyanBilgisi"));
#else
			messageContent = messageContent.Substring(messageContent.IndexOf("<OzetBeyanBilgisi"), messageContent.IndexOf("</Gelen>") - messageContent.IndexOf("<OzetBeyanBilgisi"));
#endif
			using (TextReader readerText = new StringReader(messageContent))
			{
				AssertNoExceptionThrown(() => CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.DeserializeWithXSDValidation<T>("CargoWise.Customs.TR.MessageDefinitions.Manifest.YeniOzetBeyan.xsd", readerText));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Header.AMA_JobReference = "MAN0000442";
			Header.AMA_ManifestType = "HAVİTH";
		}

		protected override void SetupUser()
		{
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			GlbStaff.CurrentUser.GS_Code = "CZH";
			currentUser.GP_UserID = "20201224104";
			currentUser.GP_PasswordType = PasswordTypesList.Codes.TRK;
			currentUser.GP_GC = Env.CurrentBranch.Company.PK;
			currentUser.GP_GS = currentUser.PK;
			currentUser.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			currentUser.GP_StatusReason = "";
			currentUser.CurrentDecryptedPassword = "12345678";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			currentUser.PINCode = "123546";
		}

		Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
				}
				return header;
			}
		}
		Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader header;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRO;

		protected override string ExpectedApplicationReference => "ULU-MAN0000442|20201224104";
		protected override string ExpectedMessageOwner => "20201224104";

		protected override IMessageSender Sender => new TRManifestMessageProviderForTesting(Header);

		protected override TRBaseMessageGenerator<TRManifestMessage> Generator => new TRManifestMessageGenerator(new TRManifestMessageProviderForTesting(Header));
	}

	class TRManifestMessageProviderForTesting : ISummaryDeclarationInformation
	{
		public TRManifestMessageProviderForTesting(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader header)
		{
			Header = header;
		}
		protected readonly Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader Header;

		public ZString BusinessRegNo => "8890024379";
		public ZString ManifestType => Header.AMA_ManifestType;
		public ZString Other => "Test Description For AMA_ManifestDescription...";
		public ZString Trailer1RegNo => "";
		public ZString Trailer1RegCountry => "";
		public ZString Trailer2RegNo => "";
		public ZString Trailer2RegCountry => "";
		public ZInt NumberofBillsInDeclaration => Header.AMA_ManifestType == "EMANIF" || Header.AMA_ManifestType == "GRUPAJ" || Header.AMA_ManifestType == "TESLİM" ? 0 : 1;
		public ZString SafetySecurity => "";
		public ZString GroupBillofLadingNumber => "";
		public ZString PresentationCustomsOffice => Header.AMA_ManifestType == "ATAİTH" || Header.AMA_ManifestType == "EMANIF" || Header.AMA_ManifestType == "CIKONC" ? "" : "067777";
		public ZString UserID => "20201224104";
		public ZString AgentType => Header.AMA_ManifestType == "EMANIF" || Header.AMA_ManifestType == "CIKONC" ? "" : "HAYIR";
		public ZString CustomsDischargePort => "TRMRA-009";
		public ZString CustomsLoadPort => Header.AMA_ManifestType == "GRUPAJ" ? "" : "ESACE";
		public ZString PreviousBillNumber => Header.AMA_ManifestType == "ATAİTH" || Header.AMA_ManifestType == "DENİHR" || Header.AMA_ManifestType == "DEMİTH" || Header.AMA_ManifestType == "DEMİHR" ? "" : "BOL";
		public ZString Voyage => Header.AMA_ManifestType == "GRUPAJ" ? "" : "VOYAGE";
		public ZString LloydsNumber => Header.AMA_ManifestType == "DEMİTH" || Header.AMA_ManifestType == "DEMİHR" ? "" : "9332975";
		public ZString RegistrationNumber => "";
		public ZString Nature => Header.AMA_ManifestType == "CIKONC" || Header.AMA_ManifestType == "DENİHR" || Header.AMA_ManifestType == "DEMİHR" ? "E" : "I";
		public ZString TransportType => Header.AMA_ManifestType == "ATAİTH" || Header.AMA_ManifestType == "GRUPAJ" || Header.AMA_ManifestType == "DENİHR" || Header.AMA_ManifestType == "DEMİTH" || Header.AMA_ManifestType == "DEMİHR" ? "" : "10";
		public ZString Vessel => Header.AMA_ManifestType == "GRUPAJ" ? "" : "VESSEL NO";
		public ZString CarrierBusinessRegNo => Header.AMA_ManifestType == "GRUPAJ" ? "" : "8890024379";
		public ZString TruckATAScorecardNumber => "";
		public ZString ConveyanceNationality => Header.AMA_ManifestType == "GRUPAJ" ? "" : "052";
		public ZString CustomsDischargeCountryCode => "052";
		public ZString CustomsLoadCountryCode => Header.AMA_ManifestType == "GRUPAJ" ? "" : "011";
		public ZString LoadingUnloadingPlace => "";
		public ZString CustomsOffice => "067777";
		public ZDateTime DateAtCustomsOffice => Header.AMA_ManifestType == "GRUPAJ" ? ZDateTime.Empty : new ZDateTime(2020, 2, 28);
		public ZString JobReference => "ULU-MAN0000442";
		public ZString XmlRefId => "D317411A-1EFB-4F4E-BD48-322ACEDAA8F5";
		public BusinessObject Parent => (BusinessObject)Header;
		public IEnumerable<IBillofLading> BillofLadings
		{
			get
			{
				var result = new List<IBillofLading>();
				result.Add(new BillProvider(Header.AMA_ManifestType));
				return result;
			}
		}
		public IEnumerable<IVehicleVisitedCountry> VehicleVisitedCountry
		{
			get
			{
				var result = new List<IVehicleVisitedCountry>();
				if (Header.AMA_ManifestType == "EMANIF" || Header.AMA_ManifestType == "CIKONC")
				{
					result.Add(new VehicleVisitedCountryProvider(Header.AMA_ManifestType));
				}
				return result;
			}
		}
		public IEnumerable<ICarrierCompany> CarrierCompany
		{
			get
			{
				var result = new List<ICarrierCompany>();
				return result;
			}
		}
		public IEnumerable<IOpeningSummaryDeclaration> OpeningSummaryDeclaration
		{
			get
			{
				var result = new List<IOpeningSummaryDeclaration>();
				if (Header.AMA_ManifestType == "GRUPAJ" || Header.AMA_ManifestType == "DENİHR")
				{
					result.Add(new OpeningSummaryDeclarationProvider());
				}
				return result;
			}
		}
		public IBusinessObjectCollection Messages => Header.Messages;
	}
	class BillProvider : IBillofLading
	{
		public BillProvider(ZString manifestTypeCode)
		{
			ManifestTypeCode = manifestTypeCode;
		}
		protected readonly ZString ManifestTypeCode;

		public ZString ContainerAgentName => "AGENT NAME";
		public ZString ContainerAgentRegNo => ManifestTypeCode == "ATAİTH" || ManifestTypeCode == "DEMİTH" || ManifestTypeCode == "DEMİHR" ? "" : "8890024379";
		public ZString ConsigneeName => "CONSIGNE NAME";
		public ZString ConsigneeRegNo => ManifestTypeCode == "TESLİM" || ManifestTypeCode == "GRUPAJ" || ManifestTypeCode == "CIKONC" ? "" : "8890024379";
		public ZString IsWarehouseExternal => ManifestTypeCode == "CIKONC" ? "" : "HAYIR";
		public ZString NotifyPartyName => ManifestTypeCode == "EMANIF" ? "BILDIRIM" : "";
		public ZString NotifyPartyRegNo => ManifestTypeCode == "EMANIF" ? "6000026814" : "";
		public ZString Origin => "011";
		public ZString SafetySecurityT => "";
		public ZString GoodsLocation => ManifestTypeCode == "ATAİTH" ? "" : "GEMI";
		public ZString TransportValueCurrency => "USD";
		public ZDecimal TransportTotalValue => ManifestTypeCode == "GRUPAJ" || ManifestTypeCode == "EMANIF" ? 0 : 100;
		public ZString ShipperName => "SHIPPER NAME";
		public ZString ShipperRegNo => "";
		public ZString IsGroup => ManifestTypeCode == "CIKONC" ? "" : "HAYIR";
		public ZString Iscontainer => "EVET";
		public ZString NKFreightValueCurrency => ManifestTypeCode == "HAVİTH" ? "EUR" : "";
		public ZDecimal FreightValue => ManifestTypeCode == "HAVİTH" ? 50 : 0;
		public ZString PaymentType => "B";
		public ZString PreviousVoyageNo => "";
		public ZDateTime PreviousVoyageArrivalDate => ZDateTime.Empty;
		public ZString EntryNumber => "";
		public ZString IsRoro => ManifestTypeCode == "EMANIF" ? "" : "HAYIR";
		public ZString SequenceNo => ManifestTypeCode == "CIKONC" ? "" : "1";
		public ZString IsTransshipmentType => ManifestTypeCode == "TESLİM" ? "" : "EVET";
		public ZString TransshipmentType => ManifestTypeCode == "TESLİM" ? "" : "Yurtiçi Aktarma";
		public ZString BillNumber => "BILL1";
		public IEnumerable<ILadingLines> LadingLines
		{
			get
			{
				var result = new List<ILadingLines>();
				result.Add(new PackProvider(ManifestTypeCode));
				return result;
			}
		}
		public IEnumerable<ILadingExports> LadingExports
		{
			get
			{
				var result = new List<ILadingExports>();
				if (ManifestTypeCode == "CIKONC" || ManifestTypeCode == "DENİHR" || ManifestTypeCode == "DEMİHR")
				{
					result.Add(new LadingExportsProvider());
				}
				return result;
			}
		}
		public IEnumerable<IBillVisitedCountry> BillVisitedCountry
		{
			get
			{
				var result = new List<IBillVisitedCountry>();
				if (ManifestTypeCode != "CIKONC")
				{
					result.Add(new VisitedCountryProvider());
				}
				return result;
			}
		}
	}
	class PackProvider : ILadingLines
	{
		public PackProvider(ZString manifestTypeCode)
		{
			ManifestTypeCode = manifestTypeCode;
		}
		protected readonly ZString ManifestTypeCode;

		public ZDecimal GrossWeight => 100;
		public ZInt PackQuantity => 10;
		public ZString PackType => "KN";
		public ZString ContainerType => ManifestTypeCode == "ATAİTH" || ManifestTypeCode == "DEMİHR" ? "" : "YAB";
		public ZString ContainerNumber => "TTNU1234640";
		public ZString SealNumber => ManifestTypeCode == "ATAİTH" || ManifestTypeCode == "DEMİHR" ? "" : "SEAL1";
		public ZDecimal NetWeight => 0;
		public ZString WeightUQ => "KGM";
		public ZInt LineNo => 1;
		public ZString ContainerLoadStatus => ManifestTypeCode == "DENİHR" || ManifestTypeCode == "DEMİHR" ? "" : "DOLU";
		public IEnumerable<IGoodsInformation> GoodsInformation
		{
			get
			{
				var result = new List<IGoodsInformation>();
				result.Add(new ItemProvider(ManifestTypeCode));
				return result;
			}
		}
		public class ItemProvider : IGoodsInformation
		{
			public ItemProvider(ZString manifestTypeCode)
			{
				ManifestTypeCode = manifestTypeCode;
			}
			protected readonly ZString ManifestTypeCode;

			public ZString UNGoodCode => "";
			public ZDecimal GrossWeight => 100;
			public ZString TariffCode => ManifestTypeCode == "CIKONC" ? "" : "081330";
			public ZString GoodsDescription => ManifestTypeCode == "DENİHR" ? "" : "GOODS DESC";
			public ZDecimal GoodsValue => 0;
			public ZString GoodsValueCurrency => "";
			public ZInt OrderNo => 1;
			public ZDecimal NetWeight => 0;
			public ZString CustomsUQ => "KGM";
		}
	}
	class VisitedCountryProvider : IBillVisitedCountry
	{
		public VisitedCountryProvider()
		{
		}

		public ZString PortLocationName => "TRMRA-001";
		public ZString CountryCode => "052";
	}
	class OpeningSummaryDeclarationProvider : IOpeningSummaryDeclaration
	{
		public OpeningSummaryDeclarationProvider()
		{
		}

		public ZString HowToOpen => "2";
		public ZString InWarehouse => "EVET";
		public ZString DeclarationNo => "15343100IM108535";
		public ZString WillOpenAnotherRegime => "HAYIR";
		public ZString Explanation => ".";
		public ZString OpeningInternalNumber => "";

		public IEnumerable<IOpeningBillofLadings> OpeningBillofLadings
		{
			get
			{
				var result = new List<IOpeningBillofLadings>();
				result.Add(new OpeningBillofLadingsProvider());
				return result;
			}
		}
	}
	class OpeningBillofLadingsProvider : IOpeningBillofLadings
	{
		public OpeningBillofLadingsProvider()
		{
		}

		public ZString InternalNoOpenBill => "";

		public ZString OpenedBillNumber => "YMLUM231260973";

		public ZString InternalNoOpenBill2 => "";

		public IEnumerable<IOpeningLadingLines> OpeningLadingLines
		{
			get
			{
				var result = new List<IOpeningLadingLines>();
				result.Add(new OpeningLadingLinesProvider());
				return result;
			}
		}
	}
	class OpeningLadingLinesProvider : IOpeningLadingLines
	{
		public OpeningLadingLinesProvider()
		{
		}

		public ZString WarehouseCode => "";
		public ZDecimal AmountInWarehouse => 0;
		public ZDecimal AmountTtoOpen => 0;
		public ZString BrandNo => "";
		public ZString ItemType => "";
		public ZString Unit => "";
		public ZInt TotalQuantity => 0;
		public ZInt AmountClosed => 0;
		public ZString MeasurementUnit => "";
		public ZInt OpeningLineNumber => 0;
	}
	class VehicleVisitedCountryProvider : IVehicleVisitedCountry
	{
		public VehicleVisitedCountryProvider(ZString manifestTypeCode)
		{
			ManifestTypeCode = manifestTypeCode;
		}
		protected readonly ZString ManifestTypeCode;

		public ZString PortLocationName => "TRMRA-001";
		public ZString CountryCode => "052";
		public ZDateTime MovementDateTime => ManifestTypeCode == "CIKONC" ? ZDateTime.Empty : new ZDateTime(2020, 2, 28);
	}
	class LadingExportsProvider : ILadingExports
	{
		public LadingExportsProvider()
		{
		}

		public ZDecimal GrossWeight => 1;
		public ZInt BoxQuantity => 1;
		public ZString ReferenceNumber => "18330100EX085097";
		public ZString IsSubType => "HAYIR";
		public ZString IsProcedure => "TCGB";
	}
}
