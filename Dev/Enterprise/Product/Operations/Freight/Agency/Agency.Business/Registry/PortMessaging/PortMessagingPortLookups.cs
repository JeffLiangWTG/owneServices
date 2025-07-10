using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortMessagingPortLookups : ZLookups
	{
		public PortMessagingPortLookups(PortMessagingPort parent, BusinessObjectFactory currentFactory) : base(parent)
		{
			if (currentFactory == null)
			{
				throw new ArgumentNullException(nameof(currentFactory));
			}

			this.currentFactory = currentFactory;
		}

		new PortMessagingPort Parent => (PortMessagingPort)base.Parent;

		public RefUNLOCOCollection Port_List
		{
			get
			{
				if (port_List == null)
				{
					var companyPK = Parent?.CurrentFallbackLevel?.CompanyPK(false);
					ZQuery filter = new ZQuery();
					if (companyPK != null)
					{
						var company = currentFactory.Load<GlbCompany>((ZGuid)companyPK);
						if (company != null)
						{
							filter.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.Equal, company.Country.Code);
						}
					}
					filter.AddToFilter(RefUNLOCOSchema.RL_Code, PortMessagingPortCountryList.EnabledPorts);

					port_List = new RefUNLOCOCollection(currentFactory, filter);
				}

				return port_List;
			}
		}
		RefUNLOCOCollection port_List;

		public ShipsAgencyPrincipalCollection Principals => principals ?? (principals = new ShipsAgencyPrincipalCollection(currentFactory));

		ShipsAgencyPrincipalCollection principals;

		readonly BusinessObjectFactory currentFactory;
	}
}
