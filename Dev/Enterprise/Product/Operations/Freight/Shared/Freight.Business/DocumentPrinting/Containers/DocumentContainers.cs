using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class DocumentContainers : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentContainers(ContainerToSelectFromForPrintingCollection containersToSelectFrom)
			: base(containersToSelectFrom.Factory)
		{
			this.containersToSelectFrom = containersToSelectFrom;
		}

		public ContainerToSelectFromForPrintingCollection ContainersToSelectFrom
		{
			get { return containersToSelectFrom; }
		}
		readonly ContainerToSelectFromForPrintingCollection containersToSelectFrom;

		public ContainerNonDependentCollection ContainersToPrint
		{
			get
			{
				ContainerNonDependentCollection result = new ContainerNonDependentCollection(Factory);

				foreach (ContainerToSelectFromForPrinting currentContainer in ContainersToSelectFrom)
				{
					if (currentContainer.JC_Calc_PrintDocumentForContainer)
					{
						result.Add(currentContainer.Container);
					}
				}

				return result;
			}
		}

		public static new string TableName
		{
			get { return "JobContainer"; }
		}
	}
}
