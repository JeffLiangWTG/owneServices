using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class MergeOrgAddress : MergeOrgElement<OrgAddress>
	{
		public MergeOrgAddress(BusinessObjectFactory factory, OrgAddress oldAddr, OrgHeader newOrg)
			: this(factory, oldAddr, newOrg, null)
		{
			ValidateAction();
		}

		public MergeOrgAddress(BusinessObjectFactory factory, OrgAddress oldAddr, OrgHeader newOrg,
			BusinessObjectCollection newOrgAddressCollection)
			: base(factory, oldAddr, newOrg, newOrgAddressCollection)
		{
			ValidateAction();
		}

		#region Schema

		public static class Schema
		{
			public const string OldAddressPK = "OldAddressPK";
			public const string OldAddressCode = "OldAddressCode";
			public const string OldAddressAddress1 = "OldAddressAddress1";
			public const string OldAddressAddress2 = "OldAddressAddress2";
			public const string OldAddressCity = "OldAddressCity";
			public const string NewAddressPK = "NewAddressPK";
		}

		protected override SchemaColumn FKSchemaColumn
		{
			get
			{
				return OrgAddressSchema.OA_OH;
			}
		}

		protected override string SchemaNewObjectPK
		{
			get { return Schema.NewAddressPK; }
		}

		#endregion

		#region Properties

		#region OldAddressPK

		protected ZGuid oldAddressPK;

		public ZGuid OldAddressPK
		{
			get
			{
				return OldObjectCore.PK;
			}
		}

		public ZPropertyInfo OldAddressPKInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldAddressPK);
			}
		}

		#endregion

		#region OldAddressCode

		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString OldAddressCode
		{
			get
			{
				return OldObjectCore.OA_Code;
			}
		}

		public ZPropertyInfo OldAddressCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldAddressCode);
			}
		}

		#endregion

		#region OldAddressAddress1

		[MaxLength(OrgAddress.Schema.OA_Address1MaxLength)]
		public ZString OldAddressAddress1
		{
			get
			{
				return OldObjectCore.OA_Address1;
			}
		}

		public ZPropertyInfo OldAddressAddress1Info
		{
			get
			{
				return GetZPropertyInfo(Schema.OldAddressAddress1);
			}
		}

		#endregion

		#region OldAddressAddress2

		[MaxLength(OrgAddress.Schema.OA_Address1MaxLength)]
		public ZString OldAddressAddress2
		{
			get
			{
				return OldObjectCore.OA_Address2;
			}
		}

		public ZPropertyInfo OldAddressAddress2Info
		{
			get
			{
				return GetZPropertyInfo(Schema.OldAddressAddress2);
			}
		}

		#endregion

		#region OldAddressCity

		[MaxLength(OrgAddress.Schema.OA_CityMaxLength)]
		public ZString OldAddressCity
		{
			get
			{
				return OldObjectCore.OA_City;
			}
		}

		public ZPropertyInfo OldAddressCityInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldAddressCity);
			}
		}

		#endregion

		#region NewAddressPK
		[List("NewObjectsCollection")]
		public ZGuid NewAddressPK
		{
			get { return NewObjectPK; }
			set { NewObjectPK = value; }
		}

		public ZPropertyInfo NewAddressPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(nameof(NewAddressPK), x => NewObjectPKInfo);
			}
		}

		protected bool NewAddressPK_ReadOnly
		{
			get
			{
				return NewObjectPK_ReadOnly;
			}
		}

		#endregion

		#endregion

		#region overrides

		public override BusinessObjectCollection GetNewObjectsCollection(BusinessObjectFactory factory, ZQuery query)
		{
			return new OrgAddressCollection(factory, query);
		}

		protected override ZGuid FindFuzzyMatch()
		{
			return FindFuzzyMatch(NewObjectsCollection, new string[] { OldAddressAddress1, OldAddressAddress2, OldAddressCode });
		}

		public override ZGuid FindFuzzyMatch(IEnumerable<BusinessObject> collection, string[] stringToMatch)
		{
			ZGuid result = ZGuid.Empty;
			foreach (OrgAddress address in collection)
			{
				ZString addressToMatch = string.IsNullOrEmpty(stringToMatch[1]) ? stringToMatch[0] : stringToMatch[0] + ", " + stringToMatch[1];
				ZString oldAddressForComparison = AddressLineFuzzyMatch.GetStringForFuzzyComparing(addressToMatch);
				ZString newdAddressForComparison = AddressLineFuzzyMatch.GetStringForFuzzyComparing(address.OA_Address2.IsEmpty ? address.OA_Address1 : new ZString(address.OA_Address1 + ", " + address.OA_Address2));

				if (oldAddressForComparison.CompareTo(newdAddressForComparison) == 0 && oldAddressForComparison != "" && stringToMatch[2].CompareTo(address.OA_Code) == 0)
				{
					result = address.PK;
					break;
				}
			}

			return result;
		}

		protected override string ElementNameForAction
		{
			get
			{
				return Res.GetString("b834a087-5dbf-4f7d-a64c-9ace27aede85", "address");
			}
		}

		public override SchemaColumn[] ColumnsForMatching
		{
			get { return new SchemaColumn[] { OrgAddressSchema.OA_Address1, OrgAddressSchema.OA_Address2, OrgAddressSchema.OA_Code }; }
		}

		public override void ValidateAction()
		{
			base.ValidateAction();
			AddWarningIfAddressWillForceToBeMerged();
		}

		void AddWarningIfAddressWillForceToBeMerged()
		{
			if (NewOrganization != null)
			{
				if (Action == ActionAdd &&
					OrganisationMerger.GetExistingAddressPKInNewOrg(OldObjectCore, ((IDbConnected)Factory).Connection,
						NewOrganization.PK.ToGuid()).HasValue)
				{
					ActionInfo.AddWarning(ResString.GetMultilingualString("A05D49F2-C1EA-47CE-A8D3-F9E428CD83DD",
						"This address will be merged because an identical address exists on the target organization"));
				}
			}
		}

		#endregion
	}
}
