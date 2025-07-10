using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public interface IPalletTransactionConfirmationProvider
	{
		IEnumerable<DtbConsignmentConfirmation> Confirmations { get; }
		ZGuid PK { get; }
		BusinessObjectFactory Factory { get; }
	}

	public class PkgPalletTransactionConfirmationDependentCollection : ActiveBusinessObjectCollection<PkgPalletTransaction>
	{
		public PkgPalletTransactionConfirmationDependentCollection(IPalletTransactionConfirmationProvider parent)
			: base(parent.Factory, GetQuery(parent))
		{
			this.parent = parent;
		}

		readonly IPalletTransactionConfirmationProvider parent;

		static ZQuery GetQuery(IPalletTransactionConfirmationProvider parent)
		{
			var query = new ZQuery(PkgPalletTransactionSchema.KTR_ParentID, parent.Confirmations.Select(x => x.PK));
			query.AddToFilter(JoinCondition.Or, PkgPalletTransactionSchema.KTR_ParentID, parent.PK);
			return query;
		}

		protected override void OnAdded(PkgPalletTransaction businessObject)
		{
			base.OnAdded(businessObject);
			businessObject.RelatedJob = parent as IPalletTransactionParent;
			if (businessObject.RelatedJob == null)
			{
				businessObject.RelatedJob = parent.Confirmations.FirstOrDefault();
			}
		}

		protected override void OnLoadedIntoCollectionCore(PkgPalletTransaction palletTransaction)
		{
			base.OnLoadedIntoCollectionCore(palletTransaction);

			var parents = new List<IPalletTransactionParent>();
			var parentAsPalletParent = parent as IPalletTransactionParent;
			if (parentAsPalletParent != null)
			{
				parents.Add(parentAsPalletParent);
			}

			parents.AddRange(parent.Confirmations);

			palletTransaction.Context = parent as BusinessObject;
			palletTransaction.PossibleParents = parents.ToArray();
		}
	}
}
