using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class RatingDocumentsChargeOrder : AutoRatingDocumentsChargeOrder
	{
		public RatingDocumentsChargeOrder(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ChargeCode

		public override AccChargeCode ChargeCode
		{
			get
			{
				if (fChargeCode == null || (!RCO_AC_ChargeCode.IsEmpty && fChargeCode.PK != RCO_AC_ChargeCode))
				{
					fChargeCode = Factory.Load<AccChargeCode>(RCO_AC_ChargeCode);
					if (fChargeCode != null &&
						!fChargeCode.MatchesFilter(((IBusinessObjectCollection)Lookups.ChargeCodes).CompleteFilter))
					{
						fChargeCode = null;
					}
				}
				return fChargeCode;
			}
		}
		AccChargeCode fChargeCode;

		#endregion

		#region Charge Description

		public ZString AC_Desc => ChargeCode != null ? ChargeCode.AC_Desc : ZString.Empty;

		public virtual ZPropertyInfo AC_DescInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(AccChargeCodeSchema.Constants.AC_Desc);
			}
		}

		#endregion

		#region Charge Base Sequence

		public ZShort AC_PrintSequence => ChargeCode != null ? ChargeCode.AC_PrintSequence : ZShort.Zero;

		#endregion

		#region RCO_AC_ChargeCode
		[List("Lookups.ChargeCodes")]
		public override ZGuid RCO_AC_ChargeCode
		{
			get
			{
				return base.RCO_AC_ChargeCode;
			}
			set
			{
				if (base.RCO_AC_ChargeCode != value)
				{
					base.RCO_AC_ChargeCode = value;
					AC_DescInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region RCO_DocumentType

		[List("Lookups.RatingDocumentsTypeList")]
		public override ZString RCO_DocumentType
		{
			get
			{
				return base.RCO_DocumentType;
			}
			set
			{
				base.RCO_DocumentType = value;
			}
		}

		#endregion

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
			=> Client == null
				|| MetaData.GetReadOnlyExcludingMethodProvider(this, property);
	}
}
