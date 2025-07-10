using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.Business
{
	public interface IEntryNumberGeneratorProvider
	{
		ZDateTime EntryNumberDate { get; }

		ZPropertyInfo EntryNumberPart1Info { get; }

		ZPropertyInfo EntryNumberPart2Info { get; }

		ZPropertyInfo CustomsBrokerageBoxNumberInfo { get; }

		ZString SequenceNumber { get; }

		ZString ShipmentType { get; }

		ZString EntryNumberType { get; }

		GlbCompany Company { get; }

		EntryNumberGeneratorCategory GetEntryNumberGeneratorCategory();

		EnterpriseBusinessObject EntryNumberGeneratorProviderBusinessObject { get; }
	}

	public enum EntryNumberGeneratorCategory
	{
		None = 0,
		A = 1,
		A1 = 2,
		B = 3,
		C = 4,
		D = 5,
		T = 6
	}
}
