using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.TransportBookings.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.TransportBookings.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
