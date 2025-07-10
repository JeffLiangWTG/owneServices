using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(ExportAWBHeader), "ExportAWBSecurityStatusLines")]
	public class ExportAWBSecurityStatusLine : AutoExportAWBSecurityStatusLine, IAWBSecurityStatusLineMessageDetailsProvider
	{
		public ExportAWBSecurityStatusLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public virtual ExportAWBHeader Master
		{
			get { return Factory.Load<ExportAWBHeader>(EAS_EH); }
		}

		#region EAS_ApprovalCategory

		[List("ApprovalCategoryList")]
		public override ZString EAS_ApprovalCategory
		{
			get { return base.EAS_ApprovalCategory; }
			set { base.EAS_ApprovalCategory = value; }
		}

		public virtual CodeDescriptionPairList ApprovalCategoryList
		{
			get { return Factory.GetCachedValue("ExportAWBSecurityStatusLine.ApprovalCategoryList", () => new AviationSecuritySchemeMembership()); }
		}

		#endregion

		#region Type

		public SecurityStatusLineType Type
		{
			get
			{
				if (!EAS_ScreeningMethod.IsEmpty)
				{
					return SecurityStatusLineType.ScreeningMethod;
				}

				if (!EAS_ExemptionGround.IsEmpty)
				{
					return SecurityStatusLineType.ExceptionCode;
				}

				return SecurityStatusLineType.KnownConsignor;
			}
		}

		#endregion

		#region IsEmpty

		public bool IsEmpty
		{
			get
			{
				return EAS_ApprovalCategory.IsEmpty
					&& EAS_RN_NKCountryCode.IsEmpty
					&& EAS_ApprovalNumber.IsEmpty
					&& EAS_ApprovalExpiryDate.IsEmpty
					&& EAS_ScreeningMethod.IsEmpty
					&& EAS_ExemptionGround.IsEmpty;
			}
		}

		#endregion

		#region IAWBSecurityStatusLineMessageDetailsProvider Members

		public ZString ApprovalCategory => EAS_ApprovalCategory;

		public ZString ApprovalNumber => EAS_ApprovalNumber;

		ZString IAWBSecurityStatusLineMessageDetailsProvider.CountryCode => EAS_RN_NKCountryCode;

		public ZDateTime ApprovalExpiryDate => EAS_ApprovalExpiryDate;

		#endregion

		#region ShouldPropertiesBeReadOnly

		public void RefreshBindingForOverride()
		{
			EAS_ApprovalCategoryInfo.RefreshBinding();
			EAS_RN_NKCountryCodeInfo.RefreshBinding();
			EAS_ApprovalNumberInfo.RefreshBinding();
			EAS_ApprovalExpiryDateInfo.RefreshBinding();
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == ExportAWBSecurityStatusLineSchema.Constants.EAS_ApprovalExpiryDate)
			{
				return EAS_ApprovalExpiryDate_ReadOnly;
			}
			else
			{
				return Master != null && !Master.IsCSDOverridden;
			}
		}

		protected virtual bool EAS_ApprovalExpiryDate_ReadOnly => false;

		#endregion
	}
}
