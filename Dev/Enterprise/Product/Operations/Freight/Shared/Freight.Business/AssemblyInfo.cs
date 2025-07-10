using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Core;
using WTG.StaticAnalysis.Annotation;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Freight Module")]
[assembly: AssemblyDescription("Freight Module")]
[assembly: AssemblyConfiguration("")]

[assembly: UsesConstants(typeof(TWAdditionalReferenceTypesForUniversalXML))]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Freight.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Freight.Business.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
