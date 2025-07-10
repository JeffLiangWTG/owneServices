using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class DeclarationImportCartageType : DeclarationCartageType
	{
		public DeclarationImportCartageType(BaseJobDeclaration declaration) : base(declaration) { }

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("760e0bcb-7c05-4667-9566-a3cbf9c8fb69", "Import"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return DeclarationParent.DocsAndCartage.DeliveryCartageCoAddr; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return DeclarationParent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo; }
		}

		public override ZString CartageJobType
		{
			get
			{
				if (DeclarationParent.IsDomestic)
				{
					if (!DeclarationParent.ShouldDefaultFCLCartageCo || DeclarationParent.IsRoad)
					{
						return Constants.CartageJobType.NEW_DomesticLooseDelivery;
					}
					else
					{
						return Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
					}
				}
				else
				{
					if (DeclarationParent.IsAir)
					{
						return Constants.CartageJobType.NEW_AirImport;
					}
					else if (!DeclarationParent.ShouldDefaultFCLCartageCo || DeclarationParent.IsRoad)
					{
						return Constants.CartageJobType.NEW_LCLImport;
					}
					else
					{
						if (!GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CFS).IsEmpty)
						{
							return Constants.CartageJobType.NEW_FCLImportUnpack;
						}
						else
						{
							return Constants.CartageJobType.NEW_FCLImportToCNE;
						}
					}
				}
			}
		}

		protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
		{
			return Array.Empty<ZPropertyInfo>();
		}

		public override void CartageAdvised(BusinessObjectFactory factoryToSaveIn)
		{
			BaseJobDeclaration declarationInFactory = (BaseJobDeclaration)GetCartageParentInFactory(factoryToSaveIn);
			if (declarationInFactory != null)
			{
				declarationInFactory.DocsAndCartage.JP_DeliveryCartageAdvisedInfo.Value = ZDateTime.Now;
			}
		}

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			if (addressType == DocAddressType.LocalCartageImporter)
			{
				container.JC_ArrivalCartageComplete = timeOut;

				bool allContainersComplete = false;

				var latestCartageArrival = ZDateTime.Empty;
				foreach (BaseCusContainer cusContainer in DeclarationParent.CusContainers)
				{
					if (cusContainer.JobContainer.JC_ArrivalCartageComplete.IsEmpty)
					{
						allContainersComplete = false;
						break;
					}
					allContainersComplete = true;

					if (latestCartageArrival.IsEmpty || latestCartageArrival < cusContainer.JobContainer.JC_ArrivalCartageComplete)
					{
						latestCartageArrival = cusContainer.JobContainer.JC_ArrivalCartageComplete;
					}
				}

				if (allContainersComplete)
				{
					DeclarationParent.DocsAndCartage.JP_DeliveryCartageCompleted = latestCartageArrival;
				}
			}
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
			if (addressType == DocAddressType.LocalCartageImporter)
			{
				DeclarationParent.DocsAndCartage.JP_DeliveryCartageCompleted = timeOut;
			}
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return DeclarationParent.DocsAndCartage.JP_EstimatedDelivery; }
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
			DeclarationParent.DocsAndCartage.JP_DeliveryTruckWaitTime = ZDateTime.MinSmallDateTimeValue + demurrage;
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return DeclarationParent.DocsAndCartage.JP_DeliveryRequiredBy; }
		}

		public override ZString DropMode
		{
			get { return DeclarationParent.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
		}

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return new ZString[] { CartageDirection.Import, CartageDirection.Destination };
		}
	}
}
