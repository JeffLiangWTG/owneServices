using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class UniversalReferenceBusinessProvider : Universal.UniversalReferenceBusinessProvider
	{
		protected override ZDateTimePickerFormat GetTariffDateTimeFormat()
		{
			return Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
		}
	}
}
