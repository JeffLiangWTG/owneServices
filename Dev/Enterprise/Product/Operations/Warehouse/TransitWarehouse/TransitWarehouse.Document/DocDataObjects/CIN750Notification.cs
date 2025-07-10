using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public abstract class CIN750Notification : DocDataObject, IDataSourceProvider
	{
		public CIN750Notification(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
			this.MessageID = ZGuid.NewZGuid().ToString();
		}

		#region IDataSourceProvider Members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region RefType

		public ICodeDescription RefType
		{
			get => refType;
			set => refType = SetChild(refType, value);
		}

		ICodeDescription refType;

		#endregion

		#region RefCode

		public ZString RefCode
		{
			get => refCode;
			set
			{
				value = value.RemoveHyphen();
				if (SetNonPersistentPropertyValue(RefCodeInfo, ref refCode, value))
				{
					Validate(RefCodeInfo);
				}
			}
		}

		ZString refCode;

		public ZPropertyInfo RefCodeInfo => GetZPropertyInfo(nameof(RefCode));

		#endregion

		#region MovementTime

		public ZDateTime MovementTime
		{
			get => movementTime;
			set
			{
				if (SetNonPersistentPropertyValue(MovementTimeInfo, ref movementTime, value))
				{
					Validate(MovementTimeInfo);
				}
			}
		}
		ZDateTime movementTime;

		public ZPropertyInfo MovementTimeInfo => GetZPropertyInfo(nameof(MovementTime));

		#endregion

		#region DeclaredInWarehouse

		public Address DeclaredInWarehouse
		{
			get => declaredInWarehouse;
			set => declaredInWarehouse = SetChild(declaredInWarehouse, value);
		}

		Address declaredInWarehouse;

		#endregion

		#region DeclaredInWarehouseCIN

		public ZString DeclaredInWarehouseCIN
		{
			get => declaredInWarehouseCIN;
			set
			{
				if (SetNonPersistentPropertyValue(DeclaredInWarehouseCINInfo, ref declaredInWarehouseCIN, value))
				{
					Validate(DeclaredInWarehouseCINInfo);
				}
			}
		}
		ZString declaredInWarehouseCIN;

		public ZPropertyInfo DeclaredInWarehouseCINInfo => GetZPropertyInfo(nameof(DeclaredInWarehouseCIN));

		public RegistrationNumber DeclaredInWarehouseCINNumber
		{
			get => declaredInWarehouseCINNumber;
			set => declaredInWarehouseCINNumber = SetChild(declaredInWarehouseCINNumber, value);
		}

		RegistrationNumber declaredInWarehouseCINNumber;

		#endregion

		#region GoodsDetails

		public IReadOnlyCollection<DocPackingLine> Goods
		{
			get => goods;
			set => goods = SetChildCollection(goods, value);
		}

		IReadOnlyCollection<DocPackingLine> goods;

		#endregion

		#region EnterpriseAndServerCode

		public ZString EnterpriseAndServerCode
		{
			get => enterpriseAndServerCode;
			set
			{
				if (SetNonPersistentPropertyValue(EnterpriseAndServerCodeInfo, ref enterpriseAndServerCode, value))
				{
					Validate(EnterpriseAndServerCodeInfo);
				}
			}
		}
		ZString enterpriseAndServerCode;
		public ZPropertyInfo EnterpriseAndServerCodeInfo => GetZPropertyInfo(nameof(EnterpriseAndServerCode));

		#endregion

		#region Consignment

		public EnterpriseBusinessObject SourceBusinessObject { get; set; }

		#endregion

		#region MessageID

		public ZString MessageID { get; set; }

		#endregion

		#region JobID

		public ZString JobID { get; set; }

		#endregion

		#region PopulateCINMessageNote

		public ZString Result { get; set; }

		public void PopulateCINMessageNote(ZString failedReason)
		{
			if (SourceBusinessObject is IStmNoteParent noteParent)
			{
				Result = failedReason.IsEmpty ? Res.GetString("89197a6a-9dea-45bb-b6c7-7500494f445f", "Succeed") : Res.GetString("9907312d-960d-4d48-8239-35496209855c", "Failed");
				noteParent.PopulateCIN750MessageNote(CIN750MessageNote, MessageID, failedReason);
			}
		}

		protected readonly ZString CIN750NotificationNoteHeader = Res.GetString("9dd68adf-c943-4322-b6e1-bf338cb3b53c", "CIN 750 Notification:");

		protected readonly ZString CIN750GoodsNoteHeader = Res.GetString("c8d33864-248f-480c-ad80-3fe4297e2bf8", "Goods Details:");

		protected readonly ZString CIN750FromGoodsNoteHeader = Res.GetString("d5e0e143-87e6-45f0-b665-163921f36ffc", "From Goods Details:");

		protected readonly ZString CIN750ToGoodsNoteHeader = Res.GetString("b97dfc45-4d6c-4a4a-8159-2afadeaaf6d4", "To Goods Details:");

		protected virtual ZString CIN750MessageNote { get; }

		public virtual ZString NotificationType { get; }

		#endregion
	}
}
