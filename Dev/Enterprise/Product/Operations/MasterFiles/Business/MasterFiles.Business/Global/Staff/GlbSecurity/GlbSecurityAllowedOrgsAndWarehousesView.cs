using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurityAllowedOrgsAndWarehousesView : BusinessObjectCollectionView<GlbSecurity>, IReadOnlyFactoryForSecurityValidationProvider
	{
		public GlbSecurityAllowedOrgsAndWarehousesView(GlbSecurityCollection collection, IOrgsAndWarehousesAccessProvider provider)
			: base(collection)
		{
			this.provider = provider;
		}

		#region BusinessObject overrides

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			GlbSecurity security = (GlbSecurity)element;
			return provider != null &&
				security.GU_SecurityItemIsAllowed &&
				(security.GU_GS == provider.PK || security.GU_GG == provider.PK) &&
				security.GU_SecurityRight == SecurityRight;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Properties

		string gu_SecurityRight = "";
		public ZString SecurityRight
		{
			get { return string.IsNullOrEmpty(gu_SecurityRight) ? GlbSecurity.AllowedPrincipalsSecurityRightName : gu_SecurityRight; }
			set
			{
				if (gu_SecurityRight != value)
				{
					gu_SecurityRight = value;
					Rebuild();
				}
			}
		}

		public Type FindBoxCollectionType
		{
			get
			{
				switch (SecurityRight)
				{
					case GlbSecurity.AllowedPrincipalsSecurityRightName:
						return typeof(ShipsAgencyPrincipalCollection);
					case GlbSecurity.AllowedClientsSecurityRightName:
						return typeof(WarehouseClientCollection);
					case GlbSecurity.AllowedWarehousesSecurityRightName:
						return CargoWise.Application.ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>();
					default:
						return null;
				}
			}
		}

		#endregion

		#region IReadOnlyFactoryForSecurityValidationProvider Members

		public BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (readOnlyFactory == null)
				{
					readOnlyFactory = new BusinessObjectFactory();
				}

				return readOnlyFactory;
			}
		}

		BusinessObjectFactory readOnlyFactory;

		#endregion

		#region Implementation

		readonly IOrgsAndWarehousesAccessProvider provider;

		#endregion
	}
}
