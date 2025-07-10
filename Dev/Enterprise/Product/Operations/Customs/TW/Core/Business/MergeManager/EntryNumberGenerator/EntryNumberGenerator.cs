using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public abstract class EntryNumberGenerator : BaseEntryNumberGenerator
	{
		protected EntryNumberGenerator(IEntryNumberGeneratorProvider provider)
			: base(provider)
		{
		}

		public ZString EntryNumberPart1 => (ZString)Provider.EntryNumberPart1Info.Value;

		public ZString EntryNumberPart2
		{
			get
			{
				var info = Provider.EntryNumberPart2Info;
				return info == null ? new ZString("  ") : (ZString)info.Value;
			}
		}

		public override ZString CustomsBrokerageBoxNumber => (ZString)Provider.CustomsBrokerageBoxNumberInfo.Value;

		#region Type Decider
		public static EntryNumberGenerator New(IEntryNumberGeneratorProvider provider)
		{
			EntryNumberGenerator result = null;
			var entryNumberGeneratorCategory = provider.GetEntryNumberGeneratorCategory();
			switch (entryNumberGeneratorCategory)
			{
				case EntryNumberGeneratorCategory.A:
					result = new EntryNumberGeneratorCatA(provider);
					break;
				case EntryNumberGeneratorCategory.A1:
					result = new EntryNumberGeneratorCatA1(provider);
					break;
				case EntryNumberGeneratorCategory.B:
					result = new EntryNumberGeneratorCatB(provider);
					break;
				case EntryNumberGeneratorCategory.C:
					result = new EntryNumberGeneratorCatC(provider);
					break;
				case EntryNumberGeneratorCategory.D:
					result = new EntryNumberGeneratorCatD(provider);
					break;
				case EntryNumberGeneratorCategory.T:
					result = new TranshipmentEntryNumberGenerator(provider);
					break;
				default:
					break;
			}
			return result;
		}
		#endregion

		protected override BusinessObjectFactory Factory => Provider.EntryNumberGeneratorProviderBusinessObject.Factory;

		public override ZString Part1 => EntryNumberPart1;

		public override ZString Part2 => EntryNumberPart2;

		public override ZString Part4 => CustomsBrokerageBoxNumber;

		public override ZString Part4Caption => Provider.CustomsBrokerageBoxNumberInfo.HumanReadableName;

		public override int Part4_Length => 3;

		public override ZString Part3 => Provider.EntryNumberDate.IsValid ? Provider.EntryNumberDate.ToTaiWanShortYear() : ZDateTime.Today.ToTaiWanShortYear();

		protected override ZString ShipmentType => Provider.ShipmentType;

		protected override GlbCompany Company => Provider.Company ?? GlbCompany.CurrentCompany;

		public override ZString EntryNumberType => Provider.EntryNumberType;

		public override ZString CannotAutoGenerateEntryNumberMessage => ZString.Empty;

		protected override ITWSequenceformatter GetSequenceformatter()
		{
			return BaseTWSequenceformatter.New(Category);
		}
	}
}
