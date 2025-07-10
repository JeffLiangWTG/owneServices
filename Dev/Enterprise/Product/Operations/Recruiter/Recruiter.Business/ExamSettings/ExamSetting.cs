using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(Schema.EXS_Code)]
	public class ExamSetting : AutoExamSetting, IExamSetting
	{
		public ExamSetting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EXS_TestResultsExpireAfterHours = 8760;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("5618BA65-807E-44BE-945A-981547653EC4", "Exam Setting"); }
		}

		public bool EXS_Code_ReadOnly
		{
			get { return true; }
		}

		public LearningCentreCampaign TestCampaign => Factory.Load<LearningCentreCampaign>(EXS_G0);

		[RelatedBusinessObject("TestCampaign")]
		[List("Lookups.TestCampaigns")]
		public override ZGuid EXS_G0
		{
			get { return base.EXS_G0; }
			set
			{
				if (value != EXS_G0)
				{
					base.EXS_G0 = value;
					RegenerateCode();
				}
			}
		}

		internal void RegenerateCode()
		{
			EXS_Code = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", TestCampaign?.CampaignID ?? ZString.Empty,
				EXS_ExamVersion);
		}

		[MaxLength(3)]
		[List("Lookups.VersionList")]
		public override ZString EXS_ExamVersion
		{
			get { return base.EXS_ExamVersion; }
			set
			{
				if (value != EXS_ExamVersion)
				{
					base.EXS_ExamVersion = value;
					RegenerateCode();
				}
			}
		}

		public ZShort TestResultsExpireAfterDays
		{
			get { return (ZShort)(EXS_TestResultsExpireAfterHours / 24); }
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			EXS_Code = ZGuid.NewZGuid().ToString().Substring(EXS_CodeInfo.MaxLength);
		}
#endif
	}
}
