using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class BaseCusSCAPivot : AutoCusSCAPivot,
		Integration.Customs.Shared.IBaseCusSCAPivot,
		IUNDGDataItemProvider,
		ISynchroniserReadOnlyMembersProvider
	{
		protected BaseCusSCAPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static readonly TypeDecider TypeDecider = new BaseCusSCAPivotTypeDecider();

		#region Related Business Objects

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public override ZGuid CV_CN
		{
			get => base.CV_CN;
			set
			{
				var oldValue = CV_CN;
				base.CV_CN = value;
				if (oldValue != CV_CN)
				{
					container?.InvalidateCache();
				}
			}
		}

		public BaseCusSCAContainer Container => (container ?? (container = new RecalculableCachedValue<BaseCusSCAContainer>(() => Factory.Load<BaseCusSCAContainer>(CV_CN)))).Value;
		RecalculableCachedValue<BaseCusSCAContainer> container;

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion
	}

	#region Type Decider

	public class BaseCusSCAPivotTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ZGuid cusSCAHousePK = (row != null) ? new ZGuid(row[BaseCusSCAPivot.Schema.CV_CA]) : ZGuid.Empty;
			BaseCusSCAHouse cusSCAHouse = factory.Load<BaseCusSCAHouse>(cusSCAHousePK);
			BaseCusSCAOceanBill cusSCAOceanBill = factory.Load<BaseCusSCAOceanBill>(cusSCAHouse != null ? cusSCAHouse.CA_CB : ZGuid.Empty);
			ZString applicationCode = (cusSCAOceanBill != null) ? cusSCAOceanBill.CB_ApplicationCode : ZString.Empty;
			switch (applicationCode)
			{
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad:
					return ObjectFactory.GetType<Integration.Customs.CA.ICusSCAPivot>();
				case Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAPackingLine>();
				case Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAPivot>();
#if DEBUG
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting:
					return typeof(TestCusSCAPivot);
#endif
				default:
					return typeof(DefaultCusSCAPivot);
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}

	#endregion

#if DEBUG
	internal class TestCusSCAPivot : BaseCusSCAPivot
	{
		public TestCusSCAPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
#endif
}
