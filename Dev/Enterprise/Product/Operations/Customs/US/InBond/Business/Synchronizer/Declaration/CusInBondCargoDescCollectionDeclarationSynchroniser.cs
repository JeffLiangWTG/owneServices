using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondCargoDescCollectionDeclarationSynchroniser : BusinessObjectCollectionSynchroniser
	{
		internal CusInBondCargoDescCollectionDeclarationSynchroniser(US.Business.Bill source, CusInBondContainer destination, ZGuid containerPK)
			: base(source, destination)
		{
			this.containerPK = containerPK;
			this.headerSynchroniser = (CusInBondHeaderDeclarationSynchronizer)destination.Header.Synchroniser;
		}

		readonly ZGuid containerPK;

		protected new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		protected new US.Business.Bill Source
		{
			get { return (US.Business.Bill)base.Source; }
		}

		readonly CusInBondHeaderDeclarationSynchronizer headerSynchroniser;

		JobDeclaration DeclarationSource
		{
			get { return Source.Declaration; }
		}

		#region Synchronise

		protected override void ForceSynchroniseCore(bool isDeleted)
		{
			base.ForceSynchroniseCore(isDeleted);
			if (!isDeleted && Destination.ShouldSynchronise)
			{
				DeleteOrAddCusInBondCargoDescs();
			}
		}

		void DeleteOrAddCusInBondCargoDescs()
		{
			var packages = new List<Package>(GetSourcePackagesToSynchronise());
			if (packages.Count > 0)
			{
				var commodities = new List<CusInBondCargoDesc>(Destination.Commodities);
				while (commodities.Count > 0)
				{
					var commodity = commodities[0];
					commodities.Remove(commodity);
					var synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescDeclarationSynchroniser>(commodity);
					if (commodity.IsDeleted || commodity.IsDeleting)
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
							if (packages.Contains(synchroniser.Source))
							{
								packages.Remove(synchroniser.Source);
								synchroniser.Synchronise();
								continue;
							}
						}
						else
						{
							commodity = FindMatchingPackageAndAddSynchroniser(packages, commodities, commodity);
						}

						if (commodity != null)
						{
							if (synchroniser != null && DeclarationSource.Packages.Contains(synchroniser.Source))
							{
								headerSynchroniser.AddToOphantInBondCargoDesc(synchroniser);
								ElementSynchronisers.Remove(synchroniser);
							}
							else
							{
								commodity.Delete();
							}
						}
					}
				}

				foreach (var package in packages)
				{
					var synchroniser = headerSynchroniser.GetOphantInBondCargoDescMatching(package);
					if (synchroniser == null)
					{
						var commodity = Destination.Commodities.AddNew();
						synchroniser = new CusInBondCargoDescDeclarationSynchroniser(commodity, package);
					}
					else if (!synchroniser.Destination.IsDeleted && !synchroniser.Destination.IsDeleting)
					{
						Destination.Commodities.Add(synchroniser.Destination);
						headerSynchroniser.RemoveFromOphantInBondCargoDesc(package);
					}
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
				}
			}
			else
			{
				foreach (CusInBondCargoDesc commodity in Destination.Commodities.ToArray())
				{
					var synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescDeclarationSynchroniser>(commodity);
					if (synchroniser != null && DeclarationSource.Packages.Contains(synchroniser.Source))
					{
						headerSynchroniser.AddToOphantInBondCargoDesc(synchroniser);
						ElementSynchronisers.Remove(synchroniser);
					}
					else
					{
						commodity.Delete();
					}
				}
			}
		}

		CusInBondCargoDesc FindMatchingPackageAndAddSynchroniser(List<Package> packages, List<CusInBondCargoDesc> commodities, CusInBondCargoDesc commodity)
		{
			Package existingPackage = null;
			var alreadyProcessedPackages = new List<Package>();
			while ((existingPackage = packages.FirstOrDefault(package => !alreadyProcessedPackages.Contains(package) && package.CW_PackType == commodity.BY_ManifestUnitCode)) != null)
			{
				alreadyProcessedPackages.Add(existingPackage);
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondCargoDescDeclarationSynchroniser>(existingPackage);
				if (synchroniser != null)
				{
					packages.Remove(existingPackage);
					commodities.Remove(synchroniser.Destination);
					synchroniser.Synchronise();
					break;
				}
				else
				{
					synchroniser = new CusInBondCargoDescDeclarationSynchroniser(commodity, existingPackage);
					ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(synchroniser, IsEnabled, DetectEnabled);
					synchroniser.Synchronise();
					packages.Remove(existingPackage);
					commodity = null;
					break;
				}
			}
			return commodity;
		}

		protected override void OnEnabledChanged()
		{
			if (Destination.IsDeleted)
			{
				foreach (var synchroniser in ElementSynchronisers.OfType<CusInBondCargoDescDeclarationSynchroniser>().ToArray())
				{
					headerSynchroniser.AddToOphantInBondCargoDesc(synchroniser);
					ElementSynchronisers.Remove(synchroniser);
				}
			}
			base.OnEnabledChanged();
		}

		internal void MarkCommoditySynchroniserAsOphant()
		{
			var declarationSource = DeclarationSource;
			foreach (var synchroniser in ElementSynchronisers.OfType<CusInBondCargoDescDeclarationSynchroniser>().ToArray())
			{
				if (declarationSource.Packages.Contains(synchroniser.Source))
				{
					headerSynchroniser.AddToOphantInBondCargoDesc(synchroniser);
					ElementSynchronisers.Remove(synchroniser);
				}
			}
		}

		protected override void HookElementSynchronisers()
		{
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				HookSynchronisationForExistingCommodities();
			}
		}

		IEnumerable<Package> GetSourcePackagesToSynchronise()
		{
			return Source.AllPackages.OfType<Package>().Where(x =>
				{
					var packingGroup = x.PackingGroup;
					return packingGroup != null && packingGroup.CR_CO_Container == containerPK;
				});
		}

		void HookSynchronisationForExistingCommodities()
		{
			var commodities = new List<CusInBondCargoDesc>(Destination.Commodities);
			foreach (var sourcePackage in GetSourcePackagesToSynchronise())
			{
				var synchroniser = ElementSynchronisers.FindMatchingSource<CusInBondCargoDescDeclarationSynchroniser>(sourcePackage);
				if (synchroniser != null)
				{
					synchroniser.SetEnabled(IsEnabled, DetectEnabled);
					commodities.Remove(synchroniser.Destination);
				}
				else
				{
					CusInBondCargoDesc commodity = null;
					while ((commodity = commodities.FirstOrDefault(x => !x.IsDeleted && x.BY_ManifestUnitCode == sourcePackage.CW_PackType)) != null)
					{
						synchroniser = ElementSynchronisers.FindMatchingDestination<CusInBondCargoDescDeclarationSynchroniser>(commodity);
						if (synchroniser == null)
						{
							ElementSynchronisers.AddIfNotExistsOtherwiseSetEnabled(new CusInBondCargoDescDeclarationSynchroniser(commodity, sourcePackage), IsEnabled, DetectEnabled);
							commodities.Remove(commodity);
							break;
						}
						else
						{
							synchroniser.SetEnabled(IsEnabled, DetectEnabled);
							commodities.Remove(commodity);
						}
					}
				}
			}
		}

		#endregion

		#region Hook/UnHook Events

		protected override IEnumerable<BusinessObjectCollection> GetCollectionsToHookCountChangedEvent()
		{
			yield return Source.AllPackages;
		}

		#endregion
	}
}
