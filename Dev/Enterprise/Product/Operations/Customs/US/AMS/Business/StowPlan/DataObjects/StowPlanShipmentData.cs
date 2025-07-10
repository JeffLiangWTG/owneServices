using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanShipmentData : AutoStowPlanShipmentData, IStowPlanShipmentData, IStowPlanNotificationProvider
	{
		public StowPlanShipmentData(BillOfLading shipment)
			: base(shipment.Factory)
		{
			this.shipment = shipment;
			InitilizeContainers();
		}
		readonly BillOfLading shipment;

		internal JobSailing Sailing
		{
			get { return shipment.Sailing; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Checked = true;
		}

		#region Proxy properties

		public ZString JS_HouseBill
		{
			get { return shipment.JS_HouseBill; }
		}

		public ZString JS_PackingMode
		{
			get { return shipment.JS_PackingMode; }
		}

		public ZString JX_JA_RL_NKPortOfLoading
		{
			get { return shipment.Sailing.JX_JA_RL_NKPortOfLoading; }
		}

		public ZString JX_JB_RL_NKPortOfDischarge
		{
			get { return shipment.Sailing.JX_JB_RL_NKPortOfDischarge; }
		}

		public ZDateTime JX_JA_E_DEP
		{
			get { return shipment.Sailing.JX_JA_E_DEP; }
		}

		public ZDateTime JX_JB_E_ARV
		{
			get { return shipment.Sailing.JX_JA_E_ARV; }
		}

		public ZString ConsignorCompanyName
		{
			get { return shipment.ConsignorDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		public ZString ConsingeeCompanyName
		{
			get { return shipment.ConsigneeDocumentaryAddress.E2_CompanyNameTruncated; }
		}

		#endregion

		#region IStowPlanShipmentData members

		public override ZString PortOfLading
		{
			get { return shipment.JS_NKLoadPort; }
		}

		public override ZString PortOfDischarge
		{
			get { return shipment.JS_NKDischargePort; }
		}

		IEnumerable<IStowPlanContainerData> IStowPlanShipmentData.Containers
		{
			get { return Containers.Cast<IStowPlanContainerData>(); }
		}

		public StowPlanDataBusinessObjectCollection<StowPlanContainerData> Containers
		{
			get
			{
				InitilizeContainers();
				return fContainers;
			}
		}
		StowPlanDataBusinessObjectCollection<StowPlanContainerData> fContainers;

		void InitilizeContainers()
		{
			if (fContainers == null)
			{
				fContainers = new StowPlanDataBusinessObjectCollection<StowPlanContainerData>(Factory);
				RegisterEditableChildObject(fContainers);
				using (fContainers.SuspendListChanged())
				{
					fContainers.SuspendValidation();
					fContainers.RemoveAll();
					fContainers.AddRange(shipment.RealContainers.Cast<BillOfLadingContainer>().Select(x => new StowPlanContainerData(x)));
					fContainers.ResumeValidation();
				}
				shipment.RealContainers.CountChanged -= ContainerCount_Changed;
				shipment.RealContainers.CountChanged += ContainerCount_Changed;
			}
		}

		void ContainerCount_Changed(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				Containers.Add(new StowPlanContainerData((BillOfLadingContainer)e.BizObject));
			}
			else
			{
				var container = Containers.Cast<StowPlanContainerData>().FirstOrDefault(x => x.IsWrapperOf((BillOfLadingContainer)e.BizObject));
				if (container != null)
				{
					Containers.Remove(container);
				}
			}
		}

		#endregion

		#region IStowPlanNotificationProvider members

		Guid IStowPlanNotificationProvider.TargetPK
		{
			get { return shipment.PK.ToGuid(); }
		}

		string IStowPlanNotificationProvider.TargetCode
		{
			get { return shipment.TablePrefix; }
		}

		string IStowPlanNotificationProvider.TargetSubject
		{
			get { return shipment.HumanReadableName; }
		}

		#endregion
	}
}
