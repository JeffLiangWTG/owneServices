using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region PickByLabel

		#region GetPickByLabelActiveJob

		[WebMethod(Description = "Gets the PickByLabel active job")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickByLabelActiveJobWebServiceResponse GetPickByLabelActiveJob()
		{
			return HandleWebServiceRequest<WhsPickByLabelActiveJobWebServiceResponse>(response => GetPickByLabelActiveJob(response));
		}

		void GetPickByLabelActiveJob(WhsPickByLabelActiveJobWebServiceResponse response)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var pickByLabelJob = WhsPickByLabelHelper.GetWhsPickByLabelJob(Factory, warehouse.PK, rfUser.GS_Code);
			if (pickByLabelJob != null && pickByLabelJob.Labels.Count > 0)
			{
				var splitJob = WhsPickByLabelHelper.CloseAndSplitNotPutawayLabelsIfNecessary(pickByLabelJob);
				if (splitJob != null || !pickByLabelJob.WTK_FinalisedDate.IsEmpty)
				{
					if (pickByLabelJob.HasErrors())
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = pickByLabelJob.GetErrors().ToUniqueMessageListString();
					}
					else if (splitJob != null && splitJob.HasErrors())
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = splitJob.GetErrors().ToUniqueMessageListString();
					}
					else
					{
						var concurrencyErrorMessage = Res.GetString("62a5d1dd-06a7-48b3-9c30-9f9082f61953", "Another user has modified the Job. Please restart the operation and try again.");
						WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
						pickByLabelJob = splitJob;
					}
				}
			}

			response.SetWhsPickByLabelActiveJobWebServiceResponse(pickByLabelJob);
		}

		#endregion

		#region GetPickByLabelPackage

		[WebMethod(Description = "Gets the PickByLabel Package for picking")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickByLabelWebServiceResponse GetPickByLabelPackage(string id, bool shouldOverrideOtherUsers)
		{
			return HandleWebServiceRequest<WhsPickByLabelWebServiceResponse>(response => LoadPickByLabelPackage(response, id, shouldOverrideOtherUsers));
		}

		void LoadPickByLabelPackage(WhsPickByLabelWebServiceResponse response, string id, bool shouldOverrideOtherUsers)
		{
			if (string.IsNullOrEmpty(id))
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("c1b36e94-43fc-4539-85fc-73cdfbd1a900", "Please provide a Label ID.");
			}
			else
			{
				var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

				if (warehouse != null && rfUser != null)
				{
					LoadPickByLabelPackageCore(response, id, shouldOverrideOtherUsers, warehouse, rfUser);
				}
				else
				{
					response.Error = ErrorTypes.LoginFailed;
					response.ErrorMessage = Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service.");
				}
			}
		}

		void LoadPickByLabelPackageCore(WhsPickByLabelWebServiceResponse response, string id, bool shouldOverrideOtherUsers, WhsWarehouse warehouse, GlbStaff rfUser)
		{
			var finder = WhsPickByLabelHelper.PickByLabelPackageFinder(Factory, id, warehouse.PK, rfUser);
			var package = PickByLabelHelper.FindPickByLabelPackage(response, finder, id);
			if (response.ErrorMessage.IsNullOrEmpty())
			{
				var allPickLines = package.PackedItemDivots.Select(d => d.PackedItem).OfType<WhsPickLine>();
				var notPickedPickLines = allPickLines.Where(pl => !pl.IsPickedFromPutawayLocation).ToArray();

				WebServiceHelper.CheckPickability(response, rfUser, notPickedPickLines, shouldOverrideOtherUsers);
				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					if (notPickedPickLines.Any(pl => package.PackType?.F3_UOMType != pl.AllocatedPackType?.F3_UOMType))
					{
						response.LogBusinessValidationError(Res.GetString("526a51d6-11b9-4e3a-b605-8766e5748183", "Package has invalid Packed Items and cannot be Picked By Label."));
					}
					else if (notPickedPickLines.Any(pl => pl.IsPickByBOMKitPickLine()))
					{
						response.LogBusinessValidationError(Res.GetString("487b32ef-ea60-4b5a-ac5f-b47a4f1a86fe", "Package has BOM kits to assemble and cannot be Picked by Label."));
					}
					else if (notPickedPickLines.Select(pl => pl.InventoryLine.WE_WL).Distinct().IsCountMoreThan(1))
					{
						response.LogBusinessValidationError(Res.GetString("3311d5dd-91d7-490f-9a8d-1a7f445d14c1", "Package has lines from multiple Locations and cannot be Picked By Label."));
					}
					else
					{
						var putawayOnly = notPickedPickLines.Length == 0;
						var pickByLabelJob = AddPackageToListOfPickByLabel(response, warehouse, rfUser, package, finder.PickDockDoorLocationPK, putawayOnly);
						if (pickByLabelJob != null)
						{
							var errorMessage = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>().GeneratePickDockDoorAssignment(package.PK, DockDoorAssignmentLinkType.PickByLabel, pickByLabelJob.PK, Factory);
							if (string.IsNullOrEmpty(errorMessage))
							{
								if (putawayOnly)
								{
									var concurrencyErrorMessage = Res.GetString("801d26ee-c1ea-4f62-9b1a-635a4fe10be3", "Another user has modified the job. Please restart the operation and try again.");
									WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);

									response.Job = new PickByLabelInfo(warehouse, pickByLabelJob, package);
								}
								else
								{
									response.Job = new PickByLabelInfo(warehouse, pickByLabelJob, package, notPickedPickLines.OrderBy(pl => pl, new SortPickLinesForPickingSlip()), allPickLines.Count() > notPickedPickLines.Length);
									WebServiceHelper.AssignPickLinesToUserAndSave(response, Factory, notPickedPickLines, rfUser);
								}
							}
							else
							{
								response.LogBusinessValidationError(errorMessage);
							}
						}
					}
				}
			}
		}

		#region AddPackgeToListOfPickByLabel

		WhsPickByLabelJob AddPackageToListOfPickByLabel(WhsPickByLabelWebServiceResponse response, WhsWarehouse warehouse, GlbStaff rfUser, Packing.Business.PkgPackage package, ZGuid ddlPK, bool putawayOnly)
		{
			WhsPickByLabelJob pickByLabelJob;

			var existingLabel = Factory.LoadTop1<WhsPickByLabelLabel>(new ZQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, package.PK));

			if (existingLabel != null)
			{
				pickByLabelJob = existingLabel.PickByLabelJob;
				var isCurrentJob = pickByLabelJob.WTK_FinalisedDate.IsEmpty;
				if (!isCurrentJob)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("df9ae168-c200-45e5-8a72-a7b0bff3edf9", "Package has been scanned before, cannot scan again.");
				}
			}
			else
			{
				pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, warehouse.PK, rfUser.GS_Code, ddlPK);
			}

			AddPackageToJobAndValidate(response, rfUser, pickByLabelJob, package, putawayOnly);

			return string.IsNullOrEmpty(response.ErrorMessage) ? pickByLabelJob : null;
		}

		void AddPackageToJobAndValidate(WhsPickByLabelWebServiceResponse response, GlbStaff rfUser, WhsPickByLabelJob pickByLabelJob, Packing.Business.PkgPackage package, bool putawayOnly)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				SetPackingStationForPackageToAdd(package, pickByLabelJob);
				var label = WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package.PK);

				label.RunPreSaveValidation();
				if (label.HasErrors)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = label.NotificationsIncludingChildren.ToUniqueMessageListString(); // has error include children
				}
			}
		}

		static WhsPick GetPickFromPackage(Packing.Business.PkgPackage package)
		{
			return ((WhsOrder)package.PackageJob?.ParentJob).Pick;
		}

		void SetPackingStationForPackageToAdd(Packing.Business.PkgPackage package, WhsPickByLabelJob pickByLabelJob)
		{
			Argument.NotNull(pickByLabelJob, nameof(pickByLabelJob));
			if (pickByLabelJob.Labels.Count > 0)
			{
				var pickForPackageToAdd = GetPickFromPackage(package);
				if (!pickForPackageToAdd.WP_WL_PackingStation.IsValid)
				{
					var labels = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>();
					Factory.AddFetchHint(PkgPackageSchema.Instance, new ZQuery(PkgPackageSchema.PK, labels.Select(l => l.WTL_KP_Package)));
					var firstPickForPickByLabelJob = GetPickFromPackage(labels.First().Package);
					if (firstPickForPickByLabelJob.WP_WL_PackingStation.IsValid)
					{
						pickForPackageToAdd.WP_WL_PackingStation = firstPickForPickByLabelJob.WP_WL_PackingStation;
					}
				}
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
