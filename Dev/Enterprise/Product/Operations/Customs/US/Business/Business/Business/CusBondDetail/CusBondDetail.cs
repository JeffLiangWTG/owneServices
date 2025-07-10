using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.Business
{
	public class CusBondDetail : MasterFiles.Business.CusBondDetail
	{
		public CusBondDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : MasterFiles.Business.CusBondDetail.Schema
		{
			public const string HasSufficientFund = "HasSufficientFund";
			public const string PW_StatusDesc = "PW_StatusDesc";
		}

		public new OrgHeader Parent
		{
			get { return (OrgHeader)base.Parent; }
			set { base.Parent = value; }
		}

		#region HasSufficientFund

		public ZBool HasSufficientFund
		{
			get { return PW_Status == FundIndicatorList.Codes.HasSufficientFund; }
			set
			{
				var oldValue = HasSufficientFund;
				if (!IsCopying && value != oldValue)
				{
					PW_Status = value ? FundIndicatorList.Codes.HasSufficientFund : FundIndicatorList.Codes.HasInsufficientFund;
					HasSufficientFundInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo HasSufficientFundInfo
		{
			get { return GetZPropertyInfo(Schema.HasSufficientFund); }
		}

		#endregion

		#region HasSufficientFund

		public ZString PW_StatusDesc
		{
			get { return Lookups.FundIndicatorList.GetDescriptionFromCode(PW_Status); }
		}

		public ZPropertyInfo PW_StatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.PW_StatusDesc); }
		}

		#endregion

		public bool IsEmpty
		{
			get { return PW_ActivityCode.IsEmpty && PW_BondAmount.IsEmpty && !PW_BondEffectiveDate.IsValid && PW_BondFiledPort.IsEmpty && PW_BondNumber.IsEmpty && PW_BondType.IsEmpty; }
		}

		public bool IsBondActive(ZDateTime date)
		{
			return (PW_BondEffectiveDate.IsEmpty || PW_BondEffectiveDate <= date) && (PW_BondExpiryDate >= date || PW_BondExpiryDate.IsEmpty);
		}

		public bool IsContinuousBond
		{
			get { return PW_BondType == ImporterBondTypeList.Codes.ContinuousBond; }
		}

		public bool IsSingleTransactionBond
		{
			get { return PW_BondType == ImporterBondTypeList.Codes.SingleTransactionBond; }
		}

		#region Implementation

		protected override MasterFiles.Business.CusBondDetailValidation GetNewValidation()
		{
			return new CusBondDetailValidation(this);
		}

		protected override MasterFiles.Business.CusBondDetailLookups GetNewLookups()
		{
			return new CusBondDetailLookups(this);
		}

		public new CusBondDetailLookups Lookups
		{
			get { return (CusBondDetailLookups)base.Lookups; }
		}

		public new CusBondDetailValidation Validation
		{
			get { return (CusBondDetailValidation)base.Validation; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
			PW_Status = FundIndicatorList.Codes.HasSufficientFund;
		}

		#endregion
	}
}
