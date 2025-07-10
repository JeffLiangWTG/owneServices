using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Flags]
	public enum WhsDocketCollectionBuilderLineOptions
	{
		None,
		MergeLine,
		AddZeroQuantityLines,
	}

	public abstract class WhsDocketCollectionBuilder<TDocket, TLineData>
		where TDocket : WhsDocket
		where TLineData : LineData, new()
	{
		#region Line Data and Options

		public static TLineData GetNewLineData() => new TLineData();

		#endregion

		#region Constructors

		protected WhsDocketCollectionBuilder(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			Factory = factory;
		}

		#endregion

		#region Add Docket Line

		public WhsDocketLine AddLine(
			WhsDocketLine line,
			ZString externalReference,
			ZString docketSubType)
		{
			return AddLine(
				line.Docket.Warehouse,
				line.Docket.Client,
				externalReference,
				docketSubType,
				line.SupplierPart,
				line.WE_TransactionQuantity,
				line.LocationString,
				line.WE_LineComment,
				line.WE_ReasonCode,
				line.WE_BondedEntryKey,
				line.WE_ExpiryDate,
				line.WE_PackingDate,
				line.WE_PartAttrib1,
				line.WE_PartAttrib2,
				line.WE_PartAttrib3,
				line.WE_SerialNumber);
		}

		public WhsDocketLine AddLine(
			WhsWarehouse whs,
			OrgHeader org,
			ZString externalReference,
			ZString docketSubType,
			OrgSupplierPart part,
			ZDecimal units,
			ZString locationString,
			ZString lineComment,
			ZString reasonCode)
		{
			return AddLine(whs.PK, org.PK, externalReference, docketSubType, part.PK, units, locationString, lineComment, reasonCode);
		}

		public WhsDocketLine AddLine(
			ZGuid whsPK,
			ZGuid orgPK,
			ZString externalReference,
			ZString docketSubType,
			ZGuid partPK,
			ZDecimal units,
			ZString locationString,
			ZString lineComment,
			ZString reasonCode)
		{
			return AddLine(whsPK, orgPK, externalReference, docketSubType, partPK, units, locationString, lineComment, reasonCode, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
		}

		public WhsDocketLine AddLine(
			 WhsWarehouse whs,
			 OrgHeader org,
			 ZString externalReference,
			 ZString docketSubType,
			 OrgSupplierPart part,
			 ZDecimal units,
			 ZString locationString2,
			 ZString lineComment,
			 ZString reasonCode,
			 ILineAttributes attributes)
		{
			return AddLine(whs, org, externalReference, docketSubType, part, units, locationString2, lineComment, reasonCode, attributes.BondedEntryKey, attributes.ExpiryDate, attributes.PackingDate, attributes.PartAttrib1, attributes.PartAttrib2, attributes.PartAttrib3, attributes.SerialNumber);
		}

		public WhsDocketLine AddLine(
			ZGuid whsPK,
			ZGuid orgPK,
			ZString externalReference,
			ZString docketSubType,
			ZGuid partPK,
			ZDecimal units,
			ZString locationString2,
			ZString lineComment,
			ZString reasonCode,
			ILineAttributes attributes)
		{
			return AddLine(whsPK, orgPK, externalReference, docketSubType, partPK, units, locationString2, lineComment, reasonCode, attributes.BondedEntryKey, attributes.ExpiryDate, attributes.PackingDate, attributes.PartAttrib1, attributes.PartAttrib2, attributes.PartAttrib3, attributes.SerialNumber);
		}

		public WhsDocketLine AddLine(
			 WhsWarehouse whs,
			 OrgHeader org,
			 ZString externalReference,
			 ZString docketSubType,
			 OrgSupplierPart part,
			 ZDecimal units,
			 ZString locationString2,
			 ZString lineComment,
			 ZString reasonCode,
			 ZString entryKey,
			 ZDate expiryDate,
			 ZDate packingDate,
			 ZString partAttrib1,
			 ZString partAttrib2,
			 ZString partAttrib3,
			 ZString serialNumber)
		{
			return AddLine(whs.PK, org.PK, externalReference, docketSubType, part.PK, units, locationString2, lineComment, reasonCode, entryKey, expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3, serialNumber);
		}

		public WhsDocketLine AddLine(
			 ZGuid whsPK,
			 ZGuid orgPK,
			 ZString externalReference,
			 ZString docketSubType,
			 ZGuid partPK,
			 ZDecimal units,
			 ZString locationString2,
			 ZString lineComment,
			 ZString reasonCode,
			 ZString entryKey,
			 ZDate expiryDate,
			 ZDate packingDate,
			 ZString partAttrib1,
			 ZString partAttrib2,
			 ZString partAttrib3,
			 ZString serialNumber)
		{
			var data = new TLineData();
			data.WhsPK = whsPK;
			data.OrgPK = orgPK;
			data.ExternalReference = externalReference;
			data.DocketSubType = docketSubType;
			data.PartPK = partPK;
			data.Quantity = units;
			data.LocationString2 = locationString2;
			data.LineComment = lineComment;
			data.ReasonCode = reasonCode;
			data.EntryKey = entryKey;
			data.ExpiryDate = expiryDate;
			data.PackingDate = packingDate;
			data.PartAttrib1 = partAttrib1;
			data.PartAttrib2 = partAttrib2;
			data.PartAttrib3 = partAttrib3;
			data.SerialNumber = serialNumber;

			return AddLine(data, WhsDocketCollectionBuilderLineOptions.None);
		}

		public virtual WhsDocketLine AddLine(TLineData data, WhsDocketCollectionBuilderLineOptions options)
		{
			return AddLineCore(data, options, ZGuid.Empty);
		}

		public WhsDocketLine AddLine(TLineData data, WhsDocketCollectionBuilderLineOptions options, ZGuid docketPK)
		{
			if (!docketPK.IsValid)
			{
				throw new ArgumentException("DocketPK is empty");
			}
			return AddLineCore(data, options, docketPK);
		}

		WhsDocketLine AddLineCore(TLineData data, WhsDocketCollectionBuilderLineOptions options, ZGuid docketPK)
		{
			WhsDocketLine line = null;

			if (CanAddLine(data, options))
			{
				var docket = GetNewOrMatchingDocket(data, docketPK);

				if (options == WhsDocketCollectionBuilderLineOptions.MergeLine)
				{
					line = FindLineToMerge(docket, data);
				}

				if (line != null)
				{
					// merge line
					line.WE_TransactionQuantity += data.Quantity;
				}
				else
				{
					line = docket.Lines.AddNew();
					line.WE_OP = data.PartPK;
					line.WE_TransactionQuantity = data.Quantity;
					line.LocationString = data.LocationString2;
					line.WE_LineComment = data.LineComment;
					line.WE_ReasonCode = data.ReasonCode;
					line.WE_BondedEntryKey = data.EntryKey;
					line.WE_ExpiryDate = data.ExpiryDate;
					line.WE_PackingDate = data.PackingDate;
					line.WE_PartAttrib1 = data.PartAttrib1;
					line.WE_PartAttrib2 = data.PartAttrib2;
					line.WE_PartAttrib3 = data.PartAttrib3;
					line.WE_SerialNumber = data.SerialNumber;
					if (line.WE_AdjustmentArrivalDate.IsEmpty && line.WE_TransactionQuantity > 0)
					{
						line.WE_AdjustmentArrivalDate = docket.WD_ArrivalDate;
					}
				}

				PostAddLine(line);
			}

			return line;
		}

		bool CanAddLine(TLineData data, WhsDocketCollectionBuilderLineOptions options)
		{
			return (data.Quantity != 0m || options.HasFlag(WhsDocketCollectionBuilderLineOptions.AddZeroQuantityLines));
		}

		TDocket GetNewOrMatchingDocket(TLineData data, ZGuid docketPK)
		{
			TDocket result;
			if (docketPK.IsValid)
			{
				result = Factory.Load<TDocket>(docketPK);
				DocketsInternal.Add(result);
			}
			else
			{
				result = FindDocket(data);
				if (result == null)
				{
					result = AddNewDocket(data);
				}
			}

			return result;
		}

		protected virtual TDocket FindDocket(TLineData data)
		{
			return Dockets.FindDocket(data.WhsPK, data.OrgPK, data.ExternalReference, d => AdditionalMatchingCriteria(d, data), data.RequiredDateOffset, useStrictComparisonRules: true);
		}

		protected virtual bool AdditionalMatchingCriteria(TDocket docket, TLineData data) => data.DocketSubType.EqualsIgnoringCase(docket.WD_DocketSubType);

		protected virtual void PostAddLine(WhsDocketLine line)
		{
		}

		TDocket AddNewDocket(TLineData data)
		{
			var docket = CreateNewDocket();
			DocketsInternal.Add(docket);

			docket.WD_WW_Whs = data.WhsPK;
			docket.WD_OH_Client = data.OrgPK;
			docket.WD_ExternalReference = data.ExternalReference;
			docket.RequiredDate = data.RequiredDateOffset.ToZDateTime();
			if (!data.DocketSubType.IsEmpty)
			{
				docket.WD_DocketSubType = data.DocketSubType;
			}
			docket.IsUniqueExternalReferenceCreatedOnSave = true;
			AddNewDocketCore(docket, data);

			return docket;
		}

		protected virtual TDocket CreateNewDocket()
		{
			return Factory.New<TDocket>();
		}

		protected virtual void AddNewDocketCore(TDocket docket, TLineData data)
		{
		}

		WhsDocketLine FindLineToMerge(TDocket docket, TLineData data)
		{
			foreach (var line in docket.Lines)
			{
				if (data.PartPK == line.WE_OP &&
					data.EntryKey == line.WE_BondedEntryKey &&
					data.ExpiryDate == line.WE_ExpiryDate &&
					data.PackingDate == line.WE_PackingDate &&
					data.PartAttrib1 == line.WE_PartAttrib1 &&
					data.PartAttrib2 == line.WE_PartAttrib2 &&
					data.PartAttrib3 == line.WE_PartAttrib3 &&
					data.SerialNumber == line.WE_SerialNumber)
				{
					return line;
				}
			}
			return null;
		}

		#endregion

		#region Finalise All Dockets

		public void FinaliseAllDockets()
		{
			foreach (var docket in Dockets)
			{
				FinaliseDocket(docket);
			}
		}

		protected virtual void FinaliseDocket(TDocket docket)
		{
			docket.FinaliseDocket();
		}

		#endregion

		#region Properties

		public IReadOnlyCollection<TDocket> Dockets
		{
			get { return DocketsInternal; }
		}

		HashSet<TDocket> DocketsInternal
		{
			get { return docketsInternal ?? (docketsInternal = new HashSet<TDocket>()); }
		}

		HashSet<TDocket> docketsInternal;

		public BusinessObjectFactory Factory
		{
			get;
			private set;
		}

		#endregion
	}

	#region LineData

	public class LineData
	{
		public ZGuid WhsPK;
		public ZGuid OrgPK;
		public ZString ExternalReference;
		public ZString DocketSubType;
		public ZDateTimeOffset RequiredDateOffset;
		public ZGuid PartPK;
		public ZDecimal Quantity;
		public ZString LocationString1;
		public ZString LocationString2;
		public ZString LineComment;
		public ZString ReasonCode;

		#region Attributes

		public ZString EntryKey;
		public ZDate ExpiryDate;
		public ZDate PackingDate;
		public ZString PartAttrib1;
		public ZString PartAttrib2;
		public ZString PartAttrib3;
		public ZString SerialNumber;

		public ILineAttributes Attributes
		{
			get { return attributes; }
			set
			{
				attributes = value;
				EntryKey = attributes.BondedEntryKey;
				ExpiryDate = attributes.ExpiryDate;
				PackingDate = attributes.PackingDate;
				PartAttrib1 = attributes.PartAttrib1;
				PartAttrib2 = attributes.PartAttrib2;
				PartAttrib3 = attributes.PartAttrib3;
				SerialNumber = attributes.SerialNumber;
			}
		}

		ILineAttributes attributes;

		#endregion
	}

	#endregion
}
