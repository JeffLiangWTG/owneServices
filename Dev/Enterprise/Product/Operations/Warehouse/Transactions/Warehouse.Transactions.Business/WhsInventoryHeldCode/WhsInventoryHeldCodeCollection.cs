using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsInventoryHeldCodes)]
	public class WhsInventoryHeldCodeCollection : ActiveBusinessObjectCollection<WhsInventoryHeldCode>, IWhsInventoryHeldCodeCollection, ICodeDescriptionPairList
	{
		public WhsInventoryHeldCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsInventoryHeldCodeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public WhsInventoryHeldCodeCollection(BusinessObjectFactory factory, ZGuid clientPK)
			: base(factory, GetClientFilterQuery(clientPK))
		{
		}

		static ZQuery GetClientFilterQuery(ZGuid clientPK)
		{
			var holdCodeQueryFilter = new ZQuery(WhsInventoryHeldCodeSchema.WHC_OH_Client, null);
			if (clientPK != Guid.Empty)
			{
				holdCodeQueryFilter.AddToFilter(JoinCondition.Or, WhsInventoryHeldCodeSchema.WHC_OH_Client, clientPK);
			}

			return holdCodeQueryFilter;
		}

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return this.Any(hc => hc.WHC_Code.EqualsIgnoringCase(code.ToString()));
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			var heldCode = this.SingleOrDefault(hc => hc.WHC_Code.EqualsIgnoringCase(code));
			return heldCode?.WHC_DescriptionMultilingual ?? ZString.Empty;
		}

		#endregion
	}
}
