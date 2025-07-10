using System;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentCustomsEntryNumber : ShipmentCustomsEntryNumber
	{
		public CommonShipmentCustomsEntryNumber(CommonShipment shipment)
			: base(shipment)
		{
		}

		#region EntryType

		protected override ZString GetEntryType()
		{
			var cusEntryNumbers = Shipment.CusEntryNumbers;
			if (cusEntryNumbers.Count == 1)
			{
				customsEntryNumberType = cusEntryNumbers[0].CE_EntryType;
			}
			else if (cusEntryNumbers.Count >= 1)
			{
				if (cusEntryNumbers.Cast<CusEntryNumber>().Select(x => x.CE_EntryType).Distinct().Count() == 1)
				{
					customsEntryNumberType = cusEntryNumbers[0].CE_EntryType;
				}
			}

			var numberType = ZString.Empty;
			if (customsEntryNumberType == ZString.Empty && cusEntryNumbers.Count <= 1)
			{
				numberType = DefaultEntryType;
			}
			else
			{
				numberType = customsEntryNumberType;
			}

			return CusEntryNumber.GetEntryNumberTypeForDisplay(numberType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Shipment.IsImport());
		}

		protected override bool EntryType_ReadOnly
		{
			get
			{
				var cusEntryNumbers = Shipment.CusEntryNumbers;
				if (cusEntryNumbers.Count > 1)
				{
					return true;
				}
				else if (cusEntryNumbers.Count == 1)
				{
					var entryNum = cusEntryNumbers[0];
					return IsProtectedEntryType(entryNum) || (!entryNum.CE_ParentTable.IsEmpty && entryNum.CE_ParentTable != JobShipmentSchema.Constants.TableName);
				}

				return false;
			}
		}

		bool IsProtectedEntryType(CusEntryNumber entryNum)
		{
			return entryNum.CE_EntryIsSystemGenerated || entryNum.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CTN;
		}

		protected virtual ZString DefaultEntryType
		{
			get { return CusEntryNumberTypes.CountrySpecificDefaultEntryNumberType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Shipment.UseImportEntryTypeList); }
		}

		#endregion

		#region EntryNumber

		protected override ZString GetEntryNumber()
		{
			ZString result = ZString.Empty;

			var cusEntryNumbers = Shipment.CusEntryNumbers;
			if (cusEntryNumbers.Count == 1)
			{
				result = cusEntryNumbers[0].CE_EntryNum;
			}
			else if (cusEntryNumbers.Count > 1)
			{
				StringBuilder builder = new StringBuilder();

				foreach (CusEntryNumber entryNumber in cusEntryNumbers)
				{
					if (!entryNumber.CE_EntryNum.IsEmpty)
					{
						if (builder.Length != 0)
						{
							builder.Append(", ");
						}
						builder.Append(entryNumber.CE_EntryNum);
					}
				}

				result = builder.ToString();
			}

			return result;
		}

		protected override bool EntryNumber_ReadOnly
		{
			get { return EntryType_ReadOnly || IsExemptionCode(EntryType); }
		}

		#endregion

		#region IssueDate

		protected override ZDateTime GetIssueDate()
		{
			var cusEntryNumbers = Shipment.CusEntryNumbers;
			return cusEntryNumbers.Count == 1 ? cusEntryNumbers[0].CE_IssueDate : ZDateTime.Empty;
		}

		protected override bool IssueDate_ReadOnly
		{
			get { return EntryNumber_ReadOnly || Shipment.CusEntryNumbers.Count != 1 || EntryNumber == ZString.Empty; }
		}

		#endregion

		#region ExpiryDate

		protected override ZDateTime GetExpiryDate()
		{
			var cusEntryNumbers = Shipment.CusEntryNumbers;
			return cusEntryNumbers.Count == 1 ? cusEntryNumbers[0].CE_ExpiryDate : ZDateTime.Empty;
		}

		protected override bool ExpiryDate_ReadOnly
		{
			get { return EntryNumber_ReadOnly || Shipment.CusEntryNumbers.Count != 1 || EntryNumber == ZString.Empty; }
		}

		#endregion

		#region Implementation

		protected override CusEntryNumber GetCusEntryNumber()
		{
			CusEntryNumber entryNumber = null;

			var cusEntryNumbers = Shipment.CusEntryNumbers;
			if (cusEntryNumbers.Count == 1)
			{
				entryNumber = cusEntryNumbers[0];
			}
			else if (cusEntryNumbers.Count == 0)
			{
				entryNumber = cusEntryNumbers.AddNew();
				entryNumber.CE_ParentTable = Shipment.TableName;
				entryNumber.CE_EntryIsSystemGenerated = false;
				entryNumber.CE_ParentID = Shipment.PK;
				entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			else
			{
				throw new InvalidOperationException("Customs Entry Number should be Read Only when more than one Declaration numbers exist");
			}

			return entryNumber;
		}

		#endregion
	}
}
