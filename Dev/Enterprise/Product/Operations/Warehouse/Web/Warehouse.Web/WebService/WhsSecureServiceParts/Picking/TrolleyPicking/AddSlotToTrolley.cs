using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region AddSlotToTrolley

		#region AddSlotToTrolleyUsingToteID

		[WebMethod(Description = "Adds Slot to a Trolley.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageWebServiceResponse AddSlotToTrolleyUsingToteID(Guid trolleyJobPK, string packageID, short slotNumber, SearchFilterCriteriaInfo criteria, string orderID = "")
		{
			return HandleWebServiceRequest<PackageWebServiceResponse>(r => AddSlotToTrolleyUsingToteIDCore(r, trolleyJobPK, packageID, slotNumber, orderID, criteria));
		}

		void AddSlotToTrolleyUsingToteIDCore(PackageWebServiceResponse r, Guid trolleyJobPK, string packageID, short slotNumber, string orderID, SearchFilterCriteriaInfo criteria)
		{
			AddSlotToTrolleyCore(r, trolleyJobPK, (response, packID) => GetOrCreateSavedPackedTotes(response, trolleyJobPK, packID, orderID, criteria), packageID, slotNumber, TrolleyPickingType.Tote);
		}

		#endregion

		#region AddSlotToTrolleyUsingPackageID

		[WebMethod(Description = "Adds Slot to a Trolley.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageWebServiceResponse AddSlotToTrolleyUsingPackageID(Guid trolleyJobPK, string packageID, short slotNumber)
		{
			return HandleWebServiceRequest<PackageWebServiceResponse>(r => AddSlotToTrolleyCore(r, trolleyJobPK, GetPackagesFromID, packageID, slotNumber, TrolleyPickingType.Carton));
		}

		IEnumerable<PkgPackage> GetPackagesFromID(PackageWebServiceResponse response, string packageID)
		{
			var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

			var query = GetDbQueryForAvailablePackages(whs, packageID.Trim().ToUpper(CultureInfo.CurrentCulture));
			AddPackageJobFetchHintForQuery(Factory, query);

			var packages = Factory.Load<PkgPackage>(query);
			if (!packages.Any())
			{
				response.ErrorMessage = Res.GetString("a352a6ed-a84a-4510-b22c-7ee6148b44e5", "Package '{0}' was not found.", packageID);
			}

			return packages.Where(p => !p.GetIsTote());
		}

		static void AddPackageJobFetchHintForQuery(BusinessObjectFactory factory, ZQuery packageQuery)
		{
			// Necessary as customs added a type decider on PkgPackage
			var rowFactory = ((IBusinessObjectFactoryInternals)factory).RowFactory;
			var packageRows = rowFactory.Load(PkgPackageSchema.Constants.TableName, packageQuery);

			var packageJobPks = packageRows.Select(r => (Guid)r[PkgPackageSchema.Constants.KP_KJ_ParentPackageJob]).Distinct();
			factory.AddFetchHint(PkgPackageJobSchema.Instance, new ZQuery(PkgPackageJobSchema.PK, packageJobPks));
		}

		IEnumerable<PkgPackage> GetOrCreateSavedPackedTotes(PackageWebServiceResponse response, Guid trolleyJobPK, string packageID, string orderID, SearchFilterCriteriaInfo criteria)
		{
			PkgPackage tote = null;

			var trolleyJob = WebServiceHelper.GetTrolleyJobUsingTrolleyJobPK(Factory, response, trolleyJobPK);
			if (trolleyJob != null)
			{
				var firstOrderOnTrolley = trolleyJob.Slots.Select(s => (WhsOrder)s.Package?.PackageJob?.ParentJob).FirstOrDefault();
				if (firstOrderOnTrolley != null)
				{
					Factory.AddFetchHint(WhsPickSchema.Instance, new ZQuery(WhsPickSchema.PK, firstOrderOnTrolley.WD_WP));
					Factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.WD_WP, firstOrderOnTrolley.WD_WP));
				}
				var dockDoorLocation = firstOrderOnTrolley?.Pick?.DockDoorPK;
				var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				var linesPicker = new PickLinesLoader(Factory, warehouse, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName), criteria);
				var disallowPicksWithInventoriesAtDockDoor = WhsPickJobInfo.WarehouseHasPackingStationLocation(Factory, warehouse.PK);
				var order = linesPicker.GetOrder(dockDoorLocation, disallowPicksWithInventoriesAtDockDoor, orderID);

				if (order == null)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					var errorMsg = orderID.IsNullOrEmpty()
						? Res.GetString("121dc456-e36e-4f90-b423-5885396483ac", "No valid orders to assign.")
						: Res.GetString("43f8baf4-7484-4919-ba2a-2d680f4c0a59", "{0} is not a valid order to assign.", orderID);
					response.ErrorMessage = errorMsg;
				}
				else
				{
					tote = TryGetOrCreateTotePackageUsingID(order, packageID, response);

					if (tote != null)
					{
						PackUnpickedUnpackagedSplitCaseItems(order, tote);
						if (tote.PackedItemDivots.Count == 0)
						{
							response.ErrorMessage = Res.GetString("A9B3B7C4-5AB3-49C9-9F6A-CF49CCA79CFF", "The next matching Order {0} didn't have any Unassigned Unpackaged Split Case Items to pack.", orderID);
						}
					}
				}
			}

			return new[] { tote };
		}

		PkgPackage TryGetOrCreateTotePackageUsingID(WhsOrder order, string packageID, PackageWebServiceResponse response)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packableItems = packageJob.PackableItemParents.Cast<PackableItemParentWrapper>().SelectMany(w => w.PackableItemParent.PackableItems);
			Factory.AddFetchHint(PkgPackageItemDivotSchema.Instance, new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, packableItems.Select(i => i.PK)));

			var package = packageJob.Packages.FirstOrDefault(p => p.KP_PackageID == packageID);
			if (package == null)
			{
				package = CreateWarehouseTote(packageID, packageJob);
			}
			else
			{
				if (package.GetIsTote())
				{
					var existingSlot = Factory.LoadTop1<WhsPickTrolleySlot>(new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, package.PK));

					if (existingSlot != null)
					{
						response.ErrorMessage = Res.GetString("5d11286c-34f4-48ea-b754-cb86cbef3d88", "Tote '{0}' is already used on this order, select another Tote or first Pack the existing contents into a Package.", packageID);
						package = null;
					}
				}
				else
				{
					response.ErrorMessage = Res.GetString("7C0EA2F0-447D-44E1-B040-C60573F11C55", "A non Tote Package was found using Package ID '{0}' already.", packageID);
					package = null;
				}
			}

			return package;
		}

		static PkgPackage CreateWarehouseTote(string packageID, PkgPackageJob packageJob)
		{
			var package = packageJob.Packages.AddNew(Core.Constants.PkgUnit.Tote);
			package.KP_PackageID = packageID;
			package.KP_GoodsDescription = Res.GetString("WhsSecureService|ToteGoodsDescription", "Warehouse Tote");
			package.KP_PackageQty = 1;
			package.SetIsTote(true);
			return package;
		}

		void PackUnpickedUnpackagedSplitCaseItems(WhsOrder order, PkgPackage tote)
		{
			var packablePickLines = GetUnpickedUnpackedSplitCasePickLines(order).ToArray();
			if (packablePickLines.Length > 0)
			{
				ClearReleaseCapturedAttributesFromPicklines(packablePickLines);

				var releaseLines = order.Lines.Cast<WhsOrderLine>().SelectMany(l =>
				{
					l.ClearReleaseLines();
					return l.ReleaseLines.Cast<WhsReleaseLine>();
				});
				var releaseLinesByPackableItemKey = releaseLines.ToDictionary(r => r.KeyForPacking);
				var picklineKeysNotInDic = new List<GroupingKey>();
				foreach (var packableItem in packablePickLines.Cast<IPackableItem>())
				{
					if (releaseLinesByPackableItemKey.TryGetValue(packableItem.Key, out var releaseLine))
					{
						tote.Pack(packableItem, releaseLine);
					}
					else
					{
						picklineKeysNotInDic.Add(packableItem.Key);
					}
				}

				if (picklineKeysNotInDic.Count > 0)
				{
					throw new KeyNotFoundException("Release Line Keys :" + string.Join("/r/n", releaseLinesByPackableItemKey.Keys) + "/r/n/r/n Pick Line Keys Not In Release Line Dictionary :" + string.Join("/r/n", picklineKeysNotInDic));
				}
			}
		}

		IEnumerable<WhsPickLine> GetUnpickedUnpackedSplitCasePickLines(WhsOrder order)
		{
			var splitCasePackTypes = GetSplitCasePackTypes();
			var pickLines = order.Lines.Where(l => l.WE_WE_ParentDocketLine.IsEmpty).SelectMany(l => l.PickLines);
			return pickLines.Where(pl => pl.WZ_Units > 0 && splitCasePackTypes.Contains(pl.WZ_F3_NKAllocatedPackType) && pl.WZ_GS_NKAssignedTo.IsEmpty && !pl.IsPickedFromPutawayLocation && pl.IsUnpacked(Factory));
		}

		static void ClearReleaseCapturedAttributesFromPicklines(IEnumerable<WhsPickLine> pickLines)
		{
			pickLines.ForEach(pl => pl.ClearReleaseCapturedAttributes());
		}

		IEnumerable<ZString> GetSplitCasePackTypes()
		{
			return Factory.Load<RefPackType>(new ZQuery(RefPackTypeSchema.F3_UOMType, UOMPackTypesList.Codes.SplitCase)).Select(t => t.F3_Code);
		}

		#endregion

		#region AddSlotToTrolleyUsingPackagePK

		[WebMethod(Description = "Adds Slot to a Trolley.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageWebServiceResponse AddSlotToTrolleyUsingPackagePK(Guid trolleyJobPK, Guid packagePK, short slotNumber)
		{
			return HandleWebServiceRequest<PackageWebServiceResponse>(r => AddSlotToTrolleyCore(r, trolleyJobPK, GetPackagesFromPK, packagePK, slotNumber, TrolleyPickingType.Carton));
		}

		IEnumerable<PkgPackage> GetPackagesFromPK(PackageWebServiceResponse response, Guid packagePK)
		{
			var package = Factory.Load<PkgPackage>(new ZGuid(packagePK));
			if (package == null)
			{
				response.ErrorMessage = Res.GetString("a41e33f3-75e4-48ac-a218-3c4c26fd09b5", "Package was not found.");
			}

			return package == null ? Array.Empty<PkgPackage>() : new[] { package };
		}

		#endregion

		#region AddSlotToTrolleyCore

		void AddSlotToTrolleyCore<TIdentifier>(PackageWebServiceResponse response, Guid trolleyJobPK, Func<PackageWebServiceResponse, TIdentifier, IEnumerable<PkgPackage>> getPackages, TIdentifier packageIdentifier, short slotNumber, TrolleyPickingType type)
		{
			var trolleyJob = Factory.Load<WhsPickTrolleyJob>(trolleyJobPK);

			if (trolleyJob == null)
			{
				response.ErrorMessage = Res.GetString("010ad1b5-9b6a-4b0f-bc57-20c822a8173d", "Trolley job was not found. Please start building trolley again.");
			}
			else
			{
				if (trolleyJob.Slots.Count > 0)
				{
					var packagePKs = trolleyJob.Slots.Select(s => s.WTS_KP_Package).ToArray();
					Factory.AddFetchHint(GenAddOnColumnSchema.Instance, new ZQuery(GenAddOnColumnSchema.XA_ParentID, packagePKs));
					Factory.AddFetchHint(WhsPickTrolleySlotSchema.Instance, new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, packagePKs)); // For validation

					var packageQuery = new ZQuery(PkgPackageSchema.PK, packagePKs);
					Factory.AddFetchHint(PkgPackageSchema.Instance, packageQuery);

					AddPackageJobFetchHintForQuery(Factory, packageQuery);
				}

				if (trolleyJob.WTJ_Status != PickTrolleyStatus.Codes.Building)
				{
					response.ErrorMessage = Res.GetString("1128bc60-1927-44a7-b4da-849c0f089953", "This trolley job is in {0} state. No slots can be added to it.", trolleyJob.WTJ_Status);
				}
				else if (trolleyJob.PickingType != type && trolleyJob.PickingType != TrolleyPickingType.None)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					var wrongTrolley = type == TrolleyPickingType.Carton ? TrolleyPickingType.Tote : TrolleyPickingType.Carton;

					response.ErrorMessage = Res.GetString("62827862-3A7E-4E9F-81EF-1A025174AFE6", "Cannot add '{0}' to Trolley of type '{1}'", GetTrolleyPickingTypeNameForMessages(type), wrongTrolley.ToString());
				}
				else
				{
					var existingSlot = trolleyJob.Slots.FirstOrDefault(sl => sl.WTS_SlotNumber == slotNumber);
					if (existingSlot != null)
					{
						response.ErrorMessage = Res.GetString("800ff73b-e568-40de-b6ad-5dad1706a005", "Slot {0} on trolley '{1}' already filled with package '{2}'. Please choose another slot.", slotNumber, trolleyJob.Equipment.RQ_Registration, existingSlot.Package.KP_PackageID);
					}
					else
					{
						var packages = getPackages(response, packageIdentifier);
						AddSlotToTrolleyCore(response, slotNumber, trolleyJob, packages);
					}
				}
			}

			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}

		void AddSlotToTrolleyCore(PackageWebServiceResponse response, short slotNumber, WhsPickTrolleyJob trolleyJob, IEnumerable<PkgPackage> packages)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage) && packages != null && packages.Any())
			{
				var packageDivots = packages.SelectMany(p => p.PackedItemDivots);
				Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.PK, packageDivots.Select(pd => pd.KI_ParentID)));

				var validPackages =
					packages
					.Where(p =>
							p.PackedItemDivots
							.Select(d => d.PackedItem)
							.OfType<WhsPickLine>()
							.Any(pl => (pl.AllocatedPackType?.F3_UOMType ?? string.Empty) == UOMPackTypesList.Codes.SplitCase))
					.ToArray();

				if (validPackages == null || !validPackages.Any())
				{
					response.ErrorMessage = Res.GetString("6b37a4a7-882e-47e7-af68-eb84610f73d4", "Package scanned is not valid for Trolley Picking.");
				}
				else
				{
					var packagePksWithSlots = Factory.Load<WhsPickTrolleySlot>(new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, packages.Select(p => p.PK))).Select(s => s.WTS_KP_Package).ToHashSet();
					var nonAssignedPackages = packages.Where(p => !packagePksWithSlots.Contains(p.PK)).ToArray();

					if (nonAssignedPackages == null || !nonAssignedPackages.Any())
					{
						response.ErrorMessage = Res.GetString("16555a45-a0bb-4550-8b30-1d72064c754a", "Package '{0}' already assigned to another trolley.", validPackages[0].KP_PackageID);
					}
					else
					{
						var packageJobs = packages.DistinctBy(p => p.KP_KJ_ParentPackageJob).Select(p => p.PackageJob);
						Factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.PK, packageJobs.Select(p => p.KJ_ParentID)));

						IReadOnlyCollection<PkgPackage> nonAssignedPackagesForSameDockDoor = nonAssignedPackages;
						var hasDifferentPackingStation = false;
						if (trolleyJob.Slots.Count > 0)
						{
							Factory.AddFetchHint(WhsPickSchema.Instance, new ZQuery(WhsPickSchema.PK, packageJobs.Select(p => ((WhsOrder)p.ParentJob).WD_WP)));

							var firstPickOnTrolley = (trolleyJob.Slots.FirstOrDefault()?.Package.PackageJob.ParentJob as WhsOrder)?.Pick;
							var dockDoorForTrolley = firstPickOnTrolley?.DockDoorPK;
							var packingStationForTrolley = firstPickOnTrolley?.WP_WL_PackingStation ?? ZGuid.Empty;

							if (dockDoorForTrolley != null)
							{
								var sameDockDoorPackageList = new List<PkgPackage>();
								foreach (var nonAssignedPackage in nonAssignedPackages)
								{
									var pick = ((WhsOrder)nonAssignedPackage.PackageJob.ParentJob).Pick;
									if (pick.DockDoorPK == dockDoorForTrolley)
									{
										if (pick.WP_WL_PackingStation.IsValid && pick.WP_WL_PackingStation != packingStationForTrolley)
										{
											hasDifferentPackingStation = true;
										}
										else
										{
											pick.WP_WL_PackingStation = packingStationForTrolley;
											sameDockDoorPackageList.Add(nonAssignedPackage);
										}
									}
								}
								nonAssignedPackagesForSameDockDoor = sameDockDoorPackageList;
							}
						}

						if (nonAssignedPackagesForSameDockDoor.Count == 0)
						{
							response.ErrorMessage = hasDifferentPackingStation
								? Res.GetString("187b1be0-64fd-4a4b-b7ca-94ffb2cd9bbe", "Package '{0}' has a different Packing station from other Picks on the trolley.", nonAssignedPackages[0].KP_PackageID)
								: Res.GetString("6e02a7c1-bcdc-427a-9a34-79ca5c0de9a2", "Package '{0}' is assigned to another Dock Door Location.", nonAssignedPackages[0].KP_PackageID);
						}
						else if (nonAssignedPackagesForSameDockDoor.Count > 1)
						{
							response.PackageChoices = new PackageChoiceInfoCollection(nonAssignedPackagesForSameDockDoor.ToArray());
						}
						else
						{
							AssignTrolleySlot(response, slotNumber, trolleyJob, nonAssignedPackagesForSameDockDoor.First());
						}
					}
				}
			}
		}

		#endregion

		#region GetDbQueryForAvailablePackages

		static ZDBOnlyQuery GetDbQueryForAvailablePackages(WhsWarehouse whs, string packageID)
		{
			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, PickStatus.Codes.Finalised);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_CartoniseSplitCases, true);

			var ordersSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), PkgPackageJobSchema.KJ_ParentID);
			ordersSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, whs.PK);
			ordersSubQuery.AddSubQuery(pickSubQuery, JoinCondition.And);

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageJobSubQuery.AddSubQuery(ordersSubQuery, JoinCondition.And);

			var packageHeaderSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageHeaderSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, packageID);

			var availablePackages = new ZDBOnlyQuery(typeof(PkgPackage));
			availablePackages.AddSubQuery(packageHeaderSubQuery, JoinCondition.And);
			availablePackages.AddSubQuery(packageJobSubQuery, JoinCondition.And);

			return availablePackages;
		}

		#endregion

		#region AssignTrolleySlot

		void AssignTrolleySlot(PackageWebServiceResponse response, short slotNumber, WhsPickTrolleyJob trolleyJob, PkgPackage package)
		{
			var newSlot = trolleyJob.Slots.AddNew();
			newSlot.WTS_KP_Package = package.PK;
			newSlot.WTS_SlotNumber = slotNumber;
			trolleyJob.RunPreSaveValidation();

			if (trolleyJob.NotificationsIncludingChildren.HasErrors())
			{
				response.ErrorMessage = string.Join(System.Environment.NewLine, trolleyJob.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList());
			}
			else
			{
				var errorMessage = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>().GeneratePickDockDoorAssignment(package.PK, DockDoorAssignmentLinkType.Trolley, trolleyJob.PK, Factory);
				if (string.IsNullOrEmpty(errorMessage))
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => Res.GetString("9b2bd41d-6f64-4e1e-bd64-195eeab6f095", "While you have been working with this job another user has made changes. Please restart the operation and try again."));
				}
				else
				{
					response.ErrorMessage = errorMessage;
				}
			}
		}

		ZString GetTrolleyPickingTypeNameForMessages(TrolleyPickingType type)
		{
			switch (type)
			{
				case TrolleyPickingType.None:
				case TrolleyPickingType.Carton:
					return Res.GetString("C8E627AB-6EB0-4C07-AD74-F98F85497EA3", "Package");
				case TrolleyPickingType.Tote:
					return Res.GetString("DA236CFF-41D3-4742-83FD-583FA5CD97CE", "Tote");
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0} TrolleyPickingType does not have it's Name mapped.", type.ToString()));
			}
		}

		#endregion

		#endregion
	}
}
