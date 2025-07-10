using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using SupplierBookingLoadMode = Enterprise.Core.Constants.SupplierBookingLoadMode;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/orderManager/containers")]
	[GlowTicketAuthentication]
	public sealed class OrderManagerContainerController : OrderManagerController
	{
		public OrderManagerContainerController(IGlowContactSecurityService securityService) : base(securityService)
		{
		}

		public OrderManagerContainerController() : this(new GlowContactSecurityService())
		{
		}

		[Route("changeContainerNumber")]
		[HttpPost]
		public IHttpActionResult ChangeContainerNumber([FromBody] ChangeContainerNumberArgs args)
		{
			var containerID = args.ContainerID;
			var containerJobID = args.ContainerJobID;
			var containerLoadListID = args.ContainerLoadListID;

			if (string.IsNullOrEmpty(containerJobID))
			{
				return BadRequest(InvalidParameters);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				CommonContainerLoadList containerLoadList = null;
				var factory = GetNewBusinessObjectFactory();
				var containerQuery = new ZQuery(JobContainerSchema.JC_ContainerJobID, containerJobID);
				var container = factory.LoadTop1<CommonContainer>(containerQuery);

				if (container == null)
				{
					return BadRequest(ContainerNotFound);
				}

				if (string.IsNullOrEmpty(containerLoadListID))
				{
					if (container.SupplierBooking is JobSupplierBooking booking)
					{
						if (!(booking.JSB_Status == SupplierBookingStatusList.Codes.PLN || booking.JSB_Status == SupplierBookingStatusList.Codes.CNV))
						{
							return BadRequest(SupplierBookingIsNotRelevantStatus);
						}
					}
					else
					{
						return BadRequest(ContainerNotRelatedToSupplierBooking);
					}
				}
				else
				{
					containerLoadList = factory.LoadFromNaturalKey<CommonContainerLoadList>(ContainerLoadListHeaderSchema.CLH_LoadListId, containerLoadListID);

					if (containerLoadList == null)
					{
						return BadRequest(ContainerLoadListNotFound);
					}
					else if (!CanEdit(containerLoadList, User, securityService))
					{
						return BadRequest(SecurityError);
					}

					if (IsContainerLoadPlan(containerLoadList))
					{
						if (containerLoadList.CLH_Status == CommonContainerLoadListStatusList.Codes.CAN || containerLoadList.CLH_Status == CommonContainerLoadListStatusList.Codes.CNV)
						{
							return BadRequest(LoadListHeaderIsNotRelevantStatus);
						}
						else if (container.JC_CLH_LoadListPlan != containerLoadList.PK)
						{
							return BadRequest(ContainerNotRelatedToLoadPlan);
						}
					}
					else
					{
						if (!(containerLoadList.CLH_Status == CommonContainerLoadListStatusList.Codes.INC || containerLoadList.CLH_Status == CommonContainerLoadListStatusList.Codes.REJ))
						{
							return BadRequest(LoadListHeaderIsNotRelevantStatus);
						}
						else if (container.JC_JSB_SupplierBooking != containerLoadList.CLH_JSB_Booking)
						{
							return BadRequest(ContainerNotRelatedToLoadList);
						}
					}
				}

				try
				{
					if (container.JC_ContainerCount == 1)
					{
						container.JC_ContainerNum = containerID;
						ThrowIfValidationErrors(container);
					}
					else if (container.JC_ContainerCount > 1 && container.Clone() is CommonContainer newContainer)
					{
						container.Consol.Containers.Add(newContainer);
						container.JC_ContainerCount = 1;
						container.JC_ContainerNum = containerID;
						newContainer.JC_ContainerCount--;
						newContainer.JC_JSB_SupplierBooking = container.JC_JSB_SupplierBooking;

						if (IsContainerLoadPlan(containerLoadList) && container.JC_CLH_LoadListPlan.Equals(containerLoadList.PK))
						{
							newContainer.JC_CLH_LoadListPlan = container.JC_CLH_LoadListPlan;
						}

						ThrowIfValidationErrors(container);
						ThrowIfValidationErrors(newContainer);
					}

					factory.Save();
				}
				catch (OrderManagerValidationError validationError)
				{
					return BadRequest(validationError.Message);
				}
				catch (Exception exception)
				{
					ErrorReporter.ReportOnce($"{nameof(ChangeContainerNumber)} request URI: {Request.RequestUri}", exception);
					return BadRequest(SomethingWentWrong);
				}
			}

			return Ok();
		}

		[Route("split")]
		[HttpPost]
		public IHttpActionResult Split([FromBody] SplitContainerArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return BadRequest(SecurityError);
			}

			var containerJobID = args.ContainerJobID;
			var splitContainerCount = (ZShort)args.ContainerCount;
			var containerLoadListID = args.ContainerLoadListID;

			if (string.IsNullOrEmpty(containerJobID))
			{
				return BadRequest(InvalidParameters);
			}
			else if (0 > splitContainerCount)
			{
				return BadRequest(NonPositiveOrZeroContainerCount);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewBusinessObjectFactory();
				var containerQuery = new ZQuery(JobContainerSchema.JC_ContainerJobID, containerJobID);
				var container = factory.LoadTop1<CommonContainer>(containerQuery);
				var containerLoadList = factory.LoadFromNaturalKey<CommonContainerLoadList>(ContainerLoadListHeaderSchema.CLH_LoadListId, containerLoadListID);

				if (container == null)
				{
					return BadRequest(ContainerNotFound);
				}
				else if (!string.IsNullOrEmpty(containerLoadListID) && containerLoadList == null)
				{
					return BadRequest(ContainerLoadListNotFound);
				}
				else
				{
					try
					{
						if (splitContainerCount == 0)
						{
							Deallocate(container, containerLoadList);
							factory.Save();
						}
						else if (splitContainerCount > container.JC_ContainerCount)
						{
							AllocateIfContainersAvailable(container, splitContainerCount);
							factory.Save();
						}
						else if (
							string.IsNullOrEmpty(container.JC_ContainerNum) &&
							container.JC_ContainerCount > 1 &&
							container.JC_ContainerCount != splitContainerCount)
						{
							Split(container, splitContainerCount, containerLoadList);
							factory.Save();
						}
						else
						{
							return BadRequest(UnableToSplit);
						}
					}
					catch (OrderManagerValidationError validationError)
					{
						return BadRequest(validationError.Message);
					}
					catch (Exception exception)
					{
						ErrorReporter.ReportOnce($"{nameof(Split)} request URI: {Request.RequestUri}", exception);
						return BadRequest(SomethingWentWrong);
					}
				}

				return Ok();
			}
		}

		[Route("getDensityFactor")]
		[HttpGet]
		public IHttpActionResult GetDensityFactor(
			ZString containerJobId,
			ZString loadedWeightUnit,
			ZString loadedVolumeUnit,
			Decimal loadedWeight = 0,
			Decimal loadedVolume = 0
			)
		{
			if (string.IsNullOrEmpty(containerJobId))
			{
				return BadRequest(InvalidParameters);
			}
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewBusinessObjectFactory();
				var containerQuery = new ZQuery(JobContainerSchema.JC_ContainerJobID, containerJobId);
				var container = factory.LoadTop1<CommonContainer>(containerQuery);
				if (container == null)
				{
					return BadRequest(ContainerNotFound);
				}
				var calculatedVolumeWeight = container.GetCalculatedVolumeWeight(loadedWeightUnit, loadedVolumeUnit, loadedWeight, loadedVolume);
				var result = new DensityFactorResponse();
				if (loadedWeight <= 0 || loadedVolume <= 0 || calculatedVolumeWeight <= 0)
				{
					result.dense = 0m;
					result.denseIndex = -1;
					return Json(result);
				}
				result.dense = container.IsContainerChargeableByWeight()
					? calculatedVolumeWeight / loadedWeight
					: loadedVolume / calculatedVolumeWeight;
				result.denseIndex = DensityValuesList.Where(value => value < result.dense).Count() - 1;
				return Json(result);
			}
		}

		static List<ZDecimal> DensityValuesList => new List<ZDecimal> { 0.0000m, 0.2505m, 0.4200m, 0.5800m, 0.7500m, 0.9200m, 1.0855m, 1.2525m, 1.4195m, 1.5865m, 1.7535m, 1.9190m };

		void AllocateIfContainersAvailable(CommonContainer container, ZShort newValue)
		{
			if (!container.JC_ContainerNum.IsEmpty)
			{
				throw new OrderManagerValidationError(ContainerNumberAlreadyAssigned);
			}

			var amountToAdd = newValue - container.JC_ContainerCount;
			var matchingUnallocatedContainers = GetMatchingUnallocatedContainers(container);
			var availableUnallocatedContainers = matchingUnallocatedContainers.Sum(x => x.JC_ContainerCount);

			if (availableUnallocatedContainers >= amountToAdd)
			{
				container.JC_ContainerCount = newValue;

				foreach (var unallocatedContainer in matchingUnallocatedContainers.ToList())
				{
					if (amountToAdd >= unallocatedContainer.JC_ContainerCount)
					{
						amountToAdd -= unallocatedContainer.JC_ContainerCount;
						unallocatedContainer.Delete();
					}
					else if (unallocatedContainer.JC_ContainerCount > amountToAdd)
					{
						unallocatedContainer.JC_ContainerCount -= amountToAdd;

						ThrowIfValidationErrors(unallocatedContainer);
						break;
					}
				}
			}
			else
			{
				throw new OrderManagerValidationError(UnableToSplitOverAllocation(availableUnallocatedContainers, container.JC_ContainerCount));
			}
		}

		void Deallocate(CommonContainer container, CommonContainerLoadList containerLoadList)
		{
			if (container.JC_ContainerNum.IsEmpty && GetMatchingUnallocatedContainerIfExists(container) is CommonContainer existingContainer)
			{
				existingContainer.JC_ContainerCount += container.JC_ContainerCount;
				container.Delete();

				ThrowIfValidationErrors(existingContainer);
			}
			else
			{
				if (containerLoadList != null && containerLoadList.CLH_LoadMode == SupplierBookingLoadMode.ContainerFreightStation)
				{
					container.JC_CLH_LoadListPlan = ZGuid.Empty;
				}
				else
				{
					container.JC_JSB_SupplierBooking = ZGuid.Empty;
				}
			}
		}

		void Split(CommonContainer container, ZShort splitContainerCount, CommonContainerLoadList containerLoadList)
		{
			if (IsContainerLoadPlan(containerLoadList)
				&& containerLoadList.CLH_Status != CommonContainerLoadListStatusList.Codes.INC
				&& containerLoadList.CLH_Status != CommonContainerLoadListStatusList.Codes.REJ)
			{
				throw new OrderManagerValidationError(NotAllowSplit);
			}

			var amountToSplit = container.JC_ContainerCount - splitContainerCount;

			if (GetMatchingUnallocatedContainerIfExists(container) is CommonContainer existingContainer)
			{
				existingContainer.JC_ContainerCount += amountToSplit;

				ThrowIfValidationErrors(existingContainer);
			}
			else if (container.Clone() is CommonContainer newContainer)
			{
				container.Consol.Containers.Add(newContainer);
				newContainer.JC_ContainerCount = amountToSplit;
				newContainer.JC_JSB_SupplierBooking = ZGuid.Empty;

				ThrowIfValidationErrors(newContainer);
			}

			container.JC_ContainerCount = splitContainerCount;
		}

		void ThrowIfValidationErrors(CommonContainer container)
		{
			if (HasValidationErrors(container, out var validationErrorMessages))
			{
				var forbiddenErrorMessages = new List<string>();
				validationErrorMessages.ForEach(errorMessage =>
				{
					if (errorMessage.Contains((NoResString)"Duplicate Container Number is entered.")) // Not a code smell.
					{
						forbiddenErrorMessages.Add(DuplicateContainerNumber);
					}
					else if (!errorMessage.Contains((NoResString)"This container row is already allocated to an active Supplier Booking") // Not a code smell.
						&& !errorMessage.Contains((NoResString)"Only approved Supplier Bookings can be linked to containers")) // Not a code smell.
					{
						var pattern = (NoResString)@"(Error - JC_([-\w]+): )"; // RegEx pattern.
						forbiddenErrorMessages.Add(Regex.Replace(errorMessage, pattern, ""));
					}
				});

				if (forbiddenErrorMessages.Count > 0)
				{
					throw new OrderManagerValidationError(string.Join("\n", forbiddenErrorMessages));
				}
			}
		}

		static bool IsContainerLoadPlan(CommonContainerLoadList containerLoadList)
		{
			return containerLoadList != null && containerLoadList.CLH_LoadMode == CommonContainerLoadListLoadModeList.Codes.CFS;
		}

		CommonContainer GetMatchingUnallocatedContainerIfExists(CommonContainer container)
			=> container
				.Consol?
				.Containers
				.OfType<CommonContainer>()
				.FirstOrDefault(otherContainer => IsUnallocatedAndMatchesRefContainer(container, otherContainer));

		IEnumerable<CommonContainer> GetMatchingUnallocatedContainers(CommonContainer container)
			=> container
				.Consol?
				.Containers
				.OfType<CommonContainer>()
				.Where(otherContainer => IsUnallocatedAndMatchesRefContainer(container, otherContainer))
				.OrderBy(otherContainer => otherContainer.JC_ContainerCount);

		bool IsUnallocatedAndMatchesRefContainer(CommonContainer container, CommonContainer otherContainer) =>
			otherContainer.PK != container.PK &&
			otherContainer.JC_RC == container.JC_RC &&
			otherContainer.JC_JSB_SupplierBooking.IsEmpty &&
			otherContainer.JC_CLH_LoadListPlan.IsEmpty &&
			string.IsNullOrEmpty(otherContainer.JC_ContainerNum);

		static string ContainerLoadListNotFound => Res.GetString("83c3e9fb-873a-409d-aa0d-7dde0767e483", "A Container Load List with the details provided could not be found.");
		static string InvalidParameters => Res.GetString("31780545-3610-48c0-9ede-5aab2d706e8d", "Please provide valid Container details.");
		static string ContainerNotRelatedToLoadList => Res.GetString("243D4194-18FD-479C-A1E4-00C455280391", "The Container provided is not related to the relevant Supplier Booking or Container Load List.");
		static string ContainerNotRelatedToSupplierBooking => Res.GetString("38d2ddbe-40ec-49dd-978e-52c32c366cb1", "The Container provided is not related to a Supplier Booking.");
		static string ContainerNotRelatedToLoadPlan => Res.GetString("de8bf50b-a27e-4b4f-96c4-cec285f31777", "The Container provided is not related to the relevant Container Load Plan.");
		static string ContainerNotFound => Res.GetString("41323b4b-196f-416d-a3fc-1884198ba4ed", "A Container with the details provided could not be found.");
		static string ContainerNumberAlreadyAssigned => Res.GetString("e8d6c3bf-abbd-47b7-8c07-b4bff10bc93d", "The Container provided already has a Container Number assigned.");
		static string DuplicateContainerNumber => Res.GetString("159a8566-9e57-4db8-8bf2-fd552b2b08e4", "This container number is already in use and cannot be entered here.");
		static string NonPositiveOrZeroContainerCount => Res.GetString("3ad92153-3f5c-4f11-9be4-4c9345533297", "Unable to split a container with zero or a negative value.");
		static string UnableToSplit => Res.GetString("1aa9c7e0-8291-4a1b-9b06-983e44238b40", "Container already has a container number assigned, a container count of one or not enough containers to split.");
		static string SupplierBookingIsNotRelevantStatus => Res.GetString("d2884058-051d-4b25-865d-6846ebfab760", "The Supplier Booking related to the Container is in an un-editable state, you cannot perform this action at this time.");
		static string LoadListHeaderIsNotRelevantStatus => Res.GetString("74d22b06-8c7c-41b6-9202-97058a64cc98", "The Container Load List/Plan is in an un-editable state, you cannot perform this action at this time.");
		static string NotAllowSplit => Res.GetString("4cce142d-0692-4c4c-b4ea-d5865d5ac809", "Reducing count or splitting container is only allowed if there are no lines linked to the container.");

		static string UnableToSplitOverAllocation(int availableContainers, int currentlyAllocatedContainers)
		{
			if (availableContainers > 0)
			{
				return Res.GetString(
					"5c2b6cae-12c4-4dac-95b9-fd1d291f7d86",
					"There are only {0} unallocated containers available on the selected consol. Please enter an allocation between zero and {1}.",
					availableContainers,
					availableContainers + currentlyAllocatedContainers
				);
			}
			else
			{
				return Res.GetString("8b7642bb-4a66-4562-babe-0ef5ad4ead0e", "There are zero unallocated containers available on the selected consol.");
			}
		}

		static BusinessObjectFactory GetNewBusinessObjectFactory() => new BusinessObjectFactory() { NameForDebugging = "OrderManagerContainerController Factory" };
	}

	public sealed class DensityFactorResponse
	{
		public decimal dense { get; set; }

		public int denseIndex { get; set; }
	}
}
