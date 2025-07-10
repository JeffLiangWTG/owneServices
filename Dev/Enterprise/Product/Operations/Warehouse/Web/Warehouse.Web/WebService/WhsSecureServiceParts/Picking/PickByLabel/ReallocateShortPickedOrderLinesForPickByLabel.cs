using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ReallocateShortPickedOrderLinesForPickByLabel

		[WebMethod(Description = "Reallocate short picked Order Lines for Pick By Label")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickByLabelWebServiceResponse ReallocateShortPickedOrderLinesForPickByLabel(Guid packagePK, Guid[] shortedOrderLinePKs, decimal packableItemQuantity)
		{
			return HandleWebServiceRequest<WhsPickByLabelWebServiceResponse>(response => ReallocateShortPickedOrderLinesForPickByLabel(response, packagePK, shortedOrderLinePKs, packableItemQuantity));
		}

		void ReallocateShortPickedOrderLinesForPickByLabel(WhsPickByLabelWebServiceResponse response, Guid packagePK, Guid[] shortedOrderLinePKs, decimal packableItemQuantity)
		{
			var package = Factory.Load<PkgPackage>(packagePK);
			if (package == null)
			{
				response.LogBusinessValidationError(Res.GetString("1a64051a-db4e-45e5-beeb-4fbe7c04255c", "Package was not found."));
			}
			else
			{
				if (package.GetPickLines().Any(l => l.IsPickedFromPutawayLocation))
				{
					response.LogBusinessValidationError(Res.GetString("bb934c94-bc2d-4d77-9412-5d2d1c7600a4", "Cannot partially reallocate Pick By Label."));
				}
				else
				{
					var packageJob = package.PackageJob;
					var order = (WhsOrder)packageJob.ParentJob;
					var pick = order.Pick;
					var newPickLines = Reallocate(response, shortedOrderLinePKs, pick, Guid.Empty);

					var newPickLinesPK = newPickLines.Select(pl => pl.PK).ToHashSet();
					var shortedOrderLinePKsSet = shortedOrderLinePKs.ToHashSet();
					var shortedOrderLines = order.Lines.Cast<WhsOrderLine>().Where(ol => shortedOrderLinePKsSet.Contains(ol.PK.ToGuid())).ToArray();
					var newPickLinesForThisOrder = shortedOrderLines.SelectMany(l => l.PickLines).Where(l => newPickLinesPK.Contains(l.PK)).ToArray();

					if (newPickLinesForThisOrder.Length > 0)
					{
						if (newPickLinesForThisOrder.Any(l => l.WZ_F3_NKAllocatedPackType != package.KP_F3_NKPackType))
						{
							response.LogBusinessValidationError(Res.GetString("581f0873-3f6d-4879-b8fc-138a3c71690a", "Inventory with the same UOM Type not available. No Stock could be reallocated for the shorted Products."));
						}
						else if (newPickLinesForThisOrder.Sum(l => l.WZ_Units) != packableItemQuantity)
						{
							response.LogBusinessValidationError(Res.GetString("6c3c90ca-3fcd-40f7-aede-9a5779fc2ffa", "No enough stock to reallocate."));
						}
						else
						{
							var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
							var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

							var newPackage = packageJob.Packages.AddNew(package.KP_F3_NKPackType);
							AllocatePackageLabelsHelper.FillPackagePropertiesFromProduct(newPackage, shortedOrderLines[0].SupplierPart, package.KP_F3_NKPackType);
							((ISupportPackageIDGeneration)newPackage).ShouldGenerateIDOnSaving = true;

							AddFetchHintsForPackPickLines(pick);
							PackPickLinesIntoPackage(shortedOrderLines, newPickLinesForThisOrder, newPackage);
							var packedItemsDivots = newPackage.PackedItemDivots.Select(d => d.KI_ParentID).ToHashSet();
							if (newPickLinesForThisOrder.Any(l => !packedItemsDivots.Contains(l.PK)))
							{
								response.LogBusinessValidationError(Res.GetString("d6bb22d7-fdc7-4a51-82fc-58b786cbc562", "Pick Line not Packed."));
							}

							package.Delete();

							var pickByLabelJob = AddPackageToListOfPickByLabel(response, warehouse, rfUser, newPackage, pick.DockDoorPK, putawayOnly: false);
							if (pickByLabelJob != null)
							{
								WebServiceHelper.AssignPickLinesToUserAndSave(response, Factory, newPickLinesForThisOrder, rfUser);

								Array.Sort(newPickLinesForThisOrder, new SortPickLinesForPickingSlip());
								response.Job = new PickByLabelInfo(warehouse, pickByLabelJob, newPackage, newPickLinesForThisOrder, hasStartedPicking: true);
								response.Job.IsUsingCarrierLabelIntegration = order.CarrierBookingAgent != null;
							}
						}
					}
				}
			}
		}

		void AddFetchHintsForPackPickLines(WhsPick pick)
		{
			var pickLines = pick.GetAllPickLines();
			foreach (var line in pickLines)
			{
				Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, line.WZ_WE_TransactionLine));
			}
		}

		#endregion
	}
}
