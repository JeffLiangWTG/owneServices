using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ContainerCalculator
	{
		public ContainerCalculator(CusEntryHeader entryHeader)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			this.entryHeader = entryHeader;
		}

		public BaseCusContainer[] GetContainers()
		{
			BaseCusContainer[] result;
			if (Declaration == null)
			{
				result = Array.Empty<BaseCusContainer>();
			}
			else
			{
				int containerCount = Declaration.CusContainers.Count;

				ArrayList selectedContainers;

				if ((Declaration.UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist && containerCount < 2)
					|| !Declaration.HasMultipleEntriesForDeterminingContainerCount)
				{
					selectedContainers = new ArrayList();
					selectedContainers.AddRange(Declaration.CusContainers);
				}
				else
				{
					Hashtable containers = new Hashtable();
					for (int entryLineNumber = 0; entryLineNumber < MergedLines.Count && containers.Count < containerCount; entryLineNumber++)
					{
						CusEntryLine entryLine = MergedLines[entryLineNumber];
						for (int invoiceLineNumber = 0; invoiceLineNumber < entryLine.InvoiceLines.Count && containers.Count < containerCount; invoiceLineNumber++)
						{
							BaseJobComInvoiceLine invoiceLine = entryLine.InvoiceLines[invoiceLineNumber];
							for (int containerLineNumber = 0; containerLineNumber < invoiceLine.ContainersPivot.Count && containers.Count < containerCount; containerLineNumber++)
							{
								BaseCusContainer container = invoiceLine.ContainersPivot[containerLineNumber].Container;
								if (container != null)
								{
									containers[container] = container;
								}
							}
						}
					}
					if (containers.Count == 0 && Declaration.UseDeclarationContainersIfNoneFoundOnEntry)    // Keeps existing behaviour - can be removed if all countries stop messages being sent in this condition
					{
						selectedContainers = new ArrayList();
						selectedContainers.AddRange(Declaration.CusContainers);
					}
					else
					{
						selectedContainers = new ArrayList(containers.Values);
					}
				}
				PropertyComparer comparer = new PropertyComparer(typeof(BaseCusContainer), CusContainerSchema.Constants.CO_ContainerNumber, ListSortDirection.Ascending);
				selectedContainers.Sort(comparer);
				result = (BaseCusContainer[])(selectedContainers.ToArray(Declaration.CusContainers.TypeOfElements));
			}
			return result;
		}

		#region Implementation

		readonly CusEntryHeader entryHeader;
		BaseJobDeclaration Declaration
		{
			get { return entryHeader.Declaration; }
		}
		ICusEntryLineCollection<CusEntryLine> MergedLines
		{
			get { return entryHeader.MergedLines; }
		}

		#endregion

	}
}
