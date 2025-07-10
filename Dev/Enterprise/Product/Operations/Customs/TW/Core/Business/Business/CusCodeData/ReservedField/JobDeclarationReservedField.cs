using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationReservedField : ReservedField
	{
		public JobDeclarationReservedField(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));
	}
}
