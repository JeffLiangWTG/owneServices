using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACECaseNumberForQuery : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string CaseNumber = "CaseNumber";
			public const int CaseNumberMaxLength = 10;
		}

		public ACECaseNumberForQuery()
			: base()
		{
		}

		[BusinessObjectTestExclude()]
		[MaxLength(Schema.CaseNumberMaxLength)]
		public ZString CaseNumber
		{
			get { return caseNumber; }
			set
			{
				CheckMaximumLength(CaseNumberInfo, value);
				SetNonPersistentPropertyValue(CaseNumberInfo, ref caseNumber, value.KeepAlphanumericCharacters());
				ValidateCaseNumber();
			}
		}
		ZString caseNumber;

		public ZPropertyInfo CaseNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CaseNumber); }
		}

		public void ValidateCaseNumber()
		{
			CaseNumberInfo.ClearAllNotifications();
			if (!IsValidationSuspended)
			{
				if (CaseNumber.Length < 7)
				{
					CaseNumberInfo.AddMessageError(CaseNoLength);
				}
			}
		}
		public const string CaseNoLength = "Case Number should be minimum 7 characters in length.";

		protected override void RunPreSaveValidationCore()
		{
			ValidateCaseNumber();
			base.RunPreSaveValidationCore();
		}
	}
}
