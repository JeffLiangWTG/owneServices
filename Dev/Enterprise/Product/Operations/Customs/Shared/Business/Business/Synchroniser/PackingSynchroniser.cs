using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class PackingSynchroniser
	{
		public PackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration)
		{
			this.Declaration = declaration;
			this.Shipment = declaration.Shipment;
			this.ParentSynchroniser = parentSynchroniser;
		}

		protected readonly JobDeclarationSynchroniser ParentSynchroniser;
		protected readonly BaseJobDeclaration Declaration;
		protected readonly ForwardingShipment Shipment;

		public bool SyncChangesDetected
		{
			get { return syncChangesDetected; }
			protected set
			{
				syncChangesDetected = value;
			}
		}
		bool syncChangesDetected;

		public bool DetectEnabled
		{
			get { return detectEnabled; }
			set
			{
				if (detectEnabled != value)
				{
					detectEnabled = value;
					OnDetectEnabledChanged();
				}
			}
		}
		bool detectEnabled;

		protected virtual void OnDetectEnabledChanged()
		{
			Declaration.Packages.Load();
			Declaration.PackingGroups.Load();
		}

		public void Synchronise(bool forceIfDisabled = false)
		{
			if (!isPackingBeingSynchronised)
			{
				isPackingBeingSynchronised = true;
				try
				{
					SyncChangesDetected = false;
					SynchroniseCore(forceIfDisabled);
				}
				finally
				{
					isPackingBeingSynchronised = false;
				}
			}
		}

		protected virtual void SynchroniseCore(bool forceIfDisabled)
		{
			if (ShouldSynchronisePackingInformation(forceIfDisabled) && Declaration.PackingInformationCollection != null)
			{
				Declaration.Packages.MarkAsDeleteForShipmentSynch();
				try
				{
					Declaration.PackingGroups.IsSynchronising = true;
					SynchroniseMultiHouseBills();
				}
				finally
				{
					Declaration.PackingGroups.IsSynchronising = false;
				}

				if (!SyncChangesDetected)
				{
					foreach (var package in Declaration.Packages.OfType<BasePackage>().Where(x => !x.IsDeleted).ToArray())
					{
						if (package.IsGoingToBeDeletedAfterShipmentSynch)
						{
							if (DetectEnabled)
							{
								SyncChangesDetected = true;
								return;
							}
							package.Delete();
						}
					}

					List<BasePackingGroup> unusedPackingGroups = Declaration.PackingGroups.GetPackingGroupsWithNoPackages();
					if (DetectEnabled && unusedPackingGroups.Count > 0)
					{
						SyncChangesDetected = true;
						return;
					}
					unusedPackingGroups.ForEach(delegate(BasePackingGroup p)
					{ if (!p.IsDeleted) { p.Delete(); } });
				}
			}
		}

		bool isPackingBeingSynchronised;

		bool ShouldSynchronisePackingInformation(bool forceIfDisabled)
		{
			return (ParentSynchroniser.IsEnabled || forceIfDisabled) &&
					(!Declaration.JE_OverrideFreightDefaults || DetectEnabled) &&
					Declaration.IsPackingInformationRelevant;
		}

		void SynchroniseMultiHouseBills()
		{
			foreach (ForwardingShipment shipment in Declaration.GetShipmentsToSync())
			{
				if (SyncChangesDetected)
				{
					return;
				}
				SynchroniseSingleBill(shipment);
			}
		}

		bool IsDirectConsol
		{
			get
			{
				var consol = Declaration.RelevantConsol;
				return consol != null && consol.IsDirect;
			}
		}

		protected virtual void SynchroniseSingleBill(ForwardingShipment shipment)
		{
			foreach (PackLine packLine in shipment.OuterPackLines)
			{
				if (SyncChangesDetected)
				{
					return;
				}
				SynchronisePackLine(packLine, shipment);
			}
		}

		Bill GetBillMatchingShipment(ZString houseBillNumber)
		{
			return Declaration.Bills.FindByBillNumberAndType(houseBillNumber, BillTypeList.Codes.HouseBill);
		}

		protected virtual Bill GetEffectiveBill(ZString houseBill, ForwardingContainer container)
		{
			var result = GetBillMatchingShipment(houseBill)
				?? (IsDirectConsol || ShouldDefaultBillToMasterIfHouseBillIsEmpty ? Declaration.PrimaryMasterBill : null);
			return result;
		}

		protected virtual ZBool ShouldDefaultBillToMasterIfHouseBillIsEmpty
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual void SynchronisePackLine(PackLine packLine, ForwardingShipment shipment)
		{
			var jobContainer = packLine.GetContainer(Declaration.RelevantConsol) as ForwardingContainer;

			var houseBill = ParentSynchroniser.GetHouseBillOfSpecificShipment(shipment);
			var effectiveBill = GetEffectiveBill(houseBill, jobContainer);

			if (effectiveBill != null)
			{
				var containerNum = jobContainer != null ? jobContainer.JC_ContainerNum : ZString.Empty;

				BaseCusContainer container = Declaration.CusContainers.Find(containerNum);

				IPackingInformation @new = Declaration.PackingInformationCollection.GetMatchingElement(effectiveBill, container);
				if (@new == null)
				{
					if (DetectEnabled)
					{
						SyncChangesDetected = true;
						return;
					}
					@new = Declaration.PackingInformationCollection.AddNew();
				}
				@new.HouseBillContainer = new HouseBillContainer(effectiveBill, container);
				if (@new.SupportMarksAndNumbers)
				{
					if (DetectEnabled)
					{
						if (@new.MarksAndNumbers != packLine.JL_MarksAndNumbers || (packLine.JL_MarksAndNumbers.IsEmpty && shipment != null && !shipment.JS_MarksAndNumbers.IsEmpty))
						{
							SyncChangesDetected = true;
							return;
						}
					}
					else
					{
						var sourceMarksAndNums = packLine.JL_MarksAndNumbers;
						if (packLine.JL_MarksAndNumbers.IsEmpty && shipment != null && !shipment.JS_MarksAndNumbers.IsEmpty)
						{
							sourceMarksAndNums = shipment.JS_MarksAndNumbers;
						}
						@new.MarksAndNumbers = GetConvertedMarksAndNumbers(sourceMarksAndNums);
					}
				}

				var newOneToOne = @new as IOneToOnePackingInformation;
				if (newOneToOne != null)
				{
					if (DetectEnabled)
					{
						if (newOneToOne.PackQty != packLine.JL_PackageCount ||
							!newOneToOne.PackType.EqualsIgnoringCase(GetConvertedPackUQ(packLine.JL_F3_NKPackType)))
						{
							SyncChangesDetected = true;
							return;
						}
					}
					else
					{
						newOneToOne.PackQty = packLine.JL_PackageCount;
						newOneToOne.PackType = GetConvertedPackUQ(packLine.JL_F3_NKPackType);
					}

					var unusedCustomsUNDGs = new List<UNDGDataItem>(newOneToOne.UNDGs);

					foreach (UNDGDataItem shipmentDG in packLine.UNDGs)
					{
						if (shipmentDG.Substance != null)
						{
							UNDGDataItem customsDG = null;

							foreach (UNDGDataItem existingDG in newOneToOne.UNDGs)
							{
								if (existingDG.DI_DG == shipmentDG.DI_DG
									&& existingDG.DI_DGFlashPoint == shipmentDG.DI_DGFlashPoint)
								{
									customsDG = existingDG;
									unusedCustomsUNDGs.Remove(existingDG);
									break;
								}
							}

							if (customsDG == null)
							{
								if (DetectEnabled)
								{
									SyncChangesDetected = true;
									return;
								}
								customsDG = newOneToOne.UNDGs.AddNew();
								customsDG.DI_DG = shipmentDG.DI_DG;
								customsDG.DI_DGFlashPoint = shipmentDG.DI_DGFlashPoint;
							}

							if (DetectEnabled)
							{
								if (customsDG.DI_OC_DGContact != shipmentDG.DI_OC_DGContact)
								{
									SyncChangesDetected = true;
									return;
								}
							}
							else
							{
								customsDG.DI_OC_DGContact = shipmentDG.DI_OC_DGContact;
							}
						}
					}

					if (unusedCustomsUNDGs.Count > 0)
					{
						if (DetectEnabled)
						{
							SyncChangesDetected = true;
							return;
						}
						unusedCustomsUNDGs.ForEach(x => x.Delete());
					}
				}

				SetSynchroniserAdditionalPackLineFields(@new, packLine);
			}
		}

		protected virtual void SetSynchroniserAdditionalPackLineFields(IPackingInformation newPackage, PackLine packLine)
		{
		}

		protected virtual ZString GetConvertedPackUQ(ZString freightPackType)
		{
			return freightPackType;
		}

		protected virtual ZString GetConvertedMarksAndNumbers(ZString marksAndNumbers)
		{
			return marksAndNumbers;
		}
	}
}
