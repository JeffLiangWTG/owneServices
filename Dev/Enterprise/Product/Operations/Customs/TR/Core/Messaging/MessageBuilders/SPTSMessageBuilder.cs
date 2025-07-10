using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageDefinitions;
using CargoWise.Customs.TR.MessageDefinitions.SPTS;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public class SPTSMessageBuilder
	{
		public SPTSMessageBuilder(ISPTS provider)
		{
			Argument.NotNull(provider, nameof(provider));
			this.provider = provider;
		}
		readonly ISPTS provider;

		public ZString GetMessageText(ZString userId, ZString userPassword, ZString applicationReference)
		{
			var sptsMessage = new SPTSMessageHeader();
			sptsMessage.ApplicationReference = applicationReference;
			sptsMessage.UserId = userId;
			sptsMessage.UserPassword = TRManifestMessageBuilderHelper.MD5Hash(userPassword);
			sptsMessage.MessageBody = GetSPTSTransferInformation();

			return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithNamespaces(sptsMessage);
		}

		protected AktarmaBilgisi GetSPTSTransferInformation()
		{
			var xmlBodyContent = new AktarmaBilgisi();
			xmlBodyContent.HareketGumrukIdaresi = provider.PortOfPresentationDCode;
			xmlBodyContent.VarisGumrukIdaresi = provider.DestinationPortDCode;
			xmlBodyContent.BeyanSahibiVergiNo = provider.BusinessRegNo;
			xmlBodyContent.TasiyiciFirmaVergiNo = provider.CarrierBusinessRegNo;
			xmlBodyContent.TasimaSekli = provider.TransportType;
			xmlBodyContent.KullaniciKodu = provider.UserID;
			xmlBodyContent.SeferNumarasi = provider.VoyageNumber;
			xmlBodyContent.XmlRefId = provider.XmlRefId;

			if (provider.SPTSBills != null)
			{
				xmlBodyContent.AktarmaSenetleri = GetBills(provider.SPTSBills);
			}

			if (provider.SPTSUlds != null)
			{
				xmlBodyContent.AktarmaUldleri = GetUlds(provider.SPTSUlds);
			}

			xmlBodyContent.GuncellenecekTescilNo = provider.RegistrationNoToBeUpdated;

			if (!provider.VoyageDate.IsEmpty)
			{
				xmlBodyContent.SeferTarihi = Convert.ToDateTime(provider.VoyageDate.ToSmallDateTime().ToString(), CultureInfo.InvariantCulture);
			}

			return xmlBodyContent;
		}

		Collection<AktarmaSenediBilgisi> GetBills(IEnumerable<ISPTSBills> bills)
		{
			var collection = new Collection<AktarmaSenediBilgisi>();
			foreach (var bill in bills)
			{
				collection.Add(new AktarmaSenediBilgisi()
				{
					TasimaSenediNumarasi = bill.BillNumber,
					TasimaSenediSiraNumarasi = bill.BillOrderNo,
					BeyanTuru = bill.DeclarationType,
					BeyanNumarasi = bill.DeclarationNo,
					ParcaliMi = bill.IsSubType,
					AktarmaSatirlari = GetBillLines(bill.SPTSBillLines)
				});
			}
			return collection;
		}

		Collection<AktarmaSatirBilgisi> GetBillLines(IEnumerable<ISPTSBillLines> sPTSBillLines)
		{
			var collection = new Collection<AktarmaSatirBilgisi>();
			if (sPTSBillLines != null)
			{
				foreach (var item in sPTSBillLines)
				{
					collection.Add(new AktarmaSatirBilgisi()
					{
						SatirSiraNumarasi = item.LineOrderNo,
						SatirCntNumarasi = item.LineContainerNo
					});
				}
			}

			return collection;
		}

		Collection<AktarmaUldBilgisi> GetUlds(IEnumerable<ISPTSUlds> sPTSUlds)
		{
			var collection = new Collection<AktarmaUldBilgisi>();
			foreach (var item in sPTSUlds)
			{
				collection.Add(new AktarmaUldBilgisi()
				{
					UldSiraNumarasi = item.UldOrderNo,
					UldNumarasi = item.UldNumber
				});
			}

			return collection;
		}
	}
}
