using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/orderManager/workflow/exceptions")]
	[GlowTicketAuthentication]
	public sealed class WorkflowExceptionsController : OrderManagerController
	{
		public WorkflowExceptionsController(IGlowContactSecurityService securityService) : base(securityService)
		{
		}

		public WorkflowExceptionsController() : this(new GlowContactSecurityService())
		{
		}

		[Route("create")]
		[HttpPost]
		public IHttpActionResult Create([FromBody] CreateWorkflowExceptionArgs args)
		{
			var parentPK = args.ParentPK;
			var parentTableCode = args.ParentTableCode;
			var exceptionTypePK = args.ExceptionTypePK;
			var exceptionTimeUTC = args.ExceptionTimeUTC;
			var exceptionPublished = args.ExceptionPublished;
			var exceptionCausePK = args.ExceptionCausePK;
			var exceptionStaffPK = args.ExceptionStaffPK;
			var exceptionStaffGroupPK = args.ExceptionStaffGroupPK;
			var exceptionDescription = args.ExceptionDescription;
			var exceptionNotes = args.ExceptionNotes;

			if (exceptionTypePK == Guid.Empty)
			{
				return BadRequest(Res.GetString("022818dc-f056-4cc4-8658-0bbe5cf58429", "Please enter an Exception Type"));
			}

			if (exceptionDescription?.Length > 50)
			{
				return BadRequest(Res.GetString("d3d89f32-e6d3-4501-847f-8d3b3bf88aef", "Exception description should not exceed max length 50"));
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewBusinessObjectFactory();

				try
				{
					var parent = factory.Load(parentTableCode, parentPK);

					if (parent is IWorkflowProvider workflowProvider)
					{
						if (!CanView(parent, User, securityService, out var isContact))
						{
							throw new OrderManagerValidationError(SecurityError);
						}

						var exception = workflowProvider.WorkflowItems.Exceptions.AddNew();
						exception.ExceptionTypeCode = factory.LoadTop1<ProcessWorkflowExceptionType>(new ZQuery(ProcessWorkflowExceptionTypeSchema.PK, exceptionTypePK))?.WET_Code ?? string.Empty;
						exception.ExceptionCausePK = exceptionCausePK;
						exception.P9_GS_NKAssignedStaffMember = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, exceptionStaffPK))?.GS_Code ?? string.Empty;
						exception.P9_GG_AssignedGroupCode = factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, exceptionStaffGroupPK))?.GG_Code ?? string.Empty;
						exception.P9_IsPublished = isContact || exceptionPublished;

						if (!string.IsNullOrEmpty(exceptionTimeUTC))
						{
							if (DateTime.TryParse(exceptionTimeUTC, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedExceptionTime))
							{
								exception.P9_ActualDateUtc = parsedExceptionTime;
							}
							else
							{
								throw new OrderManagerValidationError((NoResString)"Exception Time is not valid.");
							}
						}

						if (!string.IsNullOrEmpty(exceptionNotes))
						{
							exception.P9_NotesAsString = exceptionNotes;
						}

						if (!string.IsNullOrEmpty(exceptionDescription))
						{
							exception.P9_Description = exceptionDescription;
						}

						exception.Validation.ValidateAll();

						ThrowIfValidationErrors(exception);
						factory.Save();
					}
					else
					{
						throw new OrderManagerValidationError((NoResString)"Unable to find valid parent or it does not support workflow.");
					}
				}
				catch (OrderManagerValidationError validationError)
				{
					return BadRequest(validationError.Message);
				}
				catch (Exception exception)
				{
					ErrorReporter.ReportOnce($"{nameof(Create)} request URI: {Request.RequestUri}", exception);
					return BadRequest(SomethingWentWrong);
				}
			}

			return Ok();
		}

		[Route("description")]
		[HttpGet]
		public IHttpActionResult Description(Guid parentPK, string parentTableCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewBusinessObjectFactory();

				try
				{
					var parent = factory.Load(parentTableCode, parentPK);

					if (parent is IWorkflowProvider)
					{
						return Ok(GetDescriptionAsOrderManagerOrganisations(parent));
					}
					else
					{
						throw new OrderManagerValidationError((NoResString)"Unable to find valid parent or it does not support workflow.");
					}
				}
				catch (OrderManagerValidationError validationError)
				{
					return BadRequest(validationError.Message);
				}
				catch (Exception exception)
				{
					ErrorReporter.ReportOnce($"{nameof(Description)} request URI: {Request.RequestUri}", exception);
					return BadRequest(SomethingWentWrong);
				}
			}
		}

		[Route("{exceptionPK}/markActioned")]
		[HttpPut]
		public IHttpActionResult MarkActioned(ZGuid exceptionPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewBusinessObjectFactory();
				var exception = factory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, exceptionPK));
				if (exception == null)
				{
					return BadRequest(Res.GetString("4d1425d6-30d3-4c51-8135-9ad770a0a7f3", "Unable to find exception."));
				}

				try
				{
					exception.IsExceptionActioned = true;
					exception.Validation.ValidateAll();

					ThrowIfValidationErrors(exception);
					factory.Save();

					return Ok();
				}
				catch (OrderManagerValidationError validationError)
				{
					return BadRequest(validationError.Message);
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce($"{nameof(MarkActioned)} request URI: {Request.RequestUri}", ex);
					return BadRequest(SomethingWentWrong);
				}
			}
		}

		string GetDescriptionAsOrderManagerOrganisations(BusinessObject parent)
		{
			var orgPrefixes = new (string org, string prefix)?[]
			{
				GetControllingCustomerWithFallback(parent),
				(GetSupplier(parent), supplierPrefix),
				(GetCFS(parent), cfsPrefix),
			};

			return string.Join(", ", orgPrefixes
				.Where(orgPrefix => orgPrefix.HasValue && !string.IsNullOrEmpty(orgPrefix.Value.org))
				.Select(orgPrefix => $"{orgPrefix.Value.prefix}:{orgPrefix.Value.org}"));
		}

		(string org, string prefix)? GetControllingCustomerWithFallback<T>(T parent)
		{
			if (parent is Order order)
			{
				var controllingCustomer = order.ControllingCustomerDocAddress?.Organisation?.OH_Code;

				if (string.IsNullOrEmpty(controllingCustomer))
				{
					return (order.Buyer.OH_Code, buyerPrefix);
				}

				return (controllingCustomer, controllingCustomerPrefix);
			}
			else if (parent is JobSupplierBooking supplierBooking)
			{
				var controllingCustomer = supplierBooking.ControllingCustomerAddress?.Organisation?.OH_Code;

				if (string.IsNullOrEmpty(controllingCustomer)
					&& supplierBooking.JSB_LoadMode != SupplierBookingLoadModeList.Codes.CFS
					&& supplierBooking.SupplierBookingLines.Count > 0)
				{
					return (supplierBooking.SupplierBookingLines[0].OrderLine.Order.Buyer.OH_Code, buyerPrefix);
				}

				return (controllingCustomer, controllingCustomerPrefix);
			}
			else if (parent is CommonContainerLoadList containerLoadList)
			{
				if (containerLoadList.CLH_LoadMode == CommonContainerLoadListLoadModeList.Codes.CFS)
				{
					var controllingCustomer = containerLoadList.ControllingCustomerAddress?.Organisation?.OH_Code;

					if (string.IsNullOrEmpty(controllingCustomer)
						&& containerLoadList.CLH_LoadMode != CommonContainerLoadListLoadModeList.Codes.CFS
						&& containerLoadList.LoadListLines.Count > 0)
					{
						return (containerLoadList.LoadListLines[0].SupplierBookingLine.OrderLine.Order.Buyer.OH_Code, buyerPrefix);
					}
					return (controllingCustomer, controllingCustomerPrefix);
				}
				else if (containerLoadList.CLH_LoadMode == CommonContainerLoadListLoadModeList.Codes.CY)
				{
					return GetControllingCustomerWithFallback(containerLoadList.Booking);
				}
			}

			return null;
		}

		string GetSupplier<T>(T parent)
		{
			if (parent is Order order)
			{
				return order.Supplier?.OH_Code;
			}
			else if (parent is JobSupplierBooking supplierBooking)
			{
				return supplierBooking.SupplierAddress?.Organisation?.OH_Code;
			}
			else if (parent is CYContainerLoadList containerLoadList)
			{
				return GetSupplier(containerLoadList.Booking);
			}

			return null;
		}

		string GetCFS<T>(T parent)
		{
			if (parent is JobSupplierBooking supplierBooking && supplierBooking.JSB_LoadMode == SupplierBookingLoadModeList.Codes.CFS)
			{
				return supplierBooking.CFSAddress?.Header?.OH_Code;
			}
			else if (parent is CFSContainerLoadList containerLoadList)
			{
				return containerLoadList.CFSAddress?.Header?.OH_Code;
			}

			return null;
		}

		void ThrowIfValidationErrors(ProcessTask exception)
		{
			if (HasValidationErrors(exception, out var validationErrorMessages))
			{
				var pattern = (NoResString)@"(Error - P9_([-\w]+): )"; // RegEx pattern.
				throw new OrderManagerValidationError(string.Join("\n", validationErrorMessages.Select(errorMessage => Regex.Replace(errorMessage, pattern, "")).Distinct()));
			}
		}

		public readonly string[] ValidSupplierBookingActionNames = new string[] {
			"TransportMode",
			"PortOfLoading",
			"BookedQuantity",
			"CargoDate",
			"PartialReceived",
			"DispatchShortageForLooseCargo",
			"FinalizeBookingWithOutstandingLines",
		};

		public readonly string[] ValidContainerLoadListActionNames = new string[] {
			"PackedQtyExceedsBooked",
			"BookedQuantityAllocatedPartially",
			"UnusedContainersAllocatedToBooking",
			"MinimumRequirementOfSeal",
		};

		public readonly string[] ValidEquipmentActionNames = new string[] {
			"MinimumVolume",
			"MaximumVolume",
			(NoResString)"Overweight",
		};

		static BusinessObjectFactory GetNewBusinessObjectFactory() => new BusinessObjectFactory() { NameForDebugging = "WorkflowExceptionsController Factory" };
		readonly string controllingCustomerPrefix = "CC";
		readonly string buyerPrefix = "BUY";
		readonly string supplierPrefix = "SUP";
		readonly string cfsPrefix = "CFS";
	}
}
