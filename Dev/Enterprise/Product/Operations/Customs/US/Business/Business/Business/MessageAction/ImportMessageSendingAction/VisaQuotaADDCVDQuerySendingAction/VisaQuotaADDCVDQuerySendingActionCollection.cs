using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class VisaQuotaADDCVDQuerySendingActionCollection : ImportMessageSendingActionCollection
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public VisaQuotaADDCVDQuerySendingActionCollection(QueryTypeForQuotaVisaADDCVD queryType, JobDeclaration declaration)
			: base(declaration, queryType == QueryTypeForQuotaVisaADDCVD.ADDCVD ? ImportMessageSendingMessageType.ADDCVD : ImportMessageSendingMessageType.VisaQuotaQuery)
		{
			this.queryType = queryType;
			PopulateElements(null);
		}

		internal readonly QueryTypeForQuotaVisaADDCVD queryType;

		public new VisaQuotaADDCVDQuerySendingAction this[int index]
		{
			get { return (VisaQuotaADDCVDQuerySendingAction)base[index]; }
		}

		public new VisaQuotaADDCVDQuerySendingAction AddNew()
		{
			return (VisaQuotaADDCVDQuerySendingAction)base.AddNew();
		}

		public bool SendMessagesWithoutSaving()
		{
			bool result = false;

			if (queryType == QueryTypeForQuotaVisaADDCVD.ADDCVD)
			{
				result = new ReferenceFileRequester(Factory).RequestADDCVDsByTariff(this);
			}
			else
			{
				foreach (VisaQuotaADDCVDQuerySendingAction action in this)
				{
					if (action.US_SendMessage)
					{
						action.SendQueryMessagesWithoutSaving();
						result = true;
					}
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
			if (queryType != QueryTypeForQuotaVisaADDCVD.NA)
			{
				List<string> tariffAndOrigins = new List<string>();

				if (messageSendingMessageType == ImportMessageSendingMessageType.ADDCVD)
				{
					PopulateElementsForADDCVDQuery(tariffAndOrigins);
				}
				else
				{
					PopulateElementsForQuotaVisa(tariffAndOrigins);
				}
			}
		}

		void PopulateElementsForADDCVDQuery(List<string> tariffAndOrigins)
		{
			const string ProhibitedTariffSuffix = "99";
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				foreach (USCTariff importTariff in new USCTariff[] { invoiceLine.ImportSupTariff, invoiceLine.ImportTariff })
				{
					if (importTariff != null && !importTariff.UE_Tariff.StartsWith(ProhibitedTariffSuffix))
					{
						string key = GetKey(importTariff.UE_Tariff, ZString.Empty, invoiceLine.US_UC_NKCountryOfOrigin);

						if (!tariffAndOrigins.Contains(key))
						{
							VisaQuotaADDCVDQuerySendingAction newElement = new VisaQuotaADDCVDQuerySendingAction(importTariff.UE_Tariff, ZString.Empty, invoiceLine.US_UC_NKCountryOfOrigin, this, queryType);
							newElement.US_SendMessage = true;
							Add(newElement);
							tariffAndOrigins.Add(key);
						}
					}
				}
			}
		}

		void PopulateElementsForQuotaVisa(List<string> tariffAndOrigins)
		{
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (!invoiceLine.IsSecondaryTariffLine)
				{
					ZString countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;

					USCTariff importTariff = invoiceLine.ImportTariff;

					if (!countryOfOrigin.IsEmpty && importTariff != null)
					{
						var secondaryTariffLines = invoiceLine.SecondaryTariffLines;
						JobComInvoiceLine uniqueSecondaryTariffLine = secondaryTariffLines.IsCountEqualTo(1) ? secondaryTariffLines.ElementAt(0) : null;

						ZString firstTariff = invoiceLine.HasEmptySupTariff ? invoiceLine.JI_Tariff : invoiceLine.US_SupTariff;
						ZString secondTariff = !invoiceLine.HasEmptySupTariff ? invoiceLine.JI_Tariff : uniqueSecondaryTariffLine != null ? uniqueSecondaryTariffLine.JI_Tariff : ZString.Empty;

						string key = GetKey(firstTariff, secondTariff, countryOfOrigin);

						if (!tariffAndOrigins.Contains(key))
						{
							VisaQuotaADDCVDQuerySendingAction newElement = new VisaQuotaADDCVDQuerySendingAction(firstTariff, secondTariff, countryOfOrigin, this, queryType);
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
