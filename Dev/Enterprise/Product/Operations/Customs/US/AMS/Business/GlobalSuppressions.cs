// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1124:new CachedProperty", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.US.AMS.Business", Justification = "Properties using CachedProperty should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Baseline WI00842925", Scope = "member", Target = "~P:Enterprise.Customs.US.AMS.Business.CusInBondBill.Messages")]
[assembly: SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Baseline WI00842925", Scope = "member", Target = "~P:Enterprise.Customs.US.AMS.Business.OriginatorCodeChangedHandler.Branches")]
