using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.Freight.Forwarding.Business;
using CusEntryHeader = Enterprise.Customs.ZA.Business.CusEntryHeader;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(AsycudaBill destination, ForwardingShipment shipmentSource)
			: base(destination, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_BolTypeInfo, GetSourceShipmentType, GetInfosAffectingShipmentType));
			var randomDeclaration = Source.Declarations.OfType<JobDeclaration>().FirstOrDefault();
			if (randomDeclaration != null)
			{
				if (Source.IsRoad)
				{
					var cusEntryHeaders = from CusEntryHeader header in randomDeclaration.ActiveEntryHeaders select header;

					Destination.CustomsEntryNumbers.RemoveAndDeleteAll();
					foreach (var source in cusEntryHeaders)
					{
						var destination = Destination.CustomsEntryNumbers.AddNew();
						Synchronisers.Add(new FieldSynchroniser(destination.CE_EntryNumInfo, source.CH_BGMReferenceInfo));
						Synchronisers.Add(new FieldSynchroniser(destination.CE_EntryTypeInfo, GetNumberType, GetNoInfo));
					}
				}
				var randomInstruction = randomDeclaration.CustomsEntryInstructions.OfType<ZA.Business.CusEntryInstruction>().FirstOrDefault(ac => ac.CEI_UCROverrideInfo != ZString.Empty);
				if (randomInstruction != null)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.ABL_UCRNumberInfo, randomInstruction.CEI_UCROverrideInfo));
				}
			}
		}

		protected override IZType GetSourceShipmentType()
		{
			return new ZString(Source.JS_ShipmentType == Core.Constants.ShipmentTypes.StandardHouse ? Core.Constants.ShipmentTypes.StandardHouse : Core.Constants.ShipmentTypes.CoLoadMaster);
		}

		IZType GetNumberType() => (string)Destination.ABL_ShipmentType switch
		{
			ShipmentTypeList.Codes.Import23 or
			ShipmentTypeList.Codes.Transhipment28 or
			ShipmentTypeList.Codes.Transit24 => (ZString)ZaLRNTypes.Codes.AFM,
			ShipmentTypeList.Codes.Export22 => (ZString)ZaLRNTypes.Codes.ABT,
			_ => ZString.Empty
		};
	}
}
