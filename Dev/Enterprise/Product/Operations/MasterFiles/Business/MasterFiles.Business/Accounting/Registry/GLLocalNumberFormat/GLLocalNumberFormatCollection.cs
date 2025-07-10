using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class GLLocalNumberFormatCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new GLLocalNumberFormat this[int i]
		{
			get { return (GLLocalNumberFormat)Elements[i]; }
		}

		public GLLocalNumberFormat GetGLLocalNumberFormat(ZString language, ZString countryCode)
		{
			return this.Cast<GLLocalNumberFormat>().FirstOrDefault(element => element.Language == language && element.CountryCode == countryCode);
		}

		public new GLLocalNumberFormat AddNew()
		{
			return (GLLocalNumberFormat)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GLLocalNumberFormatCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GLLocalNumberFormat();
		}
	}
}
