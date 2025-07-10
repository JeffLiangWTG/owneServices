using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PickByBOMComponentLineInfo
	{
		public PickByBOMComponentLineInfo(WhsPickLine[] componentPickLines, IReadOnlyCollection<PickByBOMComponentKitRelation> relationPKs)
		{
			ComponentPickLines = Argument.NotNull(componentPickLines, nameof(componentPickLines));
			RelationPKs = Argument.NotNull(relationPKs, nameof(relationPKs));
		}

		public WhsPickLine[] ComponentPickLines;
		public IReadOnlyCollection<PickByBOMComponentKitRelation> RelationPKs;

		public void Deconstruct(out WhsPickLine[] componentPickLines, out IReadOnlyCollection<PickByBOMComponentKitRelation> relationPKs)
		{
			componentPickLines = ComponentPickLines;
			relationPKs = RelationPKs;
		}
	}
}
