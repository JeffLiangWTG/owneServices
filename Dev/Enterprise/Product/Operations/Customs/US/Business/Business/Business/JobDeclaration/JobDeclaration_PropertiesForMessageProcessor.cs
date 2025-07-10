using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public partial class JobDeclaration : IEntryHeaderParentBusinessObject
	{
		#region Properties

		BusinessObject IEntryHeaderParentBusinessObject.LinkedObject => this;

		CusEntryHeader IEntryHeaderParentBusinessObject.EntrySummaryEntry => ActiveEntryHeaders.EntrySummaryEntry;

		CusEntryHeader IEntryHeaderParentBusinessObject.CargoReleaseEntry => ActiveEntryHeaders.CargoReleaseEntry;

		ISimplifiedMessageLinkedObject IEntryHeaderParentBusinessObject.SimplifiedEntry => ActiveEntryHeaders.SimplifiedEntry;

		ZString IEntryHeaderParentBusinessObject.ReferenceNumber => DeclarationReferenceAppendedByFormattedEntryNumber;

		Guid IEntryHeaderParentBusinessObject.RegistryCompanyPK => RegistryCompanyPK;

		Guid IEntryHeaderParentBusinessObject.RegistryBranchPK => RegistryBranchPK;

		GlbStaff IEntryHeaderParentBusinessObject.CusAgent => CusAgent;

		OrgHeader IEntryHeaderParentBusinessObject.Importer => Importer;

		DispositionDataCollection IEntryHeaderParentBusinessObject.DispositionCodes => DispositionCodes;

		IDisposable IEntryHeaderParentBusinessObject.ReleaseStatusChangingSuspender
		{
			get
			{
				return new CargoReleaseEntryReleaseStatusChangingSuspender(this);
			}
		}

		ZString IEntryHeaderParentBusinessObject.ReleaseStatus { get => ReleaseStatus; set => ReleaseStatus = value; }

		ZDateTime IEntryHeaderParentBusinessObject.ReleaseDateTime { get => JE_EntryAuthorisationDate; set => JE_EntryAuthorisationDate = value; }

		bool IEntryHeaderParentBusinessObject.ShouldUpdateDeclarationWithCargoReleaseResults => ShouldUpdateDeclarationWithCargoReleaseResults;

		#endregion

		#region Functions without Returned Value

		void IEntryHeaderParentBusinessObject.UpdateQuotaStatus(ZString quotaStatus)
		{
			Declaration.US_QuotaStatus = quotaStatus;
		}

		void IEntryHeaderParentBusinessObject.MarkAIIRequested()
		{
			Declaration.US_IsAIIRequested = true;
		}

		void IEntryHeaderParentBusinessObject.UpdateMessageLinkedParentBOAfterReleased(ASESSO10Base blockSO10, IEnumerable<ASESSO40Base> blockSO40List, IEnumerable<ASESSO50Base> blockSO50List)
		{
			var blankPropertyInfos = GetBlankProperties();
			UpdateDeclarationBlockS010(blankPropertyInfos, blockSO10);
			UpdateDeclarationBlockS040(blankPropertyInfos, blockSO40List);
			UpdateDeclarationBlockS050(blankPropertyInfos, blockSO50List);
		}

		void IEntryHeaderParentBusinessObject.AddOrUpdateCusDisposition(Dictionary<ZString, IPGADispositionProvider> pgaEntryStatusMapping, List<IPGADispositionProvider> pgaLineStatusList)
		{
			this.EntryPGACusDispositions.AddOrUpdateCusDisposition(pgaEntryStatusMapping);
			if (pgaLineStatusList.Count > 0)
			{
				this.AddOrUpdateCusDispositionOnInvoiceLine(pgaLineStatusList);
			}
		}

		void IEntryHeaderParentBusinessObject.LogPGAEntryAndLineStatus(Dictionary<ZString, ZString> previousPGAEntryStatus, List<IPGADispositionProvider> dispositionProviders, ZString messageType)
		{
			this.LogPGAEntryStatus(previousPGAEntryStatus, messageType);
			this.LogPGALineStatus(dispositionProviders, messageType);
		}

		#endregion

		#region Functions with Returned Value

		Bill IEntryHeaderParentBusinessObject.GetFirstBillHasSameNumber(ZString billNumber)
		{
			return Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillNum == billNumber);
		}

		ZString IEntryHeaderParentBusinessObject.GetUnableToDeactivateStatementLineRemarkIfNecessary()
		{
			return this.GetUnableToDeactivateStatementLineRemarkIfNecessary();
		}

		Dictionary<ZString, ZString> IEntryHeaderParentBusinessObject.GetPGAEntryStatus()
		{
			return this.GetPGAEntryStatus();
		}

		OGADispositionData IEntryHeaderParentBusinessObject.AddOGADispositionData(IPGADispositionProvider dispositionProvider, ZString source)
		{
			return EntryStatusesAndErrors.AddOGADispositionData(dispositionProvider, source);
		}

		ErrorsRecordCollection IEntryHeaderParentBusinessObject.ENSStatusNotifications => ENSStatusNotifications;

		#endregion

		#region Other Functions

		void UpdateDeclarationBlockS010(IEnumerable<ZPropertyInfo> blankPropertyInfos, ASESSO10Base blockSO10)
		{
			if (blockSO10 != null)
			{
				UpdateValueIfBlank(blankPropertyInfos, this.US_SchDEntryInfo, blockSO10.DistrictPortOfEntry);
				UpdateValueIfBlank(blankPropertyInfos, this.US_UI_NKCarrierSCACInfo, blockSO10.CarrierCode);
				UpdateValueIfBlank(blankPropertyInfos, this.JE_VoyageFlightNoInfo, blockSO10.VoyageFlightTripManifestNumber);
				UpdateValueIfBlank(blankPropertyInfos, this.US_EntryDateInfo, blockSO10.EstimatedDateOfArrival);
			}
		}

		void UpdateDeclarationBlockS040(IEnumerable<ZPropertyInfo> blankPropertyInfos, IEnumerable<ASESSO40Base> blockSO40List)
		{
			var joinedBlockSO40s =
				from bill in this.Bills.Cast<Bill>()
				join block in blockSO40List
				on new { billNum = bill.CU_BillNum, IssuerCode = bill.US_UI_NKBillIssuerSCAC }
				equals new { billNum = block.BillOfLadingNumber, IssuerCode = block.IssuerCodeOfBillOfLadingNumber }
				where !block.Quantity.IsEmpty && !block.UnitOfMeasure.IsEmpty
				select (bill, block);

			if (joinedBlockSO40s.Any() && joinedBlockSO40s.AllSame(joined => joined.block.UnitOfMeasure))
			{
				UpdateValueIfBlank(blankPropertyInfos, this.JE_TotalNoOfPacksInfo, new ZInt(joinedBlockSO40s.Sum(joined => joined.block.Quantity)));
				UpdateValueIfBlank(blankPropertyInfos, this.JE_TotalNoOfPacksPackTypeInfo, joinedBlockSO40s.FirstOrDefault().block.UnitOfMeasure);
			}
			joinedBlockSO40s.ForEach(joined =>
			{
				var bill = joined.bill;
				UpdateBill(blankPropertyInfos, bill, joined.block);
				if (bill.AllPackages.Count == 1)
				{
					var package = bill.AllPackages[0];
					UpdagePackage(blankPropertyInfos, package, joined.block);
				}
			});
		}

		void UpdateDeclarationBlockS050(IEnumerable<ZPropertyInfo> blankPropertyInfos, IEnumerable<ASESSO50Base> blockSO50List)
		{
			if (blockSO50List.Any())
			{
				if (blockSO50List.AllSame(x => x.DateOfArrival))
				{
					UpdateValueIfBlank(blankPropertyInfos, this.JE_DateOfArrivalInfo, blockSO50List.FirstOrDefault().DateOfArrival.ToZDateTime());
				}
				if (blockSO50List.AllSame(x => x.DistrictPortOfArrival))
				{
					UpdateValueIfBlank(blankPropertyInfos, this.US_SchDArrivalInfo, blockSO50List.FirstOrDefault().DistrictPortOfArrival);
				}
			}
		}

		List<ZPropertyInfo> GetBlankProperties()
		{
			var propertyInfos = new List<ZPropertyInfo>()
			{
				this.US_SchDEntryInfo,
				this.US_UI_NKCarrierSCACInfo,
				this.JE_VoyageFlightNoInfo,
				this.US_EntryDateInfo,
				this.JE_TotalNoOfPacksInfo,
				this.JE_TotalNoOfPacksPackTypeInfo,
				this.JE_DateOfArrivalInfo,
				this.US_SchDArrivalInfo
			};
			foreach (Bill bill in this.Bills)
			{
				propertyInfos.Add(bill.CU_NoOfPacksInfo);
				propertyInfos.Add(bill.CU_PackTypeInfo);
			}

			foreach (Package pack in this.Packages)
			{
				propertyInfos.Add(pack.CW_PackQtyInfo);
				propertyInfos.Add(pack.CW_PackTypeInfo);
			}

			return propertyInfos.Where(x => x.Value.IsDefault || x.Value.IsEmpty).ToList();
		}

		void UpdateValueIfBlank(IEnumerable<ZPropertyInfo> blankPropertyInfoList, ZPropertyInfo propertyInfo, IZType value)
		{
			if (blankPropertyInfoList.Contains(propertyInfo))
			{
				propertyInfo.Value = value;
			}
		}

		void UpdateBill(IEnumerable<ZPropertyInfo> blankPropertyInfoList, Bill bill, ASESSO40Base so40)
		{
			UpdateValueIfBlank(blankPropertyInfoList, bill.CU_NoOfPacksInfo, (ZDecimal)so40.Quantity);
			UpdateValueIfBlank(blankPropertyInfoList, bill.CU_PackTypeInfo, so40.UnitOfMeasure);
		}

		void UpdagePackage(IEnumerable<ZPropertyInfo> blankPropertyInfoList, Package package, ASESSO40Base so40)
		{
			UpdateValueIfBlank(blankPropertyInfoList, package.CW_PackQtyInfo, so40.Quantity);
			UpdateValueIfBlank(blankPropertyInfoList, package.CW_PackTypeInfo, so40.UnitOfMeasure);
		}

		#endregion
	}
}
