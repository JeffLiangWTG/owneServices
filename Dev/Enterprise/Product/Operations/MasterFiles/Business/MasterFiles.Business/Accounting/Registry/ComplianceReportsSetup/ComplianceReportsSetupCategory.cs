using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	[CodeProperty(Schema.Category), DescriptionProperty(Schema.CategoryDescription)]
	public class ComplianceReportsSetupCategory : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Category = "Category";
			public const string CategoryDescription = "CategoryDescription";
			public const string Sequence = "Sequence";
			public const string AllowDuplicate = "AllowDuplicate";
		}

		#endregion

		#region Construction

		public ComplianceReportsSetupCategory() { }

		public ComplianceReportsSetupCategory(FallbackLevel fallbackLevel) : base(fallbackLevel) { }

		public ComplianceReportsSetupCategory(ZString category, ZString description, ZDecimal sequence, ZBool allowDuplicate)
		{
			Category = category;
			CategoryDescription = description;
			Sequence = sequence;
			AllowDuplicate = allowDuplicate;
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportsSetupCategory(fallbackLevel);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCategory();
		}

		public override bool CanDelete
		{
			get { return CheckCategoryInDatabase(true).IsEmpty; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				ZString localAccountNumber = CheckCategoryInDatabase(true);
				return localAccountNumber.IsEmpty
						? base.ReasonForNotAbleToDelete
						: ResString.GetMultilingualString("d3d0f766-719b-48b7-83b1-15779db0a987",
										"The category '{0}' already mapped to Local GL Account '{1}'",
										originalValue,
										localAccountNumber);
			}
		}

		ZString CheckCategoryInDatabase(bool deleteChecker)
		{
			AccGLAccountDescriptor accountGLAccountDescriptor = null;

			if ((originalValue != Category || deleteChecker) && !originalValue.IsEmpty)
			{
				ZString reportType = ZString.Empty;
				foreach (BusinessObjectCollection collection in
					ParentCollections.Where(collection => collection.Cast<ComplianceReportsSetupCategory>().Any(item => item == this)))
				{
					reportType = ((ComplianceReportsSetupCategoryCollection)collection).ReportType;
				}

				if (!reportType.IsEmpty)
				{
					ZQuery query = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, originalValue);
					query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, reportType);
					accountGLAccountDescriptor = CurrentFactory.LoadTop1<AccGLAccountDescriptor>(query);
				}
			}

			return accountGLAccountDescriptor == null ? ZString.Empty : accountGLAccountDescriptor.AJ_LocalAccountNumber;
		}

		#region Bound Properties

		#region Report Category

		ZString fCategory = ZString.Empty;
		ZString originalValue = ZString.Empty;

		[MaxLength(3)]
		public ZString Category
		{
			get
			{
				if (originalValue.IsEmpty)
				{
					originalValue = fCategory;
				}

				return fCategory;
			}
			set
			{
				CheckMaximumLength(CategoryInfo, value);
				SetNonPersistentPropertyValue(CategoryInfo, ref fCategory, value);
				CategoryInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateCategory();
				}
			}
		}

		public ZPropertyInfo CategoryInfo
		{
			get { return GetZPropertyInfo(Schema.Category); }
		}

		protected void ValidateCategory()
		{
			CategoryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CategoryInfo, Res.GetString("21e3d84b-9950-41dd-acba-cb41d5c9e1a3", "Category Code"));

			CheckRecordIsUnique();

			ZString localAccountNumber = CheckCategoryInDatabase(false);
			if (!localAccountNumber.IsEmpty)
			{
				CategoryInfo.AddError(Res.GetString("d3d0f766-719b-48b7-83b1-15779db0a987",
													"The category '{0}' already mapped to Local GL Account '{1}'", originalValue, localAccountNumber));
			}
		}

		void CheckRecordIsUnique()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (ComplianceReportsSetupCategory item in collection)
				{
					item.ClearRowNotifications();
					if (item != this && item.Category == Category)
					{
						ZString duplicateMessage = Res.GetString("72F9DC82-954F-4C3B-97BE-8D69D19E5847", "Duplicate Category Code");
						AddRowError(duplicateMessage);
						item.AddRowError(duplicateMessage);
						break;
					}

					if (item != this && item.Category == originalValue)
					{
						originalValue = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region Report Category Description

		ZString categoryDescription = ZString.Empty;

		[MaxLength(256)]
		public ZString CategoryDescription
		{
			get { return categoryDescription; }
			set
			{
				SetNonPersistentPropertyValue(CategoryDescriptionInfo, ref categoryDescription, value, false);
			}
		}

		public ZPropertyInfo CategoryDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CategoryDescription); }
		}

		#endregion

		#region Category Sequence

		ZDecimal categorySequence = 0m;
		[ReadOnly(true)]
		public ZDecimal Sequence
		{
			get { return categorySequence; }
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				SetNonPersistentPropertyValue(SequenceInfo, ref categorySequence, value, false);
			}
		}

		public ZPropertyInfo SequenceInfo
		{
			get { return GetZPropertyInfo(Schema.Sequence); }
		}

		#endregion

		#region Report Allow Duplicate

		ZBool fAllowDuplicate = false;
		public ZBool AllowDuplicate
		{
			get { return fAllowDuplicate; }
			set { SetNonPersistentPropertyValue(AllowDuplicateInfo, ref fAllowDuplicate, value); }
		}

		public ZPropertyInfo AllowDuplicateInfo
		{
			get { return GetZPropertyInfo(Schema.AllowDuplicate); }
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Category, Category);
			writer.WriteElementString(Schema.CategoryDescription, CategoryDescription);
			writer.WriteElementString(Schema.Sequence, Sequence.ToString());
			writer.WriteElementString(Schema.AllowDuplicate, AllowDuplicate.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Category = reader.ReadElementString(Schema.Category);
			CategoryDescription = (NoResString)reader.ReadElementString(Schema.CategoryDescription);
			Sequence = reader.ReadElementStringAsZDecimal(Schema.Sequence);
			AllowDuplicate = reader.ReadElementStringAsZBool(Schema.AllowDuplicate);
		}

		#endregion
	}
}
