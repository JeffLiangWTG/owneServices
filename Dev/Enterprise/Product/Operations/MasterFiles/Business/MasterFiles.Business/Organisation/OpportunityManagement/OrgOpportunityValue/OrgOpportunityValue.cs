using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityValue : AutoOrgOpportunityValue
	{
		public OrgOpportunityValue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get { return Res.GetString("497ff554-c0c1-403e-a6f5-f16fd27defcf", "Value {0}", RevenueTypeDescription); }
		}

		#endregion

		#region Properties

		#region Estimated Value Used Value

		/// <summary>
		/// The value that gets used when populating the main opportunity's estimated value.
		/// </summary>
		protected internal virtual ZDecimal EstimatedValueUsedValue
		{
			get { return ValueAfterDiscount; }
		}

		#endregion

		#region PV_Value

		public override ZDecimal PV_Value
		{
			get { return base.PV_Value; }
			set
			{
				if (PV_Value != value)
				{
					base.PV_Value = value;
					if (IsPercentDiscount)
					{
						SetDiscountBasedOnDiscountPercentage();
					}

					var opp = Opportunity;
					if (opp != null && !opp.IsDeleted)
					{
						opp.UpdateEstimatedValue();
					}
				}
			}
		}

		bool IsPercentDiscount
		{
			get { return PV_DiscountBasis == OrgOpportunityValueLookups.DiscountBasis.Percent; }
		}

		#endregion

		#region Revenue Type Description

		public ZString RevenueTypeDescription
		{
			get { return Lookups.ValueTypes.GetDescriptionFromCode(PV_RevenueType); }
		}

		public ZPropertyInfo RevenueTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RevenueTypeDescription)); }
		}

		#endregion

		#region Value After Discount

		public ZDecimal ValueAfterDiscount
		{
			get { return PV_Value - PV_Discount; }
		}

		public ZPropertyInfo ValueAfterDiscountInfo
		{
			get { return GetZPropertyInfo(nameof(ValueAfterDiscount)); }
		}

		#endregion

		#region PV_DiscountPercent

		public override ZDecimal PV_DiscountPercent
		{
			get { return base.PV_DiscountPercent; }
			set
			{
				if (PV_DiscountPercent != value)
				{
					base.PV_DiscountPercent = value;
					SetDiscountBasedOnDiscountPercentage();
				}
			}
		}

		protected bool PV_DiscountPercent_ReadOnly
		{
			get { return (PV_DiscountBasis == OrgOpportunityValueLookups.DiscountBasis.Flat); }
		}

		void SetDiscountBasedOnDiscountPercentage()
		{
			if (!PV_ValueInfo.HasErrors() && !PV_DiscountPercentInfo.HasErrors())
			{
				PV_Discount = PV_Value * (PV_DiscountPercent / 100);
			}
			else
			{
				PV_Discount = 0;
			}
		}

		#endregion

		#region PV_Discount

		public override ZDecimal PV_Discount
		{
			get { return base.PV_Discount; }
			set
			{
				base.PV_Discount = value;

				var opp = Opportunity;
				if (opp != null && !opp.IsDeleted)
				{
					opp.UpdateEstimatedValue();
				}
			}
		}

		protected bool PV_Discount_ReadOnly
		{
			get { return (PV_DiscountBasis == OrgOpportunityValueLookups.DiscountBasis.Percent); }
		}

		#endregion

		#region PV_DiscountBasis
		[List("Lookups.DiscountBasisList")]
		public override ZString PV_DiscountBasis
		{
			get { return base.PV_DiscountBasis; }
			set
			{
				if (PV_DiscountBasis != value)
				{
					base.PV_DiscountBasis = value;
					if (PV_DiscountBasis == OrgOpportunityValueLookups.DiscountBasis.Flat)
					{
						ZDecimal oldDiscount = PV_Discount;
						PV_DiscountPercent = 0m;
						PV_Discount = oldDiscount;
					}
					else if (PV_DiscountBasis == OrgOpportunityValueLookups.DiscountBasis.Percent)
					{
						PV_Discount = 0m;
					}
				}
			}
		}

		#endregion

		#region PV_RevenueType

		[List("Lookups.ActiveValueTypes")]
		public override ZString PV_RevenueType
		{
			get
			{
				return base.PV_RevenueType;
			}
			set
			{
				base.PV_RevenueType = value;
			}
		}

		#endregion

		#endregion
	}
}
