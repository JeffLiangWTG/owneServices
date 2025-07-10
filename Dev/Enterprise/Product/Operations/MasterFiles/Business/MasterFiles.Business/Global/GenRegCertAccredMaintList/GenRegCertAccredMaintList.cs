using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(GenRegCertAccredMaintListSchema.Constants.XZ_Comment)]
	public class GenRegCertAccredMaintList : AutoGenRegCertAccredMaintList
	{
		public GenRegCertAccredMaintList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.CertificateTypes_ActiveList")]
		public override ZString XZ_Type
		{
			get { return base.XZ_Type; }
			set
			{
				var oldValue = base.XZ_Type;
				base.XZ_Type = value;
				if (oldValue != value && !IsCopying)
				{
					if (MasterParent != null && !XZ_Type.IsEmpty
						&& (XZ_Comment.IsEmpty || XZ_Comment == Lookups.CertificateTypes.GetDescriptionFromCode(oldValue)))
					{
						XZ_Comment = MasterParent.GetDefaultDescription(XZ_Type).Left(XZ_CommentInfo.MaxLength);
					}

					if (IsUKStaffHandlingSecureCargoCertificate)
					{
						XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.UnitedKingdom;
					}
					else if (XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO)
					{
						XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.China;
					}

					Validation.ValidateXZ_ExpiryOrDueDate();
				}
			}
		}

		public bool IsUKStaffHandlingSecureCargoCertificate
		{
			get
			{
				return XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CO1
					|| XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CO2
					|| XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CO3
					|| XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CS1
					|| XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CS2
					|| XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CM1;
			}
		}

		public override ZString XZ_RefNumber
		{
			get { return base.XZ_RefNumber; }
			set
			{
				var oldValue = base.XZ_RefNumber;
				base.XZ_RefNumber = value;
				if (oldValue != value && !IsCopying)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateXZ_Type();
						XZ_TypeInfo.RefreshBinding();
					}
				}
			}
		}

		public ZString XZ_TypeDescription
		{
			get { return Lookups.CertificateTypes.GetDescriptionFromCode(XZ_Type); }
		}

		[List("Lookups.StatesOrProvinces")]
		public override ZString XZ_StateOrProvinceOfIssuance
		{
			get { return base.XZ_StateOrProvinceOfIssuance; }
			set { base.XZ_StateOrProvinceOfIssuance = value; }
		}

		public ICertificatesProvider MasterParent { get; set; }

		public override void OnSaving()
		{
			base.OnSaving();

			if (XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Mexico && XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK)
			{
				if (MasterParent is GlbStaff glbStaff && glbStaff != null)
				{
					var stmNumberRangeMatchingDetail = glbStaff.NumberRangeMatchingDetails?.Cast<StmNumberRangeMatchingDetail>().Where(w => w.NRM_RangeType == OrgConstants.NumberFountains.Code.PatentNumber);
					stmNumberRangeMatchingDetail?.ForEach(z => z.PatentNumber = XZ_RefNumber);
				}
			}
		}

		#endregion

		#region Overrides

		public override void Delete()
		{
			PatternMatchingRemover.DeleteAll(this);
			base.Delete();
		}

		protected override GenRegCertAccredMaintListValidation GetNewValidation()
		{
			var validationProvider = MasterParent as ICertificatesValidationProvider;
			return (validationProvider == null ? null : validationProvider.GetValidation(this)) ?? base.GetNewValidation();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(GenRegCertAccredMaintList);
			}

			public string GetByCertificateType(GlbStaff staff, ZString certificateType)
			{
				return staff.Certificates.GetFirstCertificateNumber(certificateType, ZDateTime.Today);
			}
		}

		#endregion
	}
}
