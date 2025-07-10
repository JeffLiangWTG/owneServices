using System;
using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class ExamSurveyQuestionImporterBizOValidation : ZValidation
	{
		public ExamSurveyQuestionImporterBizOValidation(ExamSurveyQuestionImporterBizO parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(ExamSurveyQuestionImporterBizOValidation); }
		}

		public override void ValidateAll()
		{
			ValidateFileLocation();
			ValidateStartingRowIndex();
		}

		#region ValidateFileLocation

		public void ValidateFileLocation()
		{
			ValidateCalculatedProperty(parent.FileLocationInfo);
		}

		protected void CheckFileLocation()
		{
			MandatoryValidation.CheckEntered(parent.FileLocationInfo);
			if (!parent.FileLocation.IsEmpty && !(parent.fileMapper.IsRemote || File.Exists(parent.FileLocation)))
			{
				parent.FileLocationInfo.AddError(Res.GetString("6b8ef1d7-64d9-4c87-b9d4-373f9a8db5ff", "File '{0}' not found or accessible", parent.FileLocation));
			}
		}

		#endregion

		#region ValidateStartingRowIndex

		public void ValidateStartingRowIndex()
		{
			ValidateCalculatedProperty(parent.StartingRowIndexInfo);
		}

		protected void CheckStartingRowIndex()
		{
			if (!parent.IsXmlFile)
			{
				MandatoryValidation.CheckNotNegative(parent.StartingRowIndexInfo);
				MandatoryValidation.CheckEntered(parent.StartingRowIndexInfo);
			}
		}

		#endregion

		readonly ExamSurveyQuestionImporterBizO parent;
	}
}
