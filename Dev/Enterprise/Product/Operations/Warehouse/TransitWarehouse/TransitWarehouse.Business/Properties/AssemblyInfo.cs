using System.Runtime.CompilerServices;
using Enterprise.Core;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(TWAdditionalReferenceTypesForUniversalXML))]
[assembly: CargoWiseOne.ResourceStrings.ResourceStringAssemblyId(18326)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Warehouse.Transit.Business.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
