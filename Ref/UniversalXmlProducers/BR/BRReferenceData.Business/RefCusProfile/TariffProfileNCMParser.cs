using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Humanizer;
using Microsoft.IdentityModel.Tokens;
using static CargoWise.RefDbRepo.BRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class TariffProfileNCMParser : BaseRefCusProfileParser<TariffAttributesDTO>
	{
		public TariffProfileNCMParser(string dataSource, bool isProduction, DateTime profileDate, DateTime questionDate) : base(dataSource)
		{
			this.isProduction = isProduction;
			publicationProfileDate = profileDate;
			publicationQuestionDate = questionDate;
		}

		bool isProduction;

		protected override string ProfileType => isProduction ? Constants.ProfileTypes.Codes.Tariff : Constants.ProfileTypes.Codes.TariffTest;

		protected override string TariffType => Constants.TariffTypes.Codes.HSN;

		protected override XmlWriterConfiguration GetRefCusProfileWriterConfiguration() => Helper.GetRefCusProfileWriterConfiguration(ProfileType, TariffType, true);

		protected override XmlWriterConfiguration GetRefCusProfileQuestionWriterConfiguration() => Helper.GetRefCusProfileQuestionWriterConfiguration(ProfileType, TariffType, true);

		public void OverridePublicationDate(DateTime date)
		{
			PublicationTimeCore = date;
		}

		protected override Func<DateTime> GetProfilePublicationTime() => () => publicationProfileDate.Add(PublicationTimeCore.TimeOfDay);
		DateTime publicationProfileDate;

		protected override Func<DateTime> GetQuestionPublicationTime() => () => publicationQuestionDate.Add(PublicationTimeCore.TimeOfDay);
		DateTime publicationQuestionDate;

		protected override IEnumerable<RefCusProfile> GetRefCusProfile(TariffAttributesDTO data)
		{
			var list = new List<RefCusProfile>();
			var tariffsAndQuestionCodes = new HashSet<string>();
			foreach (var tariff in data.Tariffs)
			{
				foreach (var attribute in tariff.listaAtributos)
				{
					var tariffCode = tariff.codigoNcm?.Replace(".", string.Empty);
					var questionCode = attribute.codigo;
					if (!tariffsAndQuestionCodes.Add(tariffCode + questionCode))
					{
						continue;
					}

					var profile = new RefCusProfile()
					{
						XX0_TariffCode = tariffCode,
						XX0_QuestionCode = questionCode,
						XX0_AllowMultipleAnswers = attribute.AllowMultipleAnswers,
						XX0_IsAnswerMandatory = attribute.IsAnswerMandatory,
					};

					if (!string.IsNullOrEmpty(attribute.dataInicioVigencia))
					{
						profile.XX0_StartDate = ParseToDate(attribute.dataInicioVigencia);
					}

					if (!string.IsNullOrEmpty(attribute.dataFimVigencia))
					{
						profile.XX0_EndDate = ParseToDate(attribute.dataFimVigencia);
					}

					var profileAtt = new RefCusProfileAttribute()
					{
						XXY_Value = Constants.ProfileAttribute.GetValueFromBRValue(attribute.modalidade),
						XXY_Name = Constants.ProfileAttribute.Names.Modality
					};

					profile.RefCusProfileAttributes = new RefCusProfileAttribute[] { profileAtt };

					list.Add(profile);
				}
			}

			return list;
		}

		protected override IEnumerable<RefCusProfileQuestion> GetRefCusProfileQuestion(TariffAttributesDTO data)
		{
			var profiles = new List<RefCusProfileQuestion>();
			var questionCodes = new HashSet<string>();
			profiles.AddRange(CreateQuestions(GetFilteredAttributes(data.Attributes), questionCodes));
			return profiles;
		}

		static List<RefCusProfileQuestion> CreateQuestions(IEnumerable<Atributo> Attributes, HashSet<string> questionCodes)
		{
			var profiles = new List<RefCusProfileQuestion>();
			if (Attributes == null)
			{
				return profiles;
			}

			foreach (var attribute in Attributes)
			{
				if (!questionCodes.Add(attribute.codigo))
				{
					continue;
				}

				var profileQuestionAttributes = new List<RefCusProfileQuestionAttribute>();
				if (attribute.objetivos != null)
				{
					foreach (var objective in attribute.objetivos)
					{
						var profileAttribute = new RefCusProfileQuestionAttribute()
						{
							XQ3_Value = objective.codigo.ToString(CultureInfo.InvariantCulture),
							XQ3_Name = Objectives.Descriptions.Objective,
						};

						profileQuestionAttributes.Add(profileAttribute);
					}
				}

				var answerList = new List<RefCusProfileQuestionAnswerList>();
				if (attribute.dominio != null)
				{
					foreach (var domain in attribute.dominio)
					{
						var answer = new RefCusProfileQuestionAnswerList()
						{
							XQ4_Description = domain.descricao,
							XQ4_Value = domain.codigo
						};
						answerList.Add(answer);
					}
				}

				var dataType = GetDataType(attribute.formaPreenchimento);
				if (dataType.IsNullOrEmpty())
				{
					continue;
				}

				var profile = new RefCusProfileQuestion()
				{
					XQ2_QuestionCode = attribute.codigo,
					XQ2_AnswerDataType = dataType,
					XQ2_AnswerMaxLength = attribute.tamanhoMaximo > 32767 ? (short)32767 : (short)attribute.tamanhoMaximo,
					XQ2_AnswerDecimalPlaces = (short)attribute.casasDecimais,
					XQ2_AllowMultipleAnswers = attribute.multivalorado,
					XQ2_IsAnswerMandatory = attribute.obrigatorio,
					XQ2_Name = attribute.nome,
					XQ2_Text = attribute.nome,
					XQ2_Note = attribute.orientacaoPreenchimento,
					RefCusProfileQuestionAttributes = profileQuestionAttributes.ToArray(),
					RefCusProfileQuestionAnswerLists = answerList.ToArray(),
				};

				if (!string.IsNullOrEmpty(attribute.dataInicioVigencia))
				{
					profile.XQ2_StartDate = ParseToDate(attribute.dataInicioVigencia);
				}

				if (!string.IsNullOrEmpty(attribute.dataFimVigencia))
				{
					profile.XQ2_EndDate = ParseToDate(attribute.dataFimVigencia);
				}

				profiles.Add(profile);
				profiles.AddRange(CreateQuestions(attribute.listaSubatributos,questionCodes));
			}
			return profiles;
		}

		public override IEnumerable<RefCusProfileQuestionPathway> GetRefCusProfileQuestionPathway(TariffAttributesDTO data)
		{
			var list = new List<RefCusProfileQuestionPathway>();

			foreach (var attribute in GetFilteredAttributes(data.Attributes))
			{
				list.AddRange(CreateRefCusProfileQuestionPathway(attribute, attribute.condicionados.Where(cond => IsValidAttribute(cond.atributo))));
				list.AddRange(CreateRefCusProfileQuestionPathway(attribute, GetFilteredAttributes(attribute.listaSubatributos)));
			}
			return list;
		}

		static IEnumerable<RefCusProfileQuestionPathway> CreateRefCusProfileQuestionPathway(Atributo parent, IEnumerable<IPathwayAttribute> conditioningList)
		{
			var pathwayList = new List<RefCusProfileQuestionPathway>();
			if (conditioningList != null)
			{
				foreach (var pathwayAttribute in conditioningList)
				{
					var pathway = new RefCusProfileQuestionPathway()
					{
						XQP_Description = pathwayAttribute.Description.Truncate(200),
						XQP_ConditionToProceedFormula = GetFormulaFromCondition(pathwayAttribute.ConditionToProceedFormula),
						XQP_XQ2_NKQuestionParent = parent.codigo,
						XQP_XQ2_NKQuestionChild = pathwayAttribute.NKQuestionChild,
						XQP_AllowMultipleAnswers = pathwayAttribute.AllowMultipleAnswers,
						XQP_IsAnswerMandatory = pathwayAttribute.IsAnswerMandatory,
					};

					if (!string.IsNullOrEmpty(parent.dataInicioVigencia))
					{
						pathway.XQP_XQ2_NKQuestionStartDateParent = ParseToDate(parent.dataInicioVigencia);
					}

					if (!string.IsNullOrEmpty(pathwayAttribute.NKQuestionStartDateChild))
					{
						pathway.XQP_XQ2_NKQuestionStartDateChild = ParseToDate(pathwayAttribute.NKQuestionStartDateChild);
					}

					if (!string.IsNullOrEmpty(pathwayAttribute.StartDate))
					{
						pathway.XQP_StartDate = ParseToDate(pathwayAttribute.StartDate);
					}

					if (!string.IsNullOrEmpty(pathwayAttribute.EndDate))
					{
						pathway.XQP_EndDate = ParseToDate(pathwayAttribute.EndDate);
					}

					pathwayList.Add(pathway);
				}
			}

			return pathwayList;
		}

		static IEnumerable<Atributo> GetFilteredAttributes(IEnumerable<Atributo> attributes) => attributes.Where(att => IsValidAttribute(att));

		static bool IsValidAttribute(Atributo att) => att.objetivos.Any(obj => obj.codigo != Constants.Objectives.LPCO && obj.codigo != Constants.Objectives.TAX_TREATMENT);

		static string GetFormulaFromCondition(Condicao condition)
		{
			var resultFormula = new StringBuilder();
			if (condition != null)
			{
				resultFormula.Append("[Answer]");
				resultFormula.Append(' ');
				resultFormula.Append(GetComparisonSymbol(condition.operador));
				resultFormula.Append(' ');
				resultFormula.Append(GetComparisonValue(condition.valor));

				if (!string.IsNullOrEmpty(condition.composicao) && condition.condicao != null)
				{
					resultFormula.Append(' ');
					resultFormula.Append(GetComparisonSymbol(condition.composicao));
					resultFormula.Append(' ');
					resultFormula.Append(GetFormulaFromCondition(condition.condicao));
				}
			}

			return resultFormula.ToString();
		}

		static string GetComparisonValue(string value)
		{
			switch (value.ToUpper(CultureInfo.InvariantCulture))
			{
				case Constants.Booleans.TRUE_TEXT:
					return Constants.Booleans.TRUE;
				case Constants.Booleans.FALSE_TEXT:
					return Constants.Booleans.FALSE;
			}

			if (value.Length > 0 && value.All(c => char.IsNumber(c)))
			{
				return value;
			}
			else if (value.Length == 1 && char.IsLetter(value[0]))
			{
				return ((int)value[0]).ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				ParserErrorCollector.Instance.AppendLine($"Value for Comparison not accepted: {value}");
			}

			return value;
		}

		static string GetComparisonSymbol(string Symbol)
		{
			switch (Symbol)
			{
				case Constants.ComparisonSymbols.BRSymbols.EQUAL:
					return Constants.ComparisonSymbols.CW1Symbols.EQUAL;
				case Constants.ComparisonSymbols.BRSymbols.OR:
					return Constants.ComparisonSymbols.CW1Symbols.OR;
				case Constants.ComparisonSymbols.BRSymbols.AND:
					return Constants.ComparisonSymbols.CW1Symbols.AND;
				case Constants.ComparisonSymbols.BRSymbols.NOT_EQUAL:
					return Constants.ComparisonSymbols.CW1Symbols.NOT_EQUAL;
				case Constants.ComparisonSymbols.BRSymbols.GREATER_THAN:
					return Constants.ComparisonSymbols.CW1Symbols.GREATER_THAN;
				case Constants.ComparisonSymbols.BRSymbols.LESS_THAN:
					return Constants.ComparisonSymbols.CW1Symbols.LESS_THAN;
				case Constants.ComparisonSymbols.BRSymbols.GREATER_OR_EQUAL:
					return Constants.ComparisonSymbols.CW1Symbols.GREATER_OR_EQUAL;
				case Constants.ComparisonSymbols.BRSymbols.LESS_OR_EQUAL:
					return Constants.ComparisonSymbols.CW1Symbols.LESS_OR_EQUAL;
				default:
					ParserErrorCollector.Instance.AppendLine($"Comparison Symbol not found: {Symbol}");
					return string.Empty;
			}
		}

		static DateTime ParseToDate(string sDate) => DateTime.ParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
	}
}
