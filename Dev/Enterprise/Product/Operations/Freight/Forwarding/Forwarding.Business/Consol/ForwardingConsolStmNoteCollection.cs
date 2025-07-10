using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolStmNoteCollection : StmNoteCollection
	{
		public ForwardingConsolStmNoteCollection(ForwardingConsol consol)
			: base(consol, consol.Factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingConsolStmNote);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			if (!FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration)
			{
				var additionalFilter = new ZQuery(StmNoteSchema.ST_Description, SQLComparisonOperator.NotEqual, PredefinedNoteTypes.Instance.OriginalBillNotes.Description);
				return base.CreateAdditionalFilter().AddToFilter(additionalFilter);
			}

			return base.CreateAdditionalFilter();
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			if (e.ItemAdded)
			{
				var addedItem = e.BizObject;
				if (addedItem is not ForwardingConsolStmNote && addedItem is StmNote)
				{
					var stmNote = (StmNote)addedItem;
					ErrorReporter.ReportOnce("StmNoteAddedIntoForwardingConsolStmNoteCollection", string.Format(CultureInfo.InvariantCulture,
						"StmNote is trying to add into ForwardingConsolStmNoteCollection. ST_Description = {0}, ST_NoteType = {1}, ST_NoteContext = {2}", stmNote.ST_Description, stmNote.ST_NoteType, stmNote.ST_NoteContext));
				}
			}
		}
	}
}
