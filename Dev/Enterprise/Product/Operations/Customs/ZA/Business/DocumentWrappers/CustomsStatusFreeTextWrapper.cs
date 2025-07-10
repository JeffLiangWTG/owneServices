using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class CustomsStatusFreeTextWrapper : NonPersistentBusinessObject
	{
		public CustomsStatusFreeTextWrapper(ZString line, ZString code, ZString freeText)
		{
			this.line = line;
			this.code = code;
			this.freeText = freeText;
		}

		public ZString Line => line;

		readonly ZString line;

		public ZString Code => code;

		readonly ZString code;

		public ZString FreeText => freeText;

		readonly ZString freeText;
	}
}
