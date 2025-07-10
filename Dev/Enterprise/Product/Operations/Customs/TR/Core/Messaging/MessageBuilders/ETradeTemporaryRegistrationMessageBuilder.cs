using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.TCGB;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.TnsTypes;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Messaging
{
	public class ETradeTemporaryRegistrationMessageBuilder
	{
		public ETradeTemporaryRegistrationMessageBuilder()
		{
		}

		public ZString GetMessageText(ZString userId, ZString userPassword, ZString applicationReference, IETradeTemporaryRegistration header)
		{
			return GetGetMessageBodyText(userId, userPassword, applicationReference, header);
		}

		ZString GetGetMessageBodyText(ZString userId, ZString userPassword, ZString applicationReference, IETradeTemporaryRegistration header)
		{
			userPassword = TRManifestMessageBuilderHelper.MD5Hash(userPassword);

			ZString xmlManifestContent = AddETradeTempRegistration(header);
			ZString xmlEncoding = xmlManifestContent.Substring(0, 38);
			xmlManifestContent = xmlManifestContent.Substring(40).RemoveSafe(5, 99);
			ZString xmlBody = ZString.Format((NoResString)"{0}\r\n" +
											 (NoResString)"<Root>\r\n" +
											 (NoResString)"<RefID>{1}</RefID>\r\n" +
											 (NoResString)"<KullaniciAdi>{2}</KullaniciAdi>\r\n" +
											 (NoResString)"<Sifre>{3}</Sifre>\r\n" +
											 (NoResString)"<RequestMessage>\r\n{4}\r\n</RequestMessage>\r\n" +
											 (NoResString)"</Root>\r\n",
			xmlEncoding, applicationReference, userId, userPassword, xmlManifestContent);
			return xmlBody;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public string AddETradeTempRegistration(IETradeTemporaryRegistration header)
		{
			var tempReg = new Tcgb();
			tempReg.GuncellenecekTcgbGeciciTescilNo = header.ReferenceNoToUpdate.ToString();

			#region Declaration
			var declaration = new Beyan();
			declaration.Beyan1 = header.Declaration1.ToString();
			declaration.Beyan2 = header.Declaration2.ToString();
			declaration.Beyan3 = header.Declaration3.ToString();
			tempReg.Beyan = declaration;
			#endregion

			tempReg.KalemSayisi = header.TotalLineCount;
			tempReg.ToplamKapAdedi = header.TotalBoxQty;
			tempReg.ReferansNumarasi = header.ReferenceNo.ToString();

			#region DeclaringRepresentative
			var declaringRepresentative = new BeyanSahibiTemsilci();
			declaringRepresentative.AdiUnvani = header.DeclaringRepresentativeNameAndTitle.ToString();
			declaringRepresentative.VergiTcNo = header.DeclaringRepresentativeTCTaxNo.ToString();
			tempReg.BeyanSahibiTemsilci = declaringRepresentative;
			#endregion

			#region Vehicles
			var vehicleOnExit = new Collection<Arac>();
			vehicleOnExit.Add(new Arac()
			{
				Tipi = header.TypeOfVehicleOnExit.ToString(),
				Numarasi = header.VehiclePlateOfVehicleOnExit.ToString(),
				Ulke = header.CountryCodeOfVehicleOnExit.ToString()
			});
			tempReg.CikistakiAracBilgileri = vehicleOnExit;

			tempReg.Konteyner = header.IsContainer;

			var vehicleOnBorder = new Collection<Arac>();
			vehicleOnBorder.Add(new Arac()
			{
				Tipi = header.TypeOfVehicleOnBorder.ToString(),
				Numarasi = header.VehiclePlateOfVehicleOnBorder.ToString(),
				Ulke = header.CountryCodeOfVehicleOnBorder.ToString()
			});
			tempReg.SiniriGecenHareketliTasimaAraclari = vehicleOnBorder;
			#endregion

			#region TotalPrice
			var totalInvoicePrice = new DovizVeToplamFaturaBedeliBilgileri();
			var invoicePrice = new FaturaBedeliBilgisi();
			invoicePrice.DovizTuru = header.TotalInvoiceCurrencyCode.ToString();
			invoicePrice.Bedeli = header.TotalInvoiceCurrencyValue;
			invoicePrice.DovizKuru = header.TotalInvoiceExchangeRate;
			totalInvoicePrice.FaturaBedeliBilgisi = invoicePrice;
			totalInvoicePrice.FaturaBedeliBilgisiTurkLirasi = header.InvoiceAmountInformationTurkishLira;
			tempReg.DovizVeToplamFaturaBedeliBilgileri = totalInvoicePrice;

			tempReg.IstatistikiKiymet = header.StatisticalValue;

			var totalExpenses = new ToplamHarcamalar();
			if (header.TotalExpensesFreightCurrencyValue > 0)
			{
				var freight = new Navlun();
				freight.DovizTuru = header.TotalExpensesFreightCurrencyCode.ToString();
				freight.Bedeli = header.TotalExpensesFreightCurrencyValue;
				totalExpenses.NavlunBilgileri = freight;
			}
			else
			{
				totalExpenses.NavlunBilgileri = null;
			}

			if (header.TotalExpensesInsuranceCurrencyValue > 0)
			{
				var insurance = new Sigorta();
				insurance.DovizTuru = header.TotalExpensesInsuranceCurrencyCode.ToString();
				insurance.Bedeli = header.TotalExpensesInsuranceCurrencyValue;
				totalExpenses.SigortaBilgileri = insurance;
			}
			else
			{
				totalExpenses.SigortaBilgileri = null;
			}

			if (header.OtherOverseasExpenditureCurrencyValue > 0)
			{
				var otherOverseas = new DigerYurtDisiHarcama();
				otherOverseas.DovizTuru = header.TotalExpensesInsuranceCurrencyCode.ToString();
				otherOverseas.Bedeli = header.TotalExpensesInsuranceCurrencyValue;
				totalExpenses.DigerYurtDisiHarcamaBilgileri = otherOverseas;
			}
			else
			{
				totalExpenses.DigerYurtDisiHarcamaBilgileri = null;
			}
			totalExpenses.YurtIciHarcamalar = header.DomesticExpenditures;
			tempReg.ToplamHarcamalar = totalExpenses;
			#endregion

			tempReg.DahiliTasimaSekliKodu = header.TransportTypeCode.ToString();
			tempReg.YuklemeBosaltmaYeri = header.LoadingUnloadingPlace.ToString();
			tempReg.GirisCikisGumrukIdaresiKodu = header.CustomsOfficeCodeOfEntryExit.ToString();

			#region GoodsLocation
			var goodsLocation = new EsyaninBulunduguYer();
			goodsLocation.YerKodu = header.GoodsLocationCode.ToString();
			goodsLocation.YerAdi = header.GoodsLocationName.ToString();
			tempReg.EsyaninBulunduguYer = goodsLocation;
			#endregion

			tempReg.Ayarlama = header.Adjustment.ToString();
			tempReg.AntreponunTipiKodu = header.WarehouseTypeCode.ToString();

			#region PrincipleResponsible
			var principleResponsible = new AsilSorumlu();
			principleResponsible.AdiUnvani = header.PrincipleResponsibleNameAndTitle.ToString();
			principleResponsible.VergiTcNo = header.PrincipleResponsibleTCTaxNo.ToString();
			tempReg.AsilSorumlu = principleResponsible;
			#endregion

			#region CustomsOffice
			var customs = new Collection<Gumruk>();
			customs.Add(new Gumruk()
			{
				GumrukKodu = header.CustomsOfficeCodeOfPredicted.ToString(),
				Ulkesi = header.CountryCodeOfPredicted.ToString()
			});
			tempReg.OngorulenGumrukIdareleriVeUlke = customs;
			#endregion

			#region TotalGuarantees
			var totalGuarantees = new Collection<TcgbTeminat>();
			if (header.TotalGuaranteesValue > 0)
			{
				totalGuarantees.Add(new TcgbTeminat()
				{
					Turu = header.TotalGuaranteesType.ToString(),
					Tutari = header.TotalGuaranteesValue
				});
			}
			tempReg.ToplamTeminatlar = totalGuarantees;
			#endregion

			tempReg.VarisGumrukIdaresi = header.CustomsOfficeCodeOfDestination.ToString();

			#region Transfers
			var transfers = new Collection<Aktarma>();
			if (header.TransfersCountryCode != ZString.Empty)
			{
				var newVehicle = new TcgbAktarmaArac();
				newVehicle.AracReferansNumarasi = header.TransfersNewVehicleReferanceNumber.ToString();
				newVehicle.Ulkesi = header.TransfersNewCountryCode.ToString();

				transfers.Add(new Aktarma()
				{
					AktarmaUlkesi = header.TransfersCountryCode.ToString(),
					AktarmaYeri = header.TransfersPlace.ToString(),
					YeniTasitAraciBilgileri = newVehicle,
					Konteyner = header.IsTransfersContainer,
					OncekiKonteynerNo = header.TransfersPreviousContainerNo.ToString(),
					YeniKonteynerNo = header.TransfersNewContainerNo.ToString()
				});
			}
			tempReg.Aktarmalar = transfers;
			#endregion

			tempReg.Aciklamalar = header.Explanations.ToString();

			#region Bill
			var bills = header.Bills;
			var billList = new Collection<TasimaSenedi>();
			if (bills != null)
			{
				AddBillofLadings(bills, billList);
			}
			tempReg.TasimaSenetleri = billList;
			#endregion

			return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(tempReg);
		}

		static void AddBillofLadings(IEnumerable<IBill> bills, Collection<TasimaSenedi> billList)
		{
			foreach (var bill in bills)
			{
				#region Pack
				var packList = new Collection<KapBilgisi>();
				packList.Add(new KapBilgisi()
				{
					Turu = bill.PackType.ToString(),
					Adedi = bill.PackQuantity
				});
				#endregion

				#region Container
				var containerList = new Collection<KonteynerBilgisi>();
				if (!bill.ContainerNo.IsEmpty)
				{
					var containerPackList = new Collection<KapBilgisi>();
					containerPackList.Add(new KapBilgisi()
					{
						Turu = bill.ContainerPackType.ToString(),
						Adedi = bill.ContainerPackQuantity
					});

					containerList.Add(new KonteynerBilgisi()
					{
						KonteynerMarkasi = bill.ContainerBrand.ToString(),
						KonteynerNo = bill.ContainerNo.ToString(),
						KapBilgileri = containerPackList
					});
				}
				#endregion

				#region Person

				var forwarder = new Kisi();
				forwarder.AdiUnvani = bill.ForwarderNameAndTitle.ToString();
				forwarder.VergiTcNo = bill.ForwarderTCTaxNo.ToString();

				var consignee = new AliciKisi();
				consignee.Adi = bill.ConsigneeName.ToString();
				consignee.Unvani = bill.ConsigneeTitle.ToString();
				consignee.VergiTcNo = bill.ConsigneeTCTaxNo.ToString();
				consignee.CaddeSokakNo = bill.ConsigneeStreetNumber.ToString();
				consignee.Ilce = bill.ConsigneeTown.ToString();
				consignee.IlKodu = bill.ConsigneeCityCode.ToString();
				consignee.PostaKodu = bill.ConsigneePostalCode.ToString();

				var marketPlace = new Kisi();
				marketPlace.AdiUnvani = bill.MarketPlaceNameAndTitle.ToString();
				marketPlace.VergiTcNo = bill.MarketPlaceTCTaxNo.ToString();

				#endregion

				#region Delivery
				var delivery = new TeslimSekli();
				delivery.TeslimSekliProperty = bill.DeliveryMethod.ToString();
				delivery.TeslimYeri = bill.DeliverLocation.ToString();
				#endregion

				#region Exception
				var exceptionList = new Collection<Muafiyet>();

				void AddException(string code)
				{
					if (!string.Empty.Equals(code))
					{
						exceptionList.Add(new Muafiyet() { MuafiyetKodu = code.Replace("DIPL", "DİPL") });
					}
				}

				AddException(bill.ExceptionCode1);
				AddException(bill.ExceptionCode2);
				#endregion

				#region Documents
				var document = bill.Documents;
				var documentList = new Collection<Belge>();
				if (document != null)
				{
					AddBillDocument(document, documentList);
				}
				#endregion

				#region FinancalBank
				var financalBankList = new Collection<FinansBanka>();
				if (bill.FinancialBankingAmount > 0)
				{
					financalBankList.Add(new FinansBanka()
					{
						BankaKodu = bill.FinancialBankingCode.ToString(),
						OdemeSekli = bill.FinancialBankingPaymentType.ToString(),
						Tutar = bill.FinancialBankingAmount
					});
				}
				#endregion

				#region Invoice
				var invoiceList = new Collection<FaturaBedeliBilgisi>();
				if (bill.InvoiceAmount > 0)
				{
					invoiceList.Add(new FaturaBedeliBilgisi()
					{
						DovizTuru = bill.InvoiceCurrencyCode.ToString(),
						Bedeli = bill.InvoiceAmount,
						DovizKuru = bill.InvoiceExchangeRate
					});
				}
				#endregion

				#region Taxes
				var taxes = bill.Taxes;
				var taxeList = new Collection<Vergi>();
				if (taxes != null)
				{
					AddBillTaxes(taxes, taxeList);
				}

				var taxPayment = new VergiOdemesi();
				taxPayment.OdenecekVergiTutariToplami = bill.TaxPaymentTotalTaxPaymentAmount;
				taxPayment.TeminataBaglanacakVergiTutari = bill.TaxPaymentTaxAmountToBeBonded;
				taxPayment.SonraOdenecekVergiTutariToplami = bill.TaxPaymentTotalTaxAmountPayableLater;
				taxPayment.Toplam = bill.TaxPaymentTotal;
				#endregion

				#region Guarantee
				var guaranteeList = new Collection<TasimaSenediTeminat>();
				if (bill.GuaranteeAmount > 0)
				{
					guaranteeList.Add(new TasimaSenediTeminat()
					{
						Turu = bill.GuaranteeType.ToString(),
						Tutari = bill.GuaranteeAmount,
						ReferansNo = bill.GuaranteeReferenceNo.ToString()
					});
				}
				#endregion

				#region Expenses
				var freight = new Navlun();
				freight.DovizTuru = bill.FreightInformationCurrencyCode.ToString();
				freight.Bedeli = bill.FreightInformationValue;

				var insurence = new Sigorta();
				insurence.DovizTuru = bill.InsuranceInformationCurrencyCode.ToString();
				insurence.Bedeli = bill.InsuranceInformationValue;

				var otherExpense = new DigerYurtDisiHarcama();
				otherExpense.DovizTuru = bill.OtherOverseasExpansesCurrencyCode.ToString();
				otherExpense.Bedeli = bill.OtherOverseasExpansesValue;

				var expences = new ToplamHarcamalar();
				expences.NavlunBilgileri = bill.FreightInformationValue > 0 ? freight : null;
				expences.SigortaBilgileri = bill.InsuranceInformationValue > 0 ? insurence : null;
				expences.DigerYurtDisiHarcamaBilgileri = bill.OtherOverseasExpansesValue > 0 ? otherExpense : null;
				expences.YurtIciHarcamalar = bill.DomesticExpanses;
				#endregion

				#region PackLines
				var packLines = bill.Packs;
				var packLineList = new Collection<Kalem>();
				if (packLines != null)
				{
					AddBillPackLines(packLines, packLineList);
				}
				#endregion

				billList.Add(new TasimaSenedi()
				{
					TasimaSenediSiraNo = bill.BillOfLadingLineNo.ToString(),
					TasimaSenediNumarasi = bill.BillOfLadingNumber.ToString(),
					Kaplar = packList,
					Konteynerler = containerList,
					BrutAgirlik = bill.GrossWeight,
					NetAgirlik = bill.NetWeight,
					GondericiIhracatci = forwarder,
					Alici = consignee,
					Pazaryeri = marketPlace,
					SevkGidecegiUlkeKodu = bill.ReferralDestinationCountryCode.ToString(),
					TicaretYapilanUlkeKodu = bill.TradeCountryCode.ToString(),
					CikisIhracatUlkesiKodu = bill.ExportCountryCode.ToString(),
					GidecegiUlkeKodu = bill.DestinationCountryCode.ToString(),
					TeslimSekliBilgileri = delivery,
					IsleminNiteligi = bill.TransactionNature.ToString(),
					Muafiyetler = (exceptionList.Count > 0 ? exceptionList : null),
					RejimKodu = bill.RegimeCode.ToString(),
					Belgeler = documentList,
					FinansalVeBankacilikVerileri = (financalBankList.Count > 0 ? financalBankList : null),
					DovizVeToplamFaturaBedeliBilgileriTs = invoiceList,
					Vergiler = taxeList,
					VergiOdemesi = taxPayment,
					Teminatlar = guaranteeList,
					Harcamalar = expences,
					Kalemler = packLineList,
					Hacim = bill.Volume,
					TicaretSekli = bill.TradeType,
				});
			}
		}

		static void AddBillDocument(IEnumerable<IDocument> document, Collection<Belge> documentList)
		{
			foreach (var doc in document)
			{
				if (doc != null)
				{
					documentList.Add(new Belge()
					{
						Kodu = doc.Code.ToString(),
						BelgeTarihi = doc.DocumentDate.IsEmpty ? DateTime.MinValue : Convert.ToDateTime(doc.DocumentDate.ToSmallDateTime().ToString(), CultureInfo.InvariantCulture),
						ReferansNo = doc.ReferenceNo.ToString(),
						Dogrulama = doc.Verfication.ToString()
					});
				}
			}
		}

		static void AddBillTaxes(IEnumerable<ITax> taxes, Collection<Vergi> taxeList)
		{
			foreach (var tax in taxes)
			{
				if (tax != null)
				{
					taxeList.Add(new Vergi()
					{
						Kodu = tax.Code.ToString(),
						Tanimi = tax.Description.ToString(),
						Matrahi = tax.Base,
						Orani = tax.Rate,
						Tutari = tax.Amount,
						OdemeSekli = tax.PaymentType.ToString()
					});
				}
			}
		}

		static void AddBillPackLines(IEnumerable<IPack> packLines, Collection<Kalem> packLineList)
		{
			foreach (var line in packLines)
			{
				#region Goods
				var goodsList = new Collection<Esya>();
				goodsList.Add(new Esya()
				{
					SeriNumarasi = line.ItemsSerialNo.ToString(),
					Adedi = line.ItemsQuantity,
					Markasi = line.ItemsBrand.ToString(),
					Modeli = line.ItemsModel.ToString()
				});

				var goodCode = new EsyaKodu();
				goodCode.EsyaKodu1 = line.ItemCode1;
				goodCode.EsyaKodu2 = line.ItemCode2;
				goodCode.EsyaKodu3 = line.ItemCode3;
				#endregion

				#region PreferentialTariff
				var preferentialTariffList = new Collection<TercihliTarife>();
				if (!line.PreferentialTariffCode1.IsEmpty)
				{
					preferentialTariffList.Add(new TercihliTarife() { TercihliTarifeKodu = line.PreferentialTariffCode1 });
				}
				if (!line.PreferentialTariffCode2.IsEmpty)
				{
					preferentialTariffList.Add(new TercihliTarife() { TercihliTarifeKodu = line.PreferentialTariffCode2 });
				}
				#endregion

				#region SupplementaryMeasures
				var supplementaryMeasuresList = new Collection<TamamlayiciOlcu>();
				supplementaryMeasuresList.Add(new TamamlayiciOlcu() { Turu = line.SupplementaryMeasuresType1.ToString(), Miktari = line.SupplementaryMeasuresQuantity1 });
				if (!line.SupplementaryMeasuresType2.IsEmpty)
				{
					supplementaryMeasuresList.Add(new TamamlayiciOlcu() { Turu = line.SupplementaryMeasuresType2.ToString(), Miktari = line.SupplementaryMeasuresQuantity2 });
				}
				#endregion

				#region InvoiceValue
				var invoiceValue = new DovizVeFaturaBedeli();
				invoiceValue.DovizTuru = line.BillAmountCurrencyCode.ToString();
				invoiceValue.Bedeli = line.BillAmountValue;
				#endregion

				packLineList.Add(new Kalem()
				{
					KalemNo = line.PackNo.ToString(),
					EsyaninTicariTanimi = line.CommercialDescription.ToString(),
					Esyalar = goodsList,
					KullanilmisEsyaKodu = line.UsedItemCode.ToString(),
					EsyaKodu = goodCode,
					TercihliTarifeKodlari = preferentialTariffList.Count > 0 ? preferentialTariffList : null,
					MenseUlkeKodu = line.CountryCodeOfOrigin.ToString(),
					KiymetBildirimFormu = line.ValueStatementForm.ToString(),
					TarimPolitikasi = line.AgriculturePolicy.ToString(),
					Kota = line.IsQuota,
					TamamlayiciOlculer = supplementaryMeasuresList,
					DovizVeFaturaBedeli = invoiceValue,
					HesaplamaYontemi = line.CalculationMethod.ToString(),
					IstatistikiKiymet = line.StatisticalValue
				});
			}
		}
	}
}
