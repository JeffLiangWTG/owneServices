using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDDataValueWrapper : NonPersistentBusinessObject
	{
		public CMDDataValueWrapper(CMDPermitNumber cMDData)
			: base(cMDData.Factory)
		{
			this.CMDDataValue = cMDData;
		}

		public void CommitChangesToCMDData()
		{
			if (HasChanges)
			{
				CMDDataValue.CY_Code = PermitOrExemptionType;
				CMDDataValue.CY_Data = PermitNumberOrExemptionRemarks;
			}
		}

		#region PermitOrExemptionType

		[MaxLength(CusCodeData.Schema.CY_TypeMaxLength)]
		[ResourceStringData("3580C322-4F6F-4FAD-AE37-E8C3D786E0D7", Caption = "Permit / Exemption Code")]
		public ZString PermitOrExemptionType
		{
			get
			{
				if (permitOrExemptionType == null)
				{
					permitOrExemptionType = CMDDataValue.CY_Code;
				}
				return permitOrExemptionType;
			}
			set
			{
				if (permitOrExemptionType != value)
				{
					CheckMaximumLength(PermitOrExemptionTypeInfo, value);
					permitOrExemptionType = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidatePermitOrExemptionType();
					}
					PermitOrExemptionTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PermitOrExemptionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(PermitOrExemptionType)); }
		}

		string permitOrExemptionType;

		#endregion

		#region PermitNumberOrExemptionRemarks

		[MaxLength(CusCodeData.Schema.CY_DataMaxLength)]
		[ResourceStringData("4119553B-2184-41F9-BF65-48C71F1D5193", Caption = "Permit No. / Exemption Remarks")]
		public ZString PermitNumberOrExemptionRemarks
		{
			get
			{
				if (permitNumberOrExemptionRemarks == null)
				{
					permitNumberOrExemptionRemarks = CMDDataValue.CY_Data;
				}
				return permitNumberOrExemptionRemarks;
			}
			set
			{
				if (permitNumberOrExemptionRemarks != value)
				{
					CheckMaximumLength(PermitNumberOrExemptionRemarksInfo, value);
					permitNumberOrExemptionRemarks = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidatePermitNumberOrExemptionRemarks();
					}
					PermitNumberOrExemptionRemarksInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PermitNumberOrExemptionRemarksInfo
		{
			get { return GetZPropertyInfo(nameof(PermitNumberOrExemptionRemarks)); }
		}

		string permitNumberOrExemptionRemarks;

		#endregion

		public bool IsTDBExemption
		{
			get { return CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption(PermitOrExemptionType); }
		}

		#region Validation

		public CMDDataValueWrapperValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		CMDDataValueWrapperValidation GetNewValidation()
		{
			return new CMDDataValueWrapperValidation(this);
		}

		#endregion

		#region Lookups

		public CMDDataValueWrapperLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		CMDDataValueWrapperLookups GetNewLookups()
		{
			return new CMDDataValueWrapperLookups(this);
		}

		CMDDataValueWrapperLookups fLookups;

		#endregion

		public readonly CMDPermitNumber CMDDataValue;
	}
}
