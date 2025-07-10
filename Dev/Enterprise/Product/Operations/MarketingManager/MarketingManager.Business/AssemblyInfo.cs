using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.MarketingManager.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.MarketingManager.ServiceTask.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
