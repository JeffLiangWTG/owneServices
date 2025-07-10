using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefAirlineSpecialHandlingCodeSchema.Constants.RHC_Code), DescriptionProperty(RefAirlineSpecialHandlingCodeSchema.Constants.RHC_Description)]
	public class RefAirlineSpecialHandlingCode : AutoRefAirlineSpecialHandlingCode
	{
		public RefAirlineSpecialHandlingCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Master

		public RefAirline Master
		{
			get
			{
				return Factory.Load<RefAirline>(RHC_RM_Airline);
			}
		}

		void RefreshCheckUniqueness()
		{
			if (Master != null)
			{
				foreach (var obj in Master.RefAirlineSpecialHandlingCodeCollection)
				{
					var refAirlineSpecialHandlingCode = (RefAirlineSpecialHandlingCode)obj;
					refAirlineSpecialHandlingCode.Validation.ValidateAll();
				}
			}
		}

		#endregion

		#region RHC_OriginPortOrCountry

		[List("Lookups.Locations")]
		public override ZString RHC_OriginPortOrCountry
		{
			get
			{
				return base.RHC_OriginPortOrCountry;
			}
			set
			{
				base.RHC_OriginPortOrCountry = value;

				Master?.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					RefreshCheckUniqueness();
				}
			}
		}

		#endregion

		#region RHC_DestinationPortOrCountry 

		[List("Lookups.Locations")]
		public override ZString RHC_DestinationPortOrCountry
		{
			get
			{
				return base.RHC_DestinationPortOrCountry;
			}
			set
			{
				base.RHC_DestinationPortOrCountry = value;

				Master?.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					RefreshCheckUniqueness();
				}
			}
		}

		#endregion

		#region RHC_Code 

		public override ZString RHC_Code
		{
			get
			{
				return base.RHC_Code;
			}
			set
			{
				base.RHC_Code = value;

				Master?.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					RefreshCheckUniqueness();
				}
			}
		}

		#endregion
	}
}
