using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusLineTariffDetailCollection<out TCusLineTariffDetail> : IBusinessObjectCollection<TCusLineTariffDetail>
		where TCusLineTariffDetail : CusLineTariffDetail
	{
		new TCusLineTariffDetail this[int index] { get; }

		new TCusLineTariffDetail AddNew();

		TCusLineTariffDetail AddNew(ZString tariffType, ZString tariffCode);
	}
}
