using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class DA74DocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper
	{
		#region Constructor

		public DA74DocumentWrapper(CusEntryHeader cusEntryHeader)
			: base(cusEntryHeader?.Factory ?? new BusinessObjectFactory())
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			this.EntryHeader = cusEntryHeader;
			this.Declaration = cusEntryHeader.Declaration;
			CUSDECSource = new MessageSendingObject(EntryHeader);
		}

		#endregion

		#region Related Objects

		public CusEntryHeader EntryHeader { get; private set; }
		public JobDeclaration Declaration { get; private set; }

		internal ICUSDECMessageDataProvider CUSDECSource;

		#endregion

		#region Fields

		#region ZString

		public ZString AgentCode => CUSDECSource.AgentCode;
		public ZString HouseBillNo => CUSDECSource.HouseBill;
		public ZString HouseBillIssuedAt { get; }
		public ZString LocationOfGoods => CUSDECSource.LocationOfGoods;
		public ZString MarksAndNumbers => marksAndNumbers ?? (marksAndNumbers = ZString.Join(System.Environment.NewLine, CUSDECSource.MarksAndNumbers.Where(x => !x.IsEmpty).ToArray()));
		string marksAndNumbers;
		public ZString OriginalMRN => CUSDECSource?.OriginalMRN ?? ZString.Empty;

		public ZString TransportDocumentNumberInBusinessLogic => CUSDECSource.GetTransportDocumentNumberInBusinessLogic();

		public ZString TransportDocumentIssuedAt => CUSDECSource.TransportDocumentIssuedAt;
		public ZString TransportMode => CUSDECSource.TransportMode;
		public ZString VoyageFlightNo => CUSDECSource.VoyageFlightNo;
		public ZString TransportName => CUSDECSource.TransportName;

		#endregion

		#region Numeric

		public ZDecimal CustomsValue => CUSDECSource.TotalCustomsValue;
		public ZDecimal GrossWeightInKG => CUSDECSource.GrossWeightInKG;
		public ZInt Packages
		{
			get
			{
				ZInt result;
				return ZInt.TryParse(CUSDECSource.TotalNoOfPacks, out result) ? result : ZInt.Zero;
			}
		}
		public ZInt PackageThousandsValue => Packages > 999 ? Packages / 1000 : 0;
		public ZInt PackageHundredsValue => Packages > 99 ? (Packages - PackageThousandsValue * 1000) / 100 : 0;
		public ZInt PackageTensValue => Packages > 9 ? (Packages - PackageThousandsValue * 1000 - PackageHundredsValue * 100) / 10 : 0;
		public ZInt PackageOnesValue
		{
			get
			{
				ZString result = Packages.ToString();
				return ZInt.Parse(result.Right(1));
			}
		}

		#endregion

		#region Dates
		public ZDateTime HouseBillIssuedDate => CUSDECSource.HouseBillIssuedDate;
		public ZDateTime TransportDocDate => CUSDECSource.TransportDocumentDate;
		public ZDateTime DateOfDepartureOrDateOfFlight => CUSDECSource.DateOfDepartureOrDateOfFlight;
		public ZDateTime DateOfArrival => CUSDECSource.DateOfArrival;
		#endregion

		public AddressInformationDocWrapper Exporter => exporter ?? (exporter = new AddressInformationDocWrapper(CUSDECSource.Exporter));
		AddressInformationDocWrapper exporter;

		public AddressInformationDocWrapper Importer => importer ?? (importer = new AddressInformationDocWrapper(CUSDECSource.Importer));
		AddressInformationDocWrapper importer;

		public GlbStaff DeclarantUser => GlbStaff.CurrentUser;

		public BusinessObjectCollectionWrapper<CusContainerDocWrapper> ContainersCollection
		{
			get
			{
				if (containersCollection == null)
				{
					containersCollection = new BusinessObjectCollectionWrapper<CusContainerDocWrapper>(CUSDECSource.Containers.Select(cont => new CusContainerDocWrapper(cont, Factory)));
				}
				return containersCollection;
			}
		}
		BusinessObjectCollectionWrapper<CusContainerDocWrapper> containersCollection;

		#endregion

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => EntryHeader;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => this.EntryHeader.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
