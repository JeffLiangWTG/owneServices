using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business
{
	public enum QueryTypeForQuotaVisaADDCVD { NA, Quota, Visa, ADDCVD }

	public class VisaQuotaADDCVDQuerySendingAction : ImportMessageSendingAction
	{
		public VisaQuotaADDCVDQuerySendingAction(ZString tariffNumber, ZString secondaryTariffNumber, ZString origin, VisaQuotaADDCVDQuerySendingActionCollection actions, QueryTypeForQuotaVisaADDCVD queryType)
			: base(actions)
		{
			this.queryType = queryType;
			this.US_TariffNumber = tariffNumber;
			this.US_SecondaryTariffNumber = secondaryTariffNumber;
			this.US_UC_NKOrigin = CanadaProvinceTerritoryCodes.IsCanadianProvince(origin) ? new ZString(Core.Constants.CountryCodes.Canada) : origin;
		}

		internal readonly QueryTypeForQuotaVisaADDCVD queryType;
		internal readonly ZString US_TariffNumber;
		internal readonly ZString US_SecondaryTariffNumber;
		internal readonly ZString US_UC_NKOrigin;

		public void SendQueryMessagesWithoutSaving()
		{
			ReferenceFileRequester requester = new ReferenceFileRequester(Factory);

			QTAU1 u1 = new QTAU1();
			u1.TariffNumberTextileCategoryNumberOrVisaNumber = US_TariffNumber;
			u1.SecondTariffNumber = US_SecondaryTariffNumber;
			u1.CountryOfOrigin = US_UC_NKOrigin;

			if (queryType == QueryTypeForQuotaVisaADDCVD.Quota)
			{
				u1.VisaQueryIndicator = "";//quota request
			}
			else if (queryType == QueryTypeForQuotaVisaADDCVD.Visa)
			{
				u1.VisaQueryIndicator = QueryTypeList.Codes.AllVisaRecordsForCountryCategoryOrCountryTariff;
			}

			MQEDIMessage message = requester.RequestSimple(ApplicationIdentifierCodeList.Codes.QueryQuota, u1, null, EM_MessageSubTypeList.Codes.QuotaVisaQuery);
			actions.declaration.Messages.Add(message);
		}

		public override ZString US_MessageDescription
		{
			get
			{
				ZString secondaryTariffNumberSection = ZString.Empty;

				TariffFormatter formatter = new TariffFormatter();
				if (!US_SecondaryTariffNumber.IsEmpty)
				{
					secondaryTariffNumberSection = "Secondary Tariff Number: " + formatter.DisplayFormat(US_SecondaryTariffNumber) + ", ";
				}

				return string.Format("Tariff Number: {0}, {1}Country Of Origin: {2}", formatter.DisplayFormat(US_TariffNumber), secondaryTariffNumberSection, US_UC_NKOrigin);
			}
		}

		protected override Customs.Business.SingleMessageManager GetMessageManager()
		{
			return null;
		}
	}
}
