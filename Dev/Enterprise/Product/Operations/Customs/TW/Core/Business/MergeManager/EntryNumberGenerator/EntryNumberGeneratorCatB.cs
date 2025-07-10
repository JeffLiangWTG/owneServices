using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class EntryNumberGeneratorCatB : EntryNumberGenerator
	{
		public EntryNumberGeneratorCatB(IEntryNumberGeneratorProvider provider)
			: base(provider)
		{
		}

		protected override ZString Category => RangeTypeList.Codes.B;

		public CusEntryInstruction EntryInstruction
		{
			get
			{
				if (entryInstruction == null)
				{
					var entryNumberGeneratorProviderBusinessObject = Provider.EntryNumberGeneratorProviderBusinessObject;
					if (entryNumberGeneratorProviderBusinessObject is JobDeclaration decl)
					{
						entryInstruction = decl.CusEntryInstruction;
					}
					else if (entryNumberGeneratorProviderBusinessObject is CusEntryHeader header)
					{
						entryInstruction = header.EntryInstruction;
					}
				}
				return entryInstruction;
			}
		}
		CusEntryInstruction entryInstruction;

		public override ZString Part4
		{
			get
			{
				var result = ZString.Empty;
				if (Part4DocumentaryAddress is TWJobDocAddress tWJobDocAddress)
				{
					var cbpCodeType = tWJobDocAddress.CBPCodeType;
					var cbpCode = tWJobDocAddress.CBPCode;
					var cbpCodeLength = cbpCode.Length;
					switch (cbpCodeType)
					{
						case OrgCusCode.TaiwanCodeTypes.EPZ:
							result = cbpCodeLength >= 3 ? $"{cbpCode.Right(3)}{cbpCode.SubstringSafe(1, 1)}" : string.Empty;
							break;
						case OrgCusCode.TaiwanCodeTypes.CBF:
							result = cbpCode.Right(4);
							break;
						case OrgCusCode.TaiwanCodeTypes.SciencePark:
							switch (EntryNumberPart2)
							{
								case Constants.DeclarationTypes.Import.G2:
									result = $"0{cbpCode.Right(3)}";
									break;
								case Constants.DeclarationTypes.Export.B1:
								case Constants.DeclarationTypes.Export.B2:
									result = $"{cbpCode.Right(3)}{cbpCode.SubstringSafe(1, 1)}";
									break;
							}
							break;
						case OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark:
							result = $"{cbpCode.Right(3)}V";
							break;
					}
				}
				return result;
			}
		}

		TWJobDocAddress Part4DocumentaryAddress => Factory.GetValue(ref part4DocumentaryAddressCached, () =>
		{
			TWJobDocAddress part4DocumentaryAddress = null;
			var decl = EntryInstruction?.JobDeclaration;
			switch (EntryNumberPart2)
			{
				case Constants.DeclarationTypes.Export.B1:
					part4DocumentaryAddress = decl?.ImporterDocumentaryAddress;
					break;
				case Constants.DeclarationTypes.Export.B2:
				case Constants.DeclarationTypes.Import.G2:
					part4DocumentaryAddress = decl?.SupplierDocumentaryAddress;
					break;
			}
			return part4DocumentaryAddress;
		});
		CachedProperty<TWJobDocAddress> part4DocumentaryAddressCached;

		public override ZString Part4Caption
		{
			get
			{
				var result = ZString.Empty;
				if (Part4DocumentaryAddress.DocAddressType == DocAddressType.SupplierDocumentaryAddress)
				{
					result = Res.GetString("2051A75E-BD06-48CB-8150-449A58C4F09B", "Supplier Bonded ID");
				}
				else if (Part4DocumentaryAddress.DocAddressType == DocAddressType.ImporterDocumentaryAddress)
				{
					result = Res.GetString("36C57255-D0F5-4C1F-AA3F-5DB0E64D0FD5", "Importer Bonded ID");
				}
				return result;
			}
		}

		public override int Part4_Length => 4;

		protected override IEnumerable<ZPropertyInfo> GetValidationPropertyInfosCore()
		{
			var infos = new List<ZPropertyInfo> { Provider.EntryNumberPart1Info, Provider.EntryNumberPart2Info };
			switch (EntryNumberPart2)
			{
				case Constants.DeclarationTypes.Export.B1:
					infos.Add(entryInstruction?.CEI_OA_Warehouse2Info);
					break;
				case Constants.DeclarationTypes.Export.B2:
				case Constants.DeclarationTypes.Import.G2:
					infos.Add(entryInstruction?.CEI_OA_WarehouseInfo);
					break;
			}
			return infos;
		}
	}
}
