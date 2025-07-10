//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExamSettingsValidation
//
//    This class should be used for overriding validation in AutoExamSettingsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	public class ExamSettingValidation : AutoExamSettingValidation
	{
		public ExamSettingValidation(AutoExamSetting parent) : base(parent)
		{
		}

		public new ExamSetting Parent
		{
			get { return (ExamSetting)base.Parent; }
		}

		protected override void CheckEXS_IsDefault()
		{
			base.CheckEXS_IsDefault();
			if (!Parent.EXS_G0.IsEmpty && Parent.TestCampaign != null)
			{
				var settings = Parent.TestCampaign.ExamSettingCollection;
				var defaults = settings.Where(s => s.EXS_IsDefault && s.PK != Parent.PK);
				var name = !Parent.TestCampaign.CampaignID.IsEmpty
					? Parent.TestCampaign.CampaignID
					: (ZString)Res.GetString("4144A13D-D8F3-48F8-88AB-2698D9D2A7FB", "the exam");

				if (Parent.EXS_IsDefault)
				{
					if (defaults.Any())
					{
						Parent.EXS_IsDefaultInfo.AddError(Res.GetString("4F72B0E4-E019-44DC-A8F5-FD4EE50FDA28",
							"Web default already exists for {0}.", name));
					}
				}
				else
				{
					if (!defaults.Any())
					{
						Parent.EXS_IsDefaultInfo.AddError(Res.GetString("6A246FC0-6109-4520-ADD5-A66476961E36",
							"The exam should have a default."));
					}
				}
			}
		}

		protected override void CheckEXS_Code()
		{
			base.CheckEXS_Code();
			MandatoryValidation.CheckEntered(Parent.EXS_CodeInfo);
			if (!Parent.EXS_G0.IsEmpty && Parent.TestCampaign != null)
			{
				var isDuplicated =
					Parent.TestCampaign.ExamSettingCollection.Any(s =>
						s.EXS_Code.Equals(Parent.EXS_Code) && s.PK != Parent.PK);

				if (isDuplicated)
				{
					Parent.EXS_CodeInfo.AddError(Res.GetString("0307B7FF-2A18-4B3E-80FD-DF559548BEA7", "Code already exists."));
				}
			}
		}

		protected override void CheckEXS_ExamVersion()
		{
			base.CheckEXS_ExamVersion();
			MandatoryValidation.CheckEntered(Parent.EXS_ExamVersionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EXS_ExamVersionInfo, (ICodeDescriptionPairList)Parent.Lookups.VersionList);
		}

		protected override void CheckEXS_TestResultsExpireAfterHours()
		{
			base.CheckEXS_TestResultsExpireAfterHours();
			MandatoryValidation.CheckNotNegative(Parent.EXS_TestResultsExpireAfterHoursInfo);
		}

		protected override void CheckEXS_MaximumAskedQuestionsPerExam()
		{
			base.CheckEXS_MaximumAskedQuestionsPerExam();
			MandatoryValidation.CheckNotNegative(Parent.EXS_MaximumAskedQuestionsPerExamInfo);
			MandatoryValidation.CheckNotZero(Parent.EXS_MaximumAskedQuestionsPerExamInfo);
		}

		protected override void CheckEXS_ExamExpiryTimeInMinutes()
		{
			base.CheckEXS_ExamExpiryTimeInMinutes();
			CompareValidation.CheckWithinRange(Parent.EXS_ExamExpiryTimeInMinutesInfo, 1, 300);
		}
	}
}
