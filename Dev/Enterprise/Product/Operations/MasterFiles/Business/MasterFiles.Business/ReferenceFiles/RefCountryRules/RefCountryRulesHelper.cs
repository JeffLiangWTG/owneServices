using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.Macros;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRulesValidator : IRefCountryRulesValidator
	{
		public void ValidateRule(BusinessObject bizO, ZPropertyInfo property, string noteText)
		{
			RefCountryRulesHelper.ValidateRule(bizO, property, noteText);
		}
	}

	public static class RefCountryRulesHelper
	{
		internal const string ValidationError = "<IsError>Y</IsError>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string MacroStart = "<Macro>";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string MacroEnd = "</Macro>";
		internal const string RulePKStart = "<RulePK>";
		internal const string RulePKEnd = "</RulePK>";

		public static void ValidateRule(BusinessObject bizO, ZPropertyInfo property, string noteText)
		{
			var isError = noteText.Contains(ValidationError);
			var macroStart = noteText.IndexOf(MacroStart, StringComparison.Ordinal) + MacroStart.Length;
			var macroEnd = noteText.IndexOf(MacroEnd, StringComparison.Ordinal);
			string macro = "";

			try
			{
				macro = noteText.Substring(macroStart, macroEnd - macroStart).Trim();
				string expression = ObjectFactory.Get<ITextMacroProcessor>().Replace(macro, new BusinessObject[] { bizO });
				var result = expression.EvaluateDocEngineExpression(RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
				if (result)
				{
					if (isError)
					{
						property.AddError(Res.GetString("adc6aa7d-2700-4c63-9749-6a9d7f4293f7", "The {0} cannot be saved due to a violation of the following Country/Region Validation Rule: {1}", bizO.HumanReadableName, macro));
					}
					else
					{
						property.AddWarning(Res.GetString("25d7a2c0-cd39-4775-9eee-0cc8af5d4f9a", "The {0} violates the following Country/Region Validation Rule: {1}", bizO.HumanReadableName, macro));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				property.AddWarning(Res.GetString("a6164dfd-ea4d-45c2-88fc-67bd0009de27", "Failed to evaluate a Country/Region Validation Rule: '{0}'. Reason: '{1}'.", macro, ex.Message));
			}
		}

		public const bool RefreshValidationRulesEachSave = true;

		[SuppressMessage("Microsoft.Maintainability", "CA1502")]
		public static void AddRulesToNotes(RefCountry origin, RefCountry destination, Notes notes, string transportMode = "", bool isInDatabase = false, bool isValidation = false)
		{
			if (!isInDatabase && !isValidation)
			{
				var allRulesClientVisible = new HashSet<RefCountryRules>();
				var allRulesInternal = new HashSet<RefCountryRules>();

				origin?.Rules.Where(r => r.R7_IsClientVisible && !r.R7_IsValidationRule).ForEach(r => allRulesClientVisible.Add(r));
				destination?.Rules.Where(r => r.R7_IsClientVisible && !r.R7_IsValidationRule).ForEach(r => allRulesClientVisible.Add(r));
				origin?.Rules.Where(r => !r.R7_IsClientVisible && !r.R7_IsValidationRule).ForEach(r => allRulesInternal.Add(r));
				destination?.Rules.Where(r => !r.R7_IsClientVisible && !r.R7_IsValidationRule).ForEach(r => allRulesInternal.Add(r));

				var filteredRulesClientVisible = FilterRules(origin, destination, transportMode, allRulesClientVisible);
				var filteredRulesInternal = FilterRules(origin, destination, transportMode, allRulesInternal);

				FormatTextForNotes(filteredRulesClientVisible.Where(r => !r.R7_Notes.IsEmpty), notes, true);
				FormatTextForNotes(filteredRulesInternal.Where(r => !r.R7_Notes.IsEmpty), notes, false);
			}

			if (isValidation)
			{
				var allRulesValidation = new HashSet<RefCountryRules>();
				origin?.Rules.Where(r => r.R7_IsValidationRule).ForEach(r => allRulesValidation.Add(r));
				destination?.Rules.Where(r => r.R7_IsValidationRule).ForEach(r => allRulesValidation.Add(r));
				var filteredRulesValidation = FilterRules(origin, destination, transportMode, allRulesValidation);
				FormatTextForNotes(filteredRulesValidation.Where(r => !r.R7_Notes.IsEmpty && !ValidationRuleIsCancelled(notes.Parent, r.PK, r.Factory)), notes, false, true);
			}
		}

		public static bool ValidationRuleIsCancelled(IStmNoteParent parent, ZGuid rulePK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(GenCustomAddOnRuleAckSchema.XK_ParentID, parent.NotesParentPK);
			query.AddToFilter(GenCustomAddOnRuleAckSchema.XK_IsCancelled, true);
			query.AddToFilter(GenCustomAddOnRuleAckSchema.XK_RuleID, rulePK);
			return factory.Load(typeof(GenCustomAddOnRuleAck), query).Length > 0;
		}

		public static GenCustomAddOnRuleAck OverrideRule(IStmNoteParent parent, Logs logs, ZGuid rulePK, BusinessObjectFactory factory)
		{
			var ack = factory.New<GenCustomAddOnRuleAck>();
			ack.XK_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.NotesParentTableName);
			ack.XK_ParentID = parent.NotesParentPK;
			ack.XK_IsCancelled = true;
			ack.XK_RuleID = rulePK;
			if (logs != null)
			{
				var rule = factory.Load<RefCountryRules>(rulePK);
				var ruleAsString = rule != null ? rule.R7_Notes.ToString() : rulePK.ToString();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				logs.AddNew(Events.EditedARecord, (NoResString)"Rule " + ruleAsString + (NoResString)" was overridden.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			return ack;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static List<RefCountryRules> FilterRules(RefCountry origin, RefCountry destination, string transportMode, IEnumerable<RefCountryRules> allRules)
		{
			var result = new HashSet<RefCountryRules>();
			if (allRules != null)
			{
				var originCode = origin == null ? ZString.Empty : origin.Code;
				var destinationCode = destination == null ? ZString.Empty : destination.Code;
				transportMode = GetTransportMode(transportMode);
				if (string.IsNullOrEmpty(transportMode))
				{
					var rulesWithEmptyTransportMode = allRules.Where(r => string.IsNullOrEmpty(r.R7_TransportMode));

					rulesWithEmptyTransportMode.Where(r => r.R7_RN_NKOrigin == originCode && r.R7_RN_NKDestination == destinationCode).ForEach(r => result.Add(r));

					if (!result.Any())
					{
						rulesWithEmptyTransportMode.Where(r => (r.R7_RN_NKOrigin == originCode && r.R7_RN_NKDestination.IsEmpty) || (r.R7_RN_NKOrigin.IsEmpty && r.R7_RN_NKDestination == destinationCode)).ForEach(r => result.Add(r));
					}
				}
				else
				{
					allRules.Where(r => r.R7_RN_NKOrigin == originCode && r.R7_RN_NKDestination == destinationCode && r.R7_TransportMode == transportMode).ForEach(r => result.Add(r));

					if (!result.Any())
					{
						allRules.Where(r => r.R7_RN_NKOrigin == originCode && r.R7_RN_NKDestination == destinationCode && r.R7_TransportMode.IsEmpty).ForEach(r => result.Add(r));
						allRules.Where(r => r.R7_RN_NKOrigin == originCode && r.R7_RN_NKDestination.IsEmpty && r.R7_TransportMode == transportMode).ForEach(r => result.Add(r));
						allRules.Where(r => r.R7_RN_NKOrigin.IsEmpty && r.R7_RN_NKDestination == destinationCode && r.R7_TransportMode == transportMode).ForEach(r => result.Add(r));

						if (!result.Any())
						{
							allRules.Where(r => r.R7_RN_NKOrigin == originCode && r.R7_RN_NKDestination.IsEmpty && r.R7_TransportMode.IsEmpty).ForEach(r => result.Add(r));
							allRules.Where(r => r.R7_RN_NKOrigin.IsEmpty && r.R7_RN_NKDestination == destinationCode && r.R7_TransportMode.IsEmpty).ForEach(r => result.Add(r));
						}
					}
				}
			}

			return result.ToList();
		}

		static string GetTransportMode(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.SeaAir:
				case Core.Constants.TransportModes.AirSea:
					return Core.Constants.TransportModes.Other;
			}

			return transportMode;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to be translated")]
		static void FormatTextForNotes(IEnumerable<RefCountryRules> rules, Notes notes, bool isClientVisible, bool validation = false)
		{
			var noteDescription = validation ? PredefinedNoteTypes.Instance.CountryRulesValidation.Description : (isClientVisible
				? PredefinedNoteTypes.Instance.CountryRules.Description
				: PredefinedNoteTypes.Instance.CountryRulesInternal.Description);

			var existingNotes = notes.FindByDescription(noteDescription).ToList();
			var ruleList = rules.ToList();
			var newNotes = new List<string>();

			if (validation)
			{
				foreach (var rule in ruleList)
				{
					var noteText = new ZStringBuilder();
					noteText.AppendLine(FormattableString.Invariant($"<RulePK>{rule.PK}</RulePK>"));
					noteText.AppendLine(FormattableString.Invariant($"<Reason>{rule.Origin?.Description ?? (NoResString)"Anywhere"} to {rule.Destination?.Description ?? "Anywhere"}:</Reason>"));
					noteText.AppendLine(FormattableString.Invariant($"<IsError>{rule.R7_IsError}</IsError>"));
					noteText.AppendLine(FormattableString.Invariant($"<Macro>{rule.R7_Notes}</Macro>"));
					newNotes.Add(noteText.ToString());
				}
			}
			else
			{
				if (existingNotes.Count > 0)
				{
					//Maintain parity with old behaviour in this case and don't attempt to add/remove/delete notes
					return;
				}
				if (ruleList.Any())
				{
					var notesText = new ZStringBuilder();

					ruleList.GroupBy(
						rule => FormattableString.Invariant($"{rule.Origin?.Description ?? "Anywhere"} to {rule.Destination?.Description ?? "Anywhere"}:"),
						rule => rule.R7_Notes).ForEach(
						gg =>
						{
							notesText.AppendLine(gg.Key);
							gg.ForEach(g => notesText.AppendLine(FormattableString.Invariant($"  {g}")));
						});

					newNotes.Add(notesText.ToString());
				}
			}

			for (var i = newNotes.Count - 1; i >= 0; --i)
			{
				var noteText = newNotes[i];
				var matchingOldNote = existingNotes.FirstOrDefault(x => x.ST_NoteText == noteText);
				if (matchingOldNote != null)
				{
					newNotes.RemoveAt(i);
					existingNotes.Remove(matchingOldNote);
				}
			}
			foreach (var note in existingNotes)
			{
				note.Delete();
			}
			foreach (var noteText in newNotes)
			{
				var result = notes.AddNew(false, noteDescription, noteText);
				if (validation)
				{
					result.SetReadOnlyIncludingChildren(true);
					result.RunPreSaveValidation();
				}
			}
		}
	}
}
