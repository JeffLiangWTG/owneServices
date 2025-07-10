using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class TraderCollection : ActiveBusinessObjectCollection<Trader>
	{
		public TraderCollection(JobDeclaration declaration)
			: base(declaration.Factory,
				  declaration,
				  GetQuery(declaration),
				  JobDocAddressSchema.E2_ParentID)
		{
		}

		static ZQuery GetQuery(JobDeclaration declaration)
		{
			var query = new ZQuery(JobDocAddressSchema.E2_ParentTableCode, declaration.TablePrefix);
			var subQuery = new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BuyerDocumentaryAddress);
			subQuery.AddToFilter(new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.SellerDocumentaryAddress), JoinCondition.Or);
			query.AddToFilter(subQuery, JoinCondition.And);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(Trader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.E2_AddressType = DocAddressTypes.Codes.BuyerDocumentaryAddress;
		}
	}
}
