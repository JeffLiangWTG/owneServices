using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.MasterFiles.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class AddressOverrideCombinationControl<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> : ZMultiCombinationControl where TCopyRecipient : BusinessObject where TCopyRecipientCollection : CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		public AddressOverrideCombinationControl(AddressOverrideColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> columnInfo)
		{
			ColumnInfo = columnInfo;
			CharacterCasing = CharacterCasing.Normal;
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
		}

		protected AddressOverrideColumnStyleInfo<TCopyRecipient, TCopyRecipientCollection, TCopyRecipientOwner> ColumnInfo { get; }

		protected override IGridControl CreateGridControl(FieldType typeToCreate)
		{
			if (typeToCreate == FieldType.TextCodeFindBox)
			{
				var findBox = new CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner>
				{
					CodeBox = { CharacterCasing = CharacterCasing.Lower }
				};
				return findBox;
			}

			return base.CreateGridControl(typeToCreate);
		}

		protected override void EditCore(CurrencyManager source, string mappingName)
		{
			if (CurrentEditor is CopyRecipientsFindBox<TCopyRecipient, TCopyRecipientOwner> copyRecipientFindBox)
			{
				var parentBusinessObject = source?.GetCurrent();
				if (parentBusinessObject != null)
				{
					copyRecipientFindBox.CopyRecipients = ColumnInfo.GetCopyRecipients((TCopyRecipientOwner)parentBusinessObject);
					copyRecipientFindBox.EmailAddressPropertyName = ColumnInfo.EmailAddressPropertyName;
				}
			}
		}
	}
}
