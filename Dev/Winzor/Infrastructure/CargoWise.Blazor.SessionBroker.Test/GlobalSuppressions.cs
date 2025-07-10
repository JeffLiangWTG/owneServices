// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "It's a mock MemoryStream for testing", Scope = "member", Target = "~M:CargoWise.Blazor.SessionBroker.Test.AffinityFailurePolicyTests.ShouldPromptMessageBoxWhenAuthorisationFailed~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>", Scope = "member", Target = "~M:CargoWise.Blazor.SessionBroker.Test.AffinityFailurePolicyTests.ShouldShowSiteOfflineMessage~System.Threading.Tasks.Task")]
