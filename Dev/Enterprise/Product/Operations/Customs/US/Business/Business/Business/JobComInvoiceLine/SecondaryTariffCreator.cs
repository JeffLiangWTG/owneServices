using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class SecondaryTariffCreator
	{
		/// <summary>
		/// This method creates secondary tariffs from secondary tariff rules.
		/// Method will only create secondary tariffs if tariff information is not defaulted from the product file
		/// </summary>
		/// <param name="invoiceLine"></param>
		public void AddSecondaryTariffsFromTariffRule(JobComInvoiceLine invoiceLine)
		{
			var declaratioin = invoiceLine == null ? null : invoiceLine.Declaration;
			if (declaratioin != null && !declaratioin.IsDefaultSecondaryTariffLinesSuspended)
			{
				if (invoiceLine.JI_ParentID.IsEmpty && invoiceLine.ImportTariff != null && invoiceLine.Pivot == null)
				{
					using (declaratioin.SuspendDefaultingSecondaryTariffLines())
					{
						USCTariffRule tariffRule = invoiceLine.ImportTariff.GetTariffRuleIfApplies(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, invoiceLine.EffectiveDateForDutyRate);

						if (tariffRule != null && tariffRule.SecondaryTariffs.Count == 1)
						{
							USCRuleSecondaryTariff secondaryTariffRule = tariffRule.SecondaryTariffs[0];

							if (invoiceLine.Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, secondaryTariffRule.U3_TariffFrom)) != null)
							{
								var previousChildLines = RemoveChildLines(invoiceLine);

								SetParentInvoiceLineNumber(invoiceLine);

								var newChildLines = new List<JobComInvoiceLine>(
									secondaryTariffRule
										.GetASetOfAssociatedTariffNumbers()
										.Select(secondaryTariffCode => GetSecondaryLine(invoiceLine, secondaryTariffCode, previousChildLines)));

								previousChildLines.ForEach(remainingLine => remainingLine.Delete());

								RenumberChildInvoiceLines(invoiceLine, newChildLines);
							}
						}
					}
				}
			}
		}

		JobComInvoiceLine GetSecondaryLine(JobComInvoiceLine parentLine, ZString secondaryTariffNumber, List<JobComInvoiceLine> previousChildLines)
		{
			JobComInvoiceLine secondarytariffline = previousChildLines.Find(x => x.JI_Tariff == secondaryTariffNumber);

			if (secondarytariffline == null)
			{
				secondarytariffline = parentLine.AddSecondaryInvoiceLine();
				secondarytariffline.JI_Tariff = secondaryTariffNumber;
			}
			else
			{
				previousChildLines.Remove(secondarytariffline);
				parentLine.InvoiceHeader.JobComInvoiceLines.Add(secondarytariffline);
			}

			return secondarytariffline;
		}

		void SetParentInvoiceLineNumber(JobComInvoiceLine invoiceLine)
		{
			JobComInvoiceHeader invoice = invoiceLine.InvoiceHeader;

			short count = (short)invoice.JobComInvoiceLines.Count;
			invoiceLine.JI_LineNo = invoice.JobComInvoiceLines.Contains(invoiceLine) ? count : (short)(count + 1);
		}

		List<JobComInvoiceLine> RemoveChildLines(JobComInvoiceLine parentLine)
		{
			List<JobComInvoiceLine> result = new List<JobComInvoiceLine>();

			JobComInvoiceHeader invoice = parentLine.InvoiceHeader;

			foreach (JobComInvoiceLine childLine in parentLine.SecondaryTariffLines)
			{
				if (invoice.JobComInvoiceLines.Contains(childLine))
				{
					invoice.JobComInvoiceLines.Remove(childLine);
					childLine.JI_JZ = ZGuid.Empty;
				}

				result.Add(childLine);
			}

			return result;
		}

		void RenumberChildInvoiceLines(JobComInvoiceLine parentLine, List<JobComInvoiceLine> linesCreated)
		{
			using (parentLine.InvoiceHeader.GetLineNumberRenumberingSuspender())
			{
				ZShort lineNumberForFirstVLineIfParentIsX = parentLine.JI_LineNo + 1;
				ZShort linesToStartFromForTheOtherLines = parentLine.IsSetXLine && HasAChildLineWithSameTariffNumber(parentLine) ? parentLine.JI_LineNo + 2 : parentLine.JI_LineNo + 1;

				foreach (JobComInvoiceLine line in linesCreated)
				{
					if (parentLine.IsSetXLine
						&& parentLine.JI_Tariff == line.JI_Tariff
						&& lineNumberForFirstVLineIfParentIsX != -1)//if not used
					{
						line.JI_LineNo = lineNumberForFirstVLineIfParentIsX;
						lineNumberForFirstVLineIfParentIsX = -1;//used
					}
					else
					{
						line.JI_LineNo = linesToStartFromForTheOtherLines++;
					}
				}
			}
		}

		bool HasAChildLineWithSameTariffNumber(JobComInvoiceLine parentLine)
		{
			List<JobComInvoiceLine> additionalTariffs = new List<JobComInvoiceLine>(parentLine.ChildLines);
			return additionalTariffs.Find(x => x.JI_Tariff == parentLine.JI_Tariff) != null;
		}
	}
}
