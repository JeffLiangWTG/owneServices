using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[CodeProperty(WhsItemTransferHeader.Schema.WTH_ReferenceNumber)]
	[DescriptionProperty(WhsItemTransferHeader.Schema.WTH_ReferenceNumber)]
	public class WhsItemTransferHeader : AutoWhsItemTransferHeader,
		IWhsItemTransferHeader,
		IDocumentSupportable
	{
		public WhsItemTransferHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region WTH_ReferenceNumber

		[ResourceStringData("c3d9ae3d-607e-4256-89f9-4aab15ae4668", Caption = "Transfer ID")]
		public override ZString WTH_ReferenceNumber
		{
			get { return base.WTH_ReferenceNumber; }
			set { base.WTH_ReferenceNumber = value; }
		}

		#endregion

		#region WTH_WW_Warehouse

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WTH_WW_Warehouse
		{
			get { return base.WTH_WW_Warehouse; }
			set { base.WTH_WW_Warehouse = value; }
		}

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WTH_WW_Warehouse); }
		}

		#endregion

		#region WTH_IsFinalised

		[ResourceStringData("28182713-4421-4d0a-9ba6-c40ad6fb0088", Caption = "Is Finalized")]
		public override ZBool WTH_IsFinalised
		{
			get { return base.WTH_IsFinalised; }
			set { base.WTH_IsFinalised = value; }
		}

		#endregion

		#region WTH_TransferType

		[ResourceStringData("68c0dd08-595c-490f-8df7-30546b3a8b44", Caption = "Transfer Type")]
		public override ZString WTH_TransferType
		{
			get { return base.WTH_TransferType; }
			set { base.WTH_TransferType = value; }
		}

		#endregion

		#region TransferLines

		public WhsItemTransferLineCollection Lines =>
			lines ??
			(lines = new WhsItemTransferLineCollection(this));

		WhsItemTransferLineCollection lines;

		#endregion

		#region PackageStates

		public IReadOnlyCollection<WhsItemPackageState> PackageStates =>
			packageStates ??
			(packageStates = Lines.Select(t => t.PackageState).ToList());

		IReadOnlyCollection<WhsItemPackageState> packageStates;

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WTH_ReferenceNumber.IsEmpty ?
					Res.GetString("4794b55c-680b-4fe4-9881-4293263522c9", "Transfer") :
					Res.GetString("4e3851ba-734e-4737-8137-bca8cbe29112", "Transfer {0}", WTH_ReferenceNumber);
			}
		}

		#endregion

		#region DocumentSupporter

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new WhsItemTransferHeaderDocumentSupporter(this));

		DocumentSupporter documentSupporter;

		#endregion

		#region Testing

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			WTH_TransferType = "PUT";

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		#endregion
	}
}
