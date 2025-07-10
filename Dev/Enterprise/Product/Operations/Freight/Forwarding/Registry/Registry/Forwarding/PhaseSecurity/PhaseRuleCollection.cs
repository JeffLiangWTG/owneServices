using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	[XmlRoot("Rules")]
	public class PhaseRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PhaseRuleCollection()
		{
		}

		public PhaseRuleCollection(Phase parent)
		{
			Parent = parent;
		}

		#region Parent

		public Phase Parent
		{
			get { return parent; }
			set
			{
				if (parent != value)
				{
					parent = value;
					foreach (PhaseRule rule in this)
					{
						rule.Parent = parent;
					}
				}
			}
		}

		Phase parent;

		#endregion

		#region Implementation

		public new PhaseRule AddNew()
		{
			return (PhaseRule)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PhaseRuleCollection(Parent);
		}

		public new PhaseRule this[int i]
		{
			get { return (PhaseRule)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PhaseRule(Parent);
		}

		#endregion
	}
}
