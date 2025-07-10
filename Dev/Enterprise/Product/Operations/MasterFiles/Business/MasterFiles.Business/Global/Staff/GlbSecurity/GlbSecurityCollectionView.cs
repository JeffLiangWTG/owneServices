using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurityCollectionView : BusinessObjectCollectionView<GlbSecurity>, IReadOnlyFactoryForSecurityValidationProvider, IGlbSecurityCollectionWithSecurity
	{
		CheckpointLookupKey key;
		BusinessObjectFactory readOnlyFactory;
		ZGuid staffPK;

		public GlbSecurityCollectionView(GlbSecurityCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public new GlbSecurityCollection CollectionToFilter
		{
			get { return (GlbSecurityCollection)base.CollectionToFilter; }
		}

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

		public void FilterBySecurityKey(string key, ZGuid staffPK)
		{
			FilterBySecurityKey(new CheckpointLookupKey(key), staffPK);
		}

		public void FilterBySecurityKey(CheckpointLookupKey key, ZGuid staffPK)
		{
			this.key = key;
			this.staffPK = staffPK;
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = !key.IsEmpty;
			if (result)
			{
				GlbSecurity security = (GlbSecurity)element;
				result =
					security.GU_GS == staffPK &&
					key.CodeEquals(security.GU_SecurityRight) &&
					key.ItemGuid == security.GU_ItemGUID;
			}
			return result;
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			base.RemoveCollectionRelationshipsCore(child, forDelete);
			((GlbSecurity)child).GU_SecurityRight = "";
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			GlbSecurity security = (GlbSecurity)child;
			security.GU_SecurityRight = key.Code;
			security.GU_ItemGUID = key.ItemGuid;
		}

		#region IGlbSecurityCollectionWithSecurity Members

		SecurityCore IGlbSecurityCollectionWithSecurity.Security
		{
			get { return CollectionToFilter.Security; }
		}

		ISecurityMap IGlbSecurityCollectionWithSecurity.SecurityMap
		{
			get { return ((IGlbSecurityCollectionWithSecurity)CollectionToFilter).SecurityMap; }
		}

		#endregion
	}
}
