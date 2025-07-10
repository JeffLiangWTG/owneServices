// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Reading from web.config so we need a primitive type", Scope = "member", Target = "~F:Enterprise.Services.Scim.Api.Config.AppSettings.throttleTimeInSeconds")]
[assembly: SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Reading from web.config so we need a primitive type", Scope = "member", Target = "~P:Enterprise.Services.Scim.Api.Config.AppSettings.ThrottleTimeInSeconds")]
[assembly: SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Reading from web.config so we need a primitive type", Scope = "member", Target = "~P:Enterprise.Services.Scim.Api.Config.IAppSettings.ThrottleTimeInSeconds")]
[assembly: SuppressMessage("CargoWiseOne", "CW1061:Do not use System.DateTime.UtcNow Rule", Justification = "Its for local cache so we can use server time", Scope = "member", Target = "~M:Enterprise.Services.Scim.Api.Middlewares.ThrottlingMiddleware.Invoke(Microsoft.Owin.IOwinContext)~System.Threading.Tasks.Task")]
