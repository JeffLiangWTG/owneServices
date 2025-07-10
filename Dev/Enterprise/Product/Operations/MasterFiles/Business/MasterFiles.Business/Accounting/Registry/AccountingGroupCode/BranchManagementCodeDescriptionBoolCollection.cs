using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class BranchManagementCodeDescriptionBoolCollection : CodeDescriptionBoolCollection
	{
		public BranchManagementCodeDescriptionBoolCollection()
			: base()
		{
		}

		public BranchManagementCodeDescriptionBoolCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new BranchManagementCodeDescriptionBool AddNew()
		{
			return (BranchManagementCodeDescriptionBool)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BranchManagementCodeDescriptionBool(CurrentFallbackLevel);
		}

		public new BranchManagementCodeDescriptionBool this[int i]
		{
			get { return (BranchManagementCodeDescriptionBool)base[i]; }
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new BranchManagementCodeDescriptionBoolCollection(CurrentFallbackLevel);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CodeDescriptionBool)child).Bool = true;
		}
	}
}
