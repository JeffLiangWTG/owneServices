using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class FTZ214EntryLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FTZ214EntryLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FTZ214EntryLine(BusinessObjectFactory factory, CusEntryLine entryLine, Bill bill, bool isFirstLine)
			: base(factory)
		{
			this.bill = bill;
			this.entryLine = entryLine;
			this.isFirstLine = isFirstLine;
		}

		readonly Bill bill;
		readonly CusEntryLine entryLine;
		readonly bool isFirstLine;

		public bool IsFirstLineForBill
		{
			get { return isFirstLine; }
		}

		#region Master Bill

		public ZString FTZPackQuantityAndType
		{
			get
			{
				var quantity = bill.TotalNoOfPacks;
				return (bill != null && quantity != 0) ? ZString.Format("{0} {1}", quantity.ToString(0), bill.PrimaryPackType) : ZString.Empty;
			}
		}

		public ZString FTZBillAndITNumber
		{
			get
			{
				var result = ZString.Empty;

				if (bill != null)
				{
					var itNumbers = bill.AllITNumbers.ToList();
					if (itNumbers.Count > 0)
					{
						var numbers = new ZStringBuilder(itNumbers);
						result = ZString.Format("IT: {0} ", itNumbers.Count > 2 ? "MULTI" : numbers.ToStringWithDelimiterBetweenAppends(", "));
					}
					result += ZString.Format("{0}\r\n{1}\r\n{2}", bill.ParentBillNumbers, bill.HouseBillNumbers, ContainerLinkedInvoiceLine);
				}
				return result.Trim();
			}
		}

		public ZString ContainerLinkedInvoiceLine
		{
			get
			{
				var result = ZString.Empty;
				if (entryLine != null)
				{
					var containerNumbersToPrint = new List<ZString>();
					foreach (JobComInvoiceLine invLine in entryLine.InvoiceLines)
					{
						var containersSelected = invLine.ContainersForInvoiceLinesForBindingOnly.OfType<NonPersistentCusContainer>().Where(x => x.IsForInvoiceLine);
						if (containersSelected.Any())
						{
							foreach (NonPersistentCusContainer listContainer in containersSelected)
							{
								var containerNum = listContainer.ContainerNumber;
								if (!containerNum.IsEmpty && !containerNumbersToPrint.Contains(containerNum))
								{
									containerNumbersToPrint.Add(containerNum);
								}
							}
						}
					}

					if (containerNumbersToPrint.Count > 0)
					{
						result = "CNR: " + string.Join(", ", containerNumbersToPrint);
					}
				}
				else if (bill != null)
				{
					result = bill.ContainerNumbers;
				}

				return result;
			}
		}

		#endregion

		#region Entry Line

		public ZString FTZWarehousePackageQtyAndUnit
		{
			get { return entryLine?.FTZWarehousePackageQtyAndUnit ?? ZString.Empty; }
		}

		public ZString FTZCountryOfOrigin
		{
			get { return entryLine != null ? ((IFTZLine)entryLine).CountryOfOrigin : ZString.Empty; }
		}

		public ZString FTZForeignPortOfLadingCode
		{
			get { return entryLine?.FTZForeignPortOfLadingCode ?? ZString.Empty; }
		}

		public ZString FTZForeignPortOfLadingName
		{
			get { return entryLine?.FTZForeignPortOfLadingName ?? ZString.Empty; }
		}

		public ZString FTZDescription
		{
			get { return entryLine?.FTZDescription ?? ZString.Empty; }
		}

		public ZString FTZTariff
		{
			get { return entryLine?.FTZTariff ?? ZString.Empty; }
		}

		public ZString FTZQuantityAndUnit
		{
			get { return entryLine?.FTZQuantityAndUnit ?? ZString.Empty; }
		}

		public ZString FTZSecondQuantityAndUnit
		{
			get { return entryLine?.FTZSecondQuantityAndUnit ?? ZString.Empty; }
		}

		public ZString FTZWeightAndUnit
		{
			get { return entryLine?.FTZWeightAndUnit ?? ZString.Empty; }
		}

		public ZDecimal FTZCustomsValue
		{
			get { return entryLine?.FTZCustomsValue ?? ZDecimal.Zero; }
		}

		public ZDecimal Charges
		{
			get { return entryLine?.Charges ?? ZDecimal.Zero; }
		}

		#endregion
	}
}
