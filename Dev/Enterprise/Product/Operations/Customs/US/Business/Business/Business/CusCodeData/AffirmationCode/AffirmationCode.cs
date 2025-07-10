using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AffirmationCode : Customs.Business.CusCodeData
	{
		public AffirmationCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetHumanReadableNameForCY_Data();
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string CY_FormattedData = "CY_FormattedData";
		}

		#region New Properties

		public override ZString Description
		{
			get { return USCAffirmationCode == null ? ZString.Empty : USCAffirmationCode.UL_Description; }
		}

		public USCAffirmationOfComplianceCollection AffirmationCodeList
		{
			get
			{
				var result = new USCAffirmationOfComplianceCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Is Expired", "Property", new ZString(YesNoDefaultList.Codes.No)));
				return result;
			}
		}

		public USCAffirmationOfCompliance USCAffirmationCode
		{
			get
			{
				if (fUSCAffirmationCode == null || fUSCAffirmationCode.UL_Code != CY_Code)
				{
					fUSCAffirmationCode = Factory.LoadFromNaturalKey<USCAffirmationOfCompliance>(USCAffirmationOfComplianceSchema.UL_Code, CY_Code);
				}
				return fUSCAffirmationCode;
			}
		}
		USCAffirmationOfCompliance fUSCAffirmationCode;

		#endregion

		#region CY_Code

		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				ZString oldValue = CY_Code;
				base.CY_Code = value;

				if (oldValue != CY_Code)
				{
					SetHumanReadableNameForCY_Data();
				}
			}
		}

		#endregion

		#region CY_Data

		[MaxLength(25)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value.ToUpper(); }
		}

		#endregion

		#region Implementation

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(AffirmationCode affirmationCode)
				: base(affirmationCode)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				AffirmationCode affirmationCode = (AffirmationCode)BusinessObject;
				Factory.AddFetchHint(typeof(USCAffirmationOfCompliance), USCAffirmationOfComplianceSchema.UL_Code, affirmationCode.CY_Code);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AffirmationCode;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new AffirmationCodeValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(FDA)); }
		}

		void SetHumanReadableNameForCY_Data()
		{
			CY_DataInfo.HumanReadableName = CY_Code.IsEmpty ? "FDA" : "FDA (" + CY_Code + ")";
		}

		#endregion
	}
}
