using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARTermsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ARTermsCollection()
		{
		}

		public ARTermsCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ARTerms this[int x]
		{
			get { return (ARTerms)Elements[x]; }
		}

		public new ARTerms AddNew()
		{
			return (ARTerms)base.AddNew();
		}

		protected override BusinessObject AddNewCore()
		{
			var result = (ARTerms)base.AddNewCore();
			if (this.Count > 0)
			{
				result.TreatDisbursementsAsStandardValue = ((ARTerms)this.First()).TreatDisbursementsAsStandardValue;
			}
			return result;
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				if (Count > 1)
				{
					return base.AllowRemoveCore;
				}
				else
				{
					return false;
				}
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARTermsCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARTerms(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
