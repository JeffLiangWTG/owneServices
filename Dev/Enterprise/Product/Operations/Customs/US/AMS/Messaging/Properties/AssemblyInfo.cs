using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("US AMS Messaging")]
[assembly: AssemblyDescription("US AMS Messaging")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.US.AMS.Messaging.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
