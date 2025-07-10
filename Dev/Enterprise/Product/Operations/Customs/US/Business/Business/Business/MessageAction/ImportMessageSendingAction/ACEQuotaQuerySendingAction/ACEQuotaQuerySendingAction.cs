using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEQuotaQuerySendingAction : ImportMessageSendingAction
	{
		public ACEQuotaQuerySendingAction(ZString tariffNumber, ZString secondaryTariffNumber, ZString origin, ACEQuotaQuerySendingActionCollection actions)
			: base(actions)
		{
			this.US_TariffNumber = tariffNumber;
			this.US_SecondaryTariffNumber = secondaryTariffNumber;
			this.US_UC_NKOrigin = CanadaProvinceTerritoryCodes.IsCanadianProvince(origin) ? new ZString(Core.Constants.CountryCodes.Canada) : origin;
		}
		internal readonly ZString US_TariffNumber;
		internal readonly ZString US_SecondaryTariffNumber;
		internal readonly ZString US_UC_NKOrigin;

		public override ZString US_MessageContents
		{
			get
			{
				if (!messageContentsCached.HasValue)
				{
					messageContentsCached = GenerateMessageContent();
				}
				return messageContentsCached.Value;
			}
		}
		ZString? messageContentsCached;

		public void SendQueryMessagesWithoutSaving()
		{
			var generator = GetGenerator();
			var message = generator.CreateMessage<MQEDIMessage>(Factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.QuotaVisaQuery;

			actions.declaration.Messages.Add(message);
		}

		ZString GenerateMessageContent()
		{
			var generator = GetGenerator();
			return generator.Serialise(true);
		}

		ACEInputBlockControlGenerator GetGenerator()
		{
			return new ACEQuotaQueryMessageBuilder().GenerateMessageBlocks(QueryTypeList.Codes.TariffNumber, US_TariffNumber, US_SecondaryTariffNumber, US_UC_NKOrigin);
		}

		public override ZString US_MessageDescription
		{
			get
			{
				var secondaryTariffNumberSection = ZString.Empty;

				var formatter = new TariffFormatter();
				if (!US_SecondaryTariffNumber.IsEmpty)
				{
					secondaryTariffNumberSection = "Secondary Tariff Number: " + formatter.DisplayFormat(US_SecondaryTariffNumber) + ", ";
				}

				return string.Format(System.Globalization.CultureInfo.CurrentCulture, "Tariff Number: {0}, {1}Country Of Origin: {2}", formatter.DisplayFormat(US_TariffNumber), secondaryTariffNumberSection, US_UC_NKOrigin);
			}
		}

		protected override Customs.Business.SingleMessageManager GetMessageManager()
		{
			return null;
		}
	}
}
