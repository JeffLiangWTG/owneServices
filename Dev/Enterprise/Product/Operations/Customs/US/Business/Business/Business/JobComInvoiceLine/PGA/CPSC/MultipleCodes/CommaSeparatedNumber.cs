using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class CommaSeparatedNumber : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected override void AddToFactoryCache()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string Number = "Number";
		}

		[MaxLength(nameof(NumberMaxLength))]
		public virtual ZString Number
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return number; }
			set
			{
				CheckMaximumLength(NumberInfo, value);
				SetNonPersistentPropertyValue(NumberInfo, ref number, value);
				if (!IsValidationSuspended)
				{
					NumberInfo.ClearAllNotifications();
					ValidateNumber();
				}
			}
		}
		ZString number;

		public abstract int NumberMaxLength { get; }

		public ZPropertyInfo NumberInfo
		{
			get { return this.GetZPropertyInfo(Schema.Number); }
		}

		public virtual void ValidateNumber()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(NumberInfo);
			MandatoryValidation.MessageErrorIfNotEntered(NumberInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateNumber();
			base.RunPreSaveValidationCore();
		}
	}
}
