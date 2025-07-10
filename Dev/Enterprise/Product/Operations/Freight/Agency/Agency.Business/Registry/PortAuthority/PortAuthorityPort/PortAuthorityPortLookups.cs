using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthorityPortLookups : ZLookups
	{
		public PortAuthorityPortLookups(PortAuthorityPort parent, BusinessObjectFactory currentFactory)
			: base(parent)
		{
			if (currentFactory == null)
			{
				throw new ArgumentNullException(nameof(currentFactory));
			}

			this.currentFactory = currentFactory;
		}

		public PortAuthorityVersionList Version_List
		{
			get { return version_List ?? (version_List = currentFactory.GetCachedValue<PortAuthorityVersionList>()); }
		}
		PortAuthorityVersionList version_List;

		public RefUNLOCOCollection Port_List
		{
			get { return new RefUNLOCOCollection(currentFactory); }
		}

		readonly BusinessObjectFactory currentFactory;
	}
}
