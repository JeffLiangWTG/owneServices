using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static OrgAddressHelper New(OrgHeader parent)
		{
			OrgAddressHelper result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(parent);
			}
			else
			{
				result = new OrgAddressHelper(parent);
			}
			return result;
		}

		protected OrgAddressHelper(OrgHeader parent)
		{
			this.Parent = parent;
		}

		public void UpdateOtherAddressesUNLOCOFromMainAddress(ZString unloco)
		{
			UpdateOtherAddressesUNLOCOFromMainAddressCore(unloco);
		}

		protected virtual void UpdateOtherAddressesUNLOCOFromMainAddressCore(ZString unloco)
		{
		}

		public OrgAddress AddressToBeUpdated
		{
			get { return addressToBeUpdated; }
			set { addressToBeUpdated = value; }
		}
		OrgAddress addressToBeUpdated;

		protected delegate OrgAddressHelper NewDelegate(OrgHeader parent);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		public readonly OrgHeader Parent;
	}
}
