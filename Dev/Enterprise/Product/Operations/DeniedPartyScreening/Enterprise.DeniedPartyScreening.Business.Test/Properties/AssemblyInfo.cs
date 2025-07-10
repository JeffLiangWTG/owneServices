using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Enterprise DeniedPartyScreening Business Test Layer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

#pragma warning disable CS0436 // Type conflicts with imported type
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DeniedPartyScreening.GUI.Test,  PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.DeniedPartyScreening.ServiceTasks.Test,  PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
#pragma warning restore CS0436 // Type conflicts with imported type
