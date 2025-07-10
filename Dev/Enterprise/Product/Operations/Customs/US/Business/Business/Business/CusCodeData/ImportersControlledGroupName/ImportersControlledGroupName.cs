using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ImportersControlledGroupName : CusCodeData, Integration.Customs.US.IImportersControlledGroupName
	{
		public ImportersControlledGroupName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string US_GroupName = "US_GroupName";
			public const int US_GroupNameMaxLength = 10;
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ImportersControlledGroupName;
			CY_Code = CusCodeDataTypeList.Codes.ImportersControlledGroupName;
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ImportersControlledGroupNameValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(OrgCountryData)); }
		}

		#endregion

		#region New Properties

		[MaxLength(Schema.US_GroupNameMaxLength)]
		[ResourceStringData("Enterprise.Customs.US.Business.ImportersControlledGroupName|US_GroupName", Caption = "Group Name")]
		public ZString US_GroupName
		{
			get => CY_Data;
			set
			{
				var newValue = value.ToUpper();
				if (US_GroupName != newValue)
				{
					CheckMaximumLength(US_GroupNameInfo, newValue);
					CY_Data = newValue;
					US_GroupNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo US_GroupNameInfo => GetZPropertyInfo(Schema.US_GroupName);

		#endregion
	}
}
