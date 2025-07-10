using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDGRestrictions : AutoJobConsolDGRestrictions
	{
		public ConsolDGRestrictions(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Consol))]
		public override ZGuid JKD_JK
		{
			get => base.JKD_JK;
			set => base.JKD_JK = value;
		}

		#region JKD_Class

		[List("Lookups.DGClassLookup")]
		public override ZString JKD_Class
		{
			get { return base.JKD_Class; }
			set
			{
				if (base.JKD_Class != value)
				{
					base.JKD_Class = value;

					if (Consol != null)
					{
						if (!value.IsEmpty)
						{
							Consol.JK_IsHazardous = true;
						}

						if (!IsValidationSuspended)
						{
							Consol.Validation.ValidateJK_IsHazardous();
						}
					}
				}
			}
		}

		#endregion

		#region JKD_Substance

		[MaxLength(6)]
		[List("Lookups.Substance")]
		public ZString JKD_Calc_Substance
		{
			get { return JKD_UNNO + JKD_Variant; }
			set
			{
				JKD_UNNO = value.Left(4);
				JKD_Variant = value.SubstringSafe(4, 2);

				if (Consol != null)
				{
					if (!value.IsEmpty
						&& !Consol.JK_IsHazardous)
					{
						Consol.JK_IsHazardous = true;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJKD_Class();
					Validation.ValidateJKD_Calc_Substance();

					if (Consol != null)
					{
						Consol.Validation.ValidateJK_IsHazardous();
					}
				}

				JKD_Calc_SubstanceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JKD_Calc_SubstanceInfo => GetZPropertyInfo(nameof(JKD_Calc_Substance));

		#endregion

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				Schema.JKD_JK
			};

			return result;
		}

		#endregion

		protected override JobConsolDGRestrictionsValidation GetNewValidation()
		{
			return new ConsolDGRestrictionsValidation(this);
		}

		protected override JobConsolDGRestrictionsLookups GetNewLookups()
		{
			return new ConsolDGRestrictionsLookups(this);
		}

		public new ConsolDGRestrictionsLookups Lookups
		{
			get { return (ConsolDGRestrictionsLookups)base.Lookups; }
		}

		public new ConsolDGRestrictionsValidation Validation
		{
			get { return (ConsolDGRestrictionsValidation)base.Validation; }
		}

		public ForwardingConsol Consol => Factory.Load<ForwardingConsol>(JKD_JK);
	}
}
