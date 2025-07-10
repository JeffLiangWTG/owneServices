using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	[XmlRoot("Phases")]
	public class PhaseCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PhaseCollection()
		{
		}

		public PhaseCollection(PhaseSecurity parent)
		{
			Parent = parent;
		}

		#region Parent

		public PhaseSecurity Parent
		{
			get { return parent; }
			set
			{
				if (parent != value)
				{
					parent = value;
					foreach (Phase phase in this)
					{
						phase.Parent = parent;
					}
				}
			}
		}

		PhaseSecurity parent;

		#endregion

		#region Implementation

		public new Phase AddNew()
		{
			return (Phase)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PhaseCollection(Parent);
		}

		public new Phase this[int i]
		{
			get { return (Phase)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Phase(Parent);
		}

		#endregion
	}
}
