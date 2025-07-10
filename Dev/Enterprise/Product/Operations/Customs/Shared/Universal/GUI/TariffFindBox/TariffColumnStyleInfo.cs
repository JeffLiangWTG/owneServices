using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Universal.GUI
{
	public class TariffColumnStyleInfo : ZBaseFindBoxColumnStyleInfo
	{
		public TariffColumnStyleInfo() : base()
		{
			PartialDescriptionMinLengthForSearch = 3;
		}

		public Func<string> GetCountryCode;

		[DefaultValue("")]
		public string TariffType { get; set; }

		public List<SelectionStyle> SelectNomenclatureModes { get; set; }
		public Func<List<SelectionStyle>> GetSelectNomenclatureModes;

		[DefaultValue(3)]
		public int PartialDescriptionMinLengthForSearch { get; set; }

		public bool ShowDescriptionFilterOnNonNomenclatureTariffModule { get; set; }

		public Func<ZDateTime> GetEffectiveDate;

		public Func<ZString> GetTariffType;

		public Func<ZString> GetDataGrouping;

		public bool NeedLoadParentDataGroup { get; set; } = true;

		public bool NeedLoadNomenclatureWhenTariffNotFound { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZQuery NomenclatureGroupAdditionalFilter => GetNomenclatureGroupAdditionalFilter();

		protected virtual ZQuery GetNomenclatureGroupAdditionalFilter() => null;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZQuery TariffAdditionalFilter => GetTariffAdditionalFilter();

		protected virtual ZQuery GetTariffAdditionalFilter() => null;

		public override Type ColumnStyleType => typeof(TariffColumnStyle);
	}
}
