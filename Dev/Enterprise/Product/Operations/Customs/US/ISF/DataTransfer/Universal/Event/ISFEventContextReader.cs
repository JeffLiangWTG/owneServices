using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal
{
	class ISFEventContextReader
	{
		public ISFEventContextReader(CusISFHeader header)
		{
			this.header = Argument.NotNull(header, "CusISFHeader");
		}
		readonly CusISFHeader header;

		public void AddCusISFContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.DeclarationReference, header.BF_JobReference);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumber, header.BF_CustomsReference);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumberType, (ZString)ISFConstants.EntryNumberConstants.ISF);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumberCountryOfIssue, (ZString)Core.Constants.CountryCodes.UnitedStates);

			header.ReferenceDatas.Where(x => x.BB_BillType == BillTypeList.Codes.OceanBillOfLading).ForEach(mbo => contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, mbo.BB_BillNum));
			header.ReferenceDatas.Where(x => x.BB_BillType == BillTypeList.Codes.HouseBillOfLading).ForEach(hbo => contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLNumber, hbo.BB_BillNum));
		}
	}
}
