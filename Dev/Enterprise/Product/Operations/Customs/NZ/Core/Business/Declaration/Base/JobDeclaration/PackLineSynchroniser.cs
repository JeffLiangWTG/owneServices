using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackLineSynchroniser : PackingSynchroniser
	{
		public PackLineSynchroniser(JobDeclarationSynchroniser parentSynchroniser, JobDeclaration declaration)
			: base(parentSynchroniser, declaration)
		{
		}

		protected override ZString GetConvertedPackUQ(ZString freightPackType)
		{
			return PackageTypeConverter.GetCustomsPackageType(freightPackType);
		}

		protected override ZBool ShouldDefaultBillToMasterIfHouseBillIsEmpty
		{
			get { return true; }
		}

		Dictionary<string, BasePackage> aggregatedPackages;

		protected override void SynchroniseSingleBill(ForwardingShipment shipment)
		{
			var duplicatePackTypes = shipment.OuterPackLines.Cast<PackLine>().Select(x => x.JL_F3_NKPackType + x.JL_JS_HouseBill + x.JL_Calc_ContainerNumber).Distinct().Count() != shipment.OuterPackLines.Count;
			if (duplicatePackTypes)
			{
				aggregatedPackages = new Dictionary<string, BasePackage>();
				foreach (PackLine packLine in shipment.OuterPackLines)
				{
					if (SyncChangesDetected)
					{
						return;
					}

					SynchroniseAggregatedPackLine(packLine, shipment);
				}
			}
			else
			{
				base.SynchroniseSingleBill(shipment);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void SynchroniseAggregatedPackLine(PackLine packLine, ForwardingShipment shipment)
		{
			var jobContainer = packLine.GetContainer(Declaration.RelevantConsol) as ForwardingContainer;
			var shipmentForPackLine = (IBillDetails)shipment;
			var houseBill = (ZString)shipmentForPackLine.BillNumberInfo.Value;
			var effectiveBill = GetEffectiveBill(houseBill, jobContainer);
			if (effectiveBill != null)
			{
				bool createNewPackingLine = false;
				var containerNum = jobContainer != null ? jobContainer.JC_ContainerNum : ZString.Empty;
				var aggregateKey = packLine.JL_F3_NKPackType + containerNum;
				BaseCusContainer container = Declaration.CusContainers.Find(containerNum);
				string packType = GetConvertedPackUQ(packLine.JL_F3_NKPackType);

				IPackingInformation packingInfo = null;
				if (aggregatedPackages.TryGetValue(aggregateKey, out BasePackage package))
				{
					packingInfo = package;
				}
				else
				{
					package = ((BaseDeclarationLevelPackageCollection<Package>)Declaration.PackingInformationCollection).GetMatchingElementForShipmentSynch((Bill)effectiveBill, container);
					if ((packingInfo = package) == null)
					{
						if (DetectEnabled)
						{
							SyncChangesDetected = true;
							return;
						}
						packingInfo = Declaration.PackingInformationCollection.AddNew();
					}

					aggregatedPackages.Add(aggregateKey, package ?? packingInfo as BasePackage);
					createNewPackingLine = true;
				}

				packingInfo.HouseBillContainer = new HouseBillContainer(effectiveBill, container);
				if (packingInfo.SupportMarksAndNumbers)
				{
					if (DetectEnabled)
					{
						if (packingInfo.MarksAndNumbers != packLine.JL_MarksAndNumbers)
						{
							SyncChangesDetected = true;
							return;
						}
					}
					else
					{
						packingInfo.MarksAndNumbers = packLine.JL_MarksAndNumbers;
					}
				}

				var newOneToOne = packingInfo as IOneToOnePackingInformation;
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
						if (createNewPackingLine)
						{
							newOneToOne.PackQty = packLine.JL_PackageCount;
							newOneToOne.PackType = GetConvertedPackUQ(packLine.JL_F3_NKPackType);
						}
						else
						{
							newOneToOne.PackQty += packLine.JL_PackageCount;
						}
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
			}
		}
	}
}
