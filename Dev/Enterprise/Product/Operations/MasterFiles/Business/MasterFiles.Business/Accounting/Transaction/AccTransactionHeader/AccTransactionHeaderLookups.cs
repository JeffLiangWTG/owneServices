//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderLookups
//
//    This class should be used for overriding collections in AutoAccTransactionHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderLookups : AutoAccTransactionHeaderLookups
	{
		public AccTransactionHeaderLookups(AutoAccTransactionHeader parent) : base(parent)
		{
		}

		public new AccTransactionHeader Parent
		{
			get { return (AccTransactionHeader)base.Parent; }
		}

		public override OrgHeaderCollection Headers
		{
			get { return new DebtorOrCreditorCollection(Parent.Factory); }
		}

		public override GlbDepartmentCollection Departments
		{
			get
			{
				GlbDepartmentCollection result = new GlbDepartmentCollection(Factory, new ZQuery(GlbDepartmentSchema.GE_IsActive, true));
				return result;
			}
		}

		public override GlbBranchCollection TaxBranches => AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory);

		#region PlacesOfSupply

		public ReadOnlyCodeDescriptionPairList PlacesOfSupply => PlaceOfSupplyListProvider.GetPlaceOfSupplyList(Parent.Company);

		public ReadOnlyCodeDescriptionPairList PlaceOfSupplyTypes => PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(Parent.Company);

		#endregion
	}
}
