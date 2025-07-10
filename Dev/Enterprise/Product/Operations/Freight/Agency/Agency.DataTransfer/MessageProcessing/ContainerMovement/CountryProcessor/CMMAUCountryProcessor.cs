using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public partial class CMMAUCountryProcessor : CMMBaseCountryProcessor
	{
		public CMMAUCountryProcessor(CMMEmailGenerator emailBuilder, ICMMProcessingAdapter adapter)
			: base(emailBuilder, adapter) { }

		public override void UpdateContainer(CMMMessageContainer containerData, AgencyShipmentContainer container)
		{
			if (!string.IsNullOrEmpty(containerData.GoodsDeclarationNumber))
			{
				AgencyShipment shipment = container.Booking;

				if (!IsEntryNumberUndefined(shipment))
				{
					if (!IsEntryNumberEqualTo(shipment, containerData))
					{
						WriteConflict(containerData, shipment);
					}
				}
				else if (!IsEntryNumberUndefined(container))
				{
					if (!IsEntryNumberEqualTo(container, containerData))
					{
						WriteConflict(containerData, container);
					}
				}
				else
				{
					SetAUCANNumber(container, containerData.GoodsDeclarationNumber);
				}
			}
		}

		public override void ValidateContainer(CMMMessageContainer cmmMessageContainer, AgencyShipmentContainer container)
		{
			if (!string.IsNullOrEmpty(cmmMessageContainer.GoodsDeclarationNumber))
			{
				AgencyShipment booking = container.Booking;

				if (!IsEntryNumberUndefined(booking))
				{
					if (!IsEntryNumberEqualTo(booking, cmmMessageContainer))
					{
						WriteConflict(cmmMessageContainer, booking);
					}
				}
				else if (!IsEntryNumberUndefined(container))
				{
					if (!IsEntryNumberEqualTo(container, cmmMessageContainer))
					{
						WriteConflict(cmmMessageContainer, container);
					}
				}
				else
				{
					EmailBuilder.WriteInfo(
						Res.GetString("1bd39627-5966-43de-9bab-852ed21e7709", "This message indicates that this container has the entry number of CAN {0} but the container could not be updated to reflect this.",
						cmmMessageContainer.GoodsDeclarationNumber));
				}
			}
		}

		void WriteConflict(CMMMessageContainer containerData, AgencyShipment booking)
		{
			CusEntryNumber num = GetAUEntryNumber(booking);

			EmailBuilder.WriteWarning(
				Res.GetString("a6d0e0c4-a5c0-4d2f-9bdb-bfb69cacc73a", "This message indicates that this container has the entry number of CAN {0} which conflicts with the shipment wide entry number of {1} {2}",
				containerData.GoodsDeclarationNumber,
				num == null ? ZString.Empty : CMRExportExemptionCodes.Get4CharCode(num.CE_EntryType),
				num == null ? ZString.Empty : num.CE_EntryNum));
		}
		void WriteConflict(CMMMessageContainer containerData, AgencyShipmentContainer container)
		{
			CusEntryNumber num = GetAUEntryNumber(container);

			EmailBuilder.WriteWarning(
				Res.GetString("dffac5f8-9282-479a-a390-115b562095da", "This message indicates that this container has the entry number of CAN {0} which conflicts with the existing container entry number of {1} {2}",
				containerData.GoodsDeclarationNumber,
				num == null ? ZString.Empty : CMRExportExemptionCodes.Get4CharCode(num.CE_EntryType),
				num == null ? ZString.Empty : num.CE_EntryNum));
		}

		bool IsEntryNumberUndefined(AgencyShipment shipment)
		{
			return IsEntryNumberUndefined(GetAUEntryNumber(shipment));
		}

		bool IsEntryNumberUndefined(AgencyShipmentContainer container)
		{
			return IsEntryNumberUndefined(GetAUEntryNumber(container));
		}

		bool IsEntryNumberUndefined(CusEntryNumber number)
		{
			return number == null || (number.CE_EntryNum.IsEmpty && number.CE_EntryType.IsEmpty);
		}

		bool IsEntryNumberEqualTo(AgencyShipment shipment, CMMMessageContainer data)
		{
			return IsEntryNumberEqualTo(GetAUEntryNumber(shipment), data);
		}

		bool IsEntryNumberEqualTo(AgencyShipmentContainer container, CMMMessageContainer data)
		{
			return IsEntryNumberEqualTo(GetAUEntryNumber(container), data);
		}

		bool IsEntryNumberEqualTo(CusEntryNumber number, CMMMessageContainer data)
		{
			if (string.IsNullOrEmpty(data.GoodsDeclarationNumber))
			{
				return IsEntryNumberUndefined(number);
			}
			else
			{
				return number.CE_EntryType == CusEntryNumberTypes.Australia.CAN
					&& number.CE_EntryNum == data.GoodsDeclarationNumber;
			}
		}

		void SetAUCANNumber(BusinessObject parent, string number)
		{
			CusEntryNumber num = GetAUEntryNumber(parent);

			if (num == null)
			{
				num = parent.Factory.New<CusEntryNumber>();
				num.CE_ParentID = parent.PK;
				num.CE_ParentTable = parent.TableName;
				num.CE_RN_NKCountryCode = Constants.CountryCodes.Australia;
				num.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
				num.CE_EntryNum = number;
				num.CE_EntryIsSystemGenerated = false;
			}
		}

		CusEntryNumber GetAUEntryNumber(BusinessObject parent)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(CusEntryNumSchema.CE_ParentID, parent.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_ParentTable, parent.TableName);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
			return parent.Factory.LoadTop1<CusEntryNumber>(filter);
		}
	}
}


