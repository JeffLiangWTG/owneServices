using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.BosaltmaListesi;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.TnsTypes;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public class ETradeDischargeListMessageBuilder
	{
		public ETradeDischargeListMessageBuilder(IDischargeList provider)
		{
			CargoWise.Common.Argument.NotNull(provider, nameof(provider));
		}

		public ZString GetMessageText(ZString userId, ZString userPassword, ZString applicationReference, IDischargeList header)
		{
			userPassword = TRManifestMessageBuilderHelper.MD5Hash(userPassword);

			var xmlContent = AddDischargeList(header);
			var xmlBody = TRMessageHelper.GenerateXml(applicationReference, userId, userPassword, xmlContent);
			return xmlBody;
		}

		ZString AddDischargeList(IDischargeList header)
		{
			var dischargeList = new BosaltmaListesi();

			dischargeList.BeyannameNo = header.RegistrationNo;
			dischargeList.GumrukIdaresi = header.CustomsOffice;
			dischargeList.TasimaSenetleri = GetBills(header.Bills);

			var esyaninBulunduguYer = new EsyaninBulunduguYer();
			esyaninBulunduguYer.YerKodu = header.GoodsLocationCode;
			esyaninBulunduguYer.YerAdi = header.GoodsLocationName;
			dischargeList.EsyaninBulunduguYer = esyaninBulunduguYer;

			var beyanSahibiTemsilci = new BeyanSahibiTemsilci();
			beyanSahibiTemsilci.AdiUnvani = header.DeclarationOwnerRepresentativeNameAndTitle;
			beyanSahibiTemsilci.VergiTcNo = header.DeclarationOwnerRepresentativeTaxNo;
			dischargeList.BeyanSahibiTemsilci = beyanSahibiTemsilci;

			var result = XmlObjectSerializer.SerializeWithNamespaces(
				xmlObject: dischargeList,
				encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
				settings: new XmlWriterSettings { OmitXmlDeclaration = true, },
				namespaces: new XmlSerializerNamespaces(new[] { new XmlQualifiedName(string.Empty, "http://tempuri.org/") }
			));

			return result;
		}

		Collection<TasimaSenediBl> GetBills(IEnumerable<IBillBL> bills)
		{
			var collection = new Collection<TasimaSenediBl>();
			foreach (var bill in bills)
			{
				collection.Add(new TasimaSenediBl()
				{
					Alici = GetPerson(bill.ConsigneeNameAndTitle, bill.ConsigneeTaxNo),
					GondericiIhracatci = GetPerson(bill.ShipperName, ""),
					KonteynerMi = bill.IsContainer ? 1 : 0,
					TasimaSenediNo = bill.BillNo,
					TasimaSenediSiraNo = bill.LineNo,
					TasimaSatirlari = GetBillLines(bill)
				});
			}
			return collection;
		}

		Collection<TasimaSatirlariBl> GetBillLines(IBillBL bill)
		{
			var collection = new Collection<TasimaSatirlariBl>();
			collection.Add(new TasimaSatirlariBl()
			{
				SiraNo = bill.SequenceNo,
				KapAdedi = bill.PackQuantity,
				KapCinsi = bill.PackType,
				MarkaNo = bill.MarksAndNumbers,
				KonteynerTipi = ZString.Empty,
				OlcuBirimi = bill.Unit,
				BrutAgirlik = bill.GrossWeight,
				NetAgirlik = bill.NetWeight,
				Kalemler = GetPacks(bill.Packs)
			});
			return collection;
		}

		Collection<KalemBl> GetPacks(IEnumerable<IPackBL> packs)
		{
			var collection = new Collection<KalemBl>();
			foreach (var pack in packs)
			{
				collection.Add(new KalemBl()
				{
					KalemSiraNo = pack.LineNo,
					EsyaCinsi = pack.GoodDescription,
					EsyaKodu = pack.Tariff,
					OlcuBirimi = pack.Unit,
					BrutAgirlik = pack.GrossWeight,
					NetAgirlik = pack.NetWeight
				});
			}
			return collection;
		}

		Kisi GetPerson(ZString nameOrTitle, ZString taxOrTCNo)
		{
			return new Kisi()
			{
				AdiUnvani = nameOrTitle,
				VergiTcNo = taxOrTCNo
			};
		}
	}
}
