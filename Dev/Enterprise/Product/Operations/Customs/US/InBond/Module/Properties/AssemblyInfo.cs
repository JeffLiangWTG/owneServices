using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ediEnterprise US InBond - Module Logic")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("68537fd0-57c5-41b3-8438-3301dc892d48")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.InBond.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
