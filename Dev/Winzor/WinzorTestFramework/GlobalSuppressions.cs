// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Test names do not need to have the Async suffix", Scope = "module")]
[assembly: SuppressMessage("CargoWiseOne", "CW1015:No Application.OpenForms Rule", Justification = "Non CW code", Scope = "module")]
[assembly: SuppressMessage("CargoWiseOne", "CW1017:Non DPI-aware code has been detected", Justification = "Winzor components should not be DPI aware", Scope = "module")]
