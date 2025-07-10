using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	public abstract class AccTransactionMatchLink : AutoAccTransactionMatchLink, IAccTransactionMatchLink, ISupportCriticalValidation, IHaveConstructorStackTrace
	{
		public static readonly AccTransactionMatchLinkTypeDecider TypeDecider = new AccTransactionMatchLinkTypeDecider();

		public AccTransactionMatchLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			this.SetConstructorStackTrace();
		}

		#region IHaveConstructorStackTrace member

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region ISupportCriticalValidation

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return GetCriticalValidation(); }
		}

		protected virtual ICriticalValidation GetCriticalValidation()
		{
			return new AccTransactionMatchLinkCriticalValidation(this);
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		public sealed override void OnSaving()
		{
			base.OnSaving();
			OnSavingCore();
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
		}

		public FunctionalitySuspender MatchDateIsNotEmptyValidationSuspender
		{
			get { return MatchDateIsNotEmptyValidationSuspenderInternal ?? (MatchDateIsNotEmptyValidationSuspenderInternal = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender MatchDateIsNotEmptyValidationSuspenderInternal;

		protected virtual void OnSavingCore()
		{
		}
	}
}
