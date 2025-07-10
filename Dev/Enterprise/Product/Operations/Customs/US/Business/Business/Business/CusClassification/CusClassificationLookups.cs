using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusClassificationLookups : Customs.Business.CusClassificationLookups
	{
		public CusClassificationLookups(CusClassification parent)
			: base(parent)
		{
		}

		public BusinessObjectCollection Tariffs
		{
			get
			{
				BusinessObjectCollection result;
				if (Parent.IsExport)
				{
					result = Factory.GetTariffs(Universal.Constants.TariffTypes.ScheduleB, Parent.CC_TariffNum, ZDateTime.Today);
				}
				else
				{
					result = new USCTariffCollection(Factory);
				}
				return result;
			}
		}

		new CusClassification Parent => (CusClassification)base.Parent;
	}
}
