using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class ApplicationStatus : AutoApplicationStatus
	{
		public ApplicationStatus()
		{
		}

		public ApplicationStatus(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public NotificationEmailTemplate EmailTemplate
		{
			get { return emailTemplate ?? (emailTemplate = GetNewEmailTemplate()); }
			set { emailTemplate = value; }
		}

		NotificationEmailTemplate emailTemplate;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ApplicationStatus();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone1)
		{
			base.CopyValuesToClone(clone1);

			ApplicationStatus clone = (ApplicationStatus)clone1;
			clone.Code = Code;
			clone.Description = Description;
			clone.EmailTemplate = (NotificationEmailTemplate)EmailTemplate.Clone(CurrentFallbackLevel, Factory);
		}

		protected override void WriteEmailTemplate(XmlWriter writer)
		{
			((IXmlSerializable)EmailTemplate).WriteXml(writer);
		}

		protected override void ReadEmailTemplate(XmlReader reader)
		{
			NotificationEmailTemplate template = GetNewEmailTemplate();
			((IXmlSerializable)template).ReadXml(reader);
			EmailTemplate = template;
		}

		NotificationEmailTemplate GetNewEmailTemplate()
		{
			return new NotificationEmailTemplate(typeof(DocHRJobApplication));
		}
	}
}
