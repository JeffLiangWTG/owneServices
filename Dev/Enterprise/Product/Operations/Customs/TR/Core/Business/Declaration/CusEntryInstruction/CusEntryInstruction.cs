using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryInstruction : AutoCusEntryInstruction, Integration.Customs.TR.ICusEntryInstruction
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AddInfoCusEntryInstruction AddInfo => (AddInfoCusEntryInstruction)base.AddInfo;

		protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(CEI_AddInfoInfo);

		public new AddInfoCusEntryInstructionLookups AddInfoLookups => AddInfo.Lookups;

		public CusBondDetail Guarantee
		{
			get
			{
				if (guarantee == null || guarantee.IsDeleted)
				{
					guarantee = Factory.LoadTop1<CusBondDetail>(new ZQuery(CusBondDetailSchema.PW_ParentID, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

					if (guarantee == null)
					{
						guarantee = Factory.New<CusBondDetail>();
						using (guarantee.SuspendSettingHasChanges())
						{
							guarantee.Parent = this;
						}
					}

					RegisterEditableChildObject(guarantee);
				}
				return guarantee;
			}
		}
		CusBondDetail guarantee;

		[ResourceStringData("04FB68CE-C674-42D6-B7D3-7802E9A25998", Caption = "Dedicated Guarantee Amount", ShortCaption = "Dedi.Guar.Amount")]
		public override ZDecimal ZG_DedicatedGuaranteeAmount { get => base.ZG_DedicatedGuaranteeAmount; set => base.ZG_DedicatedGuaranteeAmount = value; }

		[ResourceStringData("04FB68CE-C674-42D6-B7D3-7802E9A25999", Caption = "Guarantee Ratio %")]
		public override ZDecimal ZG_GuaranteeRatio
		{
			get => base.ZG_GuaranteeRatio;
			set
			{
				var oldValue = base.ZG_GuaranteeRatio;
				base.ZG_GuaranteeRatio = value;
				if (oldValue != ZG_GuaranteeRatio && !IsCopying)
				{
					if (shouldCalculateAmountWithRatio)
					{
						Guarantee.CalculateAmountWithRatio();
					}
				}
			}
		}

		internal void CalculateRatioWithAmount()
		{
			lock (calculationLock)
			{
				shouldCalculateAmountWithRatio = false;
				var dedicatedAmount = ZG_DedicatedGuaranteeAmount;
				if (dedicatedAmount != 0)
				{
					ZG_GuaranteeRatio = ZArchitecture.Core.Utilities.Round(Guarantee.PW_BondAmount * 100 / dedicatedAmount, 2);
				}
				shouldCalculateAmountWithRatio = true;
			}
		}

		readonly object calculationLock = new object();
		bool shouldCalculateAmountWithRatio = true;

		[ResourceStringData("EE06711C-B512-4298-97BE-DD824EAC8C31", ShortCaption = "Union Sec.Code", Caption = "Union Secretary Code")]
		public override ZString ZG_ExportUnionSecretaryCode { get => base.ZG_ExportUnionSecretaryCode; set => base.ZG_ExportUnionSecretaryCode = value; }

		[ResourceStringData("90EF32AD-BC01-4746-9EAF-5D4B3111F5DF", Caption = "Union Code")]
		public override ZString ZG_ExportUnionCode { get => base.ZG_ExportUnionCode; set => base.ZG_ExportUnionCode = value; }

		[ResourceStringData("96DC0911-DF66-499A-A0CE-373A1D54BB06", ShortCaption = "Exp. Un. Country", Caption = "Export Union Country Code")]
		public override ZString ZG_ExportUnionCountryCode { get => base.ZG_ExportUnionCountryCode; set => base.ZG_ExportUnionCountryCode = value; }

		[ResourceStringData("B810146C-5074-4F83-B990-8E84393E011F", ShortCaption = "Inland Trans.Type", Caption = "Inland Transport Type")]
		public override ZString ZG_InlandTransportType { get => base.ZG_InlandTransportType; set => base.ZG_InlandTransportType = value; }
	}
}
