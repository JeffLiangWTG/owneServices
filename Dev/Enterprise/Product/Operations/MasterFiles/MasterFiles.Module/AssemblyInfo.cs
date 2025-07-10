using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Master Files GUI Module")]
[assembly: AssemblyDescription("Master Files GUI")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.MasterFiles.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.MasterFiles.Module.Test.Winzor, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
