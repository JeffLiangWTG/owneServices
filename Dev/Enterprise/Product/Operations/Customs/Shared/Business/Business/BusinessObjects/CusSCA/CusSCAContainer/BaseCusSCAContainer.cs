using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class BaseCusSCAContainer : AutoCusSCAContainer, Integration.Customs.Shared.IBaseCusSCAContainer, ISynchroniserReadOnlyMembersProvider
	{
		protected BaseCusSCAContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static readonly TypeDecider TypeDecider = new BaseCusSCAContainerTypeDecider();

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property)
				|| SynchroniserReadOnlyMembers.Contains(property.Name)
				|| ShouldPropertyBeReadOnly(property);
		}

		protected virtual bool ShouldPropertyBeReadOnly(PropertyDescriptor property) => false;

		#endregion
	}

	public class BaseCusSCAContainerTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			ZGuid cusOceanBillPK = (row != null) ? new ZGuid(row[BaseCusSCAContainer.Schema.CN_CB]) : ZGuid.Empty;
			BaseCusSCAOceanBill cusSCAOceanBill = factory.Load<BaseCusSCAOceanBill>(cusOceanBillPK);
			ZString applicationCode = (cusSCAOceanBill != null) ? cusSCAOceanBill.CB_ApplicationCode : ZString.Empty;
			switch (applicationCode)
			{
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail:
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad:
					return ObjectFactory.GetType<Integration.Customs.CA.ICusSCAContainer>();
				case Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff:
					return ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAContainer>();
				case Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages:
					return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAContainer>();
#if DEBUG
				case Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting:
					return typeof(TestCusSCAContainer);
#endif
				default:
					return typeof(DefaultCusSCAContainer);
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

#if DEBUG
	internal class TestCusSCAContainer : BaseCusSCAContainer
	{
		public TestCusSCAContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("OceanBill")]
		public override ZGuid CN_CB
		{
			get => base.CN_CB;
			set => base.CN_CB = value;
		}

		public TestCusSCAOceanBill OceanBill => Factory.Load<TestCusSCAOceanBill>(CN_CB);
	}
#endif
}
