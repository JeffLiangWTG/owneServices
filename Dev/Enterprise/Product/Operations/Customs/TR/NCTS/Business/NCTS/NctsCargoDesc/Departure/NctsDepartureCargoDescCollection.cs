using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureCargoDescCollection : NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>
	{
		public NctsDepartureCargoDescCollection(NctsCommonMovementHeader movementHeader)
			: base(movementHeader)
		{
		}

		protected override void SetDefaultsForNewElementCore(NctsDepartureCargoDesc newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BY_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
		}
	}
}
