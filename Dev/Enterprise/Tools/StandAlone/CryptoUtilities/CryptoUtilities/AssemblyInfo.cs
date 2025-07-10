using System.Reflection;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Crypto Utility Module")]
[assembly: AssemblyDescription("Crypto Utilities")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CryptoUtilities.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
