using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondBillCollectionDeclarationSynchronizer : Customs.Business.BusinessObjectCollectionSynchroniser
	{
		internal CusInBondBillCollectionDeclarationSynchronizer(JobDeclaration source, CusInBondHeader destination)
			: base(source, destination)
		{
			Source.JE_TransportModeInfo.ValueChanged -= BillProperty_ValueChanged;
			Source.JE_TransportModeInfo.ValueChanged += BillProperty_ValueChanged;
		}

		protected new JobDeclaration Source
		{
			get { return (JobDeclaration)base.Source; }
		}

		protected new CusInBondHeader Destination
		{
			get { return (CusInBondHeader)base.Destination; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && Destination.ShouldSynchronise)
			{
				DeleteOrAddCusInBondBills();
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				HookSynchronisationForExistingBills();
			}
		}

		IEnumerable<Bill> GetSourceBillsToSynchronise()
		{
			var result = new List<Bill>();
			var bills = Source.Bills.OfType<Bill>();
			if (Source.IsAir || (ZZCustomsFunctionality.IsAMSHBREffective && Source.IsSea))
			{
				result.AddRange(bills.Where(x => x.IsHouseBill));
				result.AddRange(bills.Where(x => x.IsMasterBill && !bills.Any(y => y.CU_CU_ParentBill == x.PK)));
			}
			else
			{
				result.AddRange(bills.Where(x => x.IsMasterBill));
			}
			return result;
		}

		void HookSynchronisationForExistingBills()
		{
			var billList = new List<CusInBondBill>(Destination.Bills);
			foreach (var sourceBill in GetSourceBillsToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondBillDeclarationSynchronizer>(sourceBill);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					billList.Remove(synchroniser.Destination);
				}
				else
				{
					var billNumber = sourceBill.CU_BillNum;
					CusInBondBill bill = null;
					while ((bill = billList.FirstOrDefault(x => !x.IsDeleted && x.B0_MasterBillNumber == billNumber)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondBillDeclarationSynchronizer>(bill);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CusInBondBillDeclarationSynchronizer(bill, sourceBill), IsEnabled, DetectEnabled);
							billList.Remove(bill);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							billList.Remove(bill);
						}
					}
				}
			}
		}

		void DeleteOrAddCusInBondBills()
		{
			var billList = new List<Bill>(GetSourceBillsToSynchronise());
			var existingInBondBills = new List<CusInBondBill>(Destination.Bills);
			if (billList.Count > 0)
			{
				while (existingInBondBills.Count > 0)
				{
					var existingInBondBill = existingInBondBills[0];
					existingInBondBills.Remove(existingInBondBill);
					var synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondBillDeclarationSynchronizer>(existingInBondBill);
					if (existingInBondBill.IsDeleted || existingInBondBill.IsDeleting)
					{
						if (synchroniser != null)
						{
							ElementSynchronisers.Remove(synchroniser);
						}
					}
					else
					{
						if (synchroniser != null)
						{
							if (billList.Contains(synchroniser.Source))
							{
								billList.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							existingInBondBill = FindMatchingBillAndAddSynchroniser(billList, existingInBondBills, existingInBondBill);
						}

						if (existingInBondBill != null && !existingInBondBill.ActiveInMessaging)
						{
							existingInBondBill.Delete();
						}
					}
				}

				foreach (var sourceBill in billList)
				{
					var destinationBill = Destination.Bills.AddNew();
					var synchroniser = new CusInBondBillDeclarationSynchronizer(destinationBill, sourceBill);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				existingInBondBills.ForEach(inBondBill =>
				{
					if (!inBondBill.ActiveInMessaging)
					{
						inBondBill.Delete();
					}
				});
			}
		}

		CusInBondBill FindMatchingBillAndAddSynchroniser(List<Bill> bills, List<CusInBondBill> inBondBills, CusInBondBill inBondBill)
		{
			Bill existingBill = null;
			var alreadyProcessedBills = new List<Bill>();
			while ((existingBill = bills.FirstOrDefault(bill => !alreadyProcessedBills.Contains(bill) && IsBillsMatched(bill, inBondBill))) != null)
			{
				alreadyProcessedBills.Add(existingBill);
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondBillDeclarationSynchronizer>(existingBill);
				if (synchroniser != null)
				{
					bills.Remove(existingBill);
					inBondBills.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new CusInBondBillDeclarationSynchronizer(inBondBill, existingBill);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					bills.Remove(existingBill);
					inBondBill = null;
					break;
				}
			}
			return inBondBill;
		}

		bool IsBillsMatched(Bill bill, CusInBondBill inBondBill)
		{
			if (Source.IsAir && bill.IsHouseBill)
			{
				return bill.CU_MasterBill == inBondBill.B0_MasterBillNumber && bill.CU_HouseBill == inBondBill.B0_HouseBillNumber;
			}
			else
			{
				return bill.US_UI_NKBillIssuerSCAC == inBondBill.B0_IssuerCode && bill.CU_BillNum == inBondBill.B0_MasterBillNumber;
			}
		}

		#endregion

		#region Hook/UnHook Events

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (Bill bill in Source.Bills)
			{
				HookBillTypeChangeEvent(bill);
			}
		}

		protected override void UnHookEvents()
		{
			foreach (Bill bill in Source.Bills)
			{
				UnHookBillTypeChangeEvent(bill);
			}
			Source.JE_TransportModeInfo.ValueChanged -= BillProperty_ValueChanged;
			base.UnHookEvents();
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			return new[] { Source.Bills };
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var bill = e.BizObject as Bill;
				if (bill != null)
				{
					HookBillTypeChangeEvent(bill);
				}
			}
			else
			{
				var bill = e.BizObject as Bill;
				if (bill != null)
				{
					UnHookBillTypeChangeEvent(bill);
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		void UnHookBillTypeChangeEvent(Bill bill)
		{
			bill.CU_BillTypeInfo.ValueChanged -= BillProperty_ValueChanged;
			bill.CU_CU_ParentBillInfo.ValueChanged -= BillProperty_ValueChanged;
		}

		void HookBillTypeChangeEvent(Bill bill)
		{
			bill.CU_BillTypeInfo.ValueChanged -= BillProperty_ValueChanged;
			bill.CU_BillTypeInfo.ValueChanged += BillProperty_ValueChanged;
			bill.CU_CU_ParentBillInfo.ValueChanged -= BillProperty_ValueChanged;
			bill.CU_CU_ParentBillInfo.ValueChanged += BillProperty_ValueChanged;
		}

		void BillProperty_ValueChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		#endregion

		public Bill existingBill { get; set; }
	}
}
