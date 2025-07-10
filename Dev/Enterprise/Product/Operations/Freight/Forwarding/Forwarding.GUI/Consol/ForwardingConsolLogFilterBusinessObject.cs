using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Consol
{
	public class ForwardingConsolLogFilterBusinessObject : ZStmALogFilterBusinessObject
	{
		public ForwardingConsolLogFilterBusinessObject() : base()
		{
		}

		public ForwardingConsolLogFilterBusinessObject(IStmALogParent master) : base(master)
		{
		}

		protected override ZQuery GetBusinessObjectsWithRelatedEventsQuery(ZString showForCode)
		{
			var query = base.GetBusinessObjectsWithRelatedEventsQuery(showForCode);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.NotEqual, AutoEvents.OceanCarrierBookingByTEU.Code);

			return query;
		}

		protected override CodeDescriptionPairList EventCodesList
		{
			get
			{
				if (eventCodesWithoutOCB == null)
				{
					eventCodesWithoutOCB = new CodeDescriptionPairList();
					for (int i = 0; i < base.EventCodesList.Count; i++)
					{
						if (!base.EventCodesList[i].Code.Equals(AutoEvents.OceanCarrierBookingByTEU.Code))
						{
							eventCodesWithoutOCB.Add(base.EventCodesList[i]);
						}
					}
				}
				return eventCodesWithoutOCB;
			}
		}

		CodeDescriptionPairList eventCodesWithoutOCB;
	}
}
