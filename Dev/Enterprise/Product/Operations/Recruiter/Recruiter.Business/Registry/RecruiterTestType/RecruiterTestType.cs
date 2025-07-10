using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class RecruiterTestType : RegistryBusinessObject, ICodeDescription
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Category = "Category";
			public const string TestNote = "TestNote";
			public const string IsVisibleOnWeb = "IsVisibleOnWeb";
			public const string CategoryTitle = "CategoryTitle";
			public const string NotificationType = "NotificationType";
			public const string DocumentName = "DocumentName";
		}

		#endregion

		public RecruiterTestType()
		{
		}

		public RecruiterTestType(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RecruiterTestType();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone1)
		{
			base.CopyValuesToClone(clone1);

			RecruiterTestType clone = (RecruiterTestType)clone1;
			clone.CodeMaxLength = CodeMaxLength;
			clone.Code = Code;
			clone.Description = Description;
			clone.Category = Category;
			clone.TestNote = TestNote;
			clone.IsVisibleOnWeb = IsVisibleOnWeb;
			clone.NotificationType = NotificationType;
			clone.DocumentName = DocumentName;
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCategory();
			ValidateNotificationType();
			ValidateDocumentName();
		}

		#region Bound Properties

		#region Category

		[MaxLength(256)]
		public ZString Category
		{
			get { return category; }
			set
			{
				CheckMaximumLength(CategoryInfo, value);
				SetNonPersistentPropertyValue(CategoryInfo, ref category, value);
				if (!IsValidationSuspended)
				{
					ValidateCategory();
				}
			}
		}
		ZString category;

		public ZPropertyInfo CategoryInfo
		{
			get { return GetZPropertyInfo(Schema.Category); }
		}

		public void ValidateCategory()
		{
			CategoryInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CategoryInfo, CategoryList);
		}

		public ZString CategoryDescription
		{
			get { return CategoryList.GetDescriptionFromCode(Category); }
		}

		#endregion

		#region TestNote

		[MaxLength(2000)]
		public ZString TestNote
		{
			get { return testNote; }
			set
			{
				CheckMaximumLength(TestNoteInfo, value);
				SetNonPersistentPropertyValue(TestNoteInfo, ref testNote, value);
			}
		}
		ZString testNote;

		public ZPropertyInfo TestNoteInfo
		{
			get { return GetZPropertyInfo(Schema.TestNote); }
		}

		#endregion

		#region Visible On Web

		public ZBool IsVisibleOnWeb
		{
			get { return isVisibleOnWeb; }
			set { SetNonPersistentPropertyValue(IsVisibleOnWebInfo, ref isVisibleOnWeb, value); }
		}
		ZBool isVisibleOnWeb = true;

		public ZPropertyInfo IsVisibleOnWebInfo
		{
			get { return GetZPropertyInfo(Schema.IsVisibleOnWeb); }
		}

		#endregion

		#region CategoryTitle

		[MaxLength(256)]
		public ZString CategoryTitle
		{
			get { return categoryTitle; }
			set
			{
				CheckMaximumLength(CategoryTitleInfo, value);
				SetNonPersistentPropertyValue(CategoryTitleInfo, ref categoryTitle, value);
			}
		}
		ZString categoryTitle;

		public ZPropertyInfo CategoryTitleInfo
		{
			get { return GetZPropertyInfo(Schema.CategoryTitle); }
		}

		#endregion

		#region NotificationType

		[MaxLength(3)]
		public ZString NotificationType
		{
			get { return notificationType; }
			set
			{
				CheckMaximumLength(NotificationTypeInfo, value);
				SetNonPersistentPropertyValue(NotificationTypeInfo, ref notificationType, value);
				if (!IsValidationSuspended)
				{
					ValidateNotificationType();
				}
			}
		}
		ZString notificationType;

		public ZPropertyInfo NotificationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.NotificationType); }
		}

		public void ValidateNotificationType()
		{
			NotificationTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(NotificationTypeInfo, NotificationTypeList);
			MandatoryValidation.CheckEntered(NotificationTypeInfo);
		}

		#endregion

		#region DocumentName

		[MaxLength(256)]
		[ReadOnlyMember(nameof(IsDocumentNameReadOnly))]
		public ZString DocumentName
		{
			get { return documentName; }
			set
			{
				CheckMaximumLength(DocumentNameInfo, value);
				SetNonPersistentPropertyValue(DocumentNameInfo, ref documentName, value);
				if (!IsValidationSuspended)
				{
					ValidateDocumentName();
				}
			}
		}
		ZString documentName;

		public ZPropertyInfo DocumentNameInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentName); }
		}

		public void ValidateDocumentName()
		{
			DocumentNameInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DocumentNameInfo, DocumentNames);

			if (NotificationType == RecruiterTestNotificationType.Codes.DeliverDocument)
			{
				MandatoryValidation.CheckEntered(DocumentNameInfo);
			}
		}

		public ZBool IsDocumentNameReadOnly => NotificationType != RecruiterTestNotificationType.Codes.DeliverDocument;

		#endregion

		#endregion

		#region Lookups

		public RecruiterTestCategory CategoryList
		{
			get { return categoryList ?? (categoryList = new RecruiterTestCategory()); }
		}
		RecruiterTestCategory categoryList;

		public RecruiterTestNotificationType NotificationTypeList
		{
			get { return notificationTypeList ?? (notificationTypeList = new RecruiterTestNotificationType()); }
		}
		RecruiterTestNotificationType notificationTypeList;

		public CodeDescriptionPairList DocumentNames
		{
			get
			{
				if (documentNames == null)
				{
					documentNames = new CodeDescriptionPairList();
					documents = new StmMenuItemBaseCollection(new BusinessObjectFactory(), new DocumentZQuery(BusinessContext.ApplicantRatingTest));
					documents.Load();

					foreach (var document in documents.ToArray<StmMenuItem>())
					{
						documentNames.AddPair(document.SU_MenuName);
					}
				}
				return documentNames;
			}
		}
		CodeDescriptionPairList documentNames;
		StmMenuItemCollection documents;

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			Category = new ZString(reader.ReadElementString(Schema.Category));

			TestNote = reader.IsStartElement(Schema.TestNote)
				? new ZString(reader.ReadElementString(Schema.TestNote))
				: ZString.Empty;

			IsVisibleOnWeb = reader.IsStartElement(Schema.IsVisibleOnWeb)
				? new ZBool(reader.ReadElementString(Schema.IsVisibleOnWeb))
				: ZBool.True;

			CategoryTitle = reader.IsStartElement(Schema.CategoryTitle)
				? new ZString(reader.ReadElementString(Schema.CategoryTitle))
				: ZString.Empty;

			NotificationType = reader.IsStartElement(Schema.NotificationType)
				? new ZString(reader.ReadElementString(Schema.NotificationType))
				: ZString.Empty;

			DocumentName = reader.IsStartElement(Schema.DocumentName)
				? new ZString(reader.ReadElementString(Schema.DocumentName))
				: ZString.Empty;
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.Category, Category.ToString());
			writer.WriteElementString(Schema.TestNote, TestNote.ToString());
			writer.WriteElementString(Schema.IsVisibleOnWeb, IsVisibleOnWeb.ToString());
			writer.WriteElementString(Schema.CategoryTitle, CategoryTitle.ToString());
			writer.WriteElementString(Schema.NotificationType, NotificationType.ToString());
			writer.WriteElementString(Schema.DocumentName, DocumentName.ToString());
		}

		#endregion

		#region ICodeDescription

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		#endregion
	}
}
