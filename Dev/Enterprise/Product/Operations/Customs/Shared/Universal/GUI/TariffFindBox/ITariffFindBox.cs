using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public interface ITariffFindBox : IFindBox
	{
		System.Func<ZDateTime> GetEffectiveDate { get; }

		bool ShowDescriptionFilterOnNonNomenclatureTariffModule { get; }
	}
}
