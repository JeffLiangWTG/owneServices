using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyUNDGDataItemValidation : UNDGDataItemValidation
	{
		public AgencyUNDGDataItemValidation(AutoUNDGDataItem parent) : base(parent)
		{
		}

		public new AgencyUNDGDataItem Parent => (AgencyUNDGDataItem)base.Parent;

		protected override bool ShouldValidateVolumeAndWeightWhenQuantityIsLimited => false;

		protected override void CheckSubstancePK()
		{
			base.CheckSubstancePK();

			var substance = Parent?.Substance;
			if (substance != null)
			{
				if (substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA)
				{
					Parent.SubstancePKInfo.AddWarning(Res.GetString("AB5B85D8-F133-4CC2-A27A-521BDC097489", "This substance is for Air Freight. Please re-select the Sea Freight substance."));
				}
			}
		}

		protected override void CheckDI_TechnicalName()
		{
			base.CheckDI_TechnicalName();

			if (string.IsNullOrWhiteSpace(Parent.DI_TechnicalName))
			{
				var substance = Parent?.Substance;

				if (substance != null)
				{
					if (!substance.DG_TechName.IsEmpty)
					{
						Parent.DI_TechnicalNameInfo.AddWarning(Res.GetString("B85BC454-DA36-47DA-AAEA-402E774999A6", "Technical Name is required for this substance."));
						if (substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO
							&& substance.DG_MP == "Y")
						{
							Parent.DI_TechnicalNameInfo.AddWarning(Res.GetString("A54B5AD3-03BC-41EC-83E7-0F35F9BF62EB", "Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant."));
						}
					}

					if (substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO
						&& substance.SpecialProvisions.Any(data => data.DC_Index == "274" || data.DC_Index == "318"))
					{
						Parent.DI_TechnicalNameInfo.AddWarning(Res.GetString("8D808E99-5E90-41DB-8C7D-A7A35BD50F69", "Generic and 'not otherwise specified' proper shipping names that are assigned to special provisions 274 and 318 in Column 6 of the Dangerous Goods List must be supplemented with the technical or chemical group name unless a national law or international convention prohibits its disclosure if it is a controlled substance. For example: 'UN1993 Flammable liquid, n.o.s. (contains xylene and benzene), 3, PG II'. The technical name must be a recognized chemical or biological name, used in scientific and technical handbooks. Trade names must not be used for this purpose.")); // xylene not a valid dictionary word
					}
				}
			}
		}
	}
}
