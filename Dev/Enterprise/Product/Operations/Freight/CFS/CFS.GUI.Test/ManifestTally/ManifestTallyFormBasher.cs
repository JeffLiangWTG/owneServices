using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	[TestedType(typeof(ManifestTallyForm))]
	sealed class ManifestTallyFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ManifestTallyForm(Factory.New<TallyContainer>());
		}

		public void TestSeaCargoPlugInAustralia()
		{
			string storedCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				new ConstantsAndReusables(Factory).SetCountryCode(Enterprise.Core.Constants.CountryCodes.Australia);

				const string AUSeaCargoDepotPlugInClassName = "Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoDepotOutturnPlugin";
				bool plugInFound = false;

				Factory.New<Enterprise.Integration.Customs.AU.ICusOutturnHeader>();
				TallyContainer tallyContainer = Factory.New<TallyContainer>();

				// Plugin will only show if we have a linked header and outturn
				BusinessObject header = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusOutturnHeader>();
				BusinessObjectCollection collection = (BusinessObjectCollection)header.GetType().InvokeMember("Outturns", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty, Type.DefaultBinder, header, Array.Empty<object>());
				BusinessObject outturn = collection.AddNew();
				outturn[ZArchitecture.Schema.CusOutturnSchema.C5_ParentTableCode] = "JC";
				outturn[ZArchitecture.Schema.CusOutturnSchema.C5_ParentID] = tallyContainer.PK;

				tallyContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
				using (ManifestTallyForm form = new ManifestTallyForm(tallyContainer))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should of been found for Country Code AU", true, plugInFound);
				}
				plugInFound = false;
				tallyContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.AIR;
				using (ManifestTallyForm form = new ManifestTallyForm(tallyContainer))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should not of been found for Air Container", false, plugInFound);
				}
				plugInFound = false;
				new ConstantsAndReusables(Factory).SetCountryCode("ER");
				tallyContainer.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
				using (ManifestTallyForm form = new ManifestTallyForm(tallyContainer))
				{
					foreach (ZArchitecture.PlugIn.ZPlugIn plugIn in form.PlugIns.Instances)
					{
						if (plugIn.GetType().FullName == AUSeaCargoDepotPlugInClassName)
						{
							plugInFound = true;
							break;
						}
					}
					AssertEquals("Sea Cargo Depot Plug In should not of been found for Country Code ER", false, plugInFound);
				}
			}
			finally
			{
				new ConstantsAndReusables(Factory).SetCountryCode(storedCountryCode);
			}
		}
	}
}
