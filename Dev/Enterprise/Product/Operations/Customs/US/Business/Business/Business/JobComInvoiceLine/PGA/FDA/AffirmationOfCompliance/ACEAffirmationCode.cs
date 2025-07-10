using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSACEFDAAffirmationCode")]
	public class ACEAffirmationCode : Customs.Business.CusCodeData
	{
		public ACEAffirmationCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetHumanReadableNameForCY_Data();
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string CY_FormattedData = "CY_FormattedData";
			public const int DescriptionMaxLength = 30;
		}

		public override ZString Description
		{
			get { return AOCList.GetDescriptionFromCode(CY_Code); }
		}

		public CodeDescriptionPairList AOCList
		{
			get
			{
				var fda = Factory.Load<ACEFDA>(CY_ParentID);
				var programCode = fda?.US_ProgramCode ?? ZString.Empty;
				CodeDescriptionPairList result;
				result = Factory.GetCachedValue("AOCList" + programCode + ZDate.Today.ToISO8601ShortDateString(), () =>
				{
					result = new CodeDescriptionPairList();
					if (!programCode.IsEmpty)
					{
						var cusCodeList = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, AoCProgramCodePrefix + programCode, ZDate.Today);
						result.AddRange(cusCodeList);
						result.Sort();
					}
					return result;
				});
				return result;
			}
		}

		internal const string AoCProgramCodePrefix = "AC";

		#region CY_Code

		[List(nameof(AOCList))]
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

		[MaxLength(Schema.DescriptionMaxLength)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value.ToUpper(); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AffirmationCode;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new ACEAffirmationCodeValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobDeclaration), typeof(JobComInvoiceHeader), typeof(ACEFDA)); }
		}

		void SetHumanReadableNameForCY_Data()
		{
			CY_DataInfo.HumanReadableName = CY_Code.IsEmpty ? "FDA" : "FDA (" + CY_Code + ")";
		}

		#endregion
	}
}
