using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USConstituentElementAddInfoLookups : AutoUSConstituentElementAddInfoLookups
	{
		public USConstituentElementAddInfoLookups(AutoUSConstituentElementAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList US_YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		public CodeDescriptionPairList UnitsOfMeasureList
		{
			get
			{
				var constituentElement = ((ConstituentElementAddInfo)base.Parent).Parent as ConstituentElement;
				var fda = constituentElement != null ? constituentElement.Parent as ACEFDA : null;

				var listID = ZString.Empty;
				if (fda != null)
				{
					listID = fda.US_ProgramCode.ToString();
				}

				var laceyAct = constituentElement != null ? constituentElement.Parent as PGA : null;
				var isLaceyACE = laceyAct != null && (laceyAct.InvoiceLine == null || laceyAct.InvoiceLine.IsACE);
				if (isLaceyACE)
				{
					listID = "ACELacey";
				}

				return Factory.GetCachedValue("PGAUnitsOfMeasureList" + listID, delegate
				{
					var list = new CodeDescriptionPairList();
					if (fda != null)
					{
						if (listID == FDAProgramCodeList.Codes.DRU || listID == FDAProgramCodeList.Codes.VME)
						{
							list.AddPair("CGM", "Centigrams Weight");
							list.AddPair("FOZ", "Fluid Ounces (Volume)");
							list.AddPair("G", "Grams Weight");
							list.AddPair("GAL", "Gallons (US) Volume");
							list.AddPair("KGM", "Kilograms Weight");
							list.AddPair("L", "Liters Volume");
							list.AddPair("LB", "Pounds (avdp) Weight");
							list.AddPair("MG", "Milligrams Weight");
							list.AddPair("MCG", "Micrograms Weight");
							list.AddPair("ML", "Milliliters Volume");
							list.AddPair("OZ", "Ounces, weight (avdp) Weight");
							list.AddPair("PTL", "Pints, liquid (US) Volume");
							list.AddPair("QTL", "Quarts, liquid (US) Volume");
							list.AddPair("STN", "Short ton (2000 LB) Weight");
							list.AddPair("T", "Metric Ton Weight");
							list.AddPair("TON", "Long Ton (2240 LB) Weight");
							list.AddPair("TOZ", "Ounces, Troy or Apoth Weight");
							list.AddPair("VL", "Bulk Liquid");
							list.AddPair("VO", "Bulk,Solid,Lg Particles");
							list.AddPair("VR", "Bulk,Solid,Granular Parti");
							list.AddPair("VY", "Bulk,Solid,Fine Particle");
						}
						else
						{
							list = new ACE_FDABaseUQList();
						}
					}
					else if (isLaceyACE)
					{
						list = new ACELaceyUnitsOfMeasureList();
					}
					else
					{
						list = new LaceyActUnitsOfMeasureList();
					}
					return list;
				});
			}
		}

		public RefCountryCollection USCountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public PGALaceySpeciesNameCodeList SpeciesNameCodeList
		{
			get { return Factory.GetCachedValue<PGALaceySpeciesNameCodeList>(); }
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}
	}
}
