using System.Reflection;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovementDefaultFilterProvider))]
	internal class ContainerMovementDefaultFilterProviderTest : DefaultFilterProviderTest<ContainerMovementDefaultFilterProvider>
	{
		public const string MovementType = "Movement Type";
		public const string DetentionInvoiced = "Detention Invoiced";
		public const string Detentionable = "Detentionable";
		public const string TriggeringDetentions = "Triggering Detentions";
		public const string Principal = "Principal";
		public const string Client = "Client";
		public void TestMovement()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, MovementType);
			Provider.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, MovementType, "Property", (ZString)ContainerMovementTypes.Codes.WharfGateIn);
		}

		public void TestDetentionInvoiced()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, DetentionInvoiced);
			Provider.DetentionInvoiced = true;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DetentionInvoiced, "Property", (ZString)"INV");
			Provider.DetentionInvoiced = false;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, DetentionInvoiced, "Property", (ZString)"NIV");
		}

		public void TestDetentionable()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Detentionable);
			Provider.Detentionable = true;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Detentionable, "Property", (ZString)"DET");
			Provider.Detentionable = false;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Detentionable, "Property", (ZString)"NDT");
		}

		public void TestPrinicpal()
		{
			ZGuid principalPK = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Principal);
			Provider.Principal = principalPK;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Principal, "Property", principalPK);
		}

		public void TestClient()
		{
			ZGuid principalPK = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Principal);
			Provider.Principal = principalPK;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Principal, "Property", principalPK);
		}

		public void TestTriggeringDetentionType()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Principal);
			Provider.TriggeringDetentionType = DetentionInvoiceType.Codes.Export;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, TriggeringDetentions, "Property", (ZString)DetentionInvoiceType.Codes.Export);
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyContainerMove;
			}
		}

		protected override void Populate(ContainerMovementDefaultFilterProvider provider, PropertyInfo info)
		{
			switch (info.Name)
			{
				case "DetentionInvoiced":
					provider.DetentionInvoiced = true;
					break;
				case "Detentionable":
					provider.Detentionable = true;
					break;
				default:
					base.Populate(provider, info);
					break;
			}
		}
		#endregion
	}
}
