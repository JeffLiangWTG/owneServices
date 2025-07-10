using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoCusEntryInstruction : EU.Business.Declaration.AddInfoCusEntryInstruction
{
	public AddInfoCusEntryInstruction(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction.CEI_AddInfoInfo)
	{
	}

	public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	public new AddInfoCusEntryInstructionLookups Lookups => (AddInfoCusEntryInstructionLookups)base.Lookups;

	public new AddInfoCusEntryInstructionValidation Validation => (AddInfoCusEntryInstructionValidation)base.Validation;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryInstructionLookups(this);

	protected override EUAddInfoValidation GetNewValidation()
	{
		if (Parent.JobDeclaration?.IsExport ?? false)
		{
			return new ExportAddInfoCusEntryInstructionValidation(this);
		}
		return new AddInfoCusEntryInstructionValidation(this);
	}

	public override ZBool ZG_ExportManifest
	{
		get => base.ZG_ExportManifest;
		set
		{
			var oldValue = ZG_ExportManifest;
			base.ZG_ExportManifest = value;
			if (!IsCopying && oldValue != ZG_ExportManifest)
			{
				Parent?.JobDeclaration?.MarkAsNeedingValidation();
			}
		}
	}

	#region ZG_EADPrintOut

	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.EadPrintOutList))]
	[MaxLength(1)]
	public override ZString ZG_EADPrintOut { get => base.ZG_EADPrintOut; set => base.ZG_EADPrintOut = value; }

	#endregion

	#region Temporary Location

	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryInstructionLookups.TemporaryLocationCodeTypeList))]
	[MaxLength(4)]
	public override ZString ZG_TemporaryLocationCodeType
	{
		get => base.ZG_TemporaryLocationCodeType;
		set
		{
			base.ZG_TemporaryLocationCodeType = value;
			ZG_TemporaryLocation = ZG_TemporaryLocation.Left(ZG_TemporaryLocationMaxSize);
		}
	}

	public int ZG_TemporaryLocationMaxSize
	{
		get
		{
			var result = 35;
			switch (this.ZG_TemporaryLocationCodeType)
			{
				case TemporaryLocationCodeTypeList.Codes.CODE:
					result = 17;
					break;

				case TemporaryLocationCodeTypeList.Codes.DESC:
					result = 35;
					break;
			}
			return result;
		}
	}

	[MaxLength("ZG_TemporaryLocationMaxSize")]
	public override ZString ZG_TemporaryLocation { get => base.ZG_TemporaryLocation; set => base.ZG_TemporaryLocation = value; }

	#endregion
}
