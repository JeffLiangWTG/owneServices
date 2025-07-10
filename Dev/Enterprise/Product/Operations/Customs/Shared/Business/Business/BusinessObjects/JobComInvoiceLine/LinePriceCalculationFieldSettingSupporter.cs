using System;

namespace Enterprise.Customs.Business
{
	public interface ILinePriceCalculationFieldSettingSupporter
	{
		void Start(object type);
		void Stop(object type);
	}

	public class LinePriceCalculationFieldSettingSupporter : IDisposable
	{
		public LinePriceCalculationFieldSettingSupporter(ILinePriceCalculationFieldSettingSupporter supporter, object type)
		{
			this.supporter = supporter;
			this.type = type;
			supporter.Start(type);
		}

		readonly ILinePriceCalculationFieldSettingSupporter supporter;
		readonly object type;

		public void Dispose()
		{
			supporter.Stop(type);
		}
	}

	public enum LinePriceCalculationFieldSettingType
	{
		LinePrice,
		UnitPrice,
		Quantity
	}
}
