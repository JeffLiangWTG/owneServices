using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ConsolidatedDeclarationCollection<T> : ActiveBusinessObjectCollection<T> where T : ConsolidatedDeclaration
	{
		public ConsolidatedDeclarationCollection(BusinessObjectFactory factory, ZString applicationCode)
			: base(factory, SetCollectionFilter(Argument.NotNullOrEmpty(applicationCode, nameof(applicationCode))))
		{
		}

		static ZQuery SetCollectionFilter(ZString applicationCode)
		{
			var query = new ZQuery(CusReconDeclarationSchema.CRD_ApplicationCode, applicationCode);
			query.AddToFilter(CusReconDeclarationSchema.CRD_GB_Branch, GlbCompany.CurrentCompany.Branches.GetPKs());
			return query;
		}

		protected override bool AllowNew => false;
	}
}
