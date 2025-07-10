//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmNumberRangeMatchingDetailLookups
//
//    This class should be used for overriding collections in AutoStmNumberRangeMatchingDetailLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmNumberRangeMatchingDetailLookups : AutoStmNumberRangeMatchingDetailLookups
	{
		public StmNumberRangeMatchingDetailLookups(AutoStmNumberRangeMatchingDetail parent) : base(parent)
		{
		}

		#region RangeTypes

		public virtual CodeDescriptionPairList RangeTypes
		{
			get
			{
				return Factory.GetCachedValue("StmNumberRangeMatchingDetailsLookups|RangeTypes" + Parent.NRM_OwnerTableCode, () =>
				{
					var list = new CodeDescriptionPairList();

					if (Parent.NRM_OwnerTableCode == OrgHeaderSchema.Constants.Prefix)
					{
						list.AddPair(OrgConstants.NumberFountains.Code.TransportReferenceNumbers, OrgConstants.NumberFountains.Description.TransportReferenceNumbers);
					}
					else if (Parent.NRM_OwnerTableCode == GlbStaffSchema.Constants.Prefix)
					{
						list.AddPair(OrgConstants.NumberFountains.Code.PatentNumber, OrgConstants.NumberFountains.Description.PatentNumber);
					}
					//list.Sort(); when add more items please uncomment this
					return list;
				});
			}
		}

		#endregion

		#region Clients

		public override OrgHeaderCollection Clients
		{
			get { return Factory.GetCachedValue("StmNumberRangeMatchingDetailsLookups|Clients", () => new WarehouseClientCollectionWithSecurityCheck(Factory)); }
		}

		#endregion

		#region Warehouses

		public IWhsWarehouseCollection Warehouses
		{
			get { return Factory.GetCachedValue("StmNumberRangeMatchingDetailsLookups|Warehouses", () => (IWhsWarehouseCollection)Activator.CreateInstance(ObjectFactory.GetType<IWhsWarehouseCollection>(), Factory)); }
		}

		#endregion

		#region CustomsAreaList

		public IBusinessObjectCollection CustomsAreaList =>
			ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider>().GetCollection(Factory, Core.Constants.CountryCodes.Mexico, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);

		#endregion

		#region Prefixes

		public CodeDescriptionPairList Prefixes
		{
			get
			{
				CodeDescriptionPairList prefixes = null;
				var owner = Parent.Owner;
				if (owner != null)
				{
					var stmNumsPrefixes = Factory.GetCachedValue(string.Format(Culture.Invariant, "StmNumberRangeMatchingDetailsLookups|StmNumsPrefixes|{0}", owner.PK), () =>
					{
						var typePrefixes = new Dictionary<string, CodeDescriptionPairList>();
						foreach (var stmNums in owner.Fountains)
						{
							CodeDescriptionPairList prefixesForType;
							if (!typePrefixes.TryGetValue(stmNums.SN_Type, out prefixesForType))
							{
								prefixesForType = new CodeDescriptionPairList();
								typePrefixes.Add(stmNums.SN_Type, prefixesForType);
							}

							prefixesForType.InsertInSortOrder(new CodeDescriptionPair(stmNums.SN_Prefix.ToString(), null));
						}
						return typePrefixes;
					}, CacheStalenessPolicy.StaleOnFactorySave); // Currently this cache on SaveStmNumsStandAloneFactory in ConfigUserControl manually will be clear, is saved in different factory 

					stmNumsPrefixes.TryGetValue(Parent.NRM_RangeType, out prefixes);
				}
				return prefixes ?? new CodeDescriptionPairList();
			}
		}

		#endregion

		#region Implementation

		new StmNumberRangeMatchingDetail Parent
		{
			get { return (StmNumberRangeMatchingDetail)base.Parent; }
		}

		#endregion

	}
}
