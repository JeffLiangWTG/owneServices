using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(RecruiterTestType))]
	sealed class RecruiterTestTypeTest : RegistryBusinessObjectTest
	{
		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.Description_MaxLength);
		}

		public void TestCategoryTitle()
		{
			AssertEquals(ZString.Empty, BizObj.CategoryTitle);

			BizObj.CategoryTitle = "meh";
			AssertEquals("meh", BizObj.CategoryTitle);
		}

		public void TestCategory()
		{
			AssertNoErrors("Precondition: Category should not have errors.", BizObj.CategoryInfo);
			Assert("Precondition: LanguageList[0].Code should not be empty.", !string.IsNullOrEmpty(BizObj.CategoryList[0].Code));

			BizObj.Category = "*&$";
			AssertHasError(BizObj.CategoryInfo, "Enter a valid selection.");

			BizObj.Category = "NON";
			AssertHasError(BizObj.CategoryInfo, "Enter a valid selection.");

			BizObj.Category = BizObj.CategoryList[0].Code;
			AssertNoErrors(BizObj.CategoryInfo);

			BizObj.Category = "";
			AssertNoErrors("Empty category is allowed.", BizObj.CategoryInfo);
		}

		public void TestNote()
		{
			AssertEquals(ZString.Empty, BizObj.TestNote);

			BizObj.TestNote = "meh";
			AssertEquals("meh", BizObj.TestNote);
		}

		public void TestIsVisibleOnWeb()
		{
			AssertEquals(true, BizObj.IsVisibleOnWeb);

			BizObj.IsVisibleOnWeb = false;
			AssertEquals(false, BizObj.IsVisibleOnWeb);
		}

		public void TestGetTestTypeCategoryList()
		{
			AssertEquals("Count", 3, BizObj.CategoryList.Count);
			AssertEquals(true, BizObj.CategoryList.Contains(new CodeDescriptionPair(RecruiterTestCategory.Codes.Accreditation, RecruiterTestCategory.Descriptions.Accreditation)));
			AssertEquals(true, BizObj.CategoryList.Contains(new CodeDescriptionPair(RecruiterTestCategory.Codes.Assessment, RecruiterTestCategory.Descriptions.Assessment)));
			AssertEquals(true, BizObj.CategoryList.Contains(new CodeDescriptionPair(RecruiterTestCategory.Codes.SkillTest, RecruiterTestCategory.Descriptions.SkillTest)));
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Category = "NON";
			BizObj.NotificationType = "NON";
			BizObj.DocumentName = "Rhubarb";

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.CategoryInfo);
			AssertHasErrors(BizObj.NotificationTypeInfo);
			AssertHasErrors(BizObj.DocumentNameInfo);
		}

		public void TestNotificationType()
		{
			AssertNoErrors("Precondition: Notification Type should not have errors.", BizObj.NotificationTypeInfo);
			AssertNotEquals("Precondition: NotificationTypeList[0].Code should not be empty.", string.Empty, BizObj.NotificationTypeList[0].Code);

			BizObj.NotificationType = "*&$";
			AssertHasError(BizObj.NotificationTypeInfo, "Enter a valid selection.");

			BizObj.NotificationType = "NON";
			AssertHasError(BizObj.NotificationTypeInfo, "Enter a valid selection.");

			BizObj.NotificationType = BizObj.NotificationTypeList[0].Code;
			AssertNoErrors(BizObj.NotificationTypeInfo);

			BizObj.NotificationType = "";
			AssertHasError("Empty notification type is not allowed.", BizObj.NotificationTypeInfo, "Please enter a value.");
		}

		public void TestGetTestNotificationTypeList()
		{
			AssertEquals("Count", 3, BizObj.NotificationTypeList.Count);
			AssertEquals(true, BizObj.NotificationTypeList.Contains(new CodeDescriptionPair(RecruiterTestNotificationType.Codes.DeliverDocument, RecruiterTestNotificationType.Descriptions.DeliverDocument)));
			AssertEquals(true, BizObj.NotificationTypeList.Contains(new CodeDescriptionPair(RecruiterTestNotificationType.Codes.DoNotDeliver, RecruiterTestNotificationType.Descriptions.DoNotDeliver)));
			AssertEquals(true, BizObj.NotificationTypeList.Contains(new CodeDescriptionPair(RecruiterTestNotificationType.Codes.DeliverTestCompletionEmail, RecruiterTestNotificationType.Descriptions.DeliverTestCompletionEmail)));
		}

		public void TestDocumentName()
		{
			AssertNoErrors("Precondition: Document Name should not have errors.", BizObj.DocumentNameInfo);
			AssertNotEquals("Precondition: DocumentNames[0].Code should at least have Test Result as default.", string.Empty, BizObj.DocumentNames[0].Code);

			BizObj.NotificationType = RecruiterTestNotificationType.Codes.DoNotDeliver;
			AssertEquals("Document Name should not be editable", true, BizObj.IsDocumentNameReadOnly);
			AssertNoErrors(BizObj.DocumentNameInfo);

			BizObj.NotificationType = RecruiterTestNotificationType.Codes.DeliverTestCompletionEmail;
			AssertEquals("Document Name should not be editable", true, BizObj.IsDocumentNameReadOnly);
			AssertNoErrors(BizObj.DocumentNameInfo);

			BizObj.NotificationType = RecruiterTestNotificationType.Codes.DeliverDocument;
			AssertEquals("Document Name should be editable", false, BizObj.IsDocumentNameReadOnly);

			BizObj.DocumentName = "Random";
			AssertHasError(BizObj.DocumentNameInfo, "Enter a valid selection.");

			BizObj.DocumentName = BizObj.DocumentNames[0].Code;
			AssertNoErrors(BizObj.DocumentNameInfo);

			BizObj.DocumentName = string.Empty;
			AssertHasError(BizObj.DocumentNameInfo, "Please enter a value.");
		}

		public void TestXmlDeserialiseFromExistingRecord()
		{
			StringBuilder builder = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			using (XmlWriter writer = XmlWriter.Create(builder, settings))
			{
				writer.WriteStartElement("root");
				writer.WriteRaw("<CodeMaxLength>3</CodeMaxLength><Code>STD</Code><Description>Standard</Description><Category /><CategoryTitle>title</CategoryTitle><NotificationType>DOC</NotificationType><DocumentName>Test Result</DocumentName>");
				writer.WriteEndElement();
			}

			RecruiterTestType newRecord = new RecruiterTestType();
			using (StringReader sReader = new StringReader(builder.ToString()))
			using (XmlReader reader = XmlReader.Create(sReader))
			{
				reader.Read();
				((IXmlSerializable)newRecord).ReadXml(reader);
			}
			AssertEquals("STD", newRecord.Code);
			AssertEquals("Standard", newRecord.Description);
			AssertEquals("", newRecord.Category);
			AssertEquals("", newRecord.TestNote);
			AssertEquals("title", newRecord.CategoryTitle);
			AssertEquals(true, newRecord.IsVisibleOnWeb);
			AssertEquals("DOC", newRecord.NotificationType);
			AssertEquals("Test Result", newRecord.DocumentName);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			RecruiterTestType clone = clone1 as RecruiterTestType;
			AssertEquals("Code MaxLength", 3, clone.CodeMaxLength);
			AssertEquals("Code", "TST", clone.Code);
			AssertEquals("Description", "For test.", clone.Description);
			AssertEquals("Test Type Category", RecruiterTestCategory.Codes.Assessment, clone.Category);
			AssertEquals("Test Note", "This is an additional test note", clone.TestNote);
			AssertEquals("Is Visible On Web", false, clone.IsVisibleOnWeb);
			AssertEquals("Notification Type", RecruiterTestNotificationType.Codes.DeliverDocument, clone.NotificationType);
			AssertEquals("Document Name", "Test Result", clone.DocumentName);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new RecruiterTestType();
			result.Code = "TST";
			result.Description = (NoResString)"For test.";
			result.Category = RecruiterTestCategory.Codes.Assessment;
			result.TestNote = "This is an additional test note";
			result.IsVisibleOnWeb = false;
			result.NotificationType = RecruiterTestNotificationType.Codes.DeliverDocument;
			result.DocumentName = "Test Result";
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new RecruiterTestType BizObj
		{
			get { return (RecruiterTestType)base.BizObj; }
		}
		#endregion
	}
}
