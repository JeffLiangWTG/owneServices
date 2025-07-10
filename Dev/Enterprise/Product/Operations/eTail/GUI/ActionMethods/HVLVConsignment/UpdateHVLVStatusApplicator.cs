using System;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI
{
	public class UpdateHVLVStatusApplicator : OperationalActionMethodApplicator
	{
		#region Schema

		public static class Schema
		{
			public const string StatusCode = "StatusCode";
			public const int StatusCodeMaxLength = 3;
		}

		#endregion

		public UpdateHVLVStatusApplicator()
			: base("UpdateHVLVStatusApplicator")
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("161ee966-719c-44bd-9ffe-656a17ccf5e8", "No shipments selected."));
			}
			else
			{
				foreach (ForwardingShipment shipment in targets)
				{
					var shipmentInfo = Res.GetString("b9fbebee-4203-4144-a69d-2b28407b7c3e", "Processing shipment {0}:", shipment.JS_UniqueConsignRef);
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, shipmentInfo);

					if (shipment.JS_ShipmentType == ShipmentTypes.HighVolumeLowValue)
					{
						foreach (var consignment in shipment.HVLVConsignments)
						{
							var consignmentInfo = "\t" + Res.GetString("71af72f6-e57d-4a9d-8502-599c07e92b30", "Processing Consignment with Consignment ID {0}", consignment.HVC_ConsignmentId.IsEmpty ? (ZString)(NoResString)"(Empty)" : consignment.HVC_ConsignmentId);
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, consignmentInfo);

							if (consignment.HVC_Status != HVLVConsignmentStatus.Codes.Delivered && consignment.HVC_Status != StatusCode)
							{
								consignment.HVC_Status = StatusCode;

								var successInfo = "\t" + Res.GetString("6849abe9-7b34-4110-8565-14bb8a50c6db", "Processed!");
								log.NotifyFormat(OperationalActionLogErrorLevel.Informational, successInfo);
							}
							else
							{
								var processinfo = "\t\t" + (consignment.HVC_Status == HVLVConsignmentStatus.Codes.Delivered
									? Res.GetString("b60672f4-3494-4a3f-b5b3-23d6281c0819", "Skipped, because consignment status is Delivered.")
									: Res.GetString("3c8a547b-0aed-4b84-a408-ca3ae98ea22c", "Skipped, because consignment status is the same."));
								log.NotifyFormat(OperationalActionLogErrorLevel.Informational, processinfo);
							}
						}

						var shipmentFinishedInfo = Res.GetString("2a0c5834-06af-4b37-aca6-00a1858199e1", "Shipment Processed") + "\r\n";
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, shipmentFinishedInfo);
					}
					else
					{
						var processinfo = Res.GetString("0cb8d5d3-62ff-4d83-a5e0-004e191020c6", "Shipment is not of HVL type, skipped.") + "\r\n";
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, processinfo);
					}
				}
			}
		}

		#region StatusCode

		[MaxLength(Schema.StatusCodeMaxLength)]
		[List("StatusCodeList")]
		public virtual ZString StatusCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return statusCode;
			}
			set
			{
				CheckMaximumLength(StatusCodeInfo, value);
				SetNonPersistentPropertyValue(StatusCodeInfo, ref statusCode, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateStatusCode();
				}
			}
		}

		public virtual ZPropertyInfo StatusCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.StatusCode);
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZString statusCode;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public UpdateHVLVStatusValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}

		protected virtual UpdateHVLVStatusValidation GetNewValidation()
		{
			return new UpdateHVLVStatusValidation(this);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList StatusCodeList => HVLVConsignmentLookups.GetAllHVLVConsignmentStatuses();

		#endregion
	}

	public class UpdateHVLVStatusValidation : ZValidation
	{
		public UpdateHVLVStatusValidation(UpdateHVLVStatusApplicator parent)
			: base(parent)
		{
			this.parent = parent;
			ZValidationInternals = this;
			ParentListInternals = parent;
		}

		public void Add(UpdateHVLVStatusValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(UpdateHVLVStatusValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}

		protected void ValidateAllCore()
		{
			ValidateStatusCode();
		}

		#endregion

		#region StatusCode

		public void ValidateStatusCode()
		{
			ZValidationInternals.Validate(Parent.StatusCodeInfo, new RunValidationInvoker(StatusCodeValidationInvoker));
		}

		void StatusCodeValidationInvoker()
		{
			CheckStatusCodeIsWesternEuropean();
			CheckStatusCode();
		}

		protected void CheckStatusCodeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.StatusCodeInfo);
		}

		protected void CheckStatusCode()
		{
			MandatoryValidation.CheckEntered(Parent.StatusCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.StatusCodeInfo, Parent.StatusCodeList);
		}

		#endregion

		public override Type AutoValidationType
		{
			get
			{
				return typeof(UpdateHVLVStatusValidation);
			}
		}

		public UpdateHVLVStatusApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly UpdateHVLVStatusApplicator parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals ZValidationInternals;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal ParentListInternals;
	}
}
