using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMatchApprovalType
	{
		protected OrgMatchApprovalType(string code, Type parentType, Type approvalType)
		{
			this.Code = code;
			this.ParentType = parentType;
			this.ApprovalType = approvalType;
		}

		public readonly string Code;
		public readonly Type ParentType;
		public readonly Type ApprovalType;

		public static OrgMatchApprovalType Empty
		{
			get
			{
				return new OrgMatchApprovalType(
					"",
					null,
					typeof(EmptyOrgMatchApproval));
			}
		}

		public static OrgMatchApprovalType AirCargoConsignee
		{
			get
			{
				return new OrgMatchApprovalType(
					OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignee,
					ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>(),
					ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWBConsigneeMatchApproval>());
			}
		}

		public static OrgMatchApprovalType AirCargoConsignor
		{
			get
			{
				return new OrgMatchApprovalType(
					OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignor,
					ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>(),
					ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWBConsignorMatchApproval>());
			}
		}

		public static OrgMatchApprovalType AirCargoImporter
		{
			get
			{
				return new OrgMatchApprovalType(
					OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter,
					ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>(),
					ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWBImporterMatchApproval>());
			}
		}

#if DEBUG
		public static OrgMatchApprovalType DummyType
		{
			get
			{
				return new OrgMatchApprovalType(
					"DUM",
					typeof(DummyBusinessObject),
					typeof(Testing.DummyOrgMatchApproval));
			}
		}

		public static OrgMatchApprovalType DummyType2
		{
			get
			{
				return new OrgMatchApprovalType(
					"DUZ",
					typeof(DummyBusinessObject),
					typeof(Testing.DummyOrgMatchApproval));
			}
		}
#endif

		public static OrgMatchApprovalType FromCode(string code)
		{
			foreach (OrgMatchApprovalType type in All)
			{
				if (type.Code == code && !string.IsNullOrEmpty(code))
				{
					return type;
				}
			}

			return OrgMatchApprovalType.Empty;
		}

		public static OrgMatchApprovalType[] All
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(Empty);
				result.Add(AirCargoConsignee);
				result.Add(AirCargoConsignor);
				result.Add(AirCargoImporter);
#if DEBUG
				result.Add(DummyType);
				result.Add(DummyType2);
#endif
				if (AdditionalClientSpecificMatchApprovalTypes != null)
				{
					result.AddRange(AdditionalClientSpecificMatchApprovalTypes);
				}
				return (OrgMatchApprovalType[])result.ToArray(typeof(OrgMatchApprovalType));
			}
		}
		[ThreadStatic]
		protected static OrgMatchApprovalType[] AdditionalClientSpecificMatchApprovalTypes;
	}
}
