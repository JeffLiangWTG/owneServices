using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class TradePartyValueProvider
	{
		public TradePartyValueProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public IEnumerable<IDISTradeParty> TradeParties
		{
			get
			{
				var manufacturer = invoiceLine.ManufacturerAddress;

				if (manufacturer != null)
				{
					yield return new TradePartyWrapper()
					{
						ID = OrgHeaderWrapper.GetCustomsCodeFromAddress(manufacturer, OrgCusCode.USACodeTypes.ManufacturerID),
						Name = manufacturer.EffectiveCompanyNameTruncated,
						Type = DISTradePartyType.Manufacturer,
						Address = manufacturer.AddressAsASingleLineWithoutCompanyName
					};
				}

				var foreignExporter = invoiceLine.ExporterAddress;
				if (foreignExporter != null)
				{
					yield return new TradePartyWrapper()
					{
						ID = OrgHeaderWrapper.GetCustomsCodeFromAddress(foreignExporter, OrgCusCode.USACodeTypes.ManufacturerID),
						Name = foreignExporter.EffectiveCompanyNameTruncated,
						Type = DISTradePartyType.Exporter,
						Address = foreignExporter.AddressAsASingleLineWithoutCompanyName
					};
				}

				var ultimateConsignee = invoiceLine.ConsigneeOrgAddress;
				if (ultimateConsignee != null)
				{
					yield return new TradePartyWrapper()
					{
						ID = OrgHeaderWrapper.GetCustomsRelatedCode(invoiceLine.ConsigneeOrgAddress, OrgMatchedCustomsRegNoType.EIN),
						Name = ultimateConsignee.OH_FullNameTruncated,
						Type = DISTradePartyType.Consignee,
						Address = ultimateConsignee.MainAddress.AddressAsASingleLineWithoutCompanyName
					};
				}
			}
		}

		class TradePartyWrapper : IDISTradeParty
		{
			public ZString Address
			{
				get;
				internal set;
			}

			public ZString ID
			{
				get;
				internal set;
			}

			public ZString Name
			{
				get;
				internal set;
			}

			public DISTradePartyType Type
			{
				get;
				internal set;
			}
		}
	}
}
