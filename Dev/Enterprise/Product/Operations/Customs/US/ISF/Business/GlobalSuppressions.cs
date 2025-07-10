// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1124:new CachedProperty", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.US.ISF.Business", Justification = "Properties using CachedProperty should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Customs.US.ISF.Business.CusISFHeader.TypeDecider")] // Enterprise/Product/Operations/Customs/US/ISF/Business/Header/CusISFHeader.cs:102,49
[assembly: SuppressMessage("CargoWiseOne", "EDI003:Business Object Property Max Length Validation", Justification = "Baseline WI00752625", Scope = "member", Target = "~M:Enterprise.Customs.US.ISF.Business.CusISFHeader.UpdateImporterCodeDetailsIfNeeded")] // Enterprise/Product/Operations/Customs/US/ISF/Business/Header/CusISFHeader.cs:2032,25
