using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public abstract class USOperationalActionMethodApplicator : Services.OperationalActions.Support.OperationalActionMethodApplicator
	{
		public USOperationalActionMethodApplicator(string description, BusinessObjectFactory factory)
			: base(description, factory)
		{
		}

		public abstract class Schema
		{
			public const string Send = "Send";
			public const string SendWithMessageErrors = "SendWithMessageErrors";
		}
		protected List<ZGuid> jobsPK;

		protected override bool SupportsSummaryCore => true;

		public bool IsCancelled
		{
			get;
			set;
		}

		public ZBool Send
		{
			get => send;
			set => SetNonPersistentPropertyValue(SendInfo, ref send, value);
		}
		ZBool send;

		public ZPropertyInfo SendInfo => GetZPropertyInfo(Schema.Send);

		public ZBool SendWithMessageErrors
		{
			get => sendWithMessageErrors;
			set
			{
				SetNonPersistentPropertyValue(SendWithMessageErrorsInfo, ref sendWithMessageErrors, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSendWithMessageErrors();
				}
			}
		}
		ZBool sendWithMessageErrors;

		public ZPropertyInfo SendWithMessageErrorsInfo => GetZPropertyInfo(Schema.SendWithMessageErrors);

		public bool IsSendWithMessageErrorsAllowed => Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;

		internal ZString MessageDescription => MessageDescriptionCore;

		protected virtual ZString MessageDescriptionCore => ZString.Empty;

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			base.BuildCore(selectItemPKs);
			targets = selectItemPKs;
		}
		protected ZGuid[] targets;

		protected override void SetDefaultValues()
		{
			Send = true;
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public USActionMethodApplicatorValidation Validation => GetValidation();

		protected virtual USActionMethodApplicatorValidation GetValidation() => new USActionMethodApplicatorValidation(this);

		public virtual ValidationModes ValidationMode => ValidationModes.None;
	}
}
