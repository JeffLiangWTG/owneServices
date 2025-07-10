using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolStmNotes : CommonConsolNotes
	{
		public ForwardingConsolStmNotes(ForwardingConsol consol) : base(consol)
		{
			Argument.NotNull(consol, nameof(consol));
		}

		#region Implementation

		protected override Type ElementType => typeof(ForwardingConsolStmNote);

		public new ForwardingConsol Parent => (ForwardingConsol)base.Parent;

		protected override BusinessObjectCollection GetNewElementsCollection() => new ForwardingConsolStmNoteCollection(Parent);

		protected override BusinessObjectCollection GetNewAllElementsCollection() => new ForwardingConsolStmNoteCollectionWithRelatedElements(Parent);

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView() => new ForwardingConsolStmNoteCollectionView(Parent);

		#endregion
	}
}
