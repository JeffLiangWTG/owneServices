// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Enterprise.Globalization", "EDI007", Scope = "member", Target = "Enterprise.ProductionRules.Business.CW1UserDefinedPropertyLoader.#GetName(Enterprise.MasterFiles.Business.CustomValues.GenCustomColumnDefinition)", Justification = "Used as identifier")]
[assembly: SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Integer value from registry", Scope = "member", Target = "~M:Enterprise.ProductionRules.Business.CW1ProductionRulesEnginePullService.GetRulesEngineSessionCacheMinutes~System.Int32")]
