using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManTranHead : AutoCusSeaManTranHead, IStatusNeedsRecalculationProvider, ISendersMessageReferenceProvider
	{
		#region Constants

		public abstract new class Schema : AutoCusSeaManTranHead.Schema
		{
			public const string DischargePortToShow = "DischargePortToShow";
		}

		#endregion

		public CusSeaManTranHead(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusSeaManTranHeadTypeDecider TypeDecider = new CusSeaManTranHeadTypeDecider();

		public static CusSeaManTranHead LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			return factory.LoadTop1<CusSeaManTranHead>(new ZQuery(Enterprise.ZArchitecture.Schema.CusSeaManTranHeadSchema.BT_SendersMessageReference, sendersReference));
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.CusSeaManTranHeadFetchStrategy(this);
		}

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			Arrivals.RemoveAndDeleteAll();
			OceanBills.RemoveAndDeleteAll();
			SlotCharterers.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			((ISendersMessageReferenceProvider)this).PopulateSendersReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				BT_SendersMessageReference = ZString.Empty;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("de01f124-2f9e-4c48-8ee1-06a70b794cfe", "Import Manifest {0}", BT_SendersMessageReference).Trim(); }
		}

		protected sealed override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get { return GetBusinessObjectsWithRelatedLogsCore().ToArray(); }
		}

		protected virtual List<BusinessObject> GetBusinessObjectsWithRelatedLogsCore()
		{
			List<BusinessObject> result = new List<BusinessObject>();

			foreach (CusSeaManOBLHeader oceanBill in OceanBills)
			{
				result.Add(oceanBill);
				result.AddRange(oceanBill.Details);
			}

			result.AddRange(Arrivals);

			return result;
		}

		#endregion

		public ZString FirstPortOfArrival
		{
			get
			{
				foreach (CusSeaManArrivalPort arrival in Arrivals)
				{
					if (arrival.BA_IsFirstArrival)
					{
						return arrival.BA_RL_NKArrivalPort;
					}
				}
				return ZString.Empty;
			}
		}

		#region Properties

		#region DischargePortToShow

		[CargoWise.ComponentModel.MaxLength(5)]
		public ZString DischargePortToShow
		{
			get
			{
				return OceanBillsView.DischargePort;
			}
			set
			{
				if (DischargePortToShow != value)
				{
					CheckMaximumLength(DischargePortToShowInfo, value);
					OceanBillsView.DischargePort = value;
				}
				DischargePortToShowInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DischargePortToShowInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.DischargePortToShow);
			}
		}

		#endregion

		#region Arrivals

		[ChildEditable(true)]
		public CusSeaManArrivalPortCollection Arrivals
		{
			get
			{
				if (fArrivals == null)
				{
					fArrivals = GetNewArrivals();
					fArrivals.Load();
					RegisterEditableChildObject(fArrivals);
				}
				return fArrivals;
			}
		}

		protected virtual CusSeaManArrivalPortCollection GetNewArrivals()
		{
			return new CusSeaManArrivalPortCollection(this);
		}

		CusSeaManArrivalPortCollection fArrivals;

		#endregion

		#region OceanBills

		CusSeaManOBLHeaderCollectionView fOceanBillsView;
		public CusSeaManOBLHeaderCollectionView OceanBillsView
		{
			get
			{
				if (fOceanBillsView == null)
				{
					fOceanBillsView = GetNewOceanBillsView();
				}
				return fOceanBillsView;
			}
		}

		protected virtual CusSeaManOBLHeaderCollectionView GetNewOceanBillsView()
		{
			return new CusSeaManOBLHeaderCollectionView(OceanBills);
		}

		[ChildEditable(true)]
		public CusSeaManOBLHeaderCollection OceanBills
		{
			get
			{
				if (fOceanBills == null)
				{
					fOceanBills = GetNewOceanBills();
					fOceanBills.Load();
					RegisterEditableChildObject(fOceanBills);
				}
				return fOceanBills;
			}
		}

		protected virtual CusSeaManOBLHeaderCollection GetNewOceanBills()
		{
			return new CusSeaManOBLHeaderCollection(this);
		}

		CusSeaManOBLHeaderCollection fOceanBills;

		#endregion

		#region SlotCharterers

		[ChildEditable(true)]
		public CusSeaManSlotOrgCollection SlotCharterers
		{
			get
			{
				if (fSlotCharterers == null)
				{
					fSlotCharterers = GetNewSlotCharterers();
					fSlotCharterers.Load();
					RegisterEditableChildObject(fSlotCharterers);
				}
				return fSlotCharterers;
			}
		}

		protected virtual CusSeaManSlotOrgCollection GetNewSlotCharterers()
		{
			return new CusSeaManSlotOrgCollection(this);
		}

		CusSeaManSlotOrgCollection fSlotCharterers;

		#endregion

		#region Messages

		EDIMessageCollection messages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}
		#endregion

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		public bool StatusNeedsRecalculation
		{
			get
			{
				return Messages.HasChanges;
			}
		}

		#endregion

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(BT_SendersMessageReferenceInfo, Env.NumberFountains.CusSeaManTranHeaderNumber, ignoreInDatabaseCheck: true);
		}

		ZString ISendersMessageReferenceProvider.SendersReference
		{
			get { return BT_SendersMessageReference; }
		}

		#endregion
	}
}
