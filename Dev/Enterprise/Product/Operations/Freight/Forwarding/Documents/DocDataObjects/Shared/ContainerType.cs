using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class ContainerType : CodeDescription, IContainerType
	{
		public ContainerType(IFindBoxListProvider containerTypes)
			: base(containerTypes)
		{
			this.containerTypes = containerTypes;
		}

		public ContainerType(IRefContainerCollection containerTypes)
			: this(containerTypes as IFindBoxListProvider)
		{
		}

		readonly IFindBoxListProvider containerTypes;

		#region Setting defaults

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CodeInfo.ValueChanged += (s, e) =>
			{
				if (containerTypes.GetBusinessObjectFromCode(Code) is RefContainer refContainer)
				{
					ISOCode = refContainer.RC_ISOType;
					Type = new CodeDescription(refContainer.Lookups.ContainerTypes)
					{
						Code = refContainer.RC_ContainerType
					};
				}
			};
		}

		#endregion

		#region ISOCode

		public ZString ISOCode
		{
			get => isoCode;
			set
			{
				if (SetNonPersistentPropertyValue(ISOCodeInfo, ref isoCode, value))
				{
					Validate(ISOCodeInfo);
				}
			}
		}

		ZString isoCode;

		public ZPropertyInfo ISOCodeInfo => GetZPropertyInfo(nameof(ISOCode));

		#endregion

		#region Type

		public CodeDescription Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		CodeDescription type;

		#endregion

		ICodeDescription IContainerType.Type
		{
			get => Type;
		}
	}
}
