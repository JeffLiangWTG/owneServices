using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	[SystemDefinedValues]
	public class CusVehicle : EU.Business.CusVehicle, Integration.Customs.TR.ICusVehicle
	{
		public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : AutoCusVehicle.Schema
		{
			public const string BrandValue = nameof(CusVehicle.BrandValue);
			public const string BrandValueInTRY = nameof(CusVehicle.BrandValueInTRY);
			public const string Gears = nameof(CusVehicle.Gears);
		}

		protected override Customs.Business.CusVehicleLookups GetNewLookups() => new CusVehicleLookups(this);

		public new CusVehicleLookups Lookups => (CusVehicleLookups)base.Lookups;

		protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);

		public new CusVehicleValidation Validation => (CusVehicleValidation)base.Validation;

		#region InvoiceLine

		public ZDecimal BrandValue => InvoiceLine?.JI_LinePrice ?? 0;

		public ZPropertyInfo BrandValueInfo => GetZPropertyInfo(nameof(BrandValue));

		public ZDecimal BrandValueInTRY
		{
			get
			{
				var exchangeRate = InvoiceLine?.Declaration?.JE_DeclarationExchangeRate ?? 0m;
				return BrandValue * exchangeRate;
			}
		}

		public ZPropertyInfo BrandValueInTRYInfo => GetZPropertyInfo(nameof(BrandValueInTRY));

		public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		#endregion

		#region Engine

		public override EngineRelationshipType EngineRelationship => EngineRelationshipType.One;

		public CusEngine Engine => (CusEngine)FirstEngine;

		protected override ICusEngineCollection<Customs.Business.CusEngine, Customs.Business.CusVehicle> GetNewCusEngineCollection() => new CusEngineCollection<CusEngine, CusVehicle>(this);

		#endregion

		[MaxLength(30)]
		public override ZString CVH_SerialNumber
		{
			get => base.CVH_SerialNumber;
			set => base.CVH_SerialNumber = value;
		}

		[MaxLength(20)]
		public override ZString CVH_ModelName
		{
			get => base.CVH_ModelName;
			set => base.CVH_ModelName = value;
		}

		[MaxLength(15)]
		public override ZString CVH_Color
		{
			get => base.CVH_Color;
			set => base.CVH_Color = value;
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.GearsDescriptionList))]
		public ZString Gears
		{
			get
			{
				return GearsList.GetDescriptionFromCode(CVH_Gears.ToString());
			}
			set
			{
				var code = GearsList.GetCodeFromDescription(value);
				ZByte number = 0;
				if (value.IsEmpty || ZByte.TryParse(code, out number))
				{
					if (number != CVH_Gears)
					{
						base.CVH_Gears = number;
						GearsInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo GearsInfo => GetZPropertyInfo(nameof(Gears));

		CodeDescriptionPairList GearsList
		{
			get
			{
				if (gearsList == null)
				{
					gearsList = Lookups.GearsList;
				}

				return gearsList;
			}
		}
		CodeDescriptionPairList gearsList;
	}
}
