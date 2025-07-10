using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Supplies information to put in the CD Index so archived eDocs can be sorted easily
	/// </summary>
	public abstract class CDArchiveInfo
	{
		protected CDArchiveInfo(BusinessObject parent)
		{
			fBusinessEntity = parent;
		}

		protected CDArchiveInfo()
		{
		}

		protected BusinessObject BusinessEntity
		{
			get { return fBusinessEntity; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return BusinessEntity.Factory; }
		}

		readonly BusinessObject fBusinessEntity;

		public abstract ZString JobNumber { get; }
		public abstract ZString HouseBill { get; }
		public abstract ZString MasterBill { get; }
		public abstract ZString[] EntryNumbersList { get; }
		public abstract ZString[] OrderNumbersList { get; }
		public abstract ZString[] InvoiceNumbersList { get; }
		public abstract ZString[] ContainerNumbersList { get; }
		public abstract ZDateTime ETA { get; }
		public abstract ZDateTime ETD { get; }
		public abstract ZString Vessel { get; }
		public abstract ZString VoyageFlight { get; }
		public abstract ZString Origin { get; }
		public abstract ZString Destination { get; }
		public abstract ZString ConsigneeCode { get; }
		public abstract ZString ConsignorCode { get; }

		public ZString EntryNumber
		{
			get { return Stringify(EntryNumbersList); }
		}

		public ZString OrderNumbers
		{
			get { return Stringify(OrderNumbersList); }
		}

		public ZString InvoiceNumbers
		{
			get { return Stringify(InvoiceNumbersList); }
		}

		public ZString ContainerNumbers
		{
			get { return Stringify(ContainerNumbersList); }
		}

		protected internal static void AddToList(List<ZString> list, ZString item)
		{
			if (!item.IsEmpty && !list.Contains(item))
			{
				list.Add(item);
			}
		}

		protected internal static void AddToList(List<ZString> list, ZString[] items)
		{
			foreach (ZString item in items)
			{
				AddToList(list, item);
			}
		}

		protected internal ZString[] GetCombinedList(IEnumerable cdArchives, GetListDelegate listGetter)
		{
			List<ZString> result = new List<ZString>();
			foreach (ICDArchive archive in cdArchives)
			{
				AddToList(result, listGetter(archive.CDArchiveInfo));
			}
			return result.ToArray();
		}

		/// <summary>
		/// Turns an array of strings into a comma separated list in one string
		/// </summary>
		protected ZString Stringify(ZString[] stringArray)
		{
			Array.Sort(stringArray);
			return ZString.Join(", ", stringArray);
		}

		protected internal delegate ZString[] GetListDelegate(CDArchiveInfo info);

		#region Properties

		public CodeDescriptionPairList Properties
		{
			get
			{
				if (fProperties == null)
				{
					fProperties = new CodeDescriptionPairList();
					fProperties.AddPair(Res.GetString("8a519f03-39b6-4b95-864d-920c3ca53385", "Job Number"), JobNumber);
					fProperties.AddPair(Res.GetString("24295dcc-9707-45a4-b09b-50e1468b5da3", "House bill"), HouseBill);
					fProperties.AddPair(Res.GetString("fdf991ee-16c0-4c9b-96ab-4d42f2fdd6fd", "Master bill"), MasterBill);
					fProperties.AddPair(Res.GetString("40b32e9c-4692-4d05-870c-2bbafb606e78", "Vessel"), Vessel);
					fProperties.AddPair("Voyage/Flight", VoyageFlight);
					fProperties.AddPair("ETD", ETD.ToLongTimeString());
					fProperties.AddPair("ETA", ETA.ToLongTimeString());
					fProperties.AddPair(Res.GetString("19824559-c067-4127-a00a-9cc7686caa67", "Container Numbers"), ContainerNumbers);
					fProperties.AddPair(Res.GetString("44bc1522-5b39-4a99-9abe-fc2e18ec6041", "Consignee Code"), ConsigneeCode);
					fProperties.AddPair(Res.GetString("40f9f584-311a-4134-a63a-cdd0109c6e28", "Consignor Code"), ConsignorCode);
					fProperties.AddPair(Res.GetString("a5ce5b89-e8be-448e-ba27-985763adc79b", "Order Numbers"), OrderNumbers);
					fProperties.AddPair(Res.GetString("463d7b34-d92f-4d2d-82fe-6c2f8bf70c3f", "Entry Number"), EntryNumber);
					fProperties.AddPair(Res.GetString("ee4ddc02-8724-4853-8008-5eabc452e0c8", "Invoice Numbers"), InvoiceNumbers);
					fProperties.AddPair(Res.GetString("6a867d26-244f-4f79-a005-dfb70ed0ef91", "Origin"), Origin);
					fProperties.AddPair(Res.GetString("a9f7a5c5-67bc-4efd-ac5f-c6d538e41959", "Destination"), Destination);
				}
				return fProperties;
			}
		}

		CodeDescriptionPairList fProperties;

		#endregion
	}
}
