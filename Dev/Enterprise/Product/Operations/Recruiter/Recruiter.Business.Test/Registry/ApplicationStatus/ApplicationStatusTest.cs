using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ApplicationStatus))]
	sealed class ApplicationStatusTest : RegistryBusinessObjectTemplateTestCase<ApplicationStatus>
	{
		public void TestCloneProperties()
		{
			ApplicationStatus status = new ApplicationStatus();
			status.Code = "MEH";
			status.Description = "Description";
			status.EmailTemplate.EmailSubject = "My Subject";
			status.EmailTemplate.EmailBody = "My Body";

			ApplicationStatus clonedStatus = (ApplicationStatus)status.Clone(status.CurrentFallbackLevel, status.Factory);
			AssertEquals("MEH", clonedStatus.Code);
			AssertEquals("Description", clonedStatus.Description);
			AssertEquals("My Subject", clonedStatus.EmailTemplate.EmailSubject);
			AssertEquals("My Body", clonedStatus.EmailTemplate.EmailBody);
		}

		public void TestDescriptionShouldNotHaveValidationErrors()
		{
			var status = new ApplicationStatus();
			status.Description = "进行中";
			AssertNoErrors(status.DescriptionInfo);
		}

		public void TestEmailTemplate()
		{
			ApplicationStatus status = new ApplicationStatus();
			AssertEquals(typeof(DocHRJobApplication), status.EmailTemplate.DocSourceType);
		}

		public void TestXmlReadWrite()
		{
			ApplicationStatus status = new ApplicationStatus();
			status.Code = "MEH";
			status.Description = "Description";
			status.EmailTemplate.EmailSubject = "My Subject";
			status.EmailTemplate.EmailBody = "My Body";

			byte[] xmlAsBytes;
			using (MemoryStream ms = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.Default))
			{
				writer.WriteStartElement("root");
				((IXmlSerializable)status).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				xmlAsBytes = ms.ToArray();
			}

			using (MemoryStream ms = new MemoryStream(xmlAsBytes))
			using (XmlTextReader reader = new XmlTextReader(ms))
			{
				reader.Read();
				ApplicationStatus newStatus = new ApplicationStatus();
				((IXmlSerializable)newStatus).ReadXml(reader);
				AssertEquals("MEH", newStatus.Code);
				AssertEquals("Description", newStatus.Description);
				AssertEquals("My Subject", newStatus.EmailTemplate.EmailSubject);
				AssertEquals("My Body", newStatus.EmailTemplate.EmailBody);
			}
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ApplicationStatus GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override ApplicationStatus GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		ApplicationStatus NewPopulatedBusinessObject()
		{
			ApplicationStatus result = new ApplicationStatus(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			result.Code = "REJ";
			result.Description = "Rejected";
			result.EmailTemplate.EmailSubject = "You suck";
			result.EmailTemplate.EmailBody = "You are not suitable for this job";
			return result;
		}

		#endregion
	}
}
