using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public static class CusInBondHeaderOperationalActionExtensionMethods
	{
		public static LogControllerLink GetInBondHeaderIdLink(this CusInBondHeader header)
		{
			var provider = (IControllerIDProvider)header;
			return new LogControllerLink(header.BH_JobReference, provider.ControllerID, provider.BusinessObjectPK);
		}
	}
}
