using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.IO;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterData.GUI
{
	public static class UXMLMatchingDiagnosticUtils
	{
		public static OrganizationAddress GetOrganizationAddressFromXMLString(string xmlString, IXmlImportLogger logger)
		{
			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance);
			using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
			{
				new UniversalDataBuss.XmlIO.XmlReading.XmlReader().ReadXML(result, memoryStream, logger);
			}

			return result;
		}

		public static OrgHeader GetMatchedOrgHeader(OrganizationAddress organisationData, BusinessObjectFactory factory)
		{
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting);
			return matcher.GetMatchingOrganization(organisationData);
		}

		public static OrgAddress GetMatchedOrgAddress(OrganizationAddress organisationData, BusinessObjectFactory factory, ISimpleLogger logger)
		{
			var matcher = new OrganisationMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			return matcher.GetMatchingAddress(organisationData, organisationData.AddressShortCode, false);
		}

		public static string GetMatchedOrgAddressLog(IXmlImportLogger logger)
		{
			var logs = new StringBuilder();
			logger.Logs.ForEach(o =>
			{
				logs.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} : {1}", o.Type, o.Message));
			});

			return logs.ToString();
		}

		public static List<UXMLMatchingDiagnosticModel> CombineMatchedOrgAndScoreResultsToVMs(OrgHeader matchedOrg, IEnumerable<ScoringResult> scorings, BusinessObjectFactory factory)
		{
			var orgExcludeThresholdPercentage = OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.Value;
			var result = new List<UXMLMatchingDiagnosticModel>();
			var scoringResults = scorings;
			if (matchedOrg != null)
			{
				result.Add(new UXMLMatchingDiagnosticModel(
					matchedOrgHeader: matchedOrg,
					orgScore: scoringResults.First(o => o.TargetPK == matchedOrg.PK).Score,
					result: Constants.MatchAndSelected));

				scoringResults = scorings?.Where(o => o.TargetPK != matchedOrg.PK).ToList();
			}

			var notMatchedOrgs = scoringResults?.Select(o =>
			{
				return new UXMLMatchingDiagnosticModel(
					matchedOrgHeader: factory.Load<OrgHeader>(o.TargetPK),
					orgScore: o.Score,
					result: GetMatchingResult(o, orgExcludeThresholdPercentage));
			});

			if (notMatchedOrgs != null)
			{
				result.AddRange(notMatchedOrgs);
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Complexity comes from sorting, but does not affect maintainability")]
		public static List<UXMLMatchingDiagnosticModel> CombineMatchedAddressAndScoreResultsToVMs(OrgAddress matchedAddress, IEnumerable<ScoringResult> scorings, BusinessObjectFactory factory)
		{
			var scoringResults = scorings;
			var result = new List<UXMLMatchingDiagnosticModel>();
			var orgExcludeThresholdPercentage = OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.Value;
			var addressExcludeThresholdPercentage = OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.Value;
			if (matchedAddress != null)
			{
				var matchedOrg = factory.Load<OrgHeader>(matchedAddress.OA_OH);
				ScoringResult matchedScoringResult = scoringResults.FirstOrDefault(o => o.TargetPK == matchedOrg.PK);

				if (matchedScoringResult != null)
				{
					result.Add(new UXMLMatchingDiagnosticModel(
					matchedOrgHeader: matchedOrg,
					matchedOrgAddress: matchedAddress,
					orgScore: matchedScoringResult.Score,
					addressScore: matchedScoringResult.ChildResults.FirstOrDefault(childResult => childResult.MasterType == typeof(IOrgAddress))?.Score ?? 0,
					result: Constants.MatchAndSelected));

					scoringResults = scorings?.Where(o => o.TargetPK != matchedOrg.PK).ToList();
				}
			}

			var notMatchedOrgs = scoringResults?.Select(o =>
			{
				var addressScore = o.ChildResults.FirstOrDefault(childResult => childResult.MasterType == typeof(IOrgAddress));

				UXMLMatchingDiagnosticModel newModel;
				if (addressScore != null)
				{
					newModel = new UXMLMatchingDiagnosticModel(
						matchedOrgHeader: factory.Load<OrgHeader>(o.TargetPK),
						matchedOrgAddress: factory.Load<OrgAddress>(addressScore.TargetPK),
						orgScore: o.Score,
						addressScore: addressScore.Score,
						result: GetMatchingResult(o, addressScore, orgExcludeThresholdPercentage, addressExcludeThresholdPercentage)
					);
				}
				else
				{
					newModel = new UXMLMatchingDiagnosticModel(
						matchedOrgHeader: factory.Load<OrgHeader>(o.TargetPK),
						orgScore: o.Score,
						result: GetMatchingResult(o, addressScore, orgExcludeThresholdPercentage, addressExcludeThresholdPercentage)
					);
				}

				return newModel;
			});

			if (notMatchedOrgs != null)
			{
				result.AddRange(notMatchedOrgs);
			}

			var resultsForSorting = result.Select(model =>
			(
				Model: model, model.AddressScoreValue, model.OrgScoreValue, model.MatchedOrgCode, model.Result
			));

			var resultsOrdered = resultsForSorting.OrderByDescending(o => o.Result == Constants.MatchAndSelected)
				.ThenBy(o => o.AddressScoreValue * 100 < addressExcludeThresholdPercentage)
				.ThenBy(o => o.OrgScoreValue * 100 < orgExcludeThresholdPercentage)
				.ThenByDescending(o => o.AddressScoreValue)
				.ThenByDescending(o => o.OrgScoreValue)
				.ThenBy(o => o.MatchedOrgCode)
				.Select(o => o.Model)
				.ToList();

			return resultsOrdered;
		}

		static ZString GetMatchingResult(ScoringResult orgScoringResult, ScoringResult addressScoringResult, int orgExcludeThresholdPercentage, int addressExcludeThresholdPercentage)
		{
			ZString matchingResult;

			if (orgScoringResult != null && addressScoringResult != null && orgScoringResult.Score > orgExcludeThresholdPercentage / 100d && addressScoringResult.Score > addressExcludeThresholdPercentage / 100d)
			{
				matchingResult = Constants.Match;
			}
			else
			{
				matchingResult = Constants.NotMatch;
			}

			return matchingResult;
		}

		static ZString GetMatchingResult(ScoringResult scoringResult, int orgExcludeThresholdPercentage)
		{
			ZString matchingResult;
			var orgExcludeThreshold = orgExcludeThresholdPercentage / 100d;

			if (scoringResult != null && scoringResult.Score >= orgExcludeThreshold)
			{
				matchingResult = Constants.Match;
			}
			else
			{
				matchingResult = Constants.NotMatch;
			}

			return matchingResult;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not an error")]
		public static class Constants
		{
			public const string Match = "MATCH";
			public const string MatchAndSelected = "MATCH (SELECTED)";
			public const string NotMatch = "NOT MATCHED";
		}

		#region XML Formatter

		static readonly Color color_Bracket = Color.Blue;
		static readonly Color color_Node = Color.Firebrick;
		static readonly Color color_String = Color.Blue;
		static readonly Color color_Attribute = Color.Red;
		static readonly Color color_Comment = Color.GreenYellow;
		static readonly Color color_innerText = Color.Black;

		public static bool Format(RichTextBox richTextBox)
		{
			try
			{
				var text = richTextBox.Text.Trim();
				if (!string.IsNullOrEmpty(text))
				{
					richTextBox.Text = XDocument.Parse(text).ToString();
					HighLight(richTextBox);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Globals.Message.ShowError(e.Message);
				return false;
			}

			return true;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static void HighLight(RichTextBox richTextBox)
		{
			var index = 0;
			var rtbText = richTextBox.Text;
			var lastEndIndex = -1;
			while (index <= rtbText.Length)
			{
				var startIndex = rtbText.IndexOf('<', index);

				if (lastEndIndex > 1)
				{
					if (rtbText.Substring(lastEndIndex - 1, 2) == "/>")
					{
						SetTextColor(richTextBox, lastEndIndex - 1, 2, color_Bracket);

						if (startIndex != -1)
						{
							SetTextColor(richTextBox, lastEndIndex + 1, startIndex - lastEndIndex - 1, color_innerText);
						}
					}
					else
					{
						SetTextColor(richTextBox, lastEndIndex, 1, color_Bracket);

						if (startIndex != -1)
						{
							SetTextColor(richTextBox, lastEndIndex + 1, startIndex - lastEndIndex - 1, color_innerText);
						}
					}
				}

				if (startIndex < 0)
				{
					break;
				}

				var endIndex = rtbText.IndexOf('>', startIndex + 1);
				if (endIndex < 0)
				{
					break;
				}

				lastEndIndex = endIndex;
				index = endIndex + 1;

				if (rtbText[startIndex + 1] == '!')
				{
					SetTextColor(richTextBox, startIndex, 1, color_String);
					SetTextColor(richTextBox, startIndex + 1, endIndex - startIndex - 1, color_Comment);
					continue;
				}

				var state = 0;
				var lastStartIndex = -1;
				var inString = false;
				var nodeText = rtbText.Substring(startIndex, endIndex - startIndex);

				/* 0 = before node name
				   1 = in node name
				   2 = after node name
				   3 = in attribute
				   4 = in string
				   */
				var startNodeName = 0;
				var startAtt = 0;
				for (int i = 0; i < nodeText.Length; ++i)
				{
					if (nodeText[i] == '"')
					{
						inString = !inString;
					}

					if (inString && nodeText[i] == '"')
					{
						lastStartIndex = i;
					}
					else if (nodeText[i] == '"')
					{
						SetTextColor(richTextBox, lastStartIndex + startIndex + 1, i - lastStartIndex - 1, color_String);
					}

					switch (state)
					{
						case 0:
							if (!char.IsWhiteSpace(nodeText, i))
							{
								startNodeName = i;
								state = 1;
							}
							break;

						case 1:
							if (char.IsWhiteSpace(nodeText, i))
							{
								if (rtbText.Substring(startNodeName + startIndex, 2) == "</")
								{
									SetTextColor(richTextBox, startNodeName + startIndex, 2, color_Bracket);
									SetTextColor(richTextBox, startNodeName + startIndex + 2, i - startNodeName - 2, color_Node);
								}
								else
								{
									SetTextColor(richTextBox, startNodeName + startIndex, 1, color_Bracket);
									SetTextColor(richTextBox, startNodeName + startIndex + 1, i - startNodeName - 1, color_Node);
								}

								state = 2;
							}
							break;

						case 2:
							if (!char.IsWhiteSpace(nodeText, i))
							{
								startAtt = i;
								state = 3;
							}
							break;

						case 3:
							if (char.IsWhiteSpace(nodeText, i) || nodeText[i] == '=')
							{
								SetTextColor(richTextBox, startAtt + startIndex, i - startAtt, color_Attribute);
								state = 4;
							}
							break;

						case 4:
							if (nodeText[i] == '"' && !inString)
							{
								state = 2;
							}
							break;
					}
				}

				if (state == 1)
				{
					if (rtbText.Substring(startIndex, 2) == "</")
					{
						SetTextColor(richTextBox, startIndex, 2, color_Bracket);
						SetTextColor(richTextBox, startIndex + 2, nodeText.Length - 2, color_Node);
					}
					else
					{
						SetTextColor(richTextBox, startIndex, 1, color_Bracket);
						SetTextColor(richTextBox, startIndex + 1, nodeText.Length - 1, color_Node);
					}
				}
			}
		}

		static void SetTextColor(RichTextBox richTextBox, int startIndex, int length, Color color)
		{
			richTextBox.Select(startIndex, length);
			richTextBox.SelectionColor = color;
		}

		#endregion
	}

	#region Implementation

	public class XMLMatchingDummyLogger : IXmlImportLogger
	{
		public bool IsUpdatingConsol { get; set; }
		public bool HasIgnoredModule { get; set; }
		public bool OrgMatchingDisabled => false;

		public XMLMatchingDummyLogger(bool isSameSystem = false)
		{
			if (isSameSystem)
			{
				TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			}
		}

		public ITopLevelDataObject TopLevelDataObject { get; }

		IDataContextDataObject topLevelDataContext;
		public IDataContextDataObject TopLevelDataContext => topLevelDataContext ?? (topLevelDataContext = DataContextFactory.New());

		public IEnumerable<ISimpleLog> Logs { get; } = new List<SimpleLog>();

		public void FireDataImportedToBusinessObject(BusinessObject targetBO) { }
		public void Log(LogType type, string message)
		{
			(Logs as List<SimpleLog>)?.Add(new SimpleLog(type, message) { });
		}

		public void LogBoth(LogType type, string message) { }
		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey) { }

		public void LogErrorToServiceTaskOnly(string message) { }

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }
	}

	class SimpleLog : ISimpleLog
	{
		public LogType Type { get; }
		public string Message { get; }

		public SimpleLog(LogType type, string message)
		{
			Type = type;
			Message = message;
		}
	}

	#endregion
}
