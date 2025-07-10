using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Tracking.Business
{
	class PermittedStorageDocsCollection : BusinessObjectCollectionView<BusinessObject>
	{
		readonly TrackingSiteUser user;

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Call chain issues resolved by overriding RebuildOnConstruction")]
		public PermittedStorageDocsCollection(BusinessObjectCollection collection, TrackingSiteUser user)
			: base(collection)
		{
			this.user = Argument.NotNull(user, nameof(user));

			RebuildOnConstruction();
		}

		protected override void RebuildOnConstruction()
		{
			if (user != null)
			{
				base.RebuildOnConstruction();
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var storageDoc = element as StorageDocsBase;

			return storageDoc != null && user.CanViewDocument(Factory, storageDoc.DocType);
		}
	}
}
