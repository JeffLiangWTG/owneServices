using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class CommonConsolNotes : Notes
	{
		public CommonConsolNotes(CommonConsol parent) : base(parent) { }

		protected override StmNoteContexts GetNoteContextsForRelatedBizObject(BusinessObject relatedBizObject)
		{
			CommonConsol consol = (CommonConsol)Parent;
			if (relatedBizObject == null)
			{
				throw new ArgumentNullException(nameof(relatedBizObject));
			}

			StmNoteContexts parentContexts = base.GetNoteContextsForRelatedBizObject(relatedBizObject);
			StmNoteContexts contexts =
				new StmNoteContexts
				{
					Module = parentContexts.Module,
					Direction = parentContexts.Direction,
					FreightMode = parentContexts.FreightMode
				};
			if (relatedBizObject == consol.SendingForwarder)
			{
				if (relatedBizObject != consol.ReceivingForwarder)
				{
					contexts.Direction &= ~StmNoteContextDirection.I;
					if (consol.IsImport())
					{
						contexts.Direction |= StmNoteContextDirection.E;
					}
				}
			}
			else if (relatedBizObject == consol.ReceivingForwarder)
			{
				contexts.Direction &= ~StmNoteContextDirection.E;
				if (consol.IsExport())
				{
					contexts.Direction |= StmNoteContextDirection.I;
				}
			}

			return contexts;
		}
	}
}
