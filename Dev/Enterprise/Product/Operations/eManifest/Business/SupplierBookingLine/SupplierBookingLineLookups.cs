//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSupplierBookingLineLookups
//
//    This class should be used for overriding collections in AutoSupplierBookingLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingLineLookups : AutoSupplierBookingLineLookups
	{
		public SupplierBookingLineLookups(AutoSupplierBookingLine parent)
			: base(parent)
		{
		}

		public new SupplierBookingLine Parent
		{
			get { return (SupplierBookingLine)base.Parent; }
		}

		public BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public CodeDescriptionPairList DL_CubicUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList DL_GrossWeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public RefServiceLevelCollection DL_RS_NKServiceLevel_List
		{
			get { return BindingLists.RefServiceLevel_List; }
		}

		public CodeDescriptionPairList ConsigneeStateList
		{
			get
			{
				if (previousConsigneeCountryCode != Parent.DL_RN_NKConsigneeCountryCode)
				{
					consigneeStateList = Factory.GetStateList(Parent.DL_RN_NKConsigneeCountryCode, Parent.DL_RN_NKConsigneeCountryCodeInfo.HasErrors());
					previousConsigneeCountryCode = Parent.DL_RN_NKConsigneeCountryCode;
				}

				return consigneeStateList;
			}
		}
		CodeDescriptionPairList consigneeStateList = new CodeDescriptionPairList(OLookUpEditType.CustomType);
		ZString previousConsigneeCountryCode;

		public CodeDescriptionPairList ConsignorStateList
		{
			get
			{
				if (previousConsignorCountryCode != Parent.DL_RN_NKConsignorCountryCode)
				{
					consignorStateList = Factory.GetStateList(Parent.DL_RN_NKConsignorCountryCode, Parent.DL_RN_NKConsignorCountryCodeInfo.HasErrors());
					previousConsignorCountryCode = Parent.DL_RN_NKConsignorCountryCode;
				}

				return consignorStateList;
			}
		}
		CodeDescriptionPairList consignorStateList = new CodeDescriptionPairList(OLookUpEditType.CustomType);
		ZString previousConsignorCountryCode;
	}
}
