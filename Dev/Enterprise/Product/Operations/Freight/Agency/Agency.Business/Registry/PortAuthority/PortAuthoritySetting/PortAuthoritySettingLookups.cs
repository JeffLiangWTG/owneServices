using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthoritySettingLookups : ZLookups
	{
		public PortAuthoritySettingLookups(PortAuthoritySetting parent, BusinessObjectFactory currentFactory)
			: base(parent)
		{
			if (currentFactory == null)
			{
				throw new ArgumentNullException(nameof(currentFactory));
			}

			this.currentFactory = currentFactory;
		}

		public CodeDescriptionPairList Port_List
		{
			get
			{
				var elements = new CodeDescriptionPairList();
				var retriever = new RegistryItemProposedValueAccessor(AgencyRegistry.Instance.PortAuthorityPorts, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
				var ports = (PortAuthorityPortCollection)retriever.GetCurrentValue().Value;

				foreach (PortAuthorityPort port in ports)
				{
					elements.AddPair(port.Port);
				}

				return elements;
			}
		}

		public ShipsAgencyPrincipalCollection Principals
		{
			get
			{
				return new ShipsAgencyPrincipalCollection(currentFactory);
			}
		}

		public CodeDescriptionPairList Status_List
		{
			get
			{
				if (Parent.SupportsTesting)
				{
					return currentFactory.GetCachedValue<PortAuthoritySettingStatus>();
				}
				else
				{
					return currentFactory.GetCachedValue<PortAuthoritySettingStatus.NoTest>();
				}
			}
		}

		#region Implementation

		new PortAuthoritySetting Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PortAuthoritySetting)base.Parent; }
		}

		readonly BusinessObjectFactory currentFactory;

		#endregion
	}
}
