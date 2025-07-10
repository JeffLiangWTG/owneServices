//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmFeatureTestLookups
//
//    This class should be used for overriding collections in AutoStmFeatureTestLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class StmFeatureTestLookups : AutoStmFeatureTestLookups
	{
		public StmFeatureTestLookups(AutoStmFeatureTest parent) : base(parent)
		{
		}

		public List<ICodeDescription> Features
		{
			get
			{
				var allFeatures = ModuleIDs.AllIncludingClientModules
					.Where(m => !(m.Description is NoResString))
					.Select(m => (ICodeDescription)new FeatureDescription(m.ID.ToString(), m.Description)).ToList();
				allFeatures.Add(new FeatureDescription(StmFeatureTest.WinzorFeatureCode, ResString.GetMultilingualString("09c5f29e-9cae-4979-ba7b-740ac9045d62", "Winzor")));
				allFeatures.Add(new FeatureDescription(StmFeatureTest.WinzorAllFeaturesCode, ResString.GetMultilingualString("7a195873-d3eb-4581-80ec-46ebc3a9f42c", "Enable all modules for Winzor")));
				allFeatures.Add(new FeatureDescription(StmFeatureTest.WinzorMainFormFeatureCode, ResString.GetMultilingualString("42402add-5373-44de-a2fd-f0dc026c0cd8", "Relaunch main form in Winzor on startup")));
				allFeatures.Add(new FeatureDescription(StmFeatureTest.WebVersion, ResString.GetMultilingualString("302D7411-9B1C-4E09-A2BA-A013F856EE8A", "Enforce Web Version")));
				allFeatures.Add(new FeatureDescription(StmFeatureTest.ForceCW1HomeScreen, ResString.GetMultilingualString("C4F2A0B5-3D8E-4A6F-9B7C-0D1E2F3A5B8C", "Force old CW1 Home Screen")));
				allFeatures.Add(new FeatureDescription(StmFeatureTest.NetCoreVersion, ResString.GetMultilingualString("BEE21931-B0A3-42A2-A00F-E926F7D0F9FF", "Enforce .NET Core Version")));
				return allFeatures;
			}
		}

		class FeatureDescription : CodeDescriptionPair, ICodeDescription
		{
			public object PK => Guid.Empty;

			public FeatureDescription(object code, MultilingualString description) : base(code, description)
			{
			}
		}
	}
}
