using System.Diagnostics.CodeAnalysis;
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Scope = "member", Target = "Enterprise.Packing.Business.PkgPackage.#IsAvailableForPacking(CargoWise.Types.ZString&)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Scope = "member", Target = "Enterprise.Packing.Business.PkgPackage.#IsAvailableForUnpacking(CargoWise.Types.ZString&)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member", Target = "Enterprise.Packing.Business.SelectionHelper.#ItemsToUnpackBasedOnSelected(CargoWise.Types.ZString,CargoWise.Types.ZString&,System.Boolean)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Scope = "member", Target = "Enterprise.Packing.Business.PkgPackage.#IsValidCandidateForInner(CargoWise.Types.ZString&,System.Boolean)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Cancelled", Scope = "member", Target = "Enterprise.Packing.Business.PkgPackage.#PackTypeChangingFromContainerCancelled")] // Australian English.
[assembly: SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Scope = "member", Target = "Enterprise.Packing.Business.PkgPackage.#IsBreakDownAllowed(CargoWise.Types.ZString&)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Enterprise.Packing.Business.PkgPackageItemDivotCustomPropertiesHelper.#GetCustomProperties`1(Enterprise.Packing.Business.IPackableItemParent)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1006:DoNotExposeGenericLists", Scope = "member", Target = "Enterprise.Packing.Business.IPackingParent.#GetAdditionalEventContextValuesFromParent()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope = "member", Target = "Enterprise.Packing.Business.YesNoWithReasonForNo.#op_Implicit(Enterprise.Packing.Business.YesNoWithReasonForNo):System.Boolean")]
[assembly: SuppressMessage("Microsoft.Design", "CA1006:DoNotExposeGenericLists", Scope = "member", Target = "Enterprise.Packing.Business.IPackingParentWithOutturn.#GetPackagesUnpackedFromContainers()")]
[assembly: SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Auto generated baseline suppressions - WI00545660", Scope = "member", Target = "~M:Enterprise.Packing.Business.PkgPackageJob.PreventSaveIfFinalised")] // C:\git\wtg\CargoWise\Dev\Enterprise\Product\Operations\Packing\Packing.Business\PkgPackageJob\PkgPackageJob.cs:1343:18
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Packing.Business.PkgPackageJob.TypeDecider")] // Enterprise/Product/Operations/Packing/Packing.Business/PkgPackageJob/PkgPackageJob.cs:43,50
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Packing.Business.PkgPackage.TypeDecider")] // Enterprise/Product/Operations/Packing/Packing.Business/PkgPackage/PkgPackage.cs:113,47
