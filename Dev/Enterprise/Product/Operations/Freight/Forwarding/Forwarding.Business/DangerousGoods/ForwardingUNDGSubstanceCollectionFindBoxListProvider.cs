using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	class ForwardingUNDGSubstanceCollectionFindBoxListProvider : FindBoxListProvider
	{
		public ForwardingUNDGSubstanceCollectionFindBoxListProvider(ForwardingUNDGSubstanceCollection collection, ForwardingShipment parentShipment)
			: base(collection)
		{
			this.parentShipment = parentShipment;
		}

		readonly ForwardingShipment parentShipment;

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var standard = DGStandardCalculator.GetCorrespondingStandardForShipmentMode(parentShipment);
			var query = new ZQuery();
			if (!standard.IsEmpty)
			{
				query.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Standard, standard));
			}

			AddCodeStartsWithFilter(query, code);
			AddIsActiveFilter(query, code);

			var bizObj = List.Factory.LoadTop1(GetTypeOfElements(code), query);
			return bizObj != null
				? (bizObj[GetCodePropertyName(code)].ToString(), true)
				: (code, false);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			var standard = DGStandardCalculator.GetCorrespondingStandardForShipmentMode(parentShipment);
			var query = new ZQuery();
			if (!standard.IsEmpty)
			{
				query.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Standard, standard));
			}

			AddCodeEqualsFilter(query, code);
			query.AddToFilter(List.CompleteFilter);

			return List.Factory.Load(GetTypeOfElements(code), query);
		}
	}
}
