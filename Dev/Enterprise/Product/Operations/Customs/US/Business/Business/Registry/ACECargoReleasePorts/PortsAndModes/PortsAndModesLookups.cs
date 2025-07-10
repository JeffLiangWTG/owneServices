using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public class PortsAndModesLookups : ZLookups
	{
		public PortsAndModesLookups(PortsAndModes parent)
			: base(parent)
		{
		}

		public TransportTypeList TransportModeList
		{
			get { return transportModeList ?? (transportModeList = new TransportTypeList()); }
		}
		TransportTypeList transportModeList;

		public ZZRefCusCodeListCombinedCollection PortList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(new BusinessObjectFactory(), Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public CertificationOptionsList CertificationMethodsList
		{
			get { return certificationOptionsList ?? (certificationOptionsList = new CertificationOptionsList()); }
		}
		CertificationOptionsList certificationOptionsList;

		protected new PortsAndModes Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PortsAndModes)base.Parent; }
		}
	}
}
