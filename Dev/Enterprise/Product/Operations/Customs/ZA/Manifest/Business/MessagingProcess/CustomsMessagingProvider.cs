using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;
using Enterprise.Environment;

namespace Enterprise.Customs.ZA.Manifest.Business.MessagingProcess
{
	public sealed class CustomsMessagingProviderFactory : ICustomsMessagingProviderFactory
	{
		public CustomsMessagingProviderFactory(string msgSubType, IReadOnlyCollection<AsycudaBill> selectedBillCountries = null)
		{
			this.msgSubType = msgSubType;
			this.selectedBillCountries = selectedBillCountries;
		}
		readonly string msgSubType;
		readonly IReadOnlyCollection<AsycudaBill> selectedBillCountries;

		ICustomsMessagingProvider ICustomsMessagingProviderFactory.CreateProvider(BusinessObject businessObject) => CustomsMessagingProvider.New((AsycudaManifestHeader)businessObject, msgSubType, selectedBillCountries);
	}

	public sealed class CustomsMessagingProvider : ICustomsMessagingProvider, ISupportPreSendValidation
	{
		public static CustomsMessagingProvider New(AsycudaManifestHeader header, string msgSubType, IReadOnlyCollection<AsycudaBill> selectedBillCountries = null)
		{
			var carHeader = new CusCarHeader(header);

			return new CustomsMessagingProvider(header, carHeader, selectedBillCountries,
				selectedBillCountries == null ?
					CreateMessengers(header, carHeader, msgSubType) :
					CreateBillMessengers(carHeader, msgSubType, selectedBillCountries));
		}

		static List<ICustomsMessenger> CreateMessengers(AsycudaManifestHeader header, CusCarHeader cuscarHeader, string msgSubType)
		{
			var list = new List<ICustomsMessenger>();

			if (!cuscarHeader.HasBillsAndPacks)
			{
				list.Add(CustomsMessenger.New(header, cuscarHeader, cuscarHeader, msgSubType, ZString.Empty));
			}
			else
			{
				var bills = header.Bills.Cast<AsycudaBill>().ToArray();
				foreach (var issuer in bills.Select(x => x.ABL_BillIssuer).Distinct())
				{
					list.Add(CustomsMessenger.New(header, cuscarHeader, cuscarHeader, msgSubType, issuer));
				}
			}

			return list;
		}

		static List<ICustomsMessenger> CreateBillMessengers(CusCarHeader cuscarHeader, string msgSubType, IReadOnlyCollection<AsycudaBill> selectedBillCountries)
		{
			var list = new List<ICustomsMessenger>();

			foreach (var bill in selectedBillCountries)
			{
				var filteredHelper = new CusCarMessagingHelper.SingleBillCusCarHeader(cuscarHeader, bill);
				var billFunction = CusCarMessagingHelper.GetBillFunction(msgSubType, cuscarHeader, bill);
				var issuer = bill.ABL_BillIssuer;

				list.Add(CustomsMessenger.New(filteredHelper, filteredHelper, cuscarHeader, billFunction, issuer));
			}

			return list;
		}

		CustomsMessagingProvider(AsycudaManifestHeader header, CusCarHeader cuscarHeader, IReadOnlyCollection<AsycudaBill> selectedBillCountries, IReadOnlyCollection<ICustomsMessenger> messengers)
		{
			this.header = header;
			this.cuscarHeader = cuscarHeader;
			this.messengers = messengers;
			this.selectedBillCountries = selectedBillCountries;
		}
		readonly AsycudaManifestHeader header;
		readonly CusCarHeader cuscarHeader;
		readonly IReadOnlyCollection<ICustomsMessenger> messengers;
		readonly IReadOnlyCollection<AsycudaBill> selectedBillCountries;

		bool ICustomsMessagingProvider.IsInTestMode => Env.Registry.ZACustoms.GetIsTestMode(header.Branch);

		bool ICustomsMessagingProvider.EnableTestModeValidation => true;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => messengers;

		IReadOnlyCollection<MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult)
		{
			var results = new List<MessageSendingNotification>();

			var manifestType = ((ICusCarHeader)cuscarHeader)?.ManifestDocumentType ?? ManifestDocumentType.None;

			if (!CUSCARMessageBuilder.IsManifestTypeSupported(manifestType))
			{
				results.Add(new MessageSendingError(string.Format(manifestTypeError, manifestType)));
			}

			if (selectedBillCountries != null)
			{
				if (!selectedBillCountries.Any())
				{
					results.Add(new MessageSendingError(billcreatedError));
				}
			}
			else
			{
				foreach (var bill in header.Bills.Where(x => x.IsIssuerCodeMandatory && x.ABL_BillIssuer.IsEmpty))
				{
					results.Add(new MessageSendingError(string.Format(billIssuerError, bill.ABL_BillNumber)));
				}

				if (cuscarHeader.HasBillsAndPacks && !header.Bills.Cast<AsycudaBill>().Any())
				{
					results.Add(new MessageSendingError(billcreatedError));
				}
			}

			return results;
		}

		string manifestTypeError => Res.GetString("8C7BF3BA-5BC0-4DB9-B8F3-92E08DF678FB", "Cannot create a manifest message for manifest type '{0}'.");
		string billcreatedError => Res.GetString("9F1409E1-FFBC-4A2C-BD48-AE064C599F67", "At least one Bill must be created for sending.");
		string billIssuerError => Res.GetString("524A3E6A-40A0-48E4-B687-8556B0023552", "Bill '{0}' must have a Bill Issuer.");
	}
}
