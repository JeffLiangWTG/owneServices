using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDGRestrictionsValidation : JobConsolDGRestrictionsValidation
	{
		public ConsolDGRestrictionsValidation(ConsolDGRestrictions parent) : base(parent)
		{
		}

		ConsolDGRestrictions ParentRestriction => Parent as ConsolDGRestrictions;
		ConsolDGRestrictionsLookups ParentRestrictionsLookups => ParentRestriction.Lookups;

		protected override void CheckJKD_Class()
		{
			base.CheckJKD_Class();
			if (ParentRestrictionsLookups != null)
			{
				ListValidation.ErrorIfInvalidCode(ParentRestriction.JKD_ClassInfo, ParentRestrictionsLookups.DGClassLookup);
			}

			if (ParentRestriction != null && ParentRestriction.JKD_Class.IsEmpty && ParentRestriction.JKD_UNNO.IsEmpty)
			{
				ParentRestriction.JKD_ClassInfo.AddError(Res.GetString("202544c2-2dc0-2eb3-4b8f-0c9d6a653b25", "Invalid Empty Row."));
				return;
			}

			ConfirmNoDuplicates();
			ConfirmClassAndSubstanceMatch();
		}

		void ConfirmNoDuplicates()
		{
			if (ParentRestriction.Consol != null)
			{
				var duplicateRestrictionExists = ParentRestriction.Consol.ConsolDGRestrictionCollection.Any(dgGoods =>
					ParentRestriction.JKD_Class == dgGoods.JKD_Class
					&& ParentRestriction.JKD_Calc_Substance == dgGoods.JKD_Calc_Substance
					&& ParentRestriction.PK != dgGoods.PK);

				if (duplicateRestrictionExists)
				{
					ParentRestriction.JKD_ClassInfo.AddError(Res.GetString("d9cdfd22-43bb-8097-4c99-f10de59456b1", "Invalid Duplicate Restriction."));
				}
			}
		}

		void ConfirmClassAndSubstanceMatch()
		{
			if (!ParentRestriction.JKD_Calc_Substance.IsEmpty && !ParentRestriction.JKD_Class.IsEmpty)
			{
				var substanceQuery = new ZQuery(UNDGSubstanceSchema.DG_Code, ParentRestriction.JKD_Calc_Substance);
				substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Class, ParentRestriction.JKD_Class);

				var substance = ParentRestriction.Factory.LoadTop1<UNDGSubstance>(substanceQuery);
				if (substance == null)
				{
					ParentRestriction.JKD_ClassInfo.AddError(Res.GetString("6c1ed601-5e1d-f4b9-4c17-6ad4747dce32", "The substance {0} does not belong to class {1}.", ParentRestriction.JKD_Calc_Substance, ParentRestriction.JKD_Class));
				}
			}
		}

		public void ValidateJKD_Calc_Substance()
		{
			ValidateCalculatedProperty(ParentRestriction.JKD_Calc_SubstanceInfo);
		}

		protected virtual void CheckJKD_Calc_Substance()
		{
			if (ParentRestrictionsLookups != null)
			{
				ListValidation.ErrorIfInvalidCode(ParentRestriction.JKD_Calc_SubstanceInfo, ParentRestrictionsLookups.Substance);
			}

			ComfirmNoForbidenDangerousGoodsSubstancesForPassengerFlight();
		}

		void ComfirmNoForbidenDangerousGoodsSubstancesForPassengerFlight()
		{
			if (ParentRestriction.Consol != null &&
				ParentRestriction.Consol.IsAir &&
				!ParentRestriction.Consol.JK_Calc_IsCargoOnly &&
				!ParentRestriction.JKD_Calc_Substance.IsEmpty)
			{
				var substanceQuery = new ZQuery(UNDGSubstanceSchema.DG_Code, ParentRestriction.JKD_Calc_Substance);
				substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);

				if (!ParentRestriction.JKD_Class.IsEmpty)
				{
					substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Class, ParentRestriction.JKD_Class);
				}

				var substances = ParentRestriction.Factory.Load<UNDGSubstance>(substanceQuery);

				if (substances.Any(substance => substance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode))
				{
					ParentRestriction.JKD_Calc_SubstanceInfo.AddError(Res.GetString("4fe166da-444f-43a5-9c7f-bd51a99bd4ae", "The substance you have selected is forbidden for passenger flight. Please ensure all flights on this Consol are 'Is Cargo Only' flights."));
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJKD_Calc_Substance();
		}
	}
}
