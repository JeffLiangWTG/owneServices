using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class RecipientSourceCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new RecipientSource this[int index] => (RecipientSource)Elements[index];

		public new RecipientSource AddNew() => (RecipientSource)base.AddNew();

		protected override void OnAdded(BusinessObject bizoAdded)
		{
			base.OnAdded(bizoAdded);

			if (bizoAdded is RecipientSource source) // Unit tests add other random things to this collection??
			{
				source.ParentCollection = this;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var maxSequence = this.Cast<RecipientSource>().MaxOrDefault(x => x.FallbackSequence);

			return new RecipientSource
			{
				ParentCollection = this,
				FallbackSequence = maxSequence + (maxSequence == int.MaxValue ? 0 : 1),
			};
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RecipientSourceCollection();
		}
	}
}
