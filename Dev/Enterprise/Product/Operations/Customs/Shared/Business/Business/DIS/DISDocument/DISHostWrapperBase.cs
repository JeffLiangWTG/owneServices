using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.Business
{
	public abstract class DISHostWrapperBase<T> : NonPersistentBusinessObject, IObsoleteValidation, IDISHostWrapper where T : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		protected DISHostWrapperBase(IDISHost disHost)
			: base(disHost.Factory)
		{
			this.DISHost = disHost;
			isActive = true;
		}

		public readonly IDISHost DISHost;

		public bool isActive;

		public bool IsActive
		{
			get => isActive;
			set
			{
				if (!value)
				{
					DISDocuments.ClearHasChanges();
				}
				isActive = value;
			}
		}

		public abstract DISDocumentCollectionBase<T> DISDocuments { get; }

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (HasChanges && IsActive)
			{
				DISDocuments.Serialize();
			}
		}

		#region IDISHostWrapper Members

		IBusinessObjectCollection IDISHostWrapper.DISDocuments
		{
			get { return DISDocuments; }
		}

		#endregion
	}
}
