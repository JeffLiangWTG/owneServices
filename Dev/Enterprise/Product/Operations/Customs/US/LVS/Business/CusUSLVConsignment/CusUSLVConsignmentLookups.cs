//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUSLVConsignmentLookups
//
//    This class should be used for overriding collections in AutoCusUSLVConsignmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentLookups : AutoCusUSLVConsignmentLookups
	{
		public CusUSLVConsignmentLookups(AutoCusUSLVConsignment parent) : base(parent)
		{
		}

		CusUSLVConsignment Consignment => (CusUSLVConsignment)Parent;

		public USCarrierCombinedCollection ULB_HouseBillIssuerSCACList
		{
			get { return new USCarrierCombinedCollection(Factory); }
		}

		public ConsigneeCollection ConsigneeOrgList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public ConsignorCollection SellerOrgList
		{
			get { return new ConsignorCollection(Factory); }
		}

		public RefPackTypeCollection PackTypes
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		public ImportMessageStatusList ULB_MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public CRLReleaseStatusList CRLReleaseStatusList
		{
			get { return Factory.GetCachedValue<CRLReleaseStatusList>(); }
		}

		public ULBConvertActionList ULB_ConvertActionList
		{
			get { return Factory.GetCachedValue<ULBConvertActionList>(); }
		}

		public CodeDescriptionPairList LowValueEntryTypeList
		{
			get => Factory.GetCachedValue("CusUSLVConsignmentLookups|GetLowValueEntryTypeList", EntryTypeList.GetLowValueDeclarationEntryTypeList);
		}

		#region PostCodes

		public RefPostCodeCollection ConsigneePostCodes => GetPostCodes(Consignment.ConsigneeCountry);

		public RefPostCodeCollection SellerPostCodes => GetPostCodes(Consignment.SellerCountry);

		RefPostCodeCollection GetPostCodes(RefCountry country)
		{
			if (country != null)
			{
				return Factory.GetCachedValue("PostCodesByCountry" + country.Code, () => GetPostCodesByCountry(country.Code), CacheStalenessPolicy.StaleWhenDataTableChanges(AutoRefPostCode.Schema.TableName, Factory));
			}

			return new RefPostCodeCollection(Factory, new ZQuery());
		}

		RefPostCodeCollection GetPostCodesByCountry(ZString countryCode)
		{
			var postCodeCollection = new RefPostCodeCollection(Factory, new ZQuery(RefPostCodeSchema.RK_RN_NKCountry, countryCode));
			postCodeCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country", "Property", countryCode));

			return postCodeCollection;
		}
		#endregion
	}
}
