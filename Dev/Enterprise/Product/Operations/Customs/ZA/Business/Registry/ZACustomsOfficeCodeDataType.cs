using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class ZACustomsOfficeDataType : GuidRegistryDataType
	{
		public ZACustomsOfficeDataType() : base()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var code = new BusinessObjectFactory().Load<ZZRefCusCodeListCombined>(proposedValue);
			if (code != null && (code.ZZD_Code.Length > JobDeclaration.Schema.JE_CustomsOfficeMaxLength))
			{
				var errorMessage = Enterprise.Customs.ZA.Business.Res.GetString("5fd39131-27c1-4d0f-808e-29a2d2e32360", "Customs Office Code should not be longer than {0}.", JobDeclaration.Schema.JE_CustomsOfficeMaxLength);
				throw new RegistryValidationException(errorMessage);
			}
		}
	}
}
