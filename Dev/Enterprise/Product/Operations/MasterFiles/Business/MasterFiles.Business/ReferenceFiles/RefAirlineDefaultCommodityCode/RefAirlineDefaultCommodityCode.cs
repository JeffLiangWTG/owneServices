using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineDefaultCommodityCode : AutoRefAirlineDefaultCommodityCode
	{
		public RefAirlineDefaultCommodityCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new partial class Schema : AutoRefAirlineDefaultCommodityCode.Schema
		{
			public const string RDC_Description = "RDC_Description";
		}

		#endregion

		#region RDC_Description

		public ZString RDC_Description => LoadFromCommodityCode(RDC_RAC_NKCommodityCode)?.RAC_Description ?? ZString.Empty;

		RefAirlineCommodityCode LoadFromCommodityCode(ZString commodityCode)
		{
			var commodityCodeFilter = new ZQuery(RefAirlineCommodityCodeSchema.RAC_Code, commodityCode);
			return Factory.LoadTop1<RefAirlineCommodityCode>(commodityCodeFilter);
		}

		#endregion

		#region RDC_RAR_NKProductCode

		[List("Lookups.RDC_RAR_NKProductCode_List")]
		public override ZString RDC_RAR_NKProductCode
		{
			get
			{
				return base.RDC_RAR_NKProductCode;
			}
			set
			{
				base.RDC_RAR_NKProductCode = value;

				Master?.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					RefreshCheckUniqueness();
				}
			}
		}

		#endregion

		#region RDC_RAC_NKCommodityCode

		[List("Lookups.RDC_RAC_NKCommodityCode_List")]
		public override ZString RDC_RAC_NKCommodityCode
		{
			get
			{
				return base.RDC_RAC_NKCommodityCode;
			}
			set
			{
				base.RDC_RAC_NKCommodityCode = value;
			}
		}

		#endregion

		#region RDC_RL_NKOrigin

		[List("Lookups.Locations")]
		public override ZString RDC_RL_NKOrigin
		{
			get
			{
				return base.RDC_RL_NKOrigin;
			}
			set
			{
				base.RDC_RL_NKOrigin = value;

				Master?.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					RefreshCheckUniqueness();
				}
			}
		}

		#endregion

		#region RDC_RL_NKDestination

		[List("Lookups.Locations")]
		public override ZString RDC_RL_NKDestination
		{
			get
			{
				return base.RDC_RL_NKDestination;
			}
			set
			{
				base.RDC_RL_NKDestination = value;

				Master?.MarkAsNeedingValidation();

				if (!IsValidationSuspended)
				{
					RefreshCheckUniqueness();
				}
			}
		}

		#endregion

		#region Master

		public RefAirline Master
		{
			get
			{
				return Factory.Load<RefAirline>(RDC_RM);
			}
		}

		void RefreshCheckUniqueness()
		{
			if (Master != null)
			{
				foreach (var obj in Master.DefaultCommodityCodeCollection)
				{
					var defaultCommodityCode = (RefAirlineDefaultCommodityCode)obj;
					defaultCommodityCode.Validation.ValidateRDC_RAR_NKProductCode();
					defaultCommodityCode.Validation.ValidateRDC_RL_NKOrigin();
					defaultCommodityCode.Validation.ValidateRDC_RL_NKDestination();
				}
			}
		}

		#endregion

		#region GetNewLookups

		public new RefAirlineDefaultCommodityCodeLookups Lookups
		{
			get { return lookups ?? (lookups = new RefAirlineDefaultCommodityCodeLookups(this)); }
		}

		RefAirlineDefaultCommodityCodeLookups lookups;

		#endregion
	}
}
