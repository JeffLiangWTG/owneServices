using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public sealed class ExportAWBSecurityStatusLineView : BusinessObjectCollectionView<ExportAWBSecurityStatusLine>
	{
		public ExportAWBSecurityStatusLineView(ExportAWBSecurityStatusLineCollection collectionToFilter, SecurityStatusLineType type, bool allowNew, bool allowRemove)
			: base(collectionToFilter)
		{
			this.type = type;
			this.allowNew = allowNew;
			this.allowRemove = allowRemove;
		}

		readonly SecurityStatusLineType type;
		readonly bool allowNew;
		readonly bool allowRemove;

		protected override void RebuildOnConstruction()
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var line = element as ExportAWBSecurityStatusLine;

			return line != null && line.Type == type;
		}

		protected override bool AllowNewCore
		{
			get { return allowNew; }
		}

		protected override bool AllowRemoveCore
		{
			get { return allowRemove; }
		}
	}
}
