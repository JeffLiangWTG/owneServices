using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgWebURL : AutoOrgWebURL
	{
		public OrgWebURL(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override bool IsSavedByFactory
		{
			get
			{
				bool res = IsDeleted || (PU_IsPrimary && !PU_URL.IsEmpty) || !PU_IsPrimary;
				return base.IsSavedByFactory && res;
			}
		}

		#region Properties

		public override ZString PU_Description
		{
			get
			{
				return base.PU_Description;
			}
			set
			{
				if (PU_Description != value)
				{
					MainDefaultAdded = false;
				}
				base.PU_Description = value;
				MarkParentAsNeedingValidation();
			}
		}

		public override ZBool PU_IsPrimary
		{
			get
			{
				return base.PU_IsPrimary;
			}
			set
			{
				if (PU_IsPrimary != value)
				{
					MainDefaultAdded = false;
				}
				base.PU_IsPrimary = value;
				MarkParentAsNeedingValidation();
			}
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid PU_OH
		{
			get
			{
				return base.PU_OH;
			}
			set
			{
				base.PU_OH = value;
				if (Header != null && Header.OrgWebURLs != null && Header.OrgWebURLs.Count > 0)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		[List("Lookups.OrgWebURLTypeList")]
		public override ZString PU_Type
		{
			get
			{
				return base.PU_Type;
			}
			set
			{
				if (PU_Type != value)
				{
					MainDefaultAdded = false;
				}
				base.PU_Type = value;
				MarkParentAsNeedingValidation();
			}
		}

		[ActionField(FieldType = ActionFieldType.Text, MaxLength = Schema.PU_URLMaxLength)]
		public override ZString PU_URL
		{
			get
			{
				return base.PU_URL;
			}
			set
			{
				if (PU_URL != value)
				{
					if (Header != null)
					{
						MainDefaultAdded = false;
						Header.FindDuplicates();
					}
				}
				base.PU_URL = value;
				MarkParentAsNeedingValidation();
			}
		}

		public ZBool MainDefaultAdded
		{
			get { return mainDefaultAdded; }
			set { SetNonPersistentPropertyValue(MainDefaultAddedInfo, ref mainDefaultAdded, value); }
		}

		ZBool mainDefaultAdded;

		public ZPropertyInfo MainDefaultAddedInfo
		{
			get { return GetZPropertyInfo(nameof(MainDefaultAdded)); }
		}

		#endregion

		ZBool needToValidateHeader = ZBool.True;
		public void SuspendSettingMarkParentAsNeedingValidation()
		{
			needToValidateHeader = ZBool.False;
		}

		public void ResumeSettingMarkParentAsNeedingValidation()
		{
			needToValidateHeader = ZBool.True;
		}

		void MarkParentAsNeedingValidation()
		{
			if (Header != null && needToValidateHeader && !MainDefaultAdded)
			{
				Header.MarkAsNeedingValidation();
			}
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (Header != null)
			{
				shouldBeReadOnly = !Header.SecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				var query = new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, PK);
				Factory.Load<PatternMatchingDomain>(query).ForEach((x) => x.Delete());
			}

			base.Delete();
		}
	}
}
