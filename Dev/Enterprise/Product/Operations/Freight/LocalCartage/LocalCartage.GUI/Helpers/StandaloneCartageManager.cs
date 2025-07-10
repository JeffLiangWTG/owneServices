using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	class StandaloneCartageManager : CartageManager
	{
		public StandaloneCartageManager(CommonCartage cartage)
		{
			this.cartage = cartage;
		}
		readonly CommonCartage cartage;

		protected override CommonCartage GetCartageForExport(NotificationBuffer buffer)
		{
			return cartage;
		}

		protected override void CartageAdvised(BusinessObjectFactory factoryToCartageAdviseIn)
		{
		}

		protected override OrgHeader SendTo
		{
			get { return cartage.LocalClient; }
		}

		protected override ZString SendToDescription
		{
			get { return Res.GetString("16feb5ce-e1f6-4166-90fe-9d6521792f28", "Local Client"); }
		}

		protected override ZString Description
		{
			get { return ""; }
		}

		protected override BusinessObjectFactory ParentFactory
		{
			get { return cartage.Factory; }
		}

		protected override ZString ParentJobNumber
		{
			get { return cartage.JJ_ConsignmentID; }
		}

		protected override Logs ParentLogs
		{
			get { return cartage.Logs; }
		}

		protected override Notes ParentNotes
		{
			get { return cartage.Notes; }
		}

		protected override bool IsParentSaved
		{
			get { return cartage.IsInDatabase && !cartage.HasChanges; }
		}

		protected override CommonCartageType CartageJobType
		{
			get { return cartage.CartageType; }
		}
	}
}
