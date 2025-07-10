using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class AllMasterConsolCollection : ActiveBusinessObjectCollection<ForwardingConsol>
	{
		public AllMasterConsolCollection(ForwardingShipment shipment)
			: base(shipment.Factory, new AdhocCollectionRelationship(typeof(ForwardingConsol)))
		{
			this.Shipment = shipment;

			Rebuild();
			HookConsols();
		}

		readonly ForwardingShipment Shipment;

		void HookConsols()
		{
			Shipment.Consols.CountChanged += (s, e) =>
			{
				var consol = e.BizObject as ForwardingConsol;
				if (consol != null)
				{
					if (e.ItemAdded)
					{
						consol.JK_JK_MasterConsolInfo.ValueChanged += JK_JK_MasterConsolInfo_ValueChanged;
					}
					else
					{
						consol.JK_JK_MasterConsolInfo.ValueChanged -= JK_JK_MasterConsolInfo_ValueChanged;
					}

					Rebuild();
				}
			};

			foreach (ForwardingConsol consol in Shipment.Consols)
			{
				consol.JK_JK_MasterConsolInfo.ValueChanged += JK_JK_MasterConsolInfo_ValueChanged;
			}
		}

		void JK_JK_MasterConsolInfo_ValueChanged(object sender, EventArgs e)
		{
			Rebuild();
		}

		void Rebuild()
		{
			var consolPKs = new List<ZGuid>();

			foreach (ForwardingConsol consol in Shipment.Consols)
			{
				consolPKs.Add(consol.PK);
				if (!Contains(consol))
				{
					Add(consol);
				}

				var multiAWBMasterConsol = consol.MasterConsol;
				if (multiAWBMasterConsol != null)
				{
					if (!consolPKs.Contains(multiAWBMasterConsol.PK))
					{
						consolPKs.Add(multiAWBMasterConsol.PK);
					}

					if (!Contains(multiAWBMasterConsol))
					{
						Add(multiAWBMasterConsol);
					}
				}
			}

			var consolsToRemove = this.Where(x => !consolPKs.Contains(x.PK)).ToArray();
			foreach (var consolToRemove in consolsToRemove)
			{
				RemoveFromRelationship(consolToRemove);
			}
		}

		#region Implementation

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { Shipment.PK };
		}

		#endregion
	}
}
