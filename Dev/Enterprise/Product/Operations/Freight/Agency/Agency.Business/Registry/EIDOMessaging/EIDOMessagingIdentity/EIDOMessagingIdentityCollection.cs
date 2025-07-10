using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class EIDOMessagingIdentityCollection : NonPersistentBusinessObjectCollection<EIDOMessagingIdentity>
	{
		public EIDOMessagingIdentityCollection(EIDOMessagingHeader parent)
		{
			if (parent == null)
			{ throw new ArgumentNullException(nameof(parent)); }
			this.parent = parent;
		}

		public EIDOMessagingHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parent; }
		}
		readonly EIDOMessagingHeader parent;

		public EIDOMessagingIdentityCollection Clone(EIDOMessagingHeader newParent)
		{
			EIDOMessagingIdentityCollection result = new EIDOMessagingIdentityCollection(newParent);

			foreach (EIDOMessagingIdentity identity in this)
			{
				result.Add(identity.Clone(newParent));
			}

			return result;
		}

		protected override bool AllowNewCore
		{
			get { return !Parent.ReadOnly; }
		}

		protected override bool AllowRemoveCore
		{
			get { return !Parent.ReadOnly; }
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EIDOMessagingIdentity(parent);
		}

		#endregion
	}
}


