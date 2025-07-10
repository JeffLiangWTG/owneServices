using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityValueLookups : AutoOrgOpportunityValueLookups
	{
		public OrgOpportunityValueLookups(AutoOrgOpportunityValue parent) : base(parent)
		{
		}

		#region Value Types

		public CodeDescriptionPairList ActiveValueTypes
		{
			get
			{
				if (Parent != null && Factory != null)
				{
					return Factory.GetCachedValue("Enterprise.MasterFiles.Business.OrgOpportunityValueLookups.ActiveValueTypes", () => GetActiveValueTypesList());
				}
				else
				{
					return GetActiveValueTypesList();
				}
			}
		}

		public CodeDescriptionPairList ValueTypes
		{
			get
			{
				if (Parent != null && Factory != null)
				{
					return Factory.GetCachedValue("Enterprise.MasterFiles.Business.OrgOpportunityValueLookups.ValueTypes", () => GetValueTypesList());
				}
				else
				{
					return GetValueTypesList();
				}
			}
		}

		protected virtual CodeDescriptionPairList GetActiveValueTypesList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("CFS", Res.GetString("8ded53ac-616d-45b4-8dab-403a927ebb2f", "CFS Value"));
			result.AddPair("OTH", Res.GetString("e471cc2d-6dbc-44f9-9d5e-1b5d2f7eb0b2", "Other Value"));
			return result;
		}

		protected virtual CodeDescriptionPairList GetValueTypesList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("FOR", Res.GetString("2a0c6136-4ffb-43f9-9f18-51ffde8e7c92", "Forwarding Value"));
			list.AddPair("BRK", Res.GetString("4a2a2620-60a3-49b6-a635-ee9f52243aef", "Customs Brokerage Value"));
			list.AddPair("TRN", Res.GetString("1e3a86d3-04bd-451c-b522-6bb77c7077cf", "Port Transport Value"));
			list.AddPair("CFS", Res.GetString("8ded53ac-616d-45b4-8dab-403a927ebb2f", "CFS Value"));
			list.AddPair("WAR", Res.GetString("c638be24-e9ac-4965-a2c0-c6c9b52cd612", "Warehouse Value"));
			list.AddPair("SHP", Res.GetString("d9fe56d1-b795-4098-8052-a48a26cee4e9", "Ships Agency Value"));
			list.AddPair("OTH", Res.GetString("e471cc2d-6dbc-44f9-9d5e-1b5d2f7eb0b2", "Other Value"));

			return list;
		}

		#endregion

		#region Discount Basis

		public CodeDescriptionPairList DiscountBasisList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.MasterFiles.Business.OrgOpportunityValueLookups.DiscountBasisList", () =>
					{
						var discountBasisList = new CodeDescriptionPairList();
						discountBasisList.AddPair(DiscountBasis.Percent, Res.GetString("fe1cbda8-f470-4332-a7ed-312b41b47a59", "Percentage Discount"));
						discountBasisList.AddPair(DiscountBasis.Flat, Res.GetString("8db24406-43e7-41bb-ab97-0d8380b0bcb5", "Flat Discount"));
						return discountBasisList;
					});
			}
		}

		public static class DiscountBasis
		{
			public const string Percent = "PCT";
			public const string Flat = "FLT";
		}

		#endregion
	}
}
