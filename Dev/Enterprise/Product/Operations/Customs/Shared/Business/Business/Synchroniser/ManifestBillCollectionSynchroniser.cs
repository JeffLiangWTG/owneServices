using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public abstract class ManifestBillCollectionSynchroniser<THeaderDest, TBillDest> : BusinessObjectCollectionSynchroniser
		where THeaderDest : BusinessObject, IManifestHeaderForSynchroniser
		where TBillDest : BusinessObject, IManifestBillForSynchroniser
	{
		public ManifestBillCollectionSynchroniser(THeaderDest header)
				: base((ForwardingConsol)(header?.Consol), header)
		{
		}

		protected new THeaderDest Destination => (THeaderDest)base.Destination;

		protected new ForwardingConsol Source => (ForwardingConsol)base.Source;

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			foreach (var elementSynchroniser in ElementSynchronisers.ToArray())
			{
				if (elementSynchroniser.Destination is TBillDest billForSynchroniser && !Destination.Bills.Contains(billForSynchroniser))
				{
					ElementSynchronisers.Remove(elementSynchroniser);
				}
			}
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && !Destination.OverrideFreightDefaults)
			{
				DeleteOrAddManifestBills();
			}
		}

		void DeleteOrAddManifestBills()
		{
			var shipments = GetApplicableShipments(Source.Shipments.ToArray<ForwardingShipment>());
			var bills = new List<TBillDest>(Destination.Bills.Cast<TBillDest>());
			while (bills.Count > 0)
			{
				var bill = bills[0];
				bills.Remove(bill);

				var synchroniser = ElementSynchronisers.FindMatchingDestination<ManifestBillSynchroniser<TBillDest>>(bill, true);
				if (bill.IsDeleted)
				{
					if (synchroniser != null)
					{
						synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
						ElementSynchronisers.Remove(synchroniser);
					}
				}
				else
				{
					if (synchroniser != null)
					{
						if (shipments.Contains(synchroniser.Source))
						{
							shipments.Remove(synchroniser.Source);
							synchroniser.Synchronise();
							continue;
						}
					}
					else
					{
						bill = FindMatchingShipmentAndAddSynchroniser(shipments, bills, bill);
					}

					if (bill != null && !bill.IsBillAlreadyOnFile)
					{
						bill.Delete();
					}
				}
			}

			foreach (var shipment in shipments)
			{
				if (ShouldAddNewBillForThisShipment(shipment))
				{
					var bill = (TBillDest)Destination.AddNewBill();
					var synchroniser = GetNewManifestBillSynchroniser(bill, shipment);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
		}

		List<ForwardingShipment> GetApplicableShipments(IEnumerable<ForwardingShipment> shipments)
		{
			var result = new List<ForwardingShipment>();
			foreach (var shipment in shipments)
			{
				if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster && !shipment.IsAssemblyMaster)
				{
					var coLoadMasterShipment = shipment.CoLoadMasterShipment;
					if (coLoadMasterShipment == null || (!coLoadMasterShipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster && !coLoadMasterShipment.IsAssemblyMaster) || IsShipmentWhichSendingForwarderDoesNotDoDirectAMSReporting(coLoadMasterShipment))
					{
						result.Add(shipment);
					}
				}
			}
			return result;
		}

		bool IsShipmentWhichSendingForwarderDoesNotDoDirectAMSReporting(Freight.Business.CommonShipment coLoadMasterShipment)
		{
			var sendingForwarder = coLoadMasterShipment.Consignor;
			return sendingForwarder == null || sendingForwarder.MiscServ == null || !sendingForwarder.MiscServ.OM_FWDirectAMSReporter;
		}

		TBillDest FindMatchingShipmentAndAddSynchroniser(List<ForwardingShipment> shipments, List<TBillDest> bills, TBillDest bill)
		{
			ForwardingShipment existingShipment = null;
			var alreadyProcessedShipments = new List<ForwardingShipment>();
			while ((existingShipment = shipments.FirstOrDefault(shipment => !alreadyProcessedShipments.Contains(shipment) && GetBillNumber(shipment) == bill.MasterBillNumberInfo.Value.ToString() && AreAdditionalKeysMatching(shipment, bill))) != null)
			{
				alreadyProcessedShipments.Add(existingShipment);
				var synchroniser = ElementSynchronisers.FindMatchingSource<ManifestBillSynchroniser<TBillDest>>(existingShipment, true);
				if (synchroniser != null)
				{
					shipments.Remove(existingShipment);
					bills.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = GetNewManifestBillSynchroniser(bill, existingShipment);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					shipments.Remove(existingShipment);
					bill = null;
					break;
				}
			}
			return bill;
		}

		protected virtual ZString GetBillNumber(ForwardingShipment shipment)
		{
			return shipment.JS_HouseBill;
		}

		protected virtual bool ShouldAddNewBillForThisShipment(ForwardingShipment shipment)
		{
			return true;
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.OverrideFreightDefaults)
			{
				HookSynchronisationForExistingBills();
			}
		}

		void HookSynchronisationForExistingBills()
		{
			var bills = new List<TBillDest>(Destination.Bills.Cast<TBillDest>());
			var shipments = GetApplicableShipments(new TypedEnumerable<ForwardingShipment>(Source.Shipments));

			foreach (var shipment in shipments.ToArray())
			{
				var billNumber = GetBillNumber(shipment);
				var synchroniser = ElementSynchronisers.FindMatchingSource<ManifestBillSynchroniser<TBillDest>>(shipment, true);
				if (synchroniser != null)
				{
					var bill = synchroniser.Destination;
					bills.Remove(synchroniser.Destination);
					if (synchroniser.Source.IsDeleted || synchroniser.Destination.IsDeleted)
					{
						synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
						ElementSynchronisers.Remove(synchroniser);
						synchroniser = null;
					}
					else
					{
						if (bill.IsBillAlreadyOnFile)
						{
							if (!billNumber.EqualsIgnoringCase(bill.MasterBillNumberInfo.Value.ToString()) || !AreAdditionalKeysMatching(shipment, bill))
							{
								synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
								ElementSynchronisers.Remove(synchroniser);
								synchroniser = null;
							}
						}
						else
						{
							var billAlreadyOnFile = bills.FirstOrDefault(x => !x.IsDeleted && x.IsBillAlreadyOnFile && AreAdditionalKeysMatching(shipment, x) && x.MasterBillNumberInfo.Value.ToString() == billNumber);
							if (billAlreadyOnFile != null)
							{
								var synchroniserForBillAlreadyOnFile = ElementSynchronisers.FindMatchingDestination<ManifestBillSynchroniser<TBillDest>>(billAlreadyOnFile, true);
								if (synchroniserForBillAlreadyOnFile != null && !shipments.Contains(synchroniserForBillAlreadyOnFile.Source))
								{
									synchroniserForBillAlreadyOnFile.SetEnabled(false, synchroniser.DetectEnabled);
									ElementSynchronisers.Remove(synchroniserForBillAlreadyOnFile);
									synchroniserForBillAlreadyOnFile = null;
								}
								if (synchroniserForBillAlreadyOnFile == null)
								{
									synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
									ElementSynchronisers.Remove(synchroniser);
									synchroniser = null;
									ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetNewManifestBillSynchroniser(billAlreadyOnFile, shipment), IsEnabled, DetectEnabled);
									bills.Remove(billAlreadyOnFile);
								}
							}
						}
					}
				}

				if (synchroniser == null)
				{
					TBillDest bill = null;
					while ((bill = bills.FirstOrDefault(x => !x.IsDeleted && AreAdditionalKeysMatching(shipment, x) && x.MasterBillNumberInfo.Value.ToString() == billNumber)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<ManifestBillSynchroniser<TBillDest>>(bill, true);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(GetNewManifestBillSynchroniser(bill, shipment), IsEnabled, DetectEnabled);
							bills.Remove(bill);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							bills.Remove(bill);
						}
					}
				}
				else
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
				}
			}
		}

		protected override void HookEvents()
		{
			base.HookEvents();
			foreach (ForwardingShipment shipment in Source.Shipments)
			{
				HookShipmentChangeEvent(shipment);
			}
		}
		void HookShipmentChangeEvent(ForwardingShipment shipment)
		{
			HookInfoValueChanged(shipment.JS_ShipmentTypeInfo);
			HookInfoValueChanged(shipment.JS_JS_ColoadMasterShipmentInfo);
			HookInfoValueChanged(shipment.ConsignorPKInfo);
			HookInfoValueChanged(shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPKInfo);
		}

		protected void HookInfoValueChanged(ZPropertyInfo info)
		{
			info.ValueChanged -= Info_ValueChanged;
			info.ValueChanged += Info_ValueChanged;
		}

		protected override void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = e.BizObject as ForwardingShipment;
			if (shipment != null)
			{
				if (e.ItemAdded)
				{
					HookShipmentChangeEvent(shipment);
				}
				else
				{
					UnHookShipmentChangeEvent(shipment);
				}
			}
			base.Collection_CountChanged(sender, e);
		}

		protected override void UnHookEvents()
		{
			foreach (ForwardingShipment shipment in Source.Shipments)
			{
				UnHookShipmentChangeEvent(shipment);
			}
			base.UnHookEvents();
		}

		void UnHookShipmentChangeEvent(ForwardingShipment shipment)
		{
			shipment.JS_ShipmentTypeInfo.ValueChanged -= Info_ValueChanged;
			shipment.JS_JS_ColoadMasterShipmentInfo.ValueChanged -= Info_ValueChanged;
			shipment.ConsignorPKInfo.ValueChanged -= Info_ValueChanged;
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPKInfo.ValueChanged -= Info_ValueChanged;
		}

		protected void Info_ValueChanged(object sender, System.EventArgs e)
		{
			Synchronise();
		}

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.Shipments;
		}

		protected abstract ConsolDataCalculator ConsolDataCalculator { get; }

		protected abstract bool AreAdditionalKeysMatching(ForwardingShipment shipment, TBillDest bill);

		protected abstract ManifestBillSynchroniser<TBillDest> GetNewManifestBillSynchroniser(TBillDest destination, ForwardingShipment source);

		#endregion
	}
}
