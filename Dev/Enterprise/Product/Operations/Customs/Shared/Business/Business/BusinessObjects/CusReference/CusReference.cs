using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusReference : AutoCusReference, Integration.Customs.ICusReference
	{
		public CusReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusReferenceTypeDecider TypeDecider = new CusReferenceTypeDecider();

		public BusinessObject Parent => Factory.Load(CFR_ParentTableCode, CFR_ParentID);

		protected override CusReferenceLookups GetNewLookups() => new CusReferenceLookups(this);

		protected override CusReferenceValidation GetNewValidation() => new CusReferenceValidation(this);
	}
}
