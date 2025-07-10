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

[assembly: SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Scope = "member", Target = "Enterprise.TransportBookings.Business.BookingToTransportJobCommonCreator.#FailureContextToFakeWhenProcessingFailures", Justification = "For test only.")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions.#.ctor(CargoWise.EntityFramework.BusinessObjectFactory,Enterprise.TransportCommon.Shared.DtbBookingDirection)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions.#.ctor(Enterprise.TransportBookings.Shared.IDtbBookingParent,CargoWise.EntityFramework.BusinessObjectFactory,Enterprise.UniversalDataBuss.Integration.DataContextType,Enterprise.TransportCommon.Shared.DtbBookingDirection,CargoWise.Types.ZBool,CargoWise.Types.ZString,CargoWise.Types.ZString,System.Boolean)")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Moving of existing code", Scope = "member", Target = "~F:Enterprise.TransportBookings.Business.DtbBooking.TypeDecider")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Moving of existing code", Scope = "member", Target = "~F:Enterprise.TransportBookings.Business.DtbBookingConsolidation.TypeDecider")]
[assembly: SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Moving of exising code", Scope = "member", Target = "~M:Enterprise.TransportBookings.Business.DtbBooking.DefaultFromTemplate")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ReadOnly method", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingConfirmation.KK_Actual_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ReadOnly method", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingConfirmation.KK_Estimated_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ReadOnly method", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingConfirmation.KK_Quantity_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ReadOnly method", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingConfirmation.KK_ReceivedBy_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ReadOnly method", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingConfirmation.KK_ReferenceNum_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Should be a ReadOnly method, not sure if it takes priority over IsSystem", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl.K2_IsLooseRateable_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Should be a ReadOnly method, not sure if it takes priority over IsSystem", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl.K2_IsContainerRateable_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ReadOnly method", Scope = "member", Target = "~P:Enterprise.TransportBookings.Business.DtbBookingTmpl.KT_IsSystem_ReadOnly")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Technical debt: This code is never called. Should it be called?", Scope = "member", Target = "~M:Enterprise.TransportBookings.Business.DtbBookingConfirmationValidation.CheckParentID_InstructionOrPackageDivot")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Technical debt: This code is never called. Should it be called?", Scope = "member", Target = "~M:Enterprise.TransportBookings.Business.DtbBookingConfirmationValidation.CheckConfirmationDescription")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Technical debt: This code is never called. Should it be called?", Scope = "member", Target = "~M:Enterprise.TransportBookings.Business.DtbBookingInstructionValidation.CheckActual")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Technical debt: This code is never called. Should it be called?", Scope = "member", Target = "~M:Enterprise.TransportBookings.Business.DtbBookingInstructionValidation.CheckEstimated")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Technical debt: This code is never called. Should it be called?", Scope = "member", Target = "~M:Enterprise.TransportBookings.Business.DtbBookingInstructionValidation.CheckOrganisationType")]
