using System.IO;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Recruiter.Business
{
	public class ExamSurveyQuestionImporterBizO : NonPersistentBusinessObject
	{
		public ExamSurveyQuestionImporterBizO(GlbCompanyCampaign campaign, IFileMapper fileMapper)
		{
			this.campaign = campaign;
			this.fileMapper = fileMapper;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			StartingRowIndex = 2;
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ExamSurveyQuestionImporterBizOValidation Validation
		{
			get { return new ExamSurveyQuestionImporterBizOValidation(this); }
		}

		#region ShouldClearExistingQuestions

		public ZBool ShouldClearExistingQuestions
		{
			get { return shouldClearExistingQuestions; }
			set { SetNonPersistentPropertyValue(ShouldClearExistingQuestionsInfo, ref shouldClearExistingQuestions, value); }
		}

		public ZPropertyInfo ShouldClearExistingQuestionsInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldClearExistingQuestions)); }
		}

		ZBool shouldClearExistingQuestions;

		#endregion

		#region FileLocation

		[MaxLength(500)]
		public ZString FileLocation
		{
			get { return fileLocation; }
			set
			{
				if (fileLocation != value)
				{
					SetNonPersistentPropertyValue(FileLocationInfo, ref fileLocation, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateFileLocation();
					}
				}
			}
		}

		public Stream OpenFile()
		{
			return fileMapper.OpenRead(FileLocation);
		}

		public ZPropertyInfo FileLocationInfo
		{
			get { return GetZPropertyInfo(nameof(FileLocation)); }
		}

		ZString fileLocation;

		#endregion

		#region StartingRowIndex

		[ReadOnlyMember(nameof(IsXmlFile))]
		public ZInt StartingRowIndex
		{
			get { return startingRowIndex; }
			set
			{
				if (startingRowIndex != value)
				{
					SetNonPersistentPropertyValue(StartingRowIndexInfo, ref startingRowIndex, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateStartingRowIndex();
					}
				}
			}
		}

		public ZPropertyInfo StartingRowIndexInfo
		{
			get { return GetZPropertyInfo(nameof(StartingRowIndex)); }
		}

		ZInt startingRowIndex;

		#endregion

		#region IsXmlFile

		public ZBool IsXmlFile
		{
			get { return Path.GetExtension(fileLocation).ToLower() == ".xml"; }
		}

		public ZPropertyInfo IsXmlFileInfo
		{
			get { return GetZPropertyInfo(nameof(IsXmlFile)); }
		}

		#endregion

		public int ImportQuestions(INotifications notificationSubscriber)
		{
			ExamQuestionsImporter importer = ExamQuestionsImporter.New(this);
			var result = importer.ImportQuestions(notificationSubscriber);

			return result;
		}

		public readonly GlbCompanyCampaign campaign;
		public readonly IFileMapper fileMapper;
	}
}
