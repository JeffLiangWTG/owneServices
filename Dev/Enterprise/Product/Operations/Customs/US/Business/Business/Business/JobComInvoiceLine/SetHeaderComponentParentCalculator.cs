using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class SetHeaderComponentParentCalculator
	{
		public SetHeaderComponentParentCalculator(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		public IDisposable GetCalculatorSuspender()
		{
			return new CalculatorSuspender(this);
		}

		public void UpdateWhenJI_ParentChanges()
		{
			if (!IsChangingValueSuspended)
			{
				using (new CalculatorSuspender(this))
				{
					var parentLine = invoiceLine.ParentTariffLine;
					if (parentLine != null && (parentLine.IsSetXLine || parentLine.IsSetVLine))
					{
						invoiceLine.JI_CC = ZGuid.Empty;
						invoiceLine.JI_PartNo = ZString.Empty;

						SetIndicator(invoiceLine, SecondarySpecProgIndicatorList.Codes.V);
						if (parentLine.ChildLines.IsNullOrEmpty() && !parentLine.JI_Tariff.IsEmpty)
						{
							invoiceLine.JI_Tariff = parentLine.JI_Tariff;
						}
					}
					else if (invoiceLine.IsSetVLine)
					{
						SetIndicator(invoiceLine, ZString.Empty);
					}
				}
			}
		}

		public void UpdateWhenSetIndicatorChanges()
		{
			if (!IsChangingValueSuspended)
			{
				using (new CalculatorSuspender(this))
				{
					foreach (JobComInvoiceLine child in invoiceLine.ChildLines)
					{
						if (invoiceLine.IsSetXLine || invoiceLine.IsSetVLine)
						{
							SetIndicator(child, SecondarySpecProgIndicatorList.Codes.V);
						}
						else if (child.IsSetVLine)
						{
							SetIndicator(child, ZString.Empty);
						}
					}

					if (invoiceLine.IsSetXLine)
					{
						invoiceLine.JI_ParentID = ZGuid.Empty;
					}
					else if (invoiceLine.IsSetVLine)
					{
						if (invoiceLine.ParentTariffLine == null && invoiceLine.InvoiceHeader != null)
						{
							JobComInvoiceLine possibleParentLine = null;

							int indexOfInvoiceLine = -1;

							invoiceLine.InvoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.Constants.JI_LineNo, System.ComponentModel.ListSortDirection.Ascending);

							//trying to find a X line which is closest to invoice line in terms of JI_LineNo
							for (int index = invoiceLine.InvoiceHeader.JobComInvoiceLines.Count - 1; index >= 0; index--)
							{
								JobComInvoiceLine current = invoiceLine.InvoiceHeader.JobComInvoiceLines[index];
								if (current == invoiceLine)
								{
									indexOfInvoiceLine = index;
								}

								if (indexOfInvoiceLine >= 0 && current.IsSetXLine)
								{
									possibleParentLine = current;
									break;
								}
							}

							if (possibleParentLine != null)
							{
								invoiceLine.JI_ParentID = possibleParentLine.PK;
							}
						}
					}
					else if (invoiceLine.ParentTariffLine != null && invoiceLine.ParentTariffLine.IsSetXLine)//neither X nor V, but points to Header X 
					{
						invoiceLine.JI_ParentID = ZGuid.Empty;
					}
				}
			}
		}

		#region Suspend to avoid changes in chain

		int valueChangeIndex;
		bool IsChangingValueSuspended
		{
			get { return valueChangeIndex > 0; }
		}

		class CalculatorSuspender : IDisposable
		{
			public CalculatorSuspender(SetHeaderComponentParentCalculator calculator)
			{
				this.calculator = calculator;
				calculator.valueChangeIndex++;
			}

			readonly SetHeaderComponentParentCalculator calculator;

			public void Dispose()
			{
				calculator.valueChangeIndex--;
			}
		}

		void SetIndicator(JobComInvoiceLine invoiceLine, ZString indicator)
		{
			if (invoiceLine.IsACE)
			{
				invoiceLine.US_SetInd = indicator;
			}
			else
			{
				invoiceLine.US_SecondarySPI = indicator;
			}
		}

		#endregion
	}
}
