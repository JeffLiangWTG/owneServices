using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoAccGroups.Schema.AR_Desc)]
	public class AccGroups : AutoAccGroups
	{
		public AccGroups(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region AR_Desc

		[TranslatableDataField(Schema.TableName, Schema.AR_Desc, DataXmlFilePaths.RefAccounting, MaxLength = Schema.AR_DescMaxLength, Type = typeof(AccGroups), Asmid = ResString.AssemblyId)]
		public override ZString AR_Desc
		{
			get => base.AR_Desc;
			set => base.AR_Desc = value;
		}

		public MultilingualString AR_DescMultilingual => GetMultilingual(AR_DescInfo);

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("D52A6E6A-EFFC-418B-8C47-75F8C324CD49", "Sales/Expense Groups - {0}", CalculateShortcutName());
	}
}
