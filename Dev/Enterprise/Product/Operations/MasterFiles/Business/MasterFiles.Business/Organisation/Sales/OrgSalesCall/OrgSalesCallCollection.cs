using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Communication)]
	public class OrgSalesCallCollection : ActiveBusinessObjectCollection<OrgSalesCall>
	{
		public OrgSalesCallCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSalesCallCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public OrgSalesCallCollection(OrgHeader master)
			: base(master)
		{
			this.master = master;
			master.SalesCollection.OnTradeLanesChanged += new EventHandler(SalesCollection_OnTradeLanesChanged);
		}

		public OrgSalesCallCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		readonly OrgHeader master;

		protected override void SetDefaultsForNewElementCore(OrgSalesCall newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.OQ_SalesCallNotes = GetPreviousCallFollowUpNotes(newElement);

			if (master != null)
			{
				newElement.OQ_OH = master.PK;
			}
		}

		ZBlob GetPreviousCallFollowUpNotes(OrgSalesCall newCall)
		{
			ZBlob result = ZBlob.Empty;

			if (master != null && Count > 0)
			{
				ZQuery query = new ZQuery(OrgSalesCallSchema.OQ_OH, master.PK);
				query.AddToFilter(OrgSalesCallSchema.PK, SQLComparisonOperator.NotEqual, newCall.PK);
				query.OrderBy = OrgSalesCallSchema.OQ_CallDate.Name + " DESC";
				OrgSalesCall lastCall = Factory.LoadTop1<OrgSalesCall>(query);
				if (lastCall != null)
				{
					result = lastCall.OQ_FollowupNotes;
				}
			}

			return result;
		}

		void SalesCollection_OnTradeLanesChanged(object sender, EventArgs e)
		{
			foreach (OrgSalesCall salesCall in this)
			{
				salesCall.UpdateTradeProfileDescriptionList();
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		public bool shouldCheckIncludeOnSalesCallDocument;
		public bool ShouldCheckIncludeOnSalesCallDocument
		{
			get { return shouldCheckIncludeOnSalesCallDocument; }
			set { shouldCheckIncludeOnSalesCallDocument = value; }
		}
	}
}
