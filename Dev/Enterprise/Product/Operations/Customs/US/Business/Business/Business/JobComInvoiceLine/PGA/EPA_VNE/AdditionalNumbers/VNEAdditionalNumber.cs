using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class VNEAdditionalNumber : Customs.Business.CusCodeData, VNEAdditionalNumbers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public VNEAdditionalNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetHumanReadableNameForCY_Data();
		}

		void SetHumanReadableNameForCY_Data()
		{
			CY_DataInfo.HumanReadableName = CY_Code.IsEmpty ? "VDE" : "VDE (" + CY_Code + ")";
		}

		public CodeDescriptionPairList NumberTypeList
		{
			get
			{
				return Factory.GetCachedValue("VNEAdditionalNumberList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(ItemIdentityNumberQualifierList.Codes.EngineNumber, ItemIdentityNumberQualifierList.Descriptions.EngineNumber);
					list.AddPair(ItemIdentityNumberQualifierList.Codes.SerialNumber, ItemIdentityNumberQualifierList.Descriptions.SerialNumber);
					list.AddPair(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, ItemIdentityNumberQualifierList.Descriptions.VehicleIdentificationNumberVIN);
					return list;
				});
			}
		}

		#region CY_Code

		[List(nameof(NumberTypeList))]
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

		[MaxLength(17)]
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
			CY_Type = CusCodeDataTypeList.Codes.VNEAdditionalNumber;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(VehicleDetails)); }
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new VNEAdditionalNumberValidation(this);
		}

		#endregion

		ZString VNEAdditionalNumbers.NumberType
		{
			get { return CY_Code; }
		}

		ZString VNEAdditionalNumbers.Number
		{
			get { return CY_Data; }
		}
	}
}
