using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AllocateNumber : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string AE_Number = "AE_Number";
		}

		public static class Constants
		{
			public const string MustBeNumeric = "Number must be numeric.";
		}

		public AllocateNumber(AllocateNumberArgs args)
			: base(args.Factory)
		{
			this.args = args;
		}
		readonly AllocateNumberArgs args;

		[BusinessObjectTestExclude()]
		public ZString AE_Number
		{
			get { return number; }
			set
			{
				CheckMaximumLength(AE_NumberInfo, value);
				SetNonPersistentPropertyValue(AE_NumberInfo, ref number, value);

				if (!IsValidationSuspended)
				{
					ValidateAE_Number();
				}
			}
		}
		ZString number;

		public ZPropertyInfo AE_NumberInfo
		{
			get { return GetZPropertyInfo(Schema.AE_Number, "Number"); }
		}

		public int AE_Number_MaxLength
		{
			get { return args == null ? -1 : args.MaxLength; }
		}

		void ValidateAE_Number()
		{
			AE_NumberInfo.ClearAllNotifications();

			if (!AE_Number.IsEmpty && !AE_Number.IsNumbersOnlyOrEmpty)
			{
				AE_NumberInfo.AddError(Constants.MustBeNumeric);
			}

			var extraValidator = new AllocateNumberExtraValidation(args);
			args.ValidateNumber(AE_NumberInfo, extraValidator);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAE_Number();
		}
	}
}
