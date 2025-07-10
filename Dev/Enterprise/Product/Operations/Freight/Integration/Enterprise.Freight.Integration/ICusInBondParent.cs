using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface ICusInBondParent : IBusiness
	{
		ZGuid PK { get; }
		ZGuid GetDeclarationPK(ZGuid companyPK);
		string TablePrefix { get; }
		ZString ParentType { get; }
		void PopulateJobNumberIfNeeded();
		ZString JobNumber { get; }
		ZString HouseBill { get; }
		event EventHandler VisibilityChanged;
		bool IsVisible { get; }
		bool IsInternalBrokerage { get; }
	}
}
