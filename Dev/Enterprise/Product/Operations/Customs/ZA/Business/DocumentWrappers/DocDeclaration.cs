using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		#region Constructors
		protected DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(jobDeclaration, factoryToWrap)
		{
		}

		public static DocDeclaration New(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			DocDeclaration result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(jobDeclaration, factoryToWrap);
			}
			else if (jobDeclaration != null)
			{
				result = new DocDeclaration(jobDeclaration, factoryToWrap);
			}

			return result;
		}

		protected delegate DocDeclaration NewDelegate(JobDeclaration declaration, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		#endregion

		#region Collection Overrides

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((InvoiceLineCompleteCollection)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			DocJobComInvoiceHeaderCollection coll = new DocJobComInvoiceHeaderCollection(JobDeclaration.Factory);
			foreach (BaseJobComInvoiceHeader header in collectionToWrap)
			{
				coll.Add(DocJobComInvoiceHeader.New((JobComInvoiceHeader)header, Factory));
			}
			return coll;
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionToWrap)
		{
			return new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		}

		#endregion

		#region ZString Fields

		public ZString Destination
		{
			get { return PortOfArrival != null ? PortOfArrival.PortName : ZString.Empty; }
		}

		public DocCusEntryHeader FirstEntryHeader
		{
			get { return (RateEntryHeaders.Count > 0) ? (DocCusEntryHeader)RateEntryHeaders[0] : null; }
		}

		public ZString EndorsementWithClearedEntriesList()
		{
			return ZString.Empty;
		}

		public ZString BOESightNumer
		{
			get { return JobDeclaration.JE_BOESightNumber; }
		}

		public ZDateTime BOESightDate
		{
			get { return JobDeclaration.JE_BOESightDate; }
		}

		public ZString BOESightYear
		{
			get
			{
				ZString result = ZString.Empty;

				if (BOESightDate.IsValid)
				{
					result = BOESightDate.Year.ToString();
				}
				return result;
			}
		}

		public ZString BOESightMonth
		{
			get
			{
				ZString result = ZString.Empty;

				if (BOESightDate.IsValid)
				{
					result = BOESightDate.Month.ToString().PadLeft(2, '0');
				}
				return result;
			}
		}

		public ZString BOESightDay
		{
			get
			{
				ZString result = ZString.Empty;

				if (BOESightDate.IsValid)
				{
					result = BOESightDate.Day.ToString().PadLeft(2, '0');
				}
				return result;
			}
		}

		public ZString VehicleRegistration
		{
			get
			{
				ZString result = ZString.Empty;
				if (JobDeclaration.IsRoad)
				{
					result = VoyageFlightNo;
				}
				return result;
			}
		}

		public ZString VoyageFlightVehicleRegoNo
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportMode == Core.Constants.TransportModes.Sea ||
					TransportMode == Core.Constants.TransportModes.Air ||
					TransportMode == Core.Constants.TransportModes.Road)
				{
					result = VoyageFlightNo;
				}

				return result;
			}
		}

		public ZString ETDYear
		{
			get
			{
				ZString result = ZString.Empty;

				if (ExportDate.IsValid)
				{
					result = ExportDate.Year.ToString();
				}

				return result;
			}
		}

		public ZString ETDMonth
		{
			get
			{
				ZString result = ZString.Empty;

				if (ExportDate.IsValid)
				{
					result = ExportDate.Month.ToString().PadLeft(2, '0');
				}

				return result;
			}
		}

		public ZString ETDDate
		{
			get
			{
				ZString result = ZString.Empty;

				if (ExportDate.IsValid)
				{
					result = ExportDate.Day.ToString().PadLeft(2, '0');
				}

				return result;
			}
		}

		public ZString ETAYear
		{
			get
			{
				ZString result = ZString.Empty;

				if (DateOfArrival.IsValid)
				{
					result = DateOfArrival.Year.ToString();
				}

				return result;
			}
		}

		public ZString ETAMonth
		{
			get
			{
				ZString result = ZString.Empty;

				if (DateOfArrival.IsValid)
				{
					result = DateOfArrival.Month.ToString().PadLeft(2, '0');
				}

				return result;
			}
		}

		public ZString ETADate
		{
			get
			{
				ZString result = ZString.Empty;

				if (DateOfArrival.IsValid)
				{
					result = DateOfArrival.Day.ToString().PadLeft(2, '0');
				}

				return result;
			}
		}

		public ZString FirstCargoStatusCode
		{
			get
			{
				if (fFirstCargoStatusCode.IsEmpty)
				{
					SetCargoStatusCodes();
				}

				return fFirstCargoStatusCode;
			}
		}

		public ZString SecondCargoStatusCode
		{
			get
			{
				if (fSecondCargoStatusCode.IsEmpty)
				{
					SetCargoStatusCodes();
				}

				return fSecondCargoStatusCode;
			}
		}

		public ZString AgentCode
		{
			get { return JobDeclaration.AgentCode; }
		}

		public ZString AgentName
		{
			get { return JobDeclaration.AgentName; }
		}

		public ZString AgentAddress1 => JobDeclaration.DeclarantAddress?.Address1 ?? ZString.Empty;

		public ZString AgentAddress2 => JobDeclaration.DeclarantAddress?.Address2 ?? ZString.Empty;

		public ZString IssuedAtYear
		{
			get
			{
				ZString result = ZString.Empty;

				if (IssuedAtDate.IsValid)
				{
					result = IssuedAtDate.Year.ToString();
				}

				return result;
			}
		}

		public ZString IssuedAtMonth
		{
			get
			{
				ZString result = ZString.Empty;

				if (IssuedAtDate.IsValid)
				{
					result = IssuedAtDate.Month.ToString().PadLeft(2, '0');
				}

				return result;
			}
		}

		public ZString IssuedAtDay
		{
			get
			{
				ZString result = ZString.Empty;

				if (IssuedAtDate.IsValid)
				{
					result = IssuedAtDate.Day.ToString().PadLeft(2, '0');
				}

				return result;
			}
		}

		public ZString ContainerAndSealNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				String separator = " ";
				if (JobDeclaration.IsContainerised)
				{
					DocCusContainerCollection containerCollection = Containers;

					int i = 0;
					foreach (DocCusContainer currentContainer in containerCollection)
					{
						if (currentContainer.IsContainerToBeAdvised)
						{
							result += currentContainer.ContainerNumber + " / " + currentContainer.SealNumber + "-";
							i++;
						}
					}
					if (i <= 4)
					{
						separator = "\n";
					}
				}
				return result.Replace("-", separator).Trim();
			}
		}

		public ZString CustomsOfficeCode
		{
			get { return JobDeclaration.JE_CustomsOffice; }
		}

		public ZString CustomsOfficeDescription
		{
			get
			{
				var result = ZString.Empty;
				var codeList = ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);
				var customsOfficeCode = this.CustomsOfficeCode;
				if (!customsOfficeCode.IsEmpty)
				{
					result = codeList.GetDescriptionFromCode(customsOfficeCode);
				}

				return result;
			}
		}

		public ZString TermsCharOne
		{
			get { return ZString.Empty; }
		}

		public ZString TermsCharTwo
		{
			get { return ZString.Empty; }
		}

		public ZString TermsCharThree
		{
			get { return ZString.Empty; }
		}

		public ZString TransportDocumentNumber => JobDeclaration.TransportDocumentNumber;

		#region Supplier MICR Number

		public ZString MICROne
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 1)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[0].ToString();
				}

				return result;
			}
		}

		public ZString MICRTwo
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 2)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[1].ToString();
				}

				return result;
			}
		}

		public ZString MICRThree
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 3)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[2].ToString();
				}

				return result;
			}
		}

		public ZString MICRFour
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 4)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[3].ToString();
				}

				return result;
			}
		}

		public ZString MICRFive
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 5)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[4].ToString();
				}

				return result;
			}
		}

		public ZString MICRSix
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 6)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[5].ToString();
				}

				return result;
			}
		}

		public ZString MICRSeven
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 7)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[6].ToString();
				}

				return result;
			}
		}

		public ZString MICREight
		{
			get
			{
				ZString result = ZString.Empty;

				if (Supplier != null && Supplier.MiscServ != null && Supplier.MiscServ.IMEFTBankBSB.Length >= 8)
				{
					result = Supplier.MiscServ.IMEFTBankBSB[7].ToString();
				}

				return result;
			}
		}
		#endregion

		#endregion

		#region ZInt Fields

		public ZInt FirstPageContainerRowCount
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.FirstPageContainerRowCount, 11); }
		}

		public ZInt FollowOnPageContainerRowCount
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.FollowOnPageContainerRowCount, 32); }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime IssuedAtDate
		{
			get { return JobDeclaration.JE_MasterBillIssuedDate; }
		}

		public ZDateTime HouseBillIssuedDate
		{
			get { return JobDeclaration.HouseBillIssuedDate; }
		}

		#endregion

		#region Wrapper Fields

		public DocJobComInvoiceHeader InvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		public DocCusEntryLineCollection EntryLines
		{
			get
			{
				DocCusEntryLineCollection collection = new DocCusEntryLineCollection(Factory);
				foreach (CusEntryHeader header in JobDeclaration.CustomsEntryHeaders)
				{
					foreach (CusEntryLine mergedLine in header.MergedLines)
					{
						DocCusEntryLine docLine = DocCusEntryLine.New(mergedLine, Factory);
						collection.Add(docLine);
					}
				}
				return collection;
			}
		}

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader
		{
			get { return (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal; }
		}

		public DocUNLOCO IssuedAt
		{
			get { return DocUNLOCO.New(Factory, JobDeclaration.JE_RL_NKMasterBillIssuedAt); }
		}

		#endregion

		#region Collections

		public DocCusContainerCollection Containers
		{
			get
			{
				DocCusContainerCollection result = (DocCusContainerCollection)ContainersInternal;
				result.Sort("ContainerNumber", ListSortDirection.Ascending);
				return result;
			}
		}

		public DocDA74ContainerCollection DA74Containers
		{
			get
			{
				if (fDA74Containers == null)
				{
					fDA74Containers = new DocDA74ContainerCollection(Factory);

					if (Containers.Count == 0)
					{
						//Hack the collection to always have at least 1 element.
						fDA74Containers.Add(new DocDA74Container("", "", Factory));
					}
					else
					{
						BuildDA74ContainersCollection();
					}
				}
				return fDA74Containers;
			}
		}

		public DocDA74ContainerCollection DA74ContainersFollowOn
		{
			get
			{
				if (fDA74ContainersFollowOn == null)
				{
					BuildDA74ContainersCollection();
				}

				return fDA74ContainersFollowOn;
			}
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal; }
		}

		public DocJobComInvoiceHeaderCollection InvoiceHeaders
		{
			get { return (DocJobComInvoiceHeaderCollection)InvoiceHeadersInternal; }
		}

		#endregion

		#region Implementation

		protected ZString fFirstCargoStatusCode;
		protected ZString fSecondCargoStatusCode;
		protected DocDA74ContainerCollection fDA74Containers;
		protected DocDA74ContainerCollection fDA74ContainersFollowOn;

		JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)WrappedObject; }
		}

		protected void SetCargoStatusCodes()
		{
			if (!JobDeclaration.IsContainerised)
			{
				fFirstCargoStatusCode = GetCargoStatusCodeForContainerMode(ContainerMode);
				fSecondCargoStatusCode = "";
			}
			else if (TransportMode == Core.Constants.TransportModes.Sea)
			{
				ZString firstContainerMode = ZString.Empty;
				ZString secondContainerMode = ZString.Empty;

				foreach (DocCusContainer currentContainer in Containers)
				{
					if (firstContainerMode.IsEmpty)
					{
						firstContainerMode = currentContainer.Type;
					}

					if (firstContainerMode != currentContainer.Type && secondContainerMode.IsEmpty)
					{
						secondContainerMode = currentContainer.Type;
						break;
					}
				}

				fFirstCargoStatusCode = GetCargoStatusCodeForContainerMode(firstContainerMode);
				fSecondCargoStatusCode = GetCargoStatusCodeForContainerMode(secondContainerMode);
			}
		}

		protected internal ZString GetCargoStatusCodeForContainerMode(ZString containerMode)
		{
			ZString result = ZString.Empty;

			switch (containerMode)
			{
				case "FCL":
					result = "8";
					break;
				case "LCL":
					result = "7";
					break;
				case "FCG":
					result = "5";
					break;
				case "EMP":
					result = "4";
					break;
				case "BLK":
					result = "10";
					break;
				case "BBK":
					result = "11";
					break;
				case "LQD":
					result = "10";
					break;
			}

			return result;
		}

		protected void BuildDA74ContainersCollection()
		{
			ZInt count = 1;
			ZString containerNumber1 = "";
			fDA74Containers = new DocDA74ContainerCollection(Factory);
			fDA74ContainersFollowOn = new DocDA74ContainerCollection(Factory);
			foreach (DocCusContainer container in Containers)
			{
				if (count % 2 == 1)
				{
					containerNumber1 = container.ContainerNumber;
					if (Containers.Count == count)
					{
						AddDA74ContainerToCollection(count, container.ContainerNumber, "");
					}
				}
				else
				{
					AddDA74ContainerToCollection(count, containerNumber1, container.ContainerNumber);
				}

				count++;
			}
		}

		protected void AddDA74ContainerToCollection(ZInt count, ZString containerNum1, ZString containerNum2)
		{
			DocDA74Container newDA74Container = new DocDA74Container(containerNum1, containerNum2, Factory);
			if (count <= FirstPageContainerRowCount * 2)
			{
				fDA74Containers.Add(newDA74Container);
			}
			else
			{
				fDA74ContainersFollowOn.Add(newDA74Container);
			}
		}

		#endregion
	}
}
