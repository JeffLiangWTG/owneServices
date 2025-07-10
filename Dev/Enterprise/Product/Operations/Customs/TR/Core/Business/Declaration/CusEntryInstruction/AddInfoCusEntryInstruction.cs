using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
	{
		public AddInfoCusEntryInstruction(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoCusEntryInstructionValidation(this);

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.UnionSecretaryCodesList))]
		public override ZString ZG_ExportUnionSecretaryCode { get => base.ZG_ExportUnionSecretaryCode; set => base.ZG_ExportUnionSecretaryCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.UnionCodesList))]
		public override ZString ZG_ExportUnionCode { get => base.ZG_ExportUnionCode; set => base.ZG_ExportUnionCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.ExportUnionCountryCodeList))]
		public override ZString ZG_ExportUnionCountryCode { get => base.ZG_ExportUnionCountryCode; set => base.ZG_ExportUnionCountryCode = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.TransportModeInland))]
		public override ZString ZG_InlandTransportType { get => base.ZG_InlandTransportType; set => base.ZG_InlandTransportType = value; }
	}
}
