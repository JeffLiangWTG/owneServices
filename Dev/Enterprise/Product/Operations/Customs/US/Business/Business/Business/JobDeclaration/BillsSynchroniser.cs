using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class BillsSynchroniser : Customs.Business.BillsSynchroniser
	{
		public BillsSynchroniser(JobDeclaration declaration, Func<IBillDetails, ZString> getBillNumber)
			: base(declaration, getBillNumber)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void SynchroniseBill(BillDetailsWrapper sourceBill, Customs.Business.Bill destination)
		{
			base.SynchroniseBill(sourceBill, destination);
			if (!SyncChangesDetected)
			{
				var bill = (Bill)destination;
				var usDeclaration = (JobDeclaration)this.declaration;
				bill.SynchronizeFromAMS = new BillSynchronisationDataCalculator(usDeclaration, sourceBill.Bill).SyncFromAMSBillNumber;

				var scac = GetBillIssuerSCAC(sourceBill);
				if (DetectEnabled)
				{
					if (!bill.US_UI_NKBillIssuerSCAC.EqualsIgnoringCase(scac))
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					bill.US_UI_NKBillIssuerSCAC = scac;
				}

				if (DetectEnabled)
				{
					bill.ITAndSplitDetails.Load();
				}
				var itAndSplitDetails = bill.ITAndSplitDetails
					.Cast<ITAndSplitDetails>()
					.Where(i => !i.IsDeleted)
					.ToList();
				var itNumbersToDelete = itAndSplitDetails
					.Where(i => !sourceBill.ReleaseNumbers.Exists(r => r.Key.EqualsIgnoringCase(i.US_ITNumber)))
					.ToArray();

				if (DetectEnabled)
				{
					if (itNumbersToDelete.Length > 0)
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					foreach (ITAndSplitDetails itNumber in itNumbersToDelete)
					{
						itAndSplitDetails.Remove(itNumber);
						bill.ITAndSplitDetails.RemoveAndDelete(itNumber);
					}
				}
				var itDetails = new Dictionary<ZString, ZInt>();
				foreach (var releaseNumberFromSourceBill in sourceBill.ReleaseNumbers)
				{
					var itNumber = releaseNumberFromSourceBill.Key.Left(11).ToUpper();
					if (!itNumber.IsEmpty)
					{
						ZInt noOfPacks;
						if (!itDetails.TryGetValue(itNumber, out noOfPacks))
						{
							noOfPacks = ZInt.Zero;
							itDetails.Add(itNumber, noOfPacks);
						}
						itDetails[itNumber] = noOfPacks + releaseNumberFromSourceBill.Value;
					}
				}
				if (DetectEnabled)
				{
					if (itDetails.Count != itAndSplitDetails.Count)
					{
						SyncChangesDetected = true;
						return;
					}
					foreach (var itDetail in itDetails)
					{
						var itNo = itAndSplitDetails.FirstOrDefault(x => x.US_ITNumber.EqualsIgnoringCase(itDetail.Key));
						if (itNo == null || itNo.US_NoOfPacks != itDetail.Value)
						{
							SyncChangesDetected = true;
							return;
						}
					}
				}
				else
				{
					foreach (var itDetail in itDetails)
					{
						var itNo = itAndSplitDetails.FirstOrDefault(x => x.US_ITNumber.EqualsIgnoringCase(itDetail.Key));
						if (itNo == null)
						{
							itNo = bill.ITAndSplitDetails.AddNew();
							using (itNo.GetValidationSuspender())
							{
								itNo.US_ITNumber = itDetail.Key;
							}
						}
						else
						{
							itAndSplitDetails.Remove(itNo);
						}
						itNo.US_NoOfPacks = itDetail.Value;
					}
					// Delete existing duplicate data
					foreach (var itNumber in itAndSplitDetails)
					{
						bill.ITAndSplitDetails.RemoveAndDelete(itNumber);
					}
				}
			}
		}

		ZString GetBillIssuerSCAC(BillDetailsWrapper sourceBill)
		{
			var result = ZString.Empty;
			if (declaration.IsImport && sourceBill.BillType == BillTypeList.Codes.MasterBill && declaration.IsAir)
			{
				ZString scac = sourceBill.BillNumber.SubstringSafe(0, 3);
				if (scac.IsNumbersOnlyOrEmpty && sourceBill.BillNumber.Length > 3)
				{
					var carriers = declaration.Factory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, scac));
					if (carriers.Length == 1 && !carriers[0].RM_TwoCharacterCode.IsEmpty)
					{
						result = carriers[0].RM_TwoCharacterCode;
					}
				}
			}
			else
			{
				result = new BillSynchronisationDataCalculator((JobDeclaration)declaration, sourceBill.Bill).IssuerCode;
			}

			return result;
		}

		protected override void SynchronisePrimaryBills()
		{
			base.SynchronisePrimaryBills();
			if (!SyncChangesDetected)
			{
				var usDeclaration = (JobDeclaration)this.declaration;

				var primaryMB = usDeclaration.PrimaryMasterBill;
				var primaryHB = usDeclaration.PrimaryHouseBill;
				if (DetectEnabled)
				{
					if (!usDeclaration.JE_MasterBillIssuerSCAC.EqualsIgnoringCase(primaryMB != null ? primaryMB.US_UI_NKBillIssuerSCAC : ZString.Empty) ||
						!usDeclaration.JE_HouseBillIssuerSCAC.EqualsIgnoringCase(primaryHB != null ? primaryHB.US_UI_NKBillIssuerSCAC : ZString.Empty))
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					usDeclaration.JE_MasterBillIssuerSCAC = primaryMB != null ? primaryMB.US_UI_NKBillIssuerSCAC : ZString.Empty;
					usDeclaration.JE_HouseBillIssuerSCAC = primaryHB != null ? primaryHB.US_UI_NKBillIssuerSCAC : ZString.Empty;
				}
			}
		}

		protected override IEnumerable<BillDetailsWrapper> GetMasterBillsToSynchronise()
		{
			var relevantConsol = declaration.RelevantConsol;
			bool isDirectConsol = relevantConsol != null && relevantConsol.IsDirect;

			var shipment = declaration.Shipment;

			var containers = shipment.GetUniqueContainersFromConsol(relevantConsol);

			BillDetailsWrapper consolMasterBillWrapper = null;
			var isAllContainersHavingNewMBOLs = IsAllContainersHavingNewMBOLs(containers);
			if (!isAllContainersHavingNewMBOLs)
			{
				consolMasterBillWrapper = CreateConsolMasterBillWrapper();
				if (consolMasterBillWrapper != null)
				{
					if (isDirectConsol || shipment.IsRail)
					{ consolMasterBillWrapper.ReleaseNumbers.AddRange(shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(relevantConsol)); }
					yield return consolMasterBillWrapper;
				}
			}

			var sortedContainersWithNewMBOLs = GetSortedContainersWithNewMBOL(containers);
			var shouldNotCreateHouseBillFromThisShipment = isAllContainersHavingNewMBOLs || isDirectConsol;
			foreach (ForwardingContainer container in sortedContainersWithNewMBOLs)
			{
				var wrapper = new BillDetailsWrapper(container, BillTypeList.Codes.MasterBill, consolMasterBillWrapper == null && sortedContainersWithNewMBOLs[0] == container);
				if (container.HasITNumber)
				{
					if (shouldNotCreateHouseBillFromThisShipment)
					{
						wrapper.ReleaseNumbers.Add(new KeyValuePair<ZString, ZInt>(container.ITReferenceNumber, container.GetNoOfPacksOnSpecificShipment(shipment)));
					}
				}
				yield return wrapper;
			}
		}

		ZBool IsAllContainersHavingNewMBOLs(IEnumerable<ForwardingContainer> containers)
		{
			return containers.Any() && !containers.Any((x) => !x.HasNewMBOL);
		}

		List<ForwardingContainer> GetSortedContainersWithNewMBOL(IEnumerable<ForwardingContainer> containers)
		{
			var result = new List<ForwardingContainer>(containers.Where(x => x.HasNewMBOL));
			result.Sort((x, y) => x.JC_ContainerNum.CompareTo(y.JC_ContainerNum));
			return result;
		}

		List<BillDetailsWrapper> CreateHouseBillFromContainersWithNewMBOL(ForwardingShipment shipment, ref BillDetailsWrapper primaryBillWrapper)
		{
			var result = new List<BillDetailsWrapper>();

			var usDeclaration = this.declaration as JobDeclaration;
			var relevantConsol = usDeclaration.RelevantConsol;
			var containers = shipment.GetUniqueContainersFromConsol(relevantConsol);
			if (!IsAllContainersHavingNewMBOLs(containers))
			{
				var sortedContainersWithNewBOLs = GetSortedContainersWithNewMBOL(containers);
				foreach (ForwardingContainer container in sortedContainersWithNewBOLs)
				{
					var wrapper = new BillDetailsWrapper(shipment, BillTypeList.Codes.HouseBill, primaryBillWrapper == null && sortedContainersWithNewBOLs[0] == container);
					if (container.HasITNumber)
					{
						wrapper.ReleaseNumbers.Add(new KeyValuePair<ZString, ZInt>(container.ITReferenceNumber, container.GetNoOfPacksOnSpecificShipment(shipment)));
					}
					if (primaryBillWrapper == null && wrapper.IsPrimary)
					{
						primaryBillWrapper = wrapper;
					}

					var calculator = new BillSynchronisationDataCalculator(usDeclaration, container);
					wrapper.GetParentBill = () => usDeclaration.Bills.Find(calculator.BillNumber, BillTypeList.Codes.MasterBill, calculator.IssuerCode);

					result.Add(wrapper);
				}
			}
			return result;
		}

		bool IsPrimaryMasterBillCreatedFromConsol
		{
			get
			{
				var relevantConsol = declaration.RelevantConsol;
				var primaryMasterBill = declaration.PrimaryMasterBill;
				return primaryMasterBill != null && relevantConsol != null && primaryMasterBill.CU_BillNum.EqualsIgnoringCase(GetHouseBillOfSpecificShipment(relevantConsol));
			}
		}

		List<BillDetailsWrapper> CreateHouseBillFromShipment(ForwardingShipment shipment, ref BillDetailsWrapper primaryHouseBillWrapper)
		{
			var result = new List<BillDetailsWrapper>();
			var relevantConsol = declaration.RelevantConsol;
			var containers = shipment.GetUniqueContainersFromConsol(relevantConsol);
			if (!IsAllContainersHavingNewMBOLs(containers))
			{
				var wrapper = GetHouseBillToSynchronise(shipment, primaryHouseBillWrapper == null);
				wrapper.GetParentBill = () => IsPrimaryMasterBillCreatedFromConsol ? declaration.PrimaryMasterBill : null;

				if (primaryHouseBillWrapper == null && wrapper.IsPrimary)
				{
					primaryHouseBillWrapper = wrapper;
				}
				if (!shipment.IsRail)
				{
					wrapper.ReleaseNumbers.AddRange(shipment.GetAllDistinctEffectiveITNumbersFromContainersHavingNoMBOL(relevantConsol));
				}

				result.Add(wrapper);
			}

			return result;
		}

		protected override IEnumerable<BillDetailsWrapper> GetHouseBillsToSynchronise()
		{
			var shipmentsToSync = declaration.GetShipmentsToSync().Where(s => !s.JS_HouseBill.IsEmpty);

			BillDetailsWrapper primaryHouseBillWrapper = null;
			foreach (var shipment in shipmentsToSync)
			{
				foreach (var wrapper in CreateHouseBillFromShipment(shipment, ref primaryHouseBillWrapper))
				{
					yield return wrapper;
				}
				foreach (var wrapper in CreateHouseBillFromContainersWithNewMBOL(shipment, ref primaryHouseBillWrapper))
				{
					yield return wrapper;
				}
			}
		}

		protected override void OnSynchronised()
		{
			base.OnSynchronised();
			((JobDeclaration)this.declaration).DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
		}

		protected override ZString ConvertPack(ZString freightPack, ZGuid registryCompanyPK)
		{
			var mappings = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(registryCompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
			return mappings.GetMappedPackageType(freightPack);
		}
	}
}
