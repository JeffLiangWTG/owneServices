using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumberPivot : AutoWhsSerialNumberPivot
	{
		public WhsSerialNumberPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ReadOnlyMember(nameof(SerialNumberReadOnly))]
		[ResourceStringData("WhsSerialNumberPivot|SerialNumberValue", Caption = "Serial Number", ShortCaption = "S/N", MediumCaption = "Serial No.")]
		public ZString SerialNumberValue
		{
			get => SerialNumber?.WSN_SerialNumber ?? ZString.Empty;

			set
			{
				if (!value.IsEmpty || WSV_WSN_SerialNumber.IsValid)
				{
					if (SerialNumberReadOnly)
					{
						throw new InvalidOperationException("Cannot set SerialNumber because it is readonly.");
					}

					var serial = SerialNumber;

					if (serial != null && serial.IsInDatabase && !serial.WSN_SerialNumber.EqualsIgnoringCase(value))
					{
						if (HasAsnLine(serial.PK))
						{
							serial.WSN_IsInUse = false;
						}
						else
						{
							serial.Delete();
						}
						serial = null;
					}

					if (serial == null)
					{
						serial = CreateWhsSerialNumber();
					}

					serial.WSN_SerialNumber = value;
					WSV_WSN_SerialNumber = serial.PK; // Validation in the database is required, so we set it in the final action.
				}
			}
		}

		public ZPropertyInfo SerialNumberValueInfo => GetWrappedZPropertyInfo(nameof(SerialNumberValue), x => WSV_WSN_SerialNumberInfo);

		public override ZGuid WSV_WSN_SerialNumber
		{
			get => base.WSV_WSN_SerialNumber;
			set
			{
				base.WSV_WSN_SerialNumber = value;
				WSV_WSN_SerialNumberInfo.RefreshBinding();
			}
		}

		bool HasAsnLine(ZGuid serialNumberPK)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsSerialNumberPivotSchema.WSV_WSN_SerialNumber, serialNumberPK);
			query.AddToFilter(WhsSerialNumberPivotSchema.WSV_ParentTableCode, WhsAsnLineSchema.Constants.Prefix);
			return Factory.Exists(typeof(WhsSerialNumberPivot), query);
		}

		bool SerialNumberReadOnly => Parent.SerialNumberReadOnly;

		#endregion

		#region Parent and Serial Number Handling

		ISerialNumberParent Parent => parent ?? (parent = (ISerialNumberParent)Factory.Load(WSV_ParentTableCode, WSV_ParentID));
		ISerialNumberParent parent;

		public bool IsSerialNumberAlreadyInUse() => WSV_ParentTableCode.Equals(WhsDocketLineSchema.Constants.Prefix) && Parent.IsSerialNumberAlreadyInUse(this);

		public WhsSerialNumber SerialNumber
		{
			get
			{
				if (whsSerialNumber == null || !whsSerialNumber.PK.Equals(WSV_WSN_SerialNumber))
				{
					whsSerialNumber = Factory.Load<WhsSerialNumber>(WSV_WSN_SerialNumber);
				}

				return whsSerialNumber;
			}
		}

		WhsSerialNumber whsSerialNumber;

		#endregion

		#region Methods

		WhsSerialNumber CreateWhsSerialNumber()
		{
			var parent = Parent;
			if (!parent.IsAllowedToCreateOriginalSerialNumberRecord)
			{
				throw new InvalidOperationException("Try to create a serial number for the incorrect parent.");
			}

			var serial = Factory.New<WhsSerialNumber>();
			serial.WSN_OP_Product = parent.ProductPK;
			serial.WSN_OH_Client = parent.ClientPK;
			return serial;
		}

		public override void OnSaving()
		{
			if (!IsDeleted && IsInDatabase && WSV_WSN_SerialNumberInfo.HasChanges)
			{
				if (!Parent.IsAllowedToCreateOriginalSerialNumberRecord)
				{
					throw new InvalidOperationException("Try adding a serial number pivot for the incorrect parent.");
				}

				Clone();
				Delete();
			}
			base.OnSaving();
		}

		protected override bool SupportsCloneCore() => true;

		#endregion

		#region Testing

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			receiveLine.FillWithValidTestData();

			WSV_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			WSV_ParentID = receiveLine.PK;
		}
#endif

		#endregion
	}
}
