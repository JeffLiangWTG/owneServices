using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class AMSLotCode : CusCodeData, ILotCode
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AMSLotCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetHumanReadableNameForCY_Data();
		}

		public LotNumberQualifierList ALCList
		{
			get { return Factory.GetCachedValue<LotNumberQualifierList>(); }
		}

		public override ZString Description
		{
			get { return ALCList.GetDescriptionFromCode(CY_Code); }
		}

		void SetHumanReadableNameForCY_Data()
		{
			CY_DataInfo.HumanReadableName = CY_Code.IsEmpty ? "AML" : "AML (" + CY_Code + ")";
		}

		#region CY_Code

		[MaxLength(nameof(CY_CodeMaxLength))]
		[List(nameof(ALCList))]
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

		int CY_CodeMaxLength
		{
			get
			{
				var result = AutoCusCodeData.Schema.CY_CodeMaxLength;
				if (Parent is AMS ams && ams.US_Program == AMSProgramList.Codes.OR2)
				{
					result = 1;
				}
				return result;
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

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AMSLotCode;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusAddInfo)); }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new AMSLotCodeValidation(this);
		}
		#endregion

		ZString ILotCode.Code
		{
			get { return CY_Code; }
		}

		ZString ILotCode.Value
		{
			get { return CY_Data; }
		}
	}
}
