using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.ArchiveManager;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.ArchiveManager
{
	/// <summary>
	/// Base archive wrapper object for JobDeclaration
	/// </summary>
	/// <typeparam name="I">JobDeclaration Interface type for a particular country</typeparam>
	/// <typeparam name="D">JobDeclaration class type for a particular country</typeparam>
	public abstract class ArchiveableJobDeclaration<I, D> : IArchiveableBusinessObject
		where I : class
		where D : BaseJobDeclaration
	{
		protected ArchiveableJobDeclaration(I declaration)
		{
			Declaration = declaration as D;
			if (declaration == null)
			{
				throw new ArgumentException("declaration must be a " + typeof(D).ToString() + ", and must not be null.");
			}
		}

		public virtual IEnumerable<ArchiveDocumentDescriptor> ArchiveDocuments
		{
			get { yield break; }
		}

		public virtual BusinessObject ArchiveableBusinessObject
		{
			get { return Declaration; }
		}

		public virtual Guid BranchPK
		{
			get { return Declaration.JE_GB.ToGuid(); }
		}

		public virtual ArchiveReferenceKey NaturalKey
		{
			get { return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.DeclarationNo, Declaration.JE_DeclarationReference); }
		}

		public IEnumerable<ArchiveReferenceKey> AdditionalKeys
		{
			get
			{
				yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.JobNo, Declaration.JobNumber);

				foreach (Bill bill in Declaration.Bills)
				{
					if (bill.CU_BillType == BillTypeList.Codes.MasterBill)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Masterbill, bill.CU_BillNum);
					}
					else if (bill.CU_BillType == BillTypeList.Codes.HouseBill || bill.CU_BillType == BillTypeList.Codes.SubHouseBill)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Housebill, bill.CU_BillNum);
					}
				}

				yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.OwnerReference, Declaration.JE_OwnerRef);

				foreach (OrderItem orderItem in Declaration.DocsAndCartage.OrderItems)
				{
					yield return new ArchiveReferenceKey(FreightArchiveKeyTypes.Order, orderItem.JT_OrderReference);
				}

				if (Declaration.Supplier != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Consignor, Declaration.Supplier.OH_Code);
				}

				if (Declaration.Importer != null)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.Consignee, Declaration.Importer.OH_Code);
				}

				if (Declaration.IsAir)
				{
					if (!Declaration.JE_VoyageFlightNo.IsEmpty)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.FlightAndDate, string.Format("{0}/{1}", Declaration.JE_VoyageFlightNo, ArrivalDate));
					}
				}
				else if (Declaration.IsSea)
				{
					if (!Declaration.JE_LloydsIMO.IsEmpty)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.VesselVoyage, string.Format("{0}/{1}", Declaration.JE_LloydsIMO, Declaration.JE_VoyageFlightNo));
					}
				}
				else
				{
					if (!Declaration.JE_VoyageFlightNo.IsEmpty)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.RegoAndDate, string.Format("{0}/{1}", Declaration.JE_VoyageFlightNo, ArrivalDate));
					}
				}

				List<ZString> workingList = new List<ZString>();

				foreach (BaseCusContainer container in Declaration.CusContainers)
				{
					AddToListIfUnique(workingList, container.CO_ContainerNumber);
				}

				foreach (ZString workingKey in workingList)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.ContainerNo, workingKey);
				}

				List<ZGuid> cusEntryNumberParents = new List<ZGuid>();
				cusEntryNumberParents.Add(Declaration.PK);

				foreach (BaseJobComInvoiceHeader invoice in Declaration.Invoices)
				{
					if (!invoice.JZ_InvoiceNumber.IsEmpty)
					{
						yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.InvoiceNo, invoice.JZ_InvoiceNumber);
					}
					cusEntryNumberParents.Add(invoice.PK);
				}

				foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
				{
					cusEntryNumberParents.Add(entryHeader.PK);
				}

				AddAnyAdditionalParents(cusEntryNumberParents);

				workingList.Clear();

				foreach (CusEntryNumber number in Declaration.Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, cusEntryNumberParents)))
				{
					AddToListIfUnique(workingList, number.CE_EntryNum);
				}

				foreach (ZString workingKey in workingList)
				{
					yield return new ArchiveReferenceKey(CustomsArchiveReferenceKeys.CustomsReferenceNumber, workingKey);
				}

				workingList.Clear();

				foreach (BaseJobComInvoiceLine line in Declaration.InvoiceLines)
				{
					AddToListIfUnique(workingList, line.JI_PartNo);
				}

				foreach (ZString workingKey in workingList)
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.PartNo, workingKey);
				}

				foreach (var key in CountrySpecificAdditionalKeys)
				{
					yield return key;
				}
			}
		}

		protected D Declaration
		{
			get;
			private set;
		}

		protected virtual IEnumerable<ArchiveReferenceKey> CountrySpecificAdditionalKeys
		{
			get { yield break; }
		}

		protected virtual void AddAnyAdditionalParents(List<ZGuid> cusEntryNumberParents)
		{
		}

		ZString ArrivalDate
		{
			get
			{
				return Declaration.JE_DateOfArrival.IsValid ? Declaration.JE_DateOfArrival.ToString("CCyyMMdd") : "";
			}
		}

		void AddToListIfUnique(List<ZString> workingList, ZString newKey)
		{
			if (!newKey.IsEmpty && !workingList.Contains(newKey))
			{
				workingList.Add(newKey);
			}
		}
	}
}
