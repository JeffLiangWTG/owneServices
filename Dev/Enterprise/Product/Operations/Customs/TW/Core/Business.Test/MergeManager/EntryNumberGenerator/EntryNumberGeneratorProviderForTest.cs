using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class EntryNumberGeneratorProviderForTest : EnterpriseBusinessObject, IEntryNumberGeneratorProvider
	{
		public EntryNumberGeneratorProviderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public abstract class Schema
		{
			public const string TableName = Customs.Business.AutoJobDeclaration.Schema.TableName;
		}

		public ZString SequenceNumber { get; set; }

		public ZString EntryNumberPart1 { get; set; }

		public ZString EntryNumberPart2 { get; set; }

		public ZString CustomsBrokerageBoxNumber { get; set; }

		public ZDateTime EntryNumberDate { get; set; }

		public override SchemaGuidColumn PKSchemaColumn => JobDeclarationSchema.PK;

		ZDateTime IEntryNumberGeneratorProvider.EntryNumberDate => EntryNumberDate;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart1Info => GetZPropertyInfo(nameof(EntryNumberPart1));

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart2Info => GetZPropertyInfo(nameof(EntryNumberPart2));

		ZPropertyInfo IEntryNumberGeneratorProvider.CustomsBrokerageBoxNumberInfo => GetZPropertyInfo(nameof(CustomsBrokerageBoxNumber));

		ZString IEntryNumberGeneratorProvider.SequenceNumber => SequenceNumber;

		ZString IEntryNumberGeneratorProvider.ShipmentType => ZString.Empty;

		ZString IEntryNumberGeneratorProvider.EntryNumberType => null;

		GlbCompany IEntryNumberGeneratorProvider.Company => null;

		EnterpriseBusinessObject IEntryNumberGeneratorProvider.EntryNumberGeneratorProviderBusinessObject => this;

		EntryNumberGeneratorCategory IEntryNumberGeneratorProvider.GetEntryNumberGeneratorCategory() => EntryNumberGeneratorCategory.D;
	}
}
