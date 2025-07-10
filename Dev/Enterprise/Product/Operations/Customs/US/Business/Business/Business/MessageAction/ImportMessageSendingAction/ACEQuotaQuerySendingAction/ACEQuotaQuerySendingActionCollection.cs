using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEQuotaQuerySendingActionCollection : ImportMessageSendingActionCollection
	{
		public ACEQuotaQuerySendingActionCollection(JobDeclaration declaration)
			: base(declaration, ImportMessageSendingMessageType.VisaQuotaQuery)
		{
		}

		public new ACEQuotaQuerySendingAction this[int index]
		{
			get { return (ACEQuotaQuerySendingAction)base[index]; }
		}

		public new ACEQuotaQuerySendingAction AddNew()
		{
			return (ACEQuotaQuerySendingAction)base.AddNew();
		}

		public bool SendMessagesWithoutSaving()
		{
			bool result = false;

			foreach (ACEQuotaQuerySendingAction action in this)
			{
				if (action.US_SendMessage)
				{
					action.SendQueryMessagesWithoutSaving();
					result = true;
				}
			}

			return result;
		}

		protected override bool ShouldDefaultSendMessageIfThereIsOnlyOneElement
		{
			get { return false; }
		}

		protected override void PopulateElements(Func<CusEntryHeader, bool> entryFilter)
		{
			var tariffAndOrigins = new List<string>();

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (!invoiceLine.IsSecondaryTariffLine)
				{
					var countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;

					var importTariff = invoiceLine.ImportTariff;

					if (!countryOfOrigin.IsEmpty && importTariff != null)
					{
						var secondaryTariffLines = invoiceLine.SecondaryTariffLines;
						var uniqueSecondaryTariffLine = secondaryTariffLines.IsCountEqualTo(1) ? secondaryTariffLines.ElementAt(0) : null;

						var firstTariff = invoiceLine.HasEmptySupTariff ? invoiceLine.JI_Tariff : invoiceLine.US_SupTariff;
						var secondTariff = !invoiceLine.HasEmptySupTariff ? invoiceLine.JI_Tariff : uniqueSecondaryTariffLine != null ? uniqueSecondaryTariffLine.JI_Tariff : ZString.Empty;

						string key = GetKey(firstTariff, secondTariff, countryOfOrigin);

						if (!tariffAndOrigins.Contains(key))
						{
							var newElement = new ACEQuotaQuerySendingAction(firstTariff, secondTariff, countryOfOrigin, this);
							newElement.US_SendMessage = importTariff.UE_QuotaIndicator;
							Add(newElement);
							tariffAndOrigins.Add(key);
						}
					}
				}
			}
		}

		string GetKey(ZString tariffNumber, ZString secondaryTariffNumber, ZString origin)
		{
			return tariffNumber + ":" + secondaryTariffNumber + ":" + origin;
		}
	}
}
