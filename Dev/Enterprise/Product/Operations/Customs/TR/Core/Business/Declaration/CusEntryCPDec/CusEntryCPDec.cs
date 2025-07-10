using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryCPDec : BaseCusEntryCPDec
	{
		public CusEntryCPDec(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusEntryCPDecLookups GetNewLookups() => new CusEntryCPDecLookups(this);

		public new CusEntryCPDecLookups Lookups => (CusEntryCPDecLookups)base.Lookups;

		protected override Customs.Business.CusEntryCPDecValidation GetNewValidation() => new CusEntryCPDecValidation(this);

		public new CusEntryCPDecValidation Validation => (CusEntryCPDecValidation)base.Validation;

		[List(nameof(Lookups) + "." + nameof(CusEntryCPDecLookups.QuestionTypeList))]
		public override ZString ON_QuestionType
		{
			get => base.ON_QuestionType;
			set => base.ON_QuestionType = value;
		}

		[ResourceStringData("7480EF9C-E10A-4D6C-8F18-8C1DB35E91C5", Caption = "Question Type Description", ShortCaption = "Type Desc.")]
		public ZString QuestionTypeDescription => Lookups.QuestionTypeList.GetDescriptionFromCode(ON_QuestionType);

		public ZPropertyInfo QuestionTypeDescriptionInfo => GetZPropertyInfo(nameof(QuestionTypeDescription));

		[List(nameof(Lookups) + "." + nameof(CusEntryCPDecLookups.AnswerCodeList))]
		public override ZString ON_AnswerCode
		{
			get => base.ON_AnswerCode;
			set => base.ON_AnswerCode = value;
		}

		public ZString Description
		{
			get
			{
				var codeType = ON_QuestionType == QuestionTypeList.Codes.Q ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsQuestion : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsWarning;
				var refCusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, ON_CPDecNum.ToString(), Core.Constants.CountryCodes.Turkey, codeType, ZDate.Today);
				return refCusCodeList?.ZZD_Description ?? ZString.Empty;
			}
		}
	}
}
