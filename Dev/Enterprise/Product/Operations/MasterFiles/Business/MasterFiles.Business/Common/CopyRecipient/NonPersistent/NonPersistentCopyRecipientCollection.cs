using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class NonPersistentCopyRecipientCollection : NonPersistentBusinessObjectCollection<NonPersistentCopyRecipient>
	{
		public NonPersistentCopyRecipientCollection(OrgHeader organization, ZString type)
		{
			Organization = organization;
			this.type = type;
			((IBindingList)this).ListChanged += NonPersistentCopyRecipientCollection_ListChanged;
		}

		#region Organization

		public OrgHeader Organization
		{
			get { return organization; }
			set
			{
				organization = value;
				foreach (var businessObject in this)
				{
					var copyRecipient = (NonPersistentCopyRecipient)businessObject;
					copyRecipient.Organization = value;
				}
			}
		}

		OrgHeader organization;

		#endregion

		#region Type

		public ZString Type
		{
			get { return type; }
		}

		readonly ZString type;

		#endregion

		#region Updated

		/// <summary>
		/// Triggered when the collection gets updated.
		/// </summary>
		public event EventHandler Updated;

		void OnUpdated()
		{
			if (Updated != null)
			{
				Updated(this, EventArgs.Empty);
			}
		}

		void NonPersistentCopyRecipientCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			switch (e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
				case ListChangedType.ItemChanged:
				case ListChangedType.ItemDeleted:
				case ListChangedType.ItemMoved:
				case ListChangedType.Reset:
					OnUpdated();
					break;
			}
		}

		#endregion

		#region Value

		/// <summary>
		/// Gets or sets the string representation of the copy recipient collection.
		/// </summary>
		public ZString Value
		{
			get
			{
				var resultBuilder = new ZStringBuilder();
				foreach (var businessObject in this)
				{
					var copyRecipient = (NonPersistentCopyRecipient)businessObject;
					if (resultBuilder.Length > 0)
					{
						resultBuilder.Append(", ");
					}
					resultBuilder.Append(copyRecipient.EmailAddress);
				}
				return resultBuilder.ToString();
			}
			set
			{
				RemoveAndDeleteAll();
				var emailAddresses = value.Trim().ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ea => ea.Trim()).ToArray();
				foreach (var emailAddress in emailAddresses)
				{
					var copyRecipient = AddNew();
					copyRecipient.EmailAddress = emailAddress;
				}
			}
		}

		#endregion

		#region Implementations

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentCopyRecipient(Organization, type);
		}

		#endregion
	}
}