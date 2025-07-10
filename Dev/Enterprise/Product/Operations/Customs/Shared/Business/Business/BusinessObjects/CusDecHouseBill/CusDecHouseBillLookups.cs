//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusDecHouseBillLookups
//
//    This class should be used for overriding collections in AutoCusDecHouseBillLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusDecHouseBillLookups : AutoCusDecHouseBillLookups
	{
		public CusDecHouseBillLookups(AutoCusDecHouseBill parent) : base(parent)
		{
		}

		new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		public OrganisationsFindBoxCollection BillIssuers
		{
			get { return new OrganisationsFindBoxCollection(Factory, BillIssuerQuery); }
		}

		ZQuery BillIssuerQuery
		{
			get
			{
				if (fBillIssuerQuery == null)
				{
					fBillIssuerQuery = new ZQuery();
					fBillIssuerQuery.AddToFilter(OrgHeaderSchema.OH_IsAirLine, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsLocalTransport, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsRailProvider, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingLine, SQLComparisonOperator.Equal, ZBool.True);
					fBillIssuerQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingProvider, SQLComparisonOperator.Equal, ZBool.True);
				}
				return fBillIssuerQuery;
			}
		}
		ZQuery fBillIssuerQuery;

		public ICodeDescriptionPairList CU_BillTypeList
		{
			get
			{
				var dec = Parent.Declaration;
				return dec == null ? new CodeDescriptionPairList() : dec.GetBillTypeList();
			}
		}

		//Changing this to collection has a problem with ListValidation as CodeProperty of Bill is a calculated property.
		public CodeDescriptionPairList CU_ParentBillList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();

				if (Parent.Declaration != null)
				{
					foreach (Bill bill in Parent.Declaration.Bills)
					{
						if ((Parent.CU_BillType == BillTypeList.Codes.HouseBill && bill.CU_BillType == BillTypeList.Codes.MasterBill) ||
							(Parent.CU_BillType == BillTypeList.Codes.SubHouseBill && bill.CU_BillType == BillTypeList.Codes.HouseBill))
						{
							result.AddPair(bill.CU_BillUniqueCode, bill.CU_BillUniqueCode);
						}
					}
				}

				return result;
			}
		}

		public virtual CodeDescriptionPairList NoOfPacksPackType_List
		{
			get
			{
				return RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
			}
		}

		public virtual CodeDescriptionPairList MessageStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}
	}
}
