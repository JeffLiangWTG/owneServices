using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class WarehouseInBondDataEventContextReader
	{
		public WarehouseInBondDataEventContextReader(CusInBondMoveHeader moveHeader)
		{
			this.moveHeader = Argument.NotNull(moveHeader, "moveHeader");
		}

		public void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			var factory = moveHeader.Factory;
			var inBondNumber = moveHeader.InBondNumber;
			if (!inBondNumber.IsEmpty)
			{
				values.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumber, inBondNumber);
				values.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumberCountryOfIssue, (ZString)Core.Constants.CountryCodes.UnitedStates);
				values.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumberType, (ZString)Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
			}
		}

		protected readonly CusInBondMoveHeader moveHeader;
	}
}
