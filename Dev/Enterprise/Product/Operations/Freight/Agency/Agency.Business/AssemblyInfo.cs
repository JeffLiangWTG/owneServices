
using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Agency Business Module")]
[assembly: AssemblyDescription("Agency Business Module")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Freight.Agency.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Freight.Agency.DataTransfer.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
