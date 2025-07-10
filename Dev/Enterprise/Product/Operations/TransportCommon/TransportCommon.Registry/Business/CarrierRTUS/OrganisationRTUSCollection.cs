using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class OrganisationRTUSCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Overrides

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrganisationRTUSCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrganisationRTUSOption();
		}

		protected override bool AllowNewCore => true;

		#endregion

		#region Index

		public new OrganisationRTUSOption this[int i]
		{
			get { return (OrganisationRTUSOption)Elements[i]; }
		}

		public new OrganisationRTUSOption AddNew()
		{
			return (OrganisationRTUSOption)base.AddNew();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var option = (OrganisationRTUSOption)child;

			using (option.GetValidationSuspender())
			{
				option.CBACode = CBAList.Codes.TMS3G;
			}
		}

		#endregion
	}
}
