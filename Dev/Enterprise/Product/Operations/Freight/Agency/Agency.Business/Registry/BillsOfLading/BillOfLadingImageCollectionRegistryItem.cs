using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingImageCollectionRegistryItem : StronglyTypedRegistryItem<BillOfLadingImageCollection>
	{
		public BillOfLadingImageCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new BillOfLadingImageRegistryDataType(), storage, options))
		{
		}

		#region BillOfLadingImageRegistryDataType

		[RegistryEditor("Enterprise.Freight.Agency.GUI.BillOfLadingImageRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
		public class BillOfLadingImageRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BillOfLadingImageCollection> { }

		#endregion

		#region FindBillOfLadingImageForPrincipal

		public BillOfLadingImage FindBillOfLadingImageForPrincipal(ZGuid principalPK)
		{
			foreach (BillOfLadingImage billOfLadingImage in Value)
			{
				if (billOfLadingImage.PrincipalPK == principalPK)
				{
					return billOfLadingImage;
				}
			}
			return null;
		}

		#endregion
	}
}
