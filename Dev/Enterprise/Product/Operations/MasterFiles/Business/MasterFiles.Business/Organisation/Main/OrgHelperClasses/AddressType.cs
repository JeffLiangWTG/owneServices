using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	#region Address Category Enumeration

	public enum AddressCategory
	{
		Physical,
		Mailing,
		Other
	}

	#endregion

	[ImmutableObject(true)]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class OrgAddressType
	{
		OrgAddressType(string code, MultilingualString description, AddressCategory category)
		{
			this.Code = code;
			this.Description = description;
			this.Category = category;

			if (codeLookUpTable == null)
			{
				codeLookUpTable = new Dictionary<string, OrgAddressType>();
			}
			codeLookUpTable.Add(code, this);
		}

		#region Address Types

		public static readonly OrgAddressType Postal = new OrgAddressType("PST", ResString.GetMultilingualString("7022910b-7d1a-4308-bb74-1a4a595afabf", "Postal Address"), AddressCategory.Mailing);
		public static readonly OrgAddressType Receivables = new OrgAddressType("ARM", ResString.GetMultilingualString("1867c4ea-28e3-4a94-a7da-1c9451a1b4ef", "Accounts Receivable Mailing"), AddressCategory.Mailing);
		public static readonly OrgAddressType Payables = new OrgAddressType("APM", ResString.GetMultilingualString("e2db3398-9aa7-41c1-b988-d778a4dfe2c9", "Accounts Payable Mailing"), AddressCategory.Mailing);
		public static readonly OrgAddressType Sales = new OrgAddressType("SQM", ResString.GetMultilingualString("c92acecc-1586-48a3-9b1d-f99629bc3c32", "Sales updates, quotes, special offers"), AddressCategory.Mailing);
		public static readonly OrgAddressType Office = new OrgAddressType("OFC", ResString.GetMultilingualString("bdfbd1cd-ccf1-4050-aaee-699e81db7d85", "Office Address"), AddressCategory.Physical);
		public static readonly OrgAddressType PickupAndDelivery = new OrgAddressType("PAD", ResString.GetMultilingualString("b51fe85b-25cc-4c8c-b796-5fa82e4fad58", "Pickup and Delivery address"), AddressCategory.Physical);
		public static readonly OrgAddressType Pickup = new OrgAddressType("PIC", ResString.GetMultilingualString("22ff4762-1349-4c65-b469-907546865ce3", "Pickup Address"), AddressCategory.Physical);
		public static readonly OrgAddressType Delivery = new OrgAddressType("DLV", ResString.GetMultilingualString("25deefcf-914b-4ec0-bfc0-4972448d544c", "Delivery Address"), AddressCategory.Physical);
		public static readonly OrgAddressType Miscellaneous = new OrgAddressType("MSC", ResString.GetMultilingualString("fb64612a-1f67-4b30-a03b-506321572d90", "Miscellaneous Address (no Automation)"), AddressCategory.Other);
		public static readonly OrgAddressType Residential = new OrgAddressType(OrgConstants.AddressType.Residential, OrgDescriptions.AddressType.ResidentialDescription, AddressCategory.Other);
		public static readonly OrgAddressType CustomsAddressOfRecord = new OrgAddressType(OrgConstants.AddressType.CustomsAddressOfRecord, OrgDescriptions.AddressType.CustomsAddressOfRecordDescription, AddressCategory.Other);
		public static readonly OrgAddressType AWB = new OrgAddressType("AWB", ResString.GetMultilingualString("e17d7414-101f-407d-b227-863add13681d", "AWB Address"), AddressCategory.Other);
		public static readonly OrgAddressType EUCustomsAddress = new OrgAddressType(OrgConstants.AddressType.EUCustomsAddress, OrgDescriptions.AddressType.EUCustomsAddressDescription, AddressCategory.Other);

		#endregion

		#region Implementation

		public static CodeDescriptionPairList CodePairList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (OrgAddressType addressType in codeLookUpTable.Values)
				{
					result.AddPair(addressType.Code, addressType.Description);
				}
				return result;
			}
		}

		public readonly string Code;
		public readonly MultilingualString Description;
		public readonly AddressCategory Category;

		public static IEnumerable<OrgAddressType> AddressTypeList
		{
			get
			{
				if (typeList == null)
				{
					typeList = new OrgAddressTypeList();
					typeList.AddRange(codeLookUpTable.Values);
				}
				return typeList;
			}
		}

		[SuppressThreadStaticFieldMessage]
		static OrgAddressTypeList typeList;

		[SuppressThreadStaticFieldMessage]
		static Dictionary<string, OrgAddressType> codeLookUpTable;

		public static OrgAddressType Find(string code)
		{
			OrgAddressType result;
			codeLookUpTable.TryGetValue(code, out result);
			return result;
		}

		public override string ToString()
		{
			return Code;
		}

		public static implicit operator string(OrgAddressType value)
		{
			return value == null ? null : value.Code;
		}

		#endregion
	}

	#region List Implementation

	public sealed class OrgAddressTypeList : List<OrgAddressType>
	{
	}

	#endregion
}
