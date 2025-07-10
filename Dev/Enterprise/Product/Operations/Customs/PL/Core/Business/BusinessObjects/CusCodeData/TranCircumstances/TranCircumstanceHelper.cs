using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public static class TranCircumstanceHelper
{
	public static void TranCircumstanceSetter<T>(T parent, ZPropertyInfo info, ZString value, TranCircumstance tranCircusmstance, short order)
		where T : BusinessObject, ITranCircumstanceSupporter
	{
		if (!value.IsEmpty)
		{
			if (tranCircusmstance == null)
			{
				tranCircusmstance = TranCircumstance.LoadOrCreate(parent, order);
				parent.RegisterEditableChildObject(tranCircusmstance);
			}
			tranCircusmstance.CY_Code = value;
		}
		else
		{
			if (tranCircusmstance != null)
			{
				tranCircusmstance.CY_Code = string.Empty;
				tranCircusmstance.Delete();
			}
		}

		info.RefreshBinding();
	}
}
