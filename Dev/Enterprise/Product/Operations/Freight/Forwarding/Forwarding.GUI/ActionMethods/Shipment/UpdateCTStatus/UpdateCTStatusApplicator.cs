using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class UpdateCTStatusApplicator : OperationalActionMethodApplicator
	{
		#region Schema

		public static class Schema
		{
			public const string StatusCode = "StatusCode";
			public const int StatusCodeMaxLength = 5;
		}

		#endregion

		public UpdateCTStatusApplicator() : base("UpdateCTStatusApplicator")
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("b218eefe-b0ae-4ae8-9e82-731c636806c9", "No shipments selected."));
			}
			else
			{
				foreach (ForwardingShipment shipment in targets)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("b32a7e5e-e69d-4abc-9d3b-86acfc0c725d", "Processing shipment {0}:", shipment.JS_UniqueConsignRef));

					if (shipment.JS_CommunityTransitStatus != StatusCode)
					{
						shipment.JS_CommunityTransitStatus = StatusCode;

						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "\t" + Res.GetString("4d70cdfd-39ad-479c-a945-a29944c90557", "Shipment processed.") + System.Environment.NewLine);
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "\t" + Res.GetString("90d58a46-f269-4ad4-bb1d-a26762ae54ad", "Skipped, because CT Status is the same.") + System.Environment.NewLine);
					}
				}
			}
		}

		#region StatusCode

		[MaxLength(Schema.StatusCodeMaxLength)]
		[List("StatusCodeList")]
		public ZString StatusCode
		{
			get
			{
				return statusCode;
			}
			set
			{
				CheckMaximumLength(StatusCodeInfo, value);
				SetNonPersistentPropertyValue(StatusCodeInfo, ref statusCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStatusCode();
				}
			}
		}
		ZString statusCode;

		public ZPropertyInfo StatusCodeInfo => GetZPropertyInfo(Schema.StatusCode);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public UpdateCTStatusValidation Validation => GetNewValidation();

		UpdateCTStatusValidation GetNewValidation()
		{
			return new UpdateCTStatusValidation(this);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList StatusCodeList
		{
			get
			{
				if (statusCodeList == null)
				{
					statusCodeList = new CodeDescriptionPairList();
					statusCodeList.AddPair(ZString.Empty, ZString.Empty);
					statusCodeList.AddRange(new CommunityTransitStatusCodes(new BusinessObjectFactory()).GetList());
				}

				return statusCodeList;
			}
		}
		CodeDescriptionPairList statusCodeList;

		#endregion
	}

	public class UpdateCTStatusValidation : ZValidation
	{
		public UpdateCTStatusValidation(UpdateCTStatusApplicator parent)
			: base(parent)
		{
			this.parent = parent;
			this.ZValidationInternals = this;
			this.ParentListInternals = parent;
		}

		public void Add(UpdateCTStatusValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(UpdateCTStatusValidation validation)
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
			ZValidationInternals.Validate(Parent.StatusCodeInfo, new RunValidationInvoker(this.StatusCodeValidationInvoker));
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
			ListValidation.ErrorIfInvalidCode(Parent.StatusCodeInfo, Parent.StatusCodeList);
		}

		#endregion

		public override Type AutoValidationType
		{
			get
			{
				return typeof(UpdateCTStatusValidation);
			}
		}

		public UpdateCTStatusApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}
		readonly UpdateCTStatusApplicator parent;

		readonly IValidationInternals ZValidationInternals;
		readonly ISingleElementListInternal ParentListInternals;
	}
}
