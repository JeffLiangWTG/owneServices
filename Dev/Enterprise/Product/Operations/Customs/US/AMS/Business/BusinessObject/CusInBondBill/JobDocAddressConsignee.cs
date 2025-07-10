using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.AMS.Business
{
	class JobDocAddressConsignee : JobDocAddressParty, IConsignee
	{
		public JobDocAddressConsignee(JobDocAddress docAddress)
			: base(docAddress, GetEntityIDCode(docAddress.DocAddressType))
		{
			switch (docAddress.DocAddressType)
			{
				case DocAddressType.ConsigneeAddress:
					typeCode = "U";
					break;
				case DocAddressType.IntermediateConsigneeAddress:
					typeCode = "I";
					break;
				default:
					break;
			}
		}

		static ZString GetEntityIDCode(DocAddressType addressType)
		{
			var result = ZString.Empty;
			switch (addressType)
			{
				case DocAddressType.ConsigneeAddress:
					result = EntityIDCodeList.Codes.Consignee;
					break;
				case DocAddressType.IntermediateConsigneeAddress:
					result = EntityIDCodeList.Codes.IntermediateConsignee;
					break;
				default:
					break;
			}
			return result;
		}

		#region IConsignee Members

		ZString IConsignee.TypeCode
		{
			get { return typeCode; }
		}
		readonly ZString typeCode;

		#endregion
	}
}
