#if DEBUG
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo($"WinzorFramework.Test, PublicKey={CommonAssemblyInfo.PublicKey}")]
[assembly: InternalsVisibleTo($"WinzorTestFramework, PublicKey={CommonAssemblyInfo.PublicKey}")]
#endif
