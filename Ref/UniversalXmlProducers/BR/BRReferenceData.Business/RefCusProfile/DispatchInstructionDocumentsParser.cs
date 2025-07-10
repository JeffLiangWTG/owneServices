using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.BRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class DispatchInstructionDocumentsParser : BaseRefCusProfileParser<IEnumerable<DossierDataDTO>>
	{
		public DispatchInstructionDocumentsParser(string dataSource, bool isProduction) : base(dataSource)
		{
			this.isProduction = isProduction;
		}

		readonly bool isProduction;

		protected override string ProfileType => isProduction ? Constants.ProfileTypes.Codes.DispatchInstructionDocument : Constants.ProfileTypes.Codes.DispatchInstructionDocumentTest;

		protected override string TariffType => isProduction ? Constants.TariffTypes.Codes.EADOC : Constants.TariffTypes.Codes.EADTE;

		protected override bool AddTariffAttributes => true;

		readonly HashSet<string> includedKeywords = new HashSet<string>();

		public void OverridePublicationDate(DateTime date)
		{
			PublicationTimeCore = date;
		}

		protected override IEnumerable<RefCusProfile> GetRefCusProfile(IEnumerable<DossierDataDTO> data)
		{
			var list = new List<RefCusProfile>();

			foreach (var dossieData in data)
			{
				if (dossieData.OperationType == Constants.TariffAttributes.OperationTypes.LPCO)
				{
					foreach (var dossieType in dossieData.DossierTypes)
					{
						list.AddRange(CreateRefCusProfile(dossieType.DocumentTypes));
					}
				}
				else
				{
					list.AddRange(CreateRefCusProfile(dossieData.DocumentTypes));
				}
			}
			return list;
		}


		readonly HashSet<string> includedKeywordsAndTariffs = new HashSet<string>();

		IEnumerable<RefCusProfile> CreateRefCusProfile(IEnumerable<DocumentTypeDTO> docs)
		{
			foreach (var doc in docs)
			{
				foreach (var keyword in doc.Keywords)
				{
					var tariffCode = doc.DocumentTypeId.ToString(CultureInfo.InvariantCulture);
					var questionCode = keyword.KeywordId.ToString(CultureInfo.InvariantCulture);
					if (includedKeywordsAndTariffs.Add($"{tariffCode},{questionCode}"))
					{
						var refCusProfile = new RefCusProfile
						{
							XX0_XXX_NKProfileType = ProfileType,
							XX0_TariffCode = tariffCode,
							XX0_QuestionCode = questionCode,
						};

						yield return refCusProfile;
					}
				}
			}
		}

		protected override IEnumerable<RefCusProfileQuestion> GetRefCusProfileQuestion(IEnumerable<DossierDataDTO> data)
		{
			var list = new List<RefCusProfileQuestion>();
			foreach (var dossieData in data)
			{
				if (dossieData.OperationType == Constants.TariffAttributes.OperationTypes.LPCO)
				{
					foreach (var dossieType in dossieData.DossierTypes)
					{
						list.AddRange(CreateRefCusProfileQuestion(dossieType.DocumentTypes));
					}
				}
				else
				{
					list.AddRange(CreateRefCusProfileQuestion(dossieData.DocumentTypes));
				}
			}
			return list;
		}

		IEnumerable<RefCusProfileQuestion> CreateRefCusProfileQuestion(IEnumerable<DocumentTypeDTO> docs)
		{
			var list = new List<RefCusProfileQuestion>();
			foreach (var doc in docs)
			{
				foreach (var keyword in doc.Keywords)
				{
					var keywordId = keyword.KeywordId.ToString(CultureInfo.InvariantCulture);
					if (!includedKeywords.Add(keywordId))
					{
						continue;
					}
					var refCusProfileQuestion = new RefCusProfileQuestion
					{
						XQ2_XXX_NKProfileType = ProfileType,
						XQ2_QuestionCode = keywordId,
						XQ2_AnswerDataType = GetDataType(keyword.DataType),
						XQ2_AnswerDecimalPlaces = (short)keyword.DecimalPlaces,
						XQ2_AnswerMask = keyword.Mask,
						XQ2_Name = keyword.KeywordName,
						XQ2_IsAnswerMandatory = false,
						XQ2_Text = keyword.KeywordName,
					};

					var answerList = new List<RefCusProfileQuestionAnswerList>();
					foreach (var domain in keyword.Domains)
					{
						answerList.Add(
							new RefCusProfileQuestionAnswerList
							{
								XQ4_Description = domain.Description,
								XQ4_Value = domain.Code,
							}
						);
					}
					refCusProfileQuestion.RefCusProfileQuestionAnswerLists = answerList.ToArray();

					list.Add(refCusProfileQuestion);
				}
			}
			return list;
		}

		protected override IEnumerable<RefCusTariff> GetRefCusTariff(IEnumerable<DossierDataDTO> data)
		{
			var list = new List<RefCusTariff>();

			foreach (var dossieData in data)
			{
				if (dossieData.OperationType == Constants.TariffAttributes.OperationTypes.LPCO)
				{
					foreach (var dossieType in dossieData.DossierTypes)
					{
						list.AddRange(CreateRefCusTariff(dossieType.DocumentTypes, dossieData.OperationType));
					}
				}
				else
				{
					list.AddRange(CreateRefCusTariff(dossieData.DocumentTypes, dossieData.OperationType));
				}
			}

			return list;
		}

		readonly HashSet<string> includedTariffs = new HashSet<string>();

		IEnumerable<RefCusTariff> CreateRefCusTariff(IEnumerable<DocumentTypeDTO> docs, string operationType)
		{
			foreach (var doc in docs)
			{
				var tariffCode = doc.DocumentTypeId.ToString(CultureInfo.InvariantCulture);
				if (includedTariffs.Add(tariffCode))
				{
					var refCusTariff = new RefCusTariff
					{
						ZZ1_TariffCode = tariffCode,
						ZZ1_Description = doc.DocumentTypeName,
						RefCusTariffAttributes = new RefCusTariffAttribute[]
						{
							new RefCusTariffAttribute() { ZZ3_Name = TariffAttributes.EADocumentOperationType, ZZ3_Value = operationType }
						}
					};

					yield return refCusTariff;
				}
			}
		}
	}
}
