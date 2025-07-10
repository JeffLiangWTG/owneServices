using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class HouseBillRefNo : CusCodeData,
		IInBondBillReferenceNumber
	{
		public HouseBillRefNo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new HouseBillRefNoLookups Lookups
		{
			get { return (HouseBillRefNoLookups)base.Lookups; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new HouseBillRefNoLookups(this);
		}

		public new HouseBillRefNoValidation Validation
		{
			get { return (HouseBillRefNoValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new HouseBillRefNoValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.HouseBillRefNo;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(Bill)); }
		}

		#region IInBondBillReferenceNumber Members

		ZString IInBondBillReferenceNumber.Qualifier
		{
			get { return CY_Code; }
		}

		ZString IInBondBillReferenceNumber.ReferenceIdentifier
		{
			get { return CY_Data; }
		}

		#endregion

	}
}
