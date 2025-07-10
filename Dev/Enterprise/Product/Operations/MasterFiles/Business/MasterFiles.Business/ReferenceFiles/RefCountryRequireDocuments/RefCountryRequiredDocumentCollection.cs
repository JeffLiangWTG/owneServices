using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	// Make sure you functionally test changing country codes on the form when changing this over to ActiveBusinessObjectCollection.
	public class RefCountryRequiredDocumentCollection : BusinessObjectCollection<RefCountryRequiredDocument>
	{
		public RefCountryRequiredDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefCountryRequiredDocumentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public void LoadCountry(RefCountry country)
		{
			ZQuery query = new ZQuery();

			query.DefaultJoinCondition = JoinCondition.Or;
			query.AddToFilter(RefCountryRequiredDocumentSchema.RD_RN_NKDestination, country.RN_Code);
			query.AddToFilter(RefCountryRequiredDocumentSchema.RD_RN_NKOrigin, country.RN_Code);
			query.AddToFilter(RefCountryRequiredDocumentSchema.RD_RN_NKDestination, "");
			query.AddToFilter(RefCountryRequiredDocumentSchema.RD_RN_NKOrigin, "");

			RemoveAllButLeaveRelationshipsIntact();
			CountryCode = country.Code;
			AddRange(Factory.Load(typeof(RefCountryRequiredDocument), query));
			LastLoadedAdditionalFilter = query;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			RefCountryRequiredDocument requiredDocument = (RefCountryRequiredDocument)bizOAdded;
			requiredDocument.SelectedCountryCode = CountryCode;
		}

		public ZString CountryCode { get; private set; }
	}
}
