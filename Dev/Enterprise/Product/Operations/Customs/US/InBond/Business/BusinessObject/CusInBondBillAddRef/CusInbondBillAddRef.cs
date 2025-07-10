using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInbondBillAddRef : Customs.Business.CusInbondBillAddRef,
		IInBondBillReferenceNumber,
		Integration.Customs.US.InBond.ICusInbondBillAddRef
	{
		public CusInbondBillAddRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public ZString BR_QualifierDescription
		{
			get { return Lookups.AdditionalReferenceList.GetDescriptionFromCode(BR_Qualifier) ?? ZString.Empty; }
		}

		#endregion

		#region Override Properties

		#region BR_B0

		[RelatedBusinessObject("Bill")]
		public override ZGuid BR_B0
		{
			get { return base.BR_B0; }
			set { base.BR_B0 = value; }
		}

		public CusInBondBill Bill
		{
			get { return Factory.Load<CusInBondBill>(BR_B0); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInbondBillAddRefLookups.AdditionalReferenceList))]
		public override ZString BR_Qualifier
		{
			get { return base.BR_Qualifier; }
			set { base.BR_Qualifier = value; }
		}

		public new CusInbondBillAddRefLookups Lookups
		{
			get { return (CusInbondBillAddRefLookups)base.Lookups; }
		}

		public new CusInbondBillAddRefValidation Validation
		{
			get { return (CusInbondBillAddRefValidation)base.Validation; }
		}

		#endregion

		#region Related Objects

		public CusInBondHeader Header
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? null : bill.Header;
			}
		}

		#endregion

		#region Implementation

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInbondBillAddRefLookups GetNewLookups()
		{
			return new CusInbondBillAddRefLookups(this);
		}

		protected override Customs.Business.CusInbondBillAddRefValidation GetNewValidation()
		{
			return new CusInbondBillAddRefValidation(this);
		}

		#endregion

		#region IInBondBillReferenceNumber Members

		ZString IInBondBillReferenceNumber.Qualifier
		{
			get { return BR_Qualifier; }
		}

		ZString IInBondBillReferenceNumber.ReferenceIdentifier
		{
			get { return BR_ReferenceNum; }
		}

		#endregion
	}
}
