using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Enterprise.Freight.Forwarding.Documents.DataTransfer, PublicKey=" + CommonAssemblyInfo.PublicKey)]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Freight.Forwarding.Documents.DataObjects.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Freight.Forwarding.Documents.DataTransfer.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Freight.Forwarding.Documents.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
