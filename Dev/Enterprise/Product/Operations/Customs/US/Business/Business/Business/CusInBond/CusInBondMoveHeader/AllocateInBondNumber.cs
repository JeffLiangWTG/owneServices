using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class AllocateInBondNumber : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string AI_InBondNumber = "AI_InBondNumber";
			public const int ConventionalInBondNumberMaxLength = 9;
			public const int PaperlessInBondNumberMaxLength = 11;
			public const int PostDepartureMessageInbondNumberMaxLength = 11;
		}

		#endregion

		public AllocateInBondNumber(GlbBranch branch, bool isPostDepartureMessageOnly = false)
			: base(branch.Factory)
		{
			this.branch = branch;
			this.isPostDepartureMessageOnly = isPostDepartureMessageOnly;
		}
		readonly GlbBranch branch;
		readonly ZBool isPostDepartureMessageOnly;

		public bool AllowPaperlessNumber { get; set; }

		InBondNumberSetting InBondNumberSetting
		{
			get
			{
				if (inBondNumberSetting == null)
				{
					inBondNumberSetting = InBondNumberGenerator.GetInBondNumberSetting(branch);
				}
				return inBondNumberSetting;
			}
		}
		InBondNumberSetting inBondNumberSetting;

		[BusinessObjectTestExclude()]
		[MaxLength(nameof(AI_InBondNumberMaxLength))]
		public ZString AI_InBondNumber
		{
			get { return inBondNumber; }
			set
			{
				CheckMaximumLength(AI_InBondNumberInfo, value);
				SetNonPersistentPropertyValue(AI_InBondNumberInfo, ref inBondNumber, value);

				if (!IsValidationSuspended)
				{
					ValidateAI_InBondNumber();
				}
			}
		}
		ZString inBondNumber;

		public ZPropertyInfo AI_InBondNumberInfo
		{
			get { return GetZPropertyInfo(Schema.AI_InBondNumber, "In-Bond Number"); }
		}

		int AI_InBondNumberMaxLength
		{
			get { return AllowPaperlessNumber ? Schema.PaperlessInBondNumberMaxLength : isPostDepartureMessageOnly ? Schema.PostDepartureMessageInbondNumberMaxLength : Schema.ConventionalInBondNumberMaxLength; }
		}

		bool IsPaperlessNumber
		{
			get { return AllowPaperlessNumber && AI_InBondNumber.StartsWith("V"); }
		}

		void ValidateAI_InBondNumber()
		{
			AI_InBondNumberInfo.ClearAllNotifications();
			InBondNumberValidationHelper.ValidateInBondNumber(Factory, AI_InBondNumber, AI_InBondNumberInfo, branch, isPostDepartureMessageOnly, IsPaperlessNumber, Schema.ConventionalInBondNumberMaxLength, Schema.PostDepartureMessageInbondNumberMaxLength);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAI_InBondNumber();
		}

		public ZDecimal GetNextAvailableInBondNumberAndCheckReusable(out ZString reusableInformation)
		{
			reusableInformation = ZString.Empty;
			var result = ZDecimal.Zero;
			if (!isPostDepartureMessageOnly)
			{
				result = InBondNumberGenerator.GetNextAvailableInBondNumberAndCheckReusable(InBondNumberSetting, out reusableInformation);
			}
			return result;
		}

		public ZBool PostNextNumber(ZDecimal nextNumber)
		{
			var result = false;
			InBondNumberSetting.NextNumber = nextNumber;
			if (!InBondNumberSetting.NextNumberInfo.HasErrors())
			{
				InBondNumberSetting.PostNextNumber();
				result = true;
			}
			return result;
		}
	}
}
