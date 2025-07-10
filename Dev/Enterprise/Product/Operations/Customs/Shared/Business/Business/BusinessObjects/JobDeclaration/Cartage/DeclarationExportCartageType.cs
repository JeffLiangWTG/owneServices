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
	public class DeclarationExportCartageType : DeclarationCartageType
	{
		public DeclarationExportCartageType(BaseJobDeclaration declaration) : base(declaration) { }

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("b968dca1-d996-4839-b16d-1615bc4f16e3", "Export"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return DeclarationParent.DocsAndCartage.PickupCartageCoAddr; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return DeclarationParent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo; }
		}

		public override ZString CartageJobType
		{
			get
			{
				if (DeclarationParent.IsDomestic)
				{
					if (!DeclarationParent.ShouldDefaultFCLCartageCo || DeclarationParent.IsRoad)
					{
						return Constants.CartageJobType.NEW_DomesticLoosePickup;
					}
					else
					{
						return Constants.CartageJobType.NEW_DomesticContainerizedPickup;
					}
				}
				else
				{
					if (DeclarationParent.IsAir)
					{
						return Constants.CartageJobType.NEW_AirExport;
					}
					else if (!DeclarationParent.ShouldDefaultFCLCartageCo || DeclarationParent.IsRoad)
					{
						return Constants.CartageJobType.NEW_LCLExport;
					}
					else
					{
						if (!GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CFS).IsEmpty)
						{
							return Constants.CartageJobType.NEW_FCLExportPack;
						}
						else
						{
							return Constants.CartageJobType.NEW_FCLExportToSHP;
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
				declarationInFactory.DocsAndCartage.JP_PickupCartageAdvisedInfo.Value = ZDateTime.Now;
			}
		}

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			if (addressType == DocAddressType.LocalCartageExporter)
			{
				container.JC_DepartureCartageComplete = timeOut;
			}

			bool allContainersComplete = true;

			foreach (BaseCusContainer cusContainer in DeclarationParent.CusContainers)
			{
				if (cusContainer.JobContainer.JC_DepartureCartageComplete.IsEmpty)
				{
					allContainersComplete = false;
					break;
				}
			}

			if (allContainersComplete)
			{
				DeclarationParent.DocsAndCartage.JP_PickupCartageCompleted = timeOut;
			}
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
			if (addressType == DocAddressType.LocalCartageExporter)
			{
				DeclarationParent.DocsAndCartage.JP_PickupCartageCompleted = timeOut;
			}
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
			DeclarationParent.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.MinSmallDateTimeValue + demurrage;
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return DeclarationParent.DocsAndCartage.JP_EstimatedPickup; }
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return DeclarationParent.DocsAndCartage.JP_PickupRequiredBy; }
		}

		public override ZString DropMode
		{
			get { return DeclarationParent.DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return new ZString[] { CartageDirection.Export, CartageDirection.Origin };
		}
	}
}
