using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Universal
{
	public class CusRefTariffLanguageView : AutoCusRefTariffLanguageView
	{
		public CusRefTariffLanguageView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Tariff")]
		public override ZGuid ZX7_ZZ1_Tariff
		{
			get => base.ZX7_ZZ1_Tariff;
			set => base.ZX7_ZZ1_Tariff = value;
		}

		public TariffView Tariff
		{
			get { return Factory.Load<TariffView>(ZX7_ZZ1_Tariff); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("e8c821df-df9b-4dc1-9429-62505e5284ee", Caption = "Language")]
		[List(nameof(Lookups) + "." + nameof(CusRefTariffLanguageViewLookups.LanguageTypeList))]
		public override ZString ZX7_ZX6_NKLanguage
		{
			get => base.ZX7_ZX6_NKLanguage;
			set => base.ZX7_ZX6_NKLanguage = value;
		}

		[ReadOnly(true)]
		public override ZString ZX7_DataSet
		{
			get => base.ZX7_DataSet;
			set => base.ZX7_DataSet = value;
		}

		[ResourceStringData("fcfe4f97-698f-4261-a791-cefbc346d050", Caption = "Description")]
		public override ZString ZX7_Description
		{
			get => base.ZX7_Description;
			set => base.ZX7_Description = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZX7_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}
	}
}
