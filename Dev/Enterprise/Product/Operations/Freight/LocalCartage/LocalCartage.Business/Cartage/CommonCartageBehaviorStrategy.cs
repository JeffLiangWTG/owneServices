using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageBehaviorStrategy
	{
		public CommonCartageBehaviorStrategy()
		{
		}

		public virtual CodeDescriptionPairList GetJobTypeList(CommonCartage cartage)
		{
			return cartage.BindToLists.NewCartageJobTypes;
		}

		protected ZQuery GetJobTypeListQuery(CommonCartage cartage)
		{
			ZQuery filter = new ZQuery(LocalCartageJobTypeSchema.E3_IsHidden, false);

			filter.AddToFilter(JoinCondition.And, LocalCartageJobTypeSchema.E3_JobType, SQLComparisonOperator.NotEqual, "");
			if (cartage.CartageInternalType != null && !cartage.CartageInternalType.CartageJobType.IsEmpty)
			{
				filter.AddToFilter(JoinCondition.And, LocalCartageJobTypeSchema.E3_JobType, SQLComparisonOperator.StartsWith, cartage.CartageInternalType.CartageJobType.SubstringSafe(0, 2));
			}

			return filter;
		}

		public virtual ZString DefaultPackageType(CommonCartage cartage)
		{
			return FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
		}

		public virtual void BeforeCartageTypeChange(CommonCartage cartage, ZString newJobType)
		{
			if (!newJobType.IsEmpty)
			{
				cartage.MakeAddressesPersistentButDeleteEmpty();
			}
			else
			{
				cartage.ResetMainAddressesForBinding(); // only remove empty addresses if another job type was chosen
			}
		}

		public virtual void AfterCartageTypeChange(CommonCartage cartage)
		{
			if (cartage.CartageType != null)
			{
				RefreshDocAddresses(cartage);
				RebuildLegs(cartage);
			}
		}

		protected virtual void RebuildLegs(CommonCartage cartage)
		{
		}

		public void RefreshDocAddresses(CommonCartage cartage)
		{
			if (cartage.CartageType != null)
			{
				cartage.RefreshAddresses();
			}
		}

		public JobDocAddress FindOrCreateMainDocAddress(CommonCartage cartage, int mainAddressNumber)
		{
			if (mainAddressNumber > CommonCartage.MaxNumberOfJobDocAddresses)
			{
				throw new NotSupportedException("Max Addressess:" + CommonCartage.MaxNumberOfJobDocAddresses);
			}

			var requirements = new List<JobDocAddressRequirement>();
			var cartageType = cartage.CartageType;
			if (cartageType != null)
			{
				var cartageAddressHelper = CommonCartageAddressHelper.ByJobType(cartageType);
				requirements = GetAddressRequirements(cartage, cartageAddressHelper);
			}
			else
			{
				var docAddressTypes = GetOrgTypesBasedOnLegs(cartage);
				foreach (var docAddress in docAddressTypes)
				{
					requirements.Add(((IDocAddresses)cartage).GetDocAddressRequirement(docAddress));
				}
			}

			return SetupAddressBasedOnRequirements(cartage, mainAddressNumber, requirements);
		}

		IEnumerable<DocAddressType> GetOrgTypesBasedOnLegs(CommonCartage cartage)
		{
			return GetJourneyAddresses(cartage).Select(a => a.DocAddressType);
		}

		public IEnumerable<JobDocAddress> GetJourneyAddresses(CommonCartage cartage, bool onlyAddContainerised = false)
		{
			var moves = cartage.BookedMovesCollection.OrderBy(m => m.EW_DisplayOrder).ToArray();
			var containerMove = moves.FirstOrDefault(m => m.IsContainerised);
			var looseMove = moves.FirstOrDefault(m => m.IsLoose);
			var legs = new List<CommonCartageLeg>();
			var containerLegs = containerMove != null ? containerMove.CartageLegs.OrderBy(l => l.JU_DisplayOrder) : (IEnumerable<CommonCartageLeg>)Array.Empty<CommonCartageLeg>();
			var looseLegs = !onlyAddContainerised && looseMove != null ? looseMove.CartageLegs.OrderBy(l => l.JU_DisplayOrder) : (IEnumerable<CommonCartageLeg>)Array.Empty<CommonCartageLeg>();

			if (cartage.IsExportOrOrigin)
			{
				legs.AddRange(looseLegs);
				legs.AddRange(containerLegs);
			}
			else
			{
				legs.AddRange(containerLegs);
				legs.AddRange(looseLegs);
			}

			var docAddressesInSequence = new List<JobDocAddress>();

			foreach (var leg in legs)
			{
				AddOrgType(leg.PickupFromDocAddress, docAddressesInSequence);
				AddOrgType(leg.WaitPointDocAddress, docAddressesInSequence);
				AddOrgType(leg.DeliverToDocAddress, docAddressesInSequence);
			}

			return docAddressesInSequence;
		}

		void AddOrgType(JobDocAddress docAddress, List<JobDocAddress> docAddressesInSequence)
		{
			if (docAddress != null && !docAddressesInSequence.Contains(docAddress))
			{
				docAddressesInSequence.Add(docAddress);
			}
		}

		List<JobDocAddressRequirement> GetAddressRequirements(CommonCartage cartage, CommonCartageAddressHelper cartageAddressHelper)
		{
			var requirements = new List<JobDocAddressRequirement>();
			for (int i = 0; i < cartageAddressHelper.AddressOrgTypes.Count; i++)
			{
				DocAddressType docAddressType = CommonCartageAddressHelper.GetCartageDocAddressTypeFromOrgType(cartageAddressHelper.AddressOrgTypes[i].E5_OrgType);
				if (docAddressType != DocAddressType.None)
				{
					requirements.Add(((IDocAddresses)cartage).GetDocAddressRequirement(docAddressType));
				}
			}

			return requirements;
		}

		JobDocAddress SetupAddressBasedOnRequirements(CommonCartage cartage, int mainAddressNumber, List<JobDocAddressRequirement> requirements)
		{
			JobDocAddress address = null;

			if (requirements.Count >= mainAddressNumber)
			{
				int sameRequirements = 0;
				var requirement = requirements[mainAddressNumber - 1];

				if (requirement != null)
				{
					for (int i = 0; i < requirements.Count; i++)
					{
						if (mainAddressNumber - 1 == i)
						{
							break;
						}

						if (requirements[i].DefaultDocAddressType == requirement.DefaultDocAddressType)
						{
							sameRequirements++;
						}
					}

					address = cartage.DocAddresses.FindOrCreateWithRequirement(requirement, sameRequirements);
					address.MakePersistentEvenIfEmpty();
					if (!address.IsInDatabase)
					{
						SetupAddress(cartage, address, CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(address.DocAddressType));
					}
				}
			}

			return address;
		}

		protected virtual void SetupAddress(CommonCartage cartage, JobDocAddress docAddress, ZString orgType)
		{
		}

		public virtual void AfterTransportModeChange(CommonCartage cartage)
		{
		}

		public void EstimatedPickupChanged(CommonCartage cartage, ZDateTime originalValue)
		{
			if (cartage != null)
			{
				foreach (CommonCartageLeg leg in cartage.CartageLegs.OrderBy(l => l.JU_DisplayOrder))
				{
					if (!leg.JU_IsEmptyContainer && (leg.JU_PlannedPickupTime.IsEmpty || leg.JU_PlannedPickupTime == originalValue))
					{
						leg.JU_PlannedPickupTime = leg.BookedCtgMove.Cartage.JJ_EstimatedPickup;
					}
				}
			}
		}

		public void EstimatedDeliveryChanged(CommonCartage cartage, ZDateTime originalValue)
		{
			if (cartage != null && cartage.CartageType != null)
			{
				foreach (CommonCartageLeg leg in cartage.CartageLegs)
				{
					if ((leg.JU_EstimatedDeliveryTime.IsEmpty || leg.JU_EstimatedDeliveryTime == originalValue)
						&& leg.IsFirstLeg)
					{
						leg.JU_EstimatedDeliveryTime = leg.BookedCtgMove.Cartage.JJ_EstimatedDelivery;
					}
				}
			}
		}

		public virtual void AfterSailingPKChange(CommonCartage cartage)
		{
		}

		public virtual ZString GetPortOfLoading(CommonCartage cartage)
		{
			return "";
		}

		public virtual void SetPortOfLoading(CommonCartage cartage, ZString value)
		{
		}

		public virtual ZString GetPortOfDischarge(CommonCartage cartage)
		{
			return "";
		}

		public virtual void SetPortOfDischarge(CommonCartage cartage, ZString value)
		{
		}

		public virtual ZString GetVessel(CommonCartage cartage)
		{
			return "";
		}

		public virtual void SetVessel(CommonCartage cartage, ZString value)
		{
		}

		public virtual ZString GetVoyageFlight(CommonCartage cartage)
		{
			return "";
		}

		public virtual void SetVoyageFlight(CommonCartage cartage, ZString value)
		{
		}

		public virtual ZDateTime GetE_DEP(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual void SetE_DEP(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual ZDateTime GetE_ARV(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual void SetE_ARV(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual ZDateTime GetA_DEP(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetA_ARV(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual void SetFCLCutOff(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetLCLCutOff(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetFCLReceivalCommences(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetLCLReceivalCommences(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetFCLAvailabilityDate(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetLCLAvailabilityDate(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetFCLStorageDate(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual void SetLCLStorageDate(CommonCartage cartage, ZDateTime value)
		{
		}

		public virtual ZDateTime GetFCLReceivalCommences(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetFCLCutOff(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetLCLReceivalCommences(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetLCLCutOff(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetFCLAvailabilityDate(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetLCLAvailabilityDate(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetFCLStorageDate(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}

		public virtual ZDateTime GetLCLStorageDate(CommonCartage cartage)
		{
			return ZDateTime.Empty;
		}
	}
}
