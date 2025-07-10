using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class ICRClassification : IClassification
	{
		public ICRClassification(ZString tariff, ZString type)
		{
			this.tariff = tariff;
			ClassificationTypeCode = type;
		}
		readonly ZString tariff;

		public ZString Classification => tariff.Replace(".", "");

		public ZString ClassificationTypeCode { get; }
	}
}
