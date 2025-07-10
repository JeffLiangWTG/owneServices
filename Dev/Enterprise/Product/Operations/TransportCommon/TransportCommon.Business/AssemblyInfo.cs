using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.TransportCommon.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("Enterprise.TransportBookings.Business, PublicKey=" + CommonAssemblyInfo.PublicKey)]
