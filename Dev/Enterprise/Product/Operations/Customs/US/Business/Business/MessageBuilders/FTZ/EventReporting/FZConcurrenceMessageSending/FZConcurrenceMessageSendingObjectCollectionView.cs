using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZConcurrenceMessageSendingObjectCollectionView : NonPersistentBusinessObjectCollectionView<FZConcurrenceMessageSendingObject>
	{
		public FZConcurrenceMessageSendingObjectCollectionView(FZEventAction fZEventAction)
			: base(fZEventAction.MessageSendingObjects)
		{
			Rebuild();
		}

		public ZString FilterBy
		{
			get { return fFilterBy; }
			set
			{
				bool hasChanged = FilterBy != value;
				fFilterBy = value;
				if (hasChanged)
				{
					Rebuild();
				}
			}
		}
		ZString fFilterBy;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ((FZConcurrenceMessageSendingObject)element).MB_US_ActionCode == FilterBy;
		}

		protected override bool AllowNewCore => false;

		protected override FZConcurrenceMessageSendingObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
