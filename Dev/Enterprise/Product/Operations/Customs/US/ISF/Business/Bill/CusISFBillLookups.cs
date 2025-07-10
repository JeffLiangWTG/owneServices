using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFBillLookups : AutoCusISFBillLookups
	{
		public CusISFBillLookups(AutoCusISFBill parent)
			: base(parent)
		{
		}

		#region Bill Types

		public CodeDescriptionPairList ReferenceBillTypes
		{
			get
			{
				return Factory.GetCachedValue("CusISFBillReferenceBillTypes", delegate
				{
					return BillTypeList.GetReferenceBillTypes();
				});
			}
		}

		public BillTypeList BillTypes
		{
			get { return Parent.Factory.GetCachedValue<BillTypeList>(); }
		}

		#endregion

		public DispositionCodeList DispositionCodeList
		{
			get { return Factory.GetCachedValue<DispositionCodeList>(); }
		}

		public virtual JobDeclarationCollection Declarations
		{
			get { return new JobDeclarationCollection(Factory, Parent.Header.Branch.Company.PK); }
		}

		protected new CusISFBill Parent
		{
			get { return (CusISFBill)base.Parent; }
		}
	}
}
