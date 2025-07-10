// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1093:Do Not Use System.Windows.Forms.ToolStrip Controls", Justification = "Suppressed for Winzor Tests, because we need to test actual toolstrip controls implemented by us", Scope = "namespaceanddescendants", Target = "System.Windows.Forms")]
[assembly: SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Want to keep the original Winforms test names", Scope = "module")]
