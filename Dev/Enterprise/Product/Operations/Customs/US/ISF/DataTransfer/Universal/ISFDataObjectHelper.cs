using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal
{
	public class ISFDataObjectHelper : UniversalCommonHelper
	{
		public ISFDataObjectHelper(CusISFHeader header)
			: base(header.Factory)
		{
			InitializeNumbers();
		}

		public void InitializeNumbers()
		{
			LineNo = 0;
			ShipToPartyNo = 0;
		}

		#region Lists

		public WayBillTypeList WayBillTypeList
		{
			get { return factory.GetCachedValue<WayBillTypeList>(); }
		}

		public ContainerModeList ContainerModeList
		{
			get { return factory.GetCachedValue<ContainerModeList>(); }
		}

		public CodeDescriptionPairList EntryTypeList
		{
			get
			{
				if (entryTypeList == null)
				{
					entryTypeList = new CodeDescriptionPairList();
					entryTypeList.AddPair("ISF", Res.GetString("ISF|8AE73255-47A2-4138-A620-9AB08E8307AB", "ISF Transaction Number"));
					entryTypeList.AddPair("ENS", Res.GetString("ENS|AE869BB7-AA80-478C-A9C9-98B4ABB1E6D1", "US CBP Entry Number"));
				}

				return entryTypeList;
			}
		}
		CodeDescriptionPairList entryTypeList;

		#endregion

		#region Numbers

		public int LineNo
		{
			get { return lineNo++; }
			set { lineNo = value; }
		}
		int lineNo;

		public byte ShipToPartyNo
		{
			get { return shipToPartyNo++; }
			set { shipToPartyNo = value; }
		}
		byte shipToPartyNo;

		#endregion
	}
}
