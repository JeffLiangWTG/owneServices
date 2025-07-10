using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusBondDetail : Enterprise.MasterFiles.Business.CusBondDetail
	{
		public CusBondDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusEntryInstruction EntryInstruction
		{
			get
			{
				if (entryInstruction == null || entryInstruction.IsDeleted)
				{
					entryInstruction = Factory.Load<CusEntryInstruction>(PW_ParentID);
				}
				return entryInstruction;
			}
		}
		CusEntryInstruction entryInstruction;

		Customs.Business.BaseCusGuaranteeHeader Guarantee => Factory.Load<Customs.Business.BaseCusGuaranteeHeader>(PW_CPH_Guarantee);

		public new CusBondDetailLookups Lookups => (CusBondDetailLookups)base.Lookups;

		protected override Enterprise.MasterFiles.Business.CusBondDetailLookups GetNewLookups() => new CusBondDetailLookups(this);

		public new CusBondDetailValidation Validation => (CusBondDetailValidation)base.Validation;

		protected override Enterprise.MasterFiles.Business.CusBondDetailValidation GetNewValidation() => new CusBondDetailValidation(this);

		[ResourceStringData("08250DB2-2C06-4957-AA99-0FE8B37521C7", Caption = "Guarantee Existing")]
		[List(nameof(Lookups) + "." + nameof(CusBondDetailLookups.GuaranteeList))]
		public override ZGuid PW_CPH_Guarantee
		{
			get => base.PW_CPH_Guarantee;
			set
			{
				var oldValue = base.PW_CPH_Guarantee;
				base.PW_CPH_Guarantee = value;
				if (oldValue != PW_CPH_Guarantee && !IsCopying)
				{
					PW_BondType = Guarantee?.CPH_Type ?? ZString.Empty;
					PW_BondNumber = Guarantee?.CPH_Number ?? ZString.Empty;
				}
			}
		}

		[ResourceStringData("1577AC55-BB46-40D1-9557-B01E9B71791F", Caption = "Guarantee Type")]
		[List(nameof(Lookups) + "." + nameof(CusBondDetailLookups.BondTypeList))]
		public override ZString PW_BondType { get => base.PW_BondType; set => base.PW_BondType = value; }

		[ResourceStringData("1577AC55-BB46-40D1-9557-B01E9B717911", Caption = "Guarantee No")]
		[MaxLength(30)]
		public override ZString PW_BondNumber { get => base.PW_BondNumber; set => base.PW_BondNumber = value; }

		[ResourceStringData("1577AC55-BB46-40D1-9557-B01E9B717912", Caption = "Guarantee Amount")]
		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;
			set
			{
				var oldValue = base.PW_BondAmount;
				base.PW_BondAmount = value;
				if (oldValue != PW_BondAmount && !IsCopying)
				{
					if (shouldCalculateRatioWithAmount)
					{
						EntryInstruction?.CalculateRatioWithAmount();
					}
				}
			}
		}

		internal void CalculateAmountWithRatio()
		{
			lock (calculationLock)
			{
				shouldCalculateRatioWithAmount = false;
				if (EntryInstruction != null)
				{
					PW_BondAmount = ZArchitecture.Core.Utilities.Round(EntryInstruction.ZG_DedicatedGuaranteeAmount * EntryInstruction.ZG_GuaranteeRatio * 0.01m, 2);
				}
				shouldCalculateRatioWithAmount = true;
			}
		}

		readonly object calculationLock = new object();
		bool shouldCalculateRatioWithAmount = true;

		[ResourceStringData("1577AC55-BB46-40D1-9557-B01E9B717913", Caption = "Guarantee Description")]
		[MaxLength(100)]
		public override ZString PW_GuaranteeDescription
		{
			get
			{
				var result = base.PW_GuaranteeDescription;
				if (result.IsEmpty)
				{
					var bondAmount = PW_BondAmount;
					result = string.Join(" ", PW_BondType, PW_BondNumber, bondAmount.IsEmpty ? string.Empty : bondAmount.ToString(".00")).Trim();
				}
				return result;
			}
			set => base.PW_GuaranteeDescription = value;
		}
	}
}
