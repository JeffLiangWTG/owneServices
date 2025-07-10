using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.eTail.Business
{
	class AUCustomsStatusStore : DefaultCustomsStatusStore
	{
		public AUCustomsStatusStore(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override string[] ExportCodeTypes => new[] { RefCusCodeListTypes.Codes.ExportCustomsStatus };

		protected override string GetCodeTypeForImport(bool hasFormalDeclaration)
		{
			return RefCusCodeListTypes.Codes.CustomsStatus;
		}

		protected override string GetCodeTypeForExport()
		{
			return RefCusCodeListTypes.Codes.ExportCustomsStatus;
		}

		protected override ZString CountryCode => CountryCodes.Australia;
	}
}
