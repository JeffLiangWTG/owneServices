using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedBillsGenPivot : CustomsGenPivot
		, IMasterHouse
		, Integration.Customs.US.IFDARelatedBillsGenPivot
	{
		public const string RelationType = GenPivotTypeDecider.Types.FDARelatedBillsGenPivot;
		public FDARelatedBillsGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FDARelatedBillsGenPivot|XX_Relation1ID", Caption = "FDA")]
		public override ZGuid XX_Relation1ID
		{
			get { return base.XX_Relation1ID; }
			set { base.XX_Relation1ID = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FDARelatedBillsGenPivot|XX_Relation2ID", Caption = "Bill")]
		public override ZGuid XX_Relation2ID
		{
			get { return base.XX_Relation2ID; }
			set { base.XX_Relation2ID = value; }
		}

		public new FDA Relation1Object
		{
			get { return base.Relation1Object as FDA; }
			set { base.Relation1Object = value; }
		}

		public new Bill Relation2Object
		{
			get { return base.Relation2Object as Bill; }
			set { base.Relation2Object = value; }
		}

		public new FDARelatedBillsGenPivotValidation Validation
		{
			get { return (FDARelatedBillsGenPivotValidation)base.Validation; }
		}

		protected override GenPivotValidation GetNewValidation()
		{
			return new FDARelatedBillsGenPivotValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = RelationType;
			XX_Relation1TableCode = CusAddInfoSchema.Constants.Prefix;
			XX_Relation2TableCode = CusDecHouseBillSchema.Constants.Prefix;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(FDARelatedBillsGenPivot pivot)
				: base(pivot)
			{
			}

			protected new FDARelatedBillsGenPivot BusinessObject
			{
				get { return (FDARelatedBillsGenPivot)base.BusinessObject; }
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(CusAddInfoSchema.PK, BusinessObject.XX_Relation1ID);
			}
		}

		#region IMasterHouse Members

		ZString IMasterHouse.MasterBillSCAC
		{
			get { return Relation2Object != null ? Relation2Object.EffectiveMasterBillIssuerSCAC : ZString.Empty; }
		}

		ZString IMasterHouse.MasterBill
		{
			get { return Relation2Object != null ? Relation2Object.CU_MasterBill : ZString.Empty; }
		}

		ZString IMasterHouse.HouseBillSCAC
		{
			get { return Relation2Object != null ? Relation2Object.EffectiveHouseBillIssuerSCAC : ZString.Empty; }
		}

		ZString IMasterHouse.HouseBill
		{
			get { return Relation2Object != null ? Relation2Object.CU_HouseBill : ZString.Empty; }
		}

		#endregion
	}
}
