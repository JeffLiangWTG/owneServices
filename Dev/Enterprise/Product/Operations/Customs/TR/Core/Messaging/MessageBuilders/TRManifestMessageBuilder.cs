using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageDefinitions.Manifest.YeniOzetBeyan;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Messaging
{
	public class TRManifestMessageBuilder
	{
		public TRManifestMessageBuilder(ISummaryDeclarationInformation provider)
		{
			Argument.NotNull(provider, nameof(provider));
		}

		public ZString GetMessageText(ZString userId, ZString userPassword, ZString applicationReference, ISummaryDeclarationInformation header)
		{
			return GetMessageBodyText(userId, userPassword, applicationReference, header);
		}

		ZString GetMessageBodyText(ZString userId, ZString userPassword, ZString applicationReference, ISummaryDeclarationInformation header)
		{
			userPassword = TRManifestMessageBuilderHelper.MD5Hash(userPassword);

			ZString xmlManifestContent = AddSummaryDeclarationInformation(header);
			ZString xmlEncoding = xmlManifestContent.Substring(0, 38);
			xmlManifestContent = xmlManifestContent.Substring(40).Replace("<OzetBeyanBilgisi>", "<OzetBeyanBilgisi xmlns=\"http://www.gumruk.gov.tr/\">");

			ZString xmlBody = ZString.Format((NoResString)"{0}\r\n" +
											 (NoResString)"<Gelen xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
											 (NoResString)"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
											 (NoResString)"xmlns=\"http://LoginKontrol.YeniOzetBeyanGelen\">\r\n" +
											 (NoResString)"<RefID xmlns=\"\">{1}</RefID>\r\n" +
											 (NoResString)"<KullaniciAdi xmlns=\"\">{2}</KullaniciAdi>\r\n" +
											 (NoResString)"<Sifre xmlns=\"\">{3}</Sifre>\r\n" +
											 (NoResString)"{4}\r\n" +
											 (NoResString)"</Gelen>\r\n",
											 xmlEncoding, applicationReference, userId, userPassword, xmlManifestContent);
			return xmlBody;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
		public string AddSummaryDeclarationInformation(ISummaryDeclarationInformation header)
		{
			var sumDec = new OzetBeyanBilgisi();

			sumDec.BeyanSahibiVergiNo = header.BusinessRegNo.IsEmpty ? null : header.BusinessRegNo.ToString();
			sumDec.BeyanTuru = header.ManifestType.IsEmpty ? null : header.ManifestType.ToString();
			sumDec.DorseNo1 = header.Trailer1RegNo.IsEmpty ? null : header.Trailer1RegNo.ToString();
			sumDec.DorseNo1Uyrugu = header.Trailer1RegCountry.IsEmpty ? null : header.Trailer1RegCountry.ToString();
			sumDec.DorseNo2 = header.Trailer2RegNo.IsEmpty ? null : header.Trailer2RegNo.ToString();
			sumDec.DorseNo2Uyrugu = header.Trailer2RegCountry.IsEmpty ? null : header.Trailer2RegCountry.ToString();
			sumDec.Diger = TRManifestMessageBuilderHelper.IsTagNull(header.ManifestType, (NoResString)"Diger", header.Other.IsEmpty) ? null : header.Other.ToString();
			sumDec.EkBelgeSayisi = TRManifestMessageBuilderHelper.IsTagNull(header.ManifestType, "EkBelgeSayisi", header.NumberofBillsInDeclaration.IsEmpty) ? 0 : Convert.ToInt32(header.NumberofBillsInDeclaration);

			if (header.ManifestType != "CIKONC" && header.ManifestType != "VARONC" && header.ManifestType != "EMANIF")
			{
				sumDec.GrupTasimaSenediNo = header.GroupBillofLadingNumber.IsEmpty ? null : header.GroupBillofLadingNumber.ToString();
				sumDec.OncekiBeyanNo = header.PreviousBillNumber.IsEmpty ? null : header.PreviousBillNumber.ToString();
				sumDec.VarisCikisGumrukIdaresi = header.PresentationCustomsOffice.IsEmpty ? null : header.PresentationCustomsOffice.ToString();
			}
			else
			{
				sumDec.GrupTasimaSenediNo = header.GroupBillofLadingNumber;
				sumDec.OncekiBeyanNo = TRManifestMessageBuilderHelper.IsTagNull(header.ManifestType, "OncekiBeyanNo", header.PreviousBillNumber.IsEmpty) ? null : header.PreviousBillNumber.ToString();
				sumDec.VarisCikisGumrukIdaresi = TRManifestMessageBuilderHelper.IsTagNull(header.ManifestType, "VarisCikisGumrukIdaresi", header.PresentationCustomsOffice.IsEmpty) ? null : header.PresentationCustomsOffice.ToString();
			}

			sumDec.GumrukIdaresi = header.CustomsOffice.IsEmpty ? null : header.CustomsOffice.ToString();
			sumDec.KullaniciKodu = header.UserID;
			sumDec.Kurye = TRManifestMessageBuilderHelper.IsTagNull(header.ManifestType, "Kurye", header.AgentType.IsEmpty) ? null : header.AgentType.ToString();
			sumDec.LimanYerAdiBos = header.CustomsDischargePort.IsEmpty ? null : header.CustomsDischargePort.ToString();
			sumDec.LimanYerAdiYuk = header.CustomsLoadPort.IsEmpty ? null : header.CustomsLoadPort.ToString();
			sumDec.PlakaSeferNo = header.Voyage.IsEmpty ? null : header.Voyage.ToString();
			sumDec.ReferansNumarasi = header.LloydsNumber.IsEmpty ? null : header.LloydsNumber.ToString();

			if (header.RegistrationNumber.IsEmpty && header.ManifestType == "EMANIF")
			{
				sumDec.RefNo = header.RegistrationNumber.ToString();
			}
			else
			{
				sumDec.RefNo = header.RegistrationNumber.IsEmpty ? null : header.RegistrationNumber.ToString();
			}

			sumDec.Rejim = header.Nature;
			sumDec.TasimaSekli = header.TransportType.IsEmpty ? null : header.TransportType.ToString();

			var bills = header.BillofLadings;
			var billCollection = new Collection<TasimaSenediBilgisi>();
			AddBillofLadings(header.ManifestType, bills, billCollection);
			sumDec.TasimaSenetleri = billCollection;

			var openings = header.OpeningSummaryDeclaration;
			var openingsCollection = new Collection<OzbyAcmaBilgisi>();
			AddManifestToOpen(openings, openingsCollection);
			if (header.RegistrationNumber.IsEmpty && header.ManifestType != "DEMİHR")
			{
				sumDec.OzbyAcmalar = openingsCollection;
			}

			sumDec.TasitinAdi = header.Vessel.IsEmpty ? null : header.Vessel.ToString();

			var vehicleVisitedCountry = header.VehicleVisitedCountry;
			var vehicleVisitedCountryCollection = new Collection<TasitinUgradigiUlkeBilgisi>();
			AddVehicleVisitedCountry(header.ManifestType, vehicleVisitedCountry, vehicleVisitedCountryCollection);
			sumDec.TasitinUgradigiUlkeler = TRManifestMessageBuilderHelper.IsTagNull(header.ManifestType, "TasitinUgradigiUlkeler", false) ? null : vehicleVisitedCountryCollection;

			var carrierCompanies = header.CarrierCompany;
			var listCarrierCampany = new List<FirmaBilgisi>();
			var carrierCampany = new FirmaBilgisi();
			if (header.CarrierBusinessRegNo.IsEmpty)
			{
				AddCarrierCompany(carrierCompanies, listCarrierCampany);
				if (listCarrierCampany.Count > 0)
				{
					carrierCampany = listCarrierCampany[0];
				}
			}
			sumDec.TasiyiciFirma = header.ManifestType == "GRUPAJ" ? null : carrierCampany;
			sumDec.TasiyiciVergiNo = header.CarrierBusinessRegNo.ToString();

			sumDec.TirAtaKarneNo = header.TruckATAScorecardNumber.IsEmpty ? null : header.TruckATAScorecardNumber.ToString();
			sumDec.UlkeKodu = header.ConveyanceNationality.IsEmpty ? null : header.ConveyanceNationality.ToString();
			sumDec.UlkeKoduBos = header.CustomsDischargeCountryCode.IsEmpty ? null : header.CustomsDischargeCountryCode.ToString();
			sumDec.UlkeKoduYuk = header.CustomsLoadCountryCode.IsEmpty ? null : header.CustomsLoadCountryCode.ToString();
			sumDec.VarisTarihSaati = header.DateAtCustomsOffice.IsEmpty ? DateTime.MinValue : Convert.ToDateTime(header.DateAtCustomsOffice.ToSmallDateTime().ToString(), CultureInfo.InvariantCulture);
			sumDec.YuklemeBosaltmaYeri = header.LoadingUnloadingPlace.IsEmpty ? null : header.LoadingUnloadingPlace.ToString();
			sumDec.XmlRefId = header.XmlRefId;
			sumDec.EmniyetGuvenlik = header.SafetySecurity.IsEmpty ? null : header.SafetySecurity.ToString();

			return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(sumDec);
		}
		static void AddManifestToOpen(IEnumerable<IOpeningSummaryDeclaration> manifestToOpens, Collection<OzbyAcmaBilgisi> openingCollection)
		{
			foreach (var manifestToOpen in manifestToOpens)
			{
				var billCollection = new Collection<OzbyAcmaSenetBilgisi>();
				foreach (var bill in manifestToOpen.OpeningBillofLadings)
				{
					var lineCollection = new Collection<OzbyAcmaSatirBilgisi>();
					foreach (var line in bill.OpeningLadingLines)
					{
						lineCollection.Add(new OzbyAcmaSatirBilgisi()
						{
							AmbarKodu = line.WarehouseCode,
							AcilacakMiktar = decimal.ToDouble(line.AmountTtoOpen),
							AcmaSatirNo = line.OpeningLineNumber,
							AmbardakiMiktar = line.TotalQuantity
						});
					}

					billCollection.Add(new OzbyAcmaSenetBilgisi()
					{
						AcilanSenetNo = bill.OpenedBillNumber,
						OzbyAcmaSatirlari = manifestToOpen.HowToOpen == "3" ? lineCollection : null
					});
				}

				openingCollection.Add(new OzbyAcmaBilgisi()
				{
					AcmaSekli = manifestToOpen.HowToOpen,
					AmbardaMi = manifestToOpen.InWarehouse,
					BeyannameNo = manifestToOpen.DeclarationNo,
					BaskaRejimleAcilacakMi = manifestToOpen.WillOpenAnotherRegime,
					Aciklama = manifestToOpen.WillOpenAnotherRegime == "EVET" || !manifestToOpen.Explanation.IsEmpty ? manifestToOpen.Explanation : null,
					OzbyAcmaSenetleri = manifestToOpen.HowToOpen != "1" ? billCollection : null
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void AddBillofLadings(ZString manifestType, IEnumerable<IBillofLading> bills, Collection<TasimaSenediBilgisi> billCollection)
		{
			foreach (var bill in bills)
			{
				var lines = bill.LadingLines;
				var lineCollection = new Collection<TasimaSatiriBilgisi>();
				AddLadingLines(manifestType, lines, lineCollection);

				var ladingExports = bill.LadingExports;
				var ladingExportCollection = new Collection<IhracatBilgisi>();
				AddLadingExports(ladingExports, ladingExportCollection);

				var visitedCountries = bill.BillVisitedCountry;
				var visitedCountryCollection = new Collection<UgranilanUlkeBilgisi>();
				AddVisitedCountry(visitedCountries, visitedCountryCollection);

				billCollection.Add(new TasimaSenediBilgisi()
				{
					AcentaAdi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "AcentaAdi", bill.ContainerAgentName.IsEmpty) ? null : bill.ContainerAgentName.ToString(),
					AcentaVergiNo = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "AcentaVergiNo", bill.ContainerAgentRegNo.IsEmpty) ? null : bill.ContainerAgentRegNo.ToString(),
					AliciAdi = bill.ConsigneeName.ToString(),
					AliciVergiNo = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "AliciVergiNo", bill.ConsigneeRegNo.IsEmpty) ? null : bill.ConsigneeRegNo.ToString(),
					AmbarHariciMi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "AmbarHariciMi", bill.IsWarehouseExternal.IsEmpty) ? null : bill.IsWarehouseExternal.ToString(),
					BildirimTarafiAdi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "BildirimTarafiAdi", bill.NotifyPartyName.IsEmpty) ? null : bill.NotifyPartyName.ToString(),
					BildirimTarafiVergiNo = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "BildirimTarafiVergiNo", bill.NotifyPartyRegNo.IsEmpty) ? null : bill.NotifyPartyRegNo.ToString(),
					DuzenlendigiUlke = bill.Origin.IsEmpty ? null : bill.Origin.ToString(),
					EsyaninBulunduguYer = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "EsyaninBulunduguYer", bill.GoodsLocation.IsEmpty) ? null : bill.GoodsLocation.ToString(),
					FaturaDoviz = bill.TransportTotalValue.IsEmpty ? null : bill.TransportValueCurrency.ToString(),
					FaturaToplami = decimal.ToDouble(bill.TransportTotalValue),
					GondericiAdi = bill.ShipperName,
					GondericiVergiNo = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "GondericiVergiNo", bill.ShipperRegNo.IsEmpty) ? null : bill.ShipperRegNo.ToString(),
					GrupMu = bill.IsGroup,
					Ihracatlar = manifestType == "GRUPAJ" ? null : ladingExportCollection,
					KonteynerMi = bill.Iscontainer.ToString(),
					NavlunDoviz = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "NavlunDoviz", bill.NKFreightValueCurrency.IsEmpty) ? null : bill.NKFreightValueCurrency.ToString(),
					NavlunTutari = decimal.ToDouble(bill.FreightValue),
					OdemeSekli = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "OdemeSekli", bill.PaymentType.IsEmpty) ? null : bill.PaymentType.ToString(),
					OncekiSeferNumarasi = bill.PreviousVoyageNo.IsEmpty ? null : bill.PreviousVoyageNo.ToString(),
					OncekiSeferTarihi = bill.PreviousVoyageArrivalDate.IsEmpty ? DateTime.MinValue : Convert.ToDateTime(bill.PreviousVoyageArrivalDate.ToSmallDateTime().ToString(), CultureInfo.InvariantCulture),
					OzetBeyanNo = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "OzetBeyanNo", bill.EntryNumber.IsEmpty) ? null : bill.EntryNumber.ToString(),
					RoroMu = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "RoroMu", bill.IsRoro.IsEmpty) ? null : bill.IsRoro.ToString(),
					SenetSiraNo = bill.SequenceNo.ToString(),
					TasimaSenediNo = bill.BillNumber,
					AktarmaYapilacakMi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "AktarmaYapilacakMi", bill.IsTransshipmentType.IsEmpty) ? null : bill.IsTransshipmentType.ToString(),
					AktarmaTipi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "AktarmaTipi", bill.TransshipmentType.IsEmpty) ? null : bill.TransshipmentType.ToString(),
					TasimaSatirlari = lineCollection,
					EmniyetGuvenlikT = bill.SafetySecurityT.IsEmpty ? null : bill.SafetySecurityT.ToString(),
					UgranilanUlkeler = visitedCountryCollection,
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void AddLadingLines(ZString manifestType, IEnumerable<ILadingLines> lines, Collection<TasimaSatiriBilgisi> lineCollection)
		{
			foreach (var line in lines)
			{
				var goods = line.GoodsInformation;
				var goodCollection = new Collection<EsyaBilgisi>();
				AddGoodsInformation(goods, goodCollection);

				lineCollection.Add(new TasimaSatiriBilgisi()
				{
					BrutAgirlik = decimal.ToDouble(line.GrossWeight),
					EsyaBilgileri = goodCollection,
					KapAdedi = line.PackQuantity,
					KapCinsi = line.PackType.IsEmpty ? null : line.PackType.ToString(),
					KonteynerTipi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "KonteynerTipi", line.ContainerType.IsEmpty) ? null : line.ContainerType.ToString(),
					MarkaNo = line.ContainerNumber,
					MuhurNumarasi = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "MuhurNumarasi", line.SealNumber.IsEmpty) ? null : line.SealNumber.ToString(),
					NetAgirlik = decimal.ToDouble(line.NetWeight),
					OlcuBirimi = line.WeightUQ.IsEmpty ? null : line.WeightUQ.ToString(),
					SatirNo = line.LineNo,
					KonteynerYukDurumu = line.ContainerLoadStatus.IsEmpty ? null : line.ContainerLoadStatus.ToString()
				});
			}
		}
		static void AddGoodsInformation(IEnumerable<IGoodsInformation> goods, Collection<EsyaBilgisi> goodList)
		{
			foreach (var good in goods)
			{
				goodList.Add(new EsyaBilgisi()
				{
					BmEsyaKodu = good.UNGoodCode.IsEmpty ? null : good.UNGoodCode,
					BrutAgirlik = decimal.ToDouble(good.GrossWeight),
					EsyaKodu = good.TariffCode.IsEmpty ? null : good.TariffCode.ToString(),
					EsyaninTanimi = good.GoodsDescription.IsEmpty ? null : good.GoodsDescription.ToString(),
					KalemFiyati = decimal.ToDouble(good.GoodsValue),
					KalemFiyatiDoviz = good.GoodsValueCurrency.IsEmpty ? null : good.GoodsValueCurrency.ToString(),
					KalemSiraNo = good.OrderNo,
					NetAgirlik = decimal.ToDouble(good.NetWeight),
					OlcuBirimi = good.CustomsUQ.IsEmpty ? null : good.CustomsUQ.ToString()
				});
			}
		}
		static void AddLadingExports(IEnumerable<ILadingExports> ladingExports, Collection<IhracatBilgisi> listladingExports)
		{
			foreach (var ihracat in ladingExports)
			{
				listladingExports.Add(new IhracatBilgisi()
				{
					Tipi = ihracat.IsProcedure,
					Numarasi = ihracat.ReferenceNumber,
					ParcaliMi = ihracat.IsSubType,
					KapAdedi = ihracat.BoxQuantity,
					BrutAgirlik = decimal.ToDouble(ihracat.GrossWeight),
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void AddVisitedCountry(IEnumerable<IBillVisitedCountry> visitedCountries, Collection<UgranilanUlkeBilgisi> visitedCountryCollection)
		{
			foreach (var country in visitedCountries)
			{
				visitedCountryCollection.Add(new UgranilanUlkeBilgisi()
				{
					LimanYerAdi = country.PortLocationName.IsEmpty ? null : country.PortLocationName.ToString(),
					UlkeKodu = country.CountryCode.IsEmpty ? null : country.CountryCode.ToString()
				});
			}
		}
		static void AddVehicleVisitedCountry(ZString manifestType, IEnumerable<IVehicleVisitedCountry> visitedCountries, Collection<TasitinUgradigiUlkeBilgisi> visitedCountryCollection)
		{
			foreach (var country in visitedCountries)
			{
				visitedCountryCollection.Add(new TasitinUgradigiUlkeBilgisi()
				{
					HareketTarihSaati = TRManifestMessageBuilderHelper.IsTagNull(manifestType, "HareketTarihSaati", country.MovementDateTime.IsEmpty) ? null : country.MovementDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
					LimanYerAdi = country.PortLocationName.IsEmpty ? null : country.PortLocationName.ToString(),
					UlkeKodu = country.CountryCode.IsEmpty ? null : country.CountryCode.ToString()
				});
			}
		}
		static void AddCarrierCompany(IEnumerable<ICarrierCompany> carrierCompany, List<FirmaBilgisi> carrierCompanyList)
		{
			foreach (var company in carrierCompany)
			{
				carrierCompanyList.Add(new FirmaBilgisi()
				{
					AdiUnvani = company.CarrierName.IsEmpty ? null : company.CarrierName.ToString(),
					CadSNo = company.StreetNo.IsEmpty ? null : company.StreetNo.ToString(),
					Fax = company.Fax.IsEmpty ? null : company.Fax.ToString(),
					IlIlce = company.ProvinceDistrict.IsEmpty ? null : company.ProvinceDistrict.ToString(),
					KimlikTuru = company.IdentityType.IsEmpty ? null : company.IdentityType.ToString(),
					KimlikNo = company.IdentificationNumber.IsEmpty ? null : company.IdentificationNumber.ToString(),
					PostaKodu = company.PostCode.IsEmpty ? null : company.PostCode.ToString(),
					Tel = company.Phone.IsEmpty ? null : company.Phone.ToString(),
					UlkeKodu = company.CountryCode.IsEmpty ? null : company.CountryCode.ToString(),
					VergiDairesikodu = company.TaxOfficeCode.IsEmpty ? null : company.TaxOfficeCode.ToString()
				});
			}
		}
	}
}
