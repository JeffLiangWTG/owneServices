using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract class AutoPGA : Customs.Business.MultiLineAddInfos.CusAddInfo<USPGAAddInfo>
	{
		protected AutoPGA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USPGAAddInfo>.Schema
		{
			public const string US_PGACommercialDescription = USPGAAddInfoSchema.Constants.US_PGACommercialDescription;
			public const string US_PGALineItemNumber = USPGAAddInfoSchema.Constants.US_PGALineItemNumber;
			public const string US_PGALineValue = USPGAAddInfoSchema.Constants.US_PGALineValue;
			public const string US_InvCurrPGAValue = USPGAAddInfoSchema.Constants.US_InvCurrPGAValue;
			public const string US_UnknownBreakdown = USPGAAddInfoSchema.Constants.US_UnknownBreakdown;
			public const string US_UnknownBreakdownTotal = USPGAAddInfoSchema.Constants.US_UnknownBreakdownTotal;
			public const string US_NameOfConstituentElement = USPGAAddInfoSchema.Constants.US_NameOfConstituentElement;
			public const string US_QuantityOfConstituentElement = USPGAAddInfoSchema.Constants.US_QuantityOfConstituentElement;
			public const string US_UnitOfMeasure = USPGAAddInfoSchema.Constants.US_UnitOfMeasure;
			public const string US_TrackingStatus = USPGAAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_CertifyingIndividual = USPGAAddInfoSchema.Constants.US_CertifyingIndividual;
			public const string US_PGAContactEmail = USPGAAddInfoSchema.Constants.US_PGAContactEmail;
			public const string US_PGAContactName = USPGAAddInfoSchema.Constants.US_PGAContactName;
			public const string US_PGAContactPhoneNo = USPGAAddInfoSchema.Constants.US_PGAContactPhoneNo;
		}
		#endregion

		#region AddInfo Properties

		#region US_PGAContactName

		public virtual ZString US_PGAContactPhoneNo
		{
			get { return AddInfo.US_PGAContactPhoneNo; }
			set { AddInfo.US_PGAContactPhoneNo = value; }
		}

		public virtual ZPropertyInfo US_PGAContactPhoneNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAContactPhoneNo, x => AddInfo.US_PGAContactPhoneNoInfo); }
		}

		#endregion

		#region US_PGAContactName

		public virtual ZString US_PGAContactName
		{
			get { return AddInfo.US_PGAContactName; }
			set { AddInfo.US_PGAContactName = value; }
		}

		public virtual ZPropertyInfo US_PGAContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAContactName, x => AddInfo.US_PGAContactNameInfo); }
		}

		#endregion

		#region US_PGAContactEmail

		public virtual ZString US_PGAContactEmail
		{
			get { return AddInfo.US_PGAContactEmail; }
			set { AddInfo.US_PGAContactEmail = value; }
		}

		public virtual ZPropertyInfo US_PGAContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAContactEmail, x => AddInfo.US_PGAContactEmailInfo); }
		}

		#endregion

		#region US_CertifyingIndividual

		[List(nameof(AddInfoLookups) + "." + nameof(USPGAAddInfoLookups.CertifyingIndividualList))]
		public virtual ZString US_CertifyingIndividual
		{
			get { return AddInfo.US_CertifyingIndividual; }
			set { AddInfo.US_CertifyingIndividual = value; }
		}

		public virtual ZPropertyInfo US_CertifyingIndividualInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertifyingIndividual, x => AddInfo.US_CertifyingIndividualInfo); }
		}

		#endregion

		#region US_PGACommercialDescription

		public virtual ZString US_PGACommercialDescription
		{
			get { return AddInfo.US_PGACommercialDescription; }
			set { AddInfo.US_PGACommercialDescription = value; }
		}

		public virtual ZPropertyInfo US_PGACommercialDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGACommercialDescription, x => AddInfo.US_PGACommercialDescriptionInfo); }
		}

		#endregion

		#region US_PGALineItemNumber

		public virtual ZInt US_PGALineItemNumber
		{
			get { return AddInfo.US_PGALineItemNumber; }
			set { AddInfo.US_PGALineItemNumber = value; }
		}

		public virtual ZPropertyInfo US_PGALineItemNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGALineItemNumber, x => AddInfo.US_PGALineItemNumberInfo); }
		}

		public bool US_PGALineItemNumber_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region US_PGALineValue

		public virtual ZDecimal US_PGALineValue
		{
			get { return AddInfo.US_PGALineValue; }
			set { AddInfo.US_PGALineValue = value; }
		}

		public virtual ZPropertyInfo US_PGALineValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGALineValue, x => AddInfo.US_PGALineValueInfo); }
		}

		#endregion

		#region US_InvCurrPGAValue

		public virtual ZDecimal US_InvCurrPGAValue
		{
			get { return AddInfo.US_InvCurrPGAValue; }
			set { AddInfo.US_InvCurrPGAValue = value; }
		}

		public virtual ZPropertyInfo US_InvCurrPGAValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InvCurrPGAValue, x => AddInfo.US_InvCurrPGAValueInfo); }
		}

		#endregion

		#region US_UnknownBreakdown

		public virtual ZBool US_UnknownBreakdown
		{
			get { return AddInfo.US_UnknownBreakdown; }
			set { AddInfo.US_UnknownBreakdown = value; }
		}

		public virtual ZPropertyInfo US_UnknownBreakdownInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnknownBreakdown, x => AddInfo.US_UnknownBreakdownInfo); }
		}

		#endregion

		#region US_UnknownBreakdownTotal

		public virtual ZBool US_UnknownBreakdownTotal
		{
			get { return AddInfo.US_UnknownBreakdownTotal; }
			set { AddInfo.US_UnknownBreakdownTotal = value; }
		}

		public virtual ZPropertyInfo US_UnknownBreakdownTotalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnknownBreakdownTotal, x => AddInfo.US_UnknownBreakdownTotalInfo); }
		}

		#endregion

		#region US_NameOfConstituentElement

		public virtual ZString US_NameOfConstituentElement
		{
			get { return AddInfo.US_NameOfConstituentElement; }
			set { AddInfo.US_NameOfConstituentElement = value; }
		}

		public virtual ZPropertyInfo US_NameOfConstituentElementInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NameOfConstituentElement, x => AddInfo.US_NameOfConstituentElementInfo); }
		}

		public bool US_NameOfConstituentElement_ReadOnly
		{
			get { return !US_UnknownBreakdownTotal; }
		}

		#endregion

		#region US_QuantityOfConstituentElement

		public virtual ZDecimal US_QuantityOfConstituentElement
		{
			get { return AddInfo.US_QuantityOfConstituentElement; }
			set { AddInfo.US_QuantityOfConstituentElement = value; }
		}

		public virtual ZPropertyInfo US_QuantityOfConstituentElementInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_QuantityOfConstituentElement, x => AddInfo.US_QuantityOfConstituentElementInfo); }
		}

		public bool US_QuantityOfConstituentElement_ReadOnly
		{
			get { return !US_UnknownBreakdownTotal; }
		}

		#endregion

		#region US_UnitOfMeasure

		[List(nameof(AddInfoLookups) + "." + nameof(USPGAAddInfoLookups.UnitOfMeasureList))]
		public virtual ZString US_UnitOfMeasure
		{
			get { return AddInfo.US_UnitOfMeasure; }
			set { AddInfo.US_UnitOfMeasure = value; }
		}

		public virtual ZPropertyInfo US_UnitOfMeasureInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnitOfMeasure, x => AddInfo.US_UnitOfMeasureInfo); }
		}

		public bool US_UnitOfMeasure_ReadOnly
		{
			get { return !US_UnknownBreakdownTotal; }
		}

		#endregion

		#region US_TrackingStatus
		public virtual ZString US_TrackingStatus
		{
			get { return AddInfo.US_TrackingStatus; }
			set { AddInfo.US_TrackingStatus = value; }
		}

		public virtual ZPropertyInfo US_TrackingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TrackingStatus, x => AddInfo.US_TrackingStatusInfo); }
		}
		#endregion

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USPGAAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USPGAAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		protected USPGAAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USPGAAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USPGAAddInfo fAddInfo;

		#endregion

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}
