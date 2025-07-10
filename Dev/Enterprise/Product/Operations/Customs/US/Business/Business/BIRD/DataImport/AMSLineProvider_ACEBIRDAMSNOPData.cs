using System;
using System.Collections.Generic;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class AMSLineProvider_ACEBIRDAMSNOPData
	{
		public AEPAPG10 PG10 { get; set; }

		public Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>> OrganizationsDict => organizationsDict;
		readonly Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>> organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

		public AEPAPG25 PG25 { get; set; }

		public bool ORCHaveBeenAssigned { get; set; }

		public bool ORPHaveBeenAssigned { get; set; }
	}
}
