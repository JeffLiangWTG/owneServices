using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISToxicSubstanceData : AutoDISToxicSubstanceData
	{
		public DISToxicSubstanceData(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DISAdditionalNumberCollection CASNumbers
		{
			get
			{
				if (casNumbers == null)
				{
					casNumbers = new DISAdditionalNumberCollection(Factory);
					RegisterEditableChildObject(casNumbers);
				}
				return casNumbers;
			}
		}
		DISAdditionalNumberCollection casNumbers;
	}
}
