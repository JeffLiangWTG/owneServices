using System.Diagnostics.CodeAnalysis;
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "Enterprise.TransportCommon.Business.IDtbTransportConsolidation", Justification = "Compile time identification of Consolidation")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "Enterprise.TransportCommon.Business.DtbTransport.#AdditionalSupportedAddressTypes", Justification = "Required in part to implement the IDocAddresses interface, which expects an array")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Enterprise.TransportCommon.Business.PackageDivotInstructionConfirmationRelationship`1.#.ctor(Enterprise.TransportCommon.Business.DtbTransportInstructionPkgDivot)", Justification = "Code appears to be safe, which is a valid reason to ignore the warning")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.TransportCommon.Business.DtbTransport.TypeDecider")] // Enterprise/Product/Operations/TransportCommon/TransportCommon.Business/DtbTransport/DtbTransport.cs:71,49
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.TransportCommon.Business.DtbTransportConsolidation.TypeDecider")] // Enterprise/Product/Operations/TransportCommon/TransportCommon.Business/DtbTransportConsolidation/DtbTransportConsolidation.cs:46,62
