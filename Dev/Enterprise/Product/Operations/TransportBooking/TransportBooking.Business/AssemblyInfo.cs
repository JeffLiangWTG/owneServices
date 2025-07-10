using System.Runtime.CompilerServices;
#if DEBUG
#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: InternalsVisibleTo("Enterprise.TransportBookings.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#pragma warning restore CS0436 // Type conflicts with imported type
#endif
