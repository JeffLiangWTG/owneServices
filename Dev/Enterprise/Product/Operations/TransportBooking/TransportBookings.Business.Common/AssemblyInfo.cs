using System.Runtime.CompilerServices;

#if DEBUG
[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.TransportCommon.Shared.TransportConsolidationJobTypes))]
[assembly: InternalsVisibleTo("Enterprise.TransportBookings.Shared.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
