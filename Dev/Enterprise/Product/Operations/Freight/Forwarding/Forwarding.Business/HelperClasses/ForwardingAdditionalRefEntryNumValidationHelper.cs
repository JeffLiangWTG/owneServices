using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	static class ForwardingAdditionalRefEntryNumValidationHelper
	{
		public static bool IsRUCFormatValid(ZString entryNumber)
		{
			// Legend: <description, length, type (n=numeric, a=alphabetic, an=alphanumeric)>
			// CPF: <year, 1, n><country, 2, a><shipper, 11, n><decade, 1, n><reference, 1 to 20, an>
			// CNPJ: <year, 1, n><country, 2, a><shipper, 8, n><decade, 1, n><reference, 1 to 23, an>

			var matchesCPF = Regex.IsMatch(entryNumber, @"^[0-9][a-zA-Z]{2}[0-9]{11}[0-9][a-zA-Z0-9]{1,20}$");
			if (!matchesCPF)
			{
				var matchesCNPJ = Regex.IsMatch(entryNumber, @"^[0-9][a-zA-Z]{2}[0-9]{8}[0-9][a-zA-Z0-9]{1,23}$");
				return matchesCNPJ;
			}

			return true;
		}

		public static BusinessObject GetDuplicateRUCParent(CusEntryNumber entryNumber)
		{
			if (entryNumber.Factory.LoadTop1<CusEntryNumber>(GetDuplicateNumQuery(entryNumber)) is CusEntryNumber duplicateEntryNumber)
			{
				switch (duplicateEntryNumber.CE_ParentTable)
				{
					case JobShipmentSchema.Constants.TableName:
						return entryNumber.Factory.Load<ForwardingShipment>(duplicateEntryNumber.CE_ParentID);

					case JobConsolSchema.Constants.TableName:
						return entryNumber.Factory.Load<ForwardingConsol>(duplicateEntryNumber.CE_ParentID);
				}
			}

			return null;
		}

		static ZQuery GetDuplicateNumQuery(CusEntryNumber entryNumber)
		{
			return new ZQuery(CusEntryNumSchema.PK, SQLComparisonOperator.NotEqual, entryNumber.PK)
						.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber)
						.AddToFilter(CusEntryNumSchema.CE_EntryType, entryNumber.CE_EntryType)
						.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber.CE_EntryNum)
						.AddToFilter(CusEntryNumSchema.CE_ParentTable, entryNumber.CE_ParentTable)
						.AddToFilter(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, entryNumber.CE_ParentID); //reasoning is that, if the xml import happens, this shouldnt be the point of failure imo (should be the uniqueness validation)
		}
	}
}
