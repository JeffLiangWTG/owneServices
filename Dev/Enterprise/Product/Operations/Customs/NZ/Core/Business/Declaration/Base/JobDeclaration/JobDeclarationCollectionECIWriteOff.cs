using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationCollectionECIWriteOff : JobDeclarationCollection
	{
		public JobDeclarationCollectionECIWriteOff(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override ZQuery CreateAdditionalFilter() => new ZQuery(JobDeclarationSchema.JE_MessageSubType, JobMessageSubTypeList.Codes.WriteOff);

		protected override bool AllowNewCore => false;
	}
}
