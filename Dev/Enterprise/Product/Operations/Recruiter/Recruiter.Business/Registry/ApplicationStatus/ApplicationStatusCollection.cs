using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class ApplicationStatusCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ApplicationStatusCollection()
			: base() { }

		public ApplicationStatusCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public ApplicationStatusCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public ApplicationStatusCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new ApplicationStatus this[int index]
		{
			get { return (ApplicationStatus)Elements[index]; }
		}

		public new ApplicationStatus AddNew()
		{
			return (ApplicationStatus)base.AddNew();
		}

		public ApplicationStatus AddPair(string code, string description)
		{
			ApplicationStatus result = AddNew();
			result.Code = code;
			result.Description = description;
			return result;
		}

		public CodeDescriptionPairList AsCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (ApplicationStatus status in this)
			{
				result.AddPair(status.Code, status.Description);
			}
			return result;
		}

		public NotificationEmailTemplate GetTemplateFromCode(string code)
		{
			foreach (ApplicationStatus status in this)
			{
				if (status.Code == code)
				{
					return status.EmailTemplate;
				}
			}

			return null;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ApplicationStatusCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ApplicationStatus(CurrentFallbackLevel);
		}

		#endregion
	}
}
