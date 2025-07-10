using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobShipmentWorkflowCondition2CodeList : CodeDescriptionPairList
	{
		public JobShipmentWorkflowCondition2CodeList()
		{
			AddPair(Codes.Import, Res.GetString("a49fee5e-0b1f-4bb4-8a8e-6cebdc29bc9d", "Import"));
			AddPair(Codes.Export, Res.GetString("8a9cc9aa-a698-4400-bcdb-ec1981938ddd", "Export"));
			AddPair(Codes.Domestic, Res.GetString("97f73b58-f1dc-440d-b5e6-f0b52a2d8498", "Domestic"));
			AddPair("", "");
			AddRange(new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode));
			RemoveCode(Core.Constants.ContainerModes.BuyersConsol);
			RemoveCode(Core.Constants.ContainerModes.Other);
			AddPair("", "");
			AddPair(Codes.BuyersConsolMaster, Res.GetString("8b20c78c-54dc-4a8b-a3e1-c886c1edb882", "Buyer's Consol Master"));
			AddPair(Codes.BuyersConsolSub, Res.GetString("e0eb8794-8af0-4551-983a-689e1072c5ff", "Buyer's Consol Sub"));
			AddPair(Codes.ColoadMaster, Res.GetString("9485eefa-d8f9-4c72-bf63-ddfb4f0cf167", "Co-Load Master"));
			AddPair(Codes.ColoadSub, Res.GetString("dd5ccc60-e0e4-4dc9-a128-0c05445d1795", "Co-Load Sub"));
			AddPair(Codes.AssemblyMaster, Res.GetString("cc8c9138-d197-4f0a-aac0-e6d736d1c20d", "Assembly Master"));
			AddPair(Codes.AssemblySub, Res.GetString("c5670bfa-4852-4852-8f2a-4ef2ef469f88", "Assembly Sub"));
			AddPair(Codes.ReleaseType, Res.GetString("4570d84e-8df5-4a2e-b42d-e436b502b52b", "Release Type"));
		}

		public abstract class Codes
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Domestic = "DOM";
			public const string LCL = Core.Constants.ContainerModes.LCL;
			public const string FCL = Core.Constants.ContainerModes.FCL;
			public const string BuyersConsolMaster = "BCM";
			public const string BuyersConsolSub = "BCS";
			public const string ColoadMaster = "CLM";
			public const string ColoadSub = "CLS";
			public const string AssemblyMaster = "ASM";
			public const string AssemblySub = "ASS";
			public const string ReleaseType = "REL";
		}
	}
}
