using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	class BillsSynchroniser : Customs.Business.BillsSynchroniser
	{
		public BillsSynchroniser(JobDeclaration declaration, Func<IBillDetails, ZString> getHouseBillOfSpecificShipment, Func<ForwardingConsol> getOutwardConsol, Func<ForwardingConsol> getInwardConsol)
			: base(declaration, getHouseBillOfSpecificShipment)
		{
			this.getInwardConsol = getInwardConsol;
			this.getOutwardConsol = getOutwardConsol;
		}

		readonly Func<ForwardingConsol> getOutwardConsol;
		readonly Func<ForwardingConsol> getInwardConsol;

		protected override IEnumerable<BillDetailsWrapper> GetMasterBillsToSynchronise()
		{
			var inwardConsol = getInwardConsol();

			if (inwardConsol != null && !inwardConsol.JK_MasterBillNum.IsEmpty)
			{
				yield return new BillDetailsWrapper(inwardConsol, BillTypeList.Codes.MasterBill, true);
			}
		}

		protected override void SynchronisePrimaryBills()
		{
			base.SynchronisePrimaryBills();
			if (!SyncChangesDetected)
			{
				var declaration = (JobDeclaration)this.declaration;
				var outwardConsol = getOutwardConsol();
				if (DetectEnabled)
				{
					SyncChangesDetected = !declaration.SG_OutwardMAWB.EqualsIgnoringCase(outwardConsol != null ? outwardConsol.JK_MasterBillNum : ZString.Empty) ||
					 !declaration.SG_OutwardHAWB.EqualsIgnoringCase(declaration.JE_HouseBill);
				}
				else
				{
					declaration.SG_OutwardMAWB = outwardConsol != null ? outwardConsol.JK_MasterBillNum : ZString.Empty;
					declaration.SG_OutwardHAWB = declaration.JE_HouseBill;
				}
			}
		}
	}
}
