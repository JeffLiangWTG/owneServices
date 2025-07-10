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
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.WhsDocketJobPivot.#.ctor(CargoWise.EntityFramework.BusinessObjectFactory,System.Data.DataRow)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine.#.ctor(Enterprise.Warehouse.Transactions.Business.WhsOrderLine)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.ILineWithCommittedPickLines.#PerPackageQty")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.ILineWithMatchingLinesExtensions.#SplitIntoMatchingLine`1(!!0,CargoWise.Types.ZDecimal)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.WhsInventoryQuery.#.ctor(CargoWise.EntityFramework.BusinessObjectFactory,System.Collections.Generic.IEnumerable`1<Enterprise.Warehouse.Transactions.Business.WhsInventoryView>)")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.WhsDocketLineFetchStrategy.#FetchForViewCore(CargoWise.EntityFramework.TableColumn[])")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.WhsModuleInventoryFetchStrategy.#FetchForViewCore(CargoWise.EntityFramework.TableColumn[])")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.WhsModuleInventoryFetchStrategy.#FetchForViewCore(CargoWise.EntityFramework.TableColumn[])")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.Printing.WhsDocumentPrinter.#LastPrintedDocumentName")]
[assembly: SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.SortByPropertiesComparer`1.#GetElementaryComparers()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.SortPickInventoryForPicking.#GetEntryDateComparer()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.Common.ExtensionMethods.#SynchroniseCachedBusinessObjectsWithDB`1(CargoWise.EntityFramework.BusinessObjectFactory,System.Boolean)")]
[assembly: SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Scope = "member", Target = "Enterprise.Warehouse.Transactions.Business.PutawayEngineManagerForVASTransferLine+<AddInputFactsFromLineCore>d__1.#MoveNext()", Justification = "Used as identifier, should not be translated.")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in ReadOnly attributes", Scope = "member", Target = "~P:Enterprise.Warehouse.Transactions.Business.WhsPick.StandardReadOnly")]
[assembly: SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Baseline WI00586871", Scope = "member", Target = "~M:Enterprise.Warehouse.Transactions.Business.Testing.UniqueNameForFatDatTest.GetUniqueNames(CargoWise.Schema.SchemaColumn)")] // Enterprise/Product/Operations/Warehouse/Transactions/Warehouse.Transactions.Business/Testing/WhsTestHelperFunctions.cs:5034,23
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Warehouse.Transactions.Business.WhsPickableDocketLine.TypeDecider")] // Enterprise/Product/Operations/Warehouse/Transactions/Warehouse.Transactions.Business/WhsPickableDocketLine/WhsPickableDocketLine.cs:47,62
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Warehouse.Transactions.Business.WhsDocketLine.TypeDecider")] // Enterprise/Product/Operations/Warehouse/Transactions/Warehouse.Transactions.Business/WhsDocketLine/WhsDocketLine.cs:116,50
