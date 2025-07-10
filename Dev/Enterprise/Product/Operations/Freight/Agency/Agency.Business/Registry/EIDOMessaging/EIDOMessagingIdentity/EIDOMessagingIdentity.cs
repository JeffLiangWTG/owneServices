using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public sealed class EIDOMessagingIdentity : AutoEIDOMessagingIdentity
	{
		public EIDOMessagingIdentity(EIDOMessagingHeader parent)
			: base(parent == null ? null : parent.CurrentFactory)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			this.parent = parent;
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PrincipalPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PrincipalPK = value; }
		}

		public EIDOMessagingHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parent; }
		}
		readonly EIDOMessagingHeader parent;

		public EIDOMessagingIdentity Clone(EIDOMessagingHeader newParent)
		{
			EIDOMessagingIdentity result = new EIDOMessagingIdentity(newParent);
			result.PrincipalPK = PrincipalPK;
			result.Password = Password;
			result.SenderID = SenderID;
			result.RecipientID = RecipientID;
			return result;
		}

		public EIDOMessagingIdentityLookups Lookups
		{
			get { return lookups ?? (lookups = new EIDOMessagingIdentityLookups(this)); }
		}
		EIDOMessagingIdentityLookups lookups;
	}
}


