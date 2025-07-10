using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class USVoyagePortCollection : NonPersistentBusinessObjectCollectionView<VoyagePort>, IDisposable
	{
		public USVoyagePortCollection(VoyagePortCollection collection)
			: base(collection)
		{ }

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var port = (VoyagePort)element;
			return port.Port.StartsWith(Core.Constants.CountryCodes.UnitedStates) || port.Port.StartsWith(Core.Constants.CountryCodes.PuertoRico) || (port.Messages != null && port.Messages.Count > 0);
		}

		public void Dispose()
		{
			((VoyagePortCollection)CollectionToFilter).Dispose();
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override VoyagePort CreateNonPersistentBusinessObject()
		{
			return (VoyagePort)this.collectionToFilter.AddNew();
		}
	}
}
