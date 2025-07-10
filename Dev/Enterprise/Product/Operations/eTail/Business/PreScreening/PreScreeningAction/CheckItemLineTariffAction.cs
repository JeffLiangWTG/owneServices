using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	class CheckItemLineTariffAction : CheckNormalPropertyAction
	{
		public CheckItemLineTariffAction(HVLVPreScreeningField field, HVLVConsignment consignment) : base(field, consignment)
		{
		}
		protected override IEnumerable<ZString> PropertyValues
		{
			get
			{
				var itemLines = consignment.Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>();
				if (itemLines != null && itemLines.Any())
				{
					foreach (var itemLine in itemLines)
					{
						if (field.FieldName == HVLVItemLineSchema.Constants.HVS_OriginTariff)
						{
							yield return itemLine.HVS_FormattedOriginTariff;
						}
						else if (field.FieldName == HVLVItemLineSchema.Constants.HVS_DestinationTariff)
						{
							yield return itemLine.HVS_FormattedDestinationTariff;
						}
					}
				}
			}
		}
	}
}
