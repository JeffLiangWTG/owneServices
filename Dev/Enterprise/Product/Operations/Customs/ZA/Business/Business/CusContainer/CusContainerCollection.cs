using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusContainerCollection : BaseCusContainerCollection<CusContainer>
	{
		public CusContainerCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		public CusContainerCollection(JobDeclaration jobDeclaration, ZQuery filter)
			: base(jobDeclaration, filter)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (((JobDeclaration)Master).IsExport)
			{
				int i = 0;
				foreach (var container in this)
				{
					if (container.CO_ContainerNumber.StartsWith("TBA") && container.CO_ContainerNumber.Length >= 4)
					{
						var lastNumber = container.CO_ContainerNumber.Substring(3, 1);
						if (lastNumber.IsNumbersOnlyOrEmpty)
						{
							i = Math.Max(i, int.Parse(lastNumber));
						}
					}
				}
				((CusContainer)child).CO_ContainerNumber = "TBA" + (i + 1).ToString();
			}
		}
	}
}
