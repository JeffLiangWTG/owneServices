using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBulkRelationshipChanger : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgSupplierBulkRelationshipChanger(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region From Organisation

		public OrgHeader FromOrganisation
		{
			get { return Factory.Load<OrgHeader>(FromOrganisationPK); }
		}
		[List("OrganisationList")]
		public ZGuid FromOrganisationPK
		{
			get { return fromOrganisationPK; }
			set
			{
				if (fromOrganisationPK != value)
				{
					fromOrganisationPK = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateFromOrganisationPK();
					RunPreSaveValidationCore();
				}

				FromOrganisationPKInfo.RefreshBinding();
			}
		}
		ZGuid fromOrganisationPK = ZGuid.Empty;

		public ZPropertyInfo FromOrganisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(FromOrganisationPK)); }
		}

		void ValidateFromOrganisationPK()
		{
			FromOrganisationPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(FromOrganisationPKInfo);

			if (FromOrganisation != null)
			{
				if (!FromOrganisation.OH_IsActive)
				{
					FromOrganisationPKInfo.AddError(Res.GetString("6c7fae4e-8a55-4f22-95d0-d3c7ea5a1685", "This Organization is marked as inactive, please enter a valid Organization."));
				}
				if (FromRelationship.IsEmpty)
				{
					FromRelationshipInfo.ClearAllNotifications();
					FromRelationshipInfo.AddError(Res.GetString("05657a5b-d342-4574-a0ae-c8b5506b8804", "Please enter valid relationship for this organization."));
				}
			}
		}

		#endregion

		#region From Relationship
		[List("RelationshipTypeList")]
		[MaxLength(3)]
		public ZString FromRelationship
		{
			get { return fromRelationship; }
			set
			{
				if (fromRelationship != value)
				{
					CheckMaximumLength(FromRelationshipInfo, value);

					fromRelationship = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateFromRelationship();
					RunPreSaveValidationCore();
				}

				FromRelationshipInfo.RefreshBinding();
			}
		}
		ZString fromRelationship = ZString.Empty;

		public ZPropertyInfo FromRelationshipInfo
		{
			get { return GetZPropertyInfo(nameof(FromRelationship)); }
		}

		void ValidateFromRelationship()
		{
			FromRelationshipInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(FromRelationshipInfo);

			if (!FromRelationship.IsEmpty && FromOrganisation == null)
			{
				FromOrganisationPKInfo.AddError(Res.GetString("145bc8d0-97b1-4515-b336-0ec82a8c27ce", "Please enter valid organization for this relationship."));
			}
		}

		#endregion

		#region To Organisation

		public OrgHeader ToOrganisation
		{
			get { return Factory.Load<OrgHeader>(ToOrganisationPK); }
		}

		[List("OrganisationList")]
		public ZGuid ToOrganisationPK
		{
			get { return toOrganisationPK; }
			set
			{
				if (toOrganisationPK != value)
				{
					toOrganisationPK = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateToOrganisationPK();
					RunPreSaveValidationCore();
				}

				ToOrganisationPKInfo.RefreshBinding();
			}
		}
		ZGuid toOrganisationPK = ZGuid.Empty;

		public ZPropertyInfo ToOrganisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(ToOrganisationPK)); }
		}

		void ValidateToOrganisationPK()
		{
			ToOrganisationPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(ToOrganisationPKInfo);

			if (FromOrganisation == null && ToOrganisation == null)
			{
				ToOrganisationPKInfo.AddError(Res.GetString("585807f2-3d12-4cc2-a718-41e2f48081dc", "Please enter valid Organization."));
			}

			if (ToOrganisation != null)
			{
				if (!ToOrganisation.OH_IsActive)
				{
					ToOrganisationPKInfo.AddError(Res.GetString("6c7fae4e-8a55-4f22-95d0-d3c7ea5a1685", "This Organization is marked as inactive, please enter a valid Organization."));
				}
			}
		}

		#endregion

		#region To Relationship
		[List("RelationshipTypeList")]
		[MaxLength(3)]
		public ZString ToRelationship
		{
			get { return toRelationship; }
			set
			{
				if (toRelationship != value)
				{
					CheckMaximumLength(ToRelationshipInfo, value);

					toRelationship = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateToRelationship();
					RunPreSaveValidationCore();
				}

				ToRelationshipInfo.RefreshBinding();
			}
		}
		ZString toRelationship = ZString.Empty;

		public ZPropertyInfo ToRelationshipInfo
		{
			get { return GetZPropertyInfo(nameof(ToRelationship)); }
		}

		void ValidateToRelationship()
		{
			ToRelationshipInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ToRelationshipInfo);

			if (ToRelationship.IsEmpty)
			{
				ToRelationshipInfo.AddError(Res.GetString("1a03a35e-08ad-4409-b35e-554e20d2eef9", "Please enter valid relationship."));
			}
		}

		#endregion

		[ReadOnly(true)]
		public ZInt EstimatedProductCount
		{
			get { return estimatedProductCount; }
			set { SetNonPersistentPropertyValue(EstimatedProductCountInfo, ref estimatedProductCount, value); }
		}
		ZInt estimatedProductCount;

		public ZPropertyInfo EstimatedProductCountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(EstimatedProductCount));
			}
		}

		public ZInt RelationshipsUpdated { get; private set; }
		public ZInt RelationshipsThatFailedValidaton { get; private set; }
		public ZInt RelationshipsAdded { get; private set; }
		public ZInt ProductsSkipped { get; private set; }
		public ZInt RelationshipsDeleted { get; private set; }
		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFromOrganisationPK();
			ValidateFromRelationship();
			ValidateToOrganisationPK();
			ValidateToRelationship();

			if (FromOrganisation == ToOrganisation)
			{
				FromOrganisationPKInfo.AddError(Res.GetString("a6f934b1-5406-4097-bcf7-9007a32b26a5", "The From and To Organizations are the same, leave the To Organization blank or enter a different Organization."));
				ToOrganisationPKInfo.AddError(Res.GetString("a6f934b1-5406-4097-bcf7-9007a32b26a5", "The From and To Organizations are the same, leave the To Organization blank or enter a different Organization."));
			}
		}

		#endregion

		#region Lookups

		#region Organisation List

		public OrgHeaderCollection OrganisationList
		{
			get { return organisationList ?? (organisationList = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection organisationList;

		#endregion

		#region RelationshipType List

		public CodeDescriptionPairList RelationshipTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(OrgPartRelation.RelationshipTypes.Both, Res.GetString("410b8031-5414-471b-8f5f-d65ed4f2da7e", "Both Owner(Importer) and Supplier(Exporter)"));
				result.AddPair(OrgPartRelation.RelationshipTypes.Owner, Res.GetString("3baf9c70-8e42-4ef1-a623-48c8799962b8", "Owner"));
				result.AddPair(OrgPartRelation.RelationshipTypes.Supplier, Res.GetString("f7eb957e-a513-45dd-8e0c-a24832048947", "Supplier"));
				result.AddPair(OrgPartRelation.RelationshipTypes.WarehouseConsignee, Res.GetString("3a292f81-d75d-4748-baab-341ef12cc388", "Warehouse Consignee"));
				return result;
			}
		}

		#endregion

		#endregion

		#region ChangeRelatedOrganisations

		public void ChangeRelatedOrganisations(ZQuery productFilter)
		{
			if (FromOrganisation != null && ToOrganisation == null && !String.IsNullOrEmpty(FromRelationship) && !String.IsNullOrEmpty(ToRelationship))
			{
				var originalAddOptionRecompile = productFilter.AddOptionRecompileConditionally;
				productFilter.AddOptionRecompileConditionally = false;
				var moduleWhereClause = productFilter.GetAsWhereClause(false);
				productFilter.AddOptionRecompileConditionally = originalAddOptionRecompile;

				var fastSql = string.Format(CultureInfo.InvariantCulture,
								@"
								-- Store details of what OUs we might change

								declare @relationshipsToUpdate table(OU uniqueIdentifier, OP uniqueIdentifier, OH uniqueIdentifier, hasErrors bit not null default 0);
								declare @relationshipsToDelete table(OU uniqueIdentifier, OP uniqueIdentifier, OH uniqueIdentifier);
								declare @relationshipsFailed  int	set @relationshipsFailed = 0;
								declare @relationshipsSkipped int	set @relationshipsSkipped = 0;
								declare @relationshipsUpdated int	set @relationshipsUpdated = 0;
								declare @relationshipsDeleted int	set @relationshipsDeleted = 0;
								declare @productsSkipped	  int	set @productsSkipped = 0;

								IF (@ToRelationship IN ('OWN', 'BTH'))
								BEGIN
									insert into @relationshipsToDelete (OU, OP, OH)
									select OU_PK, OU_OP, OU_OH FROM dbo.OrgPartRelation outRorg
									where exists
									(
										select null FROM dbo.OrgPartRelation inRorg
										where OU_OH = @FromOrganisationPK 
										and inRorg.OU_OP = outRorg.OU_OP
										and inRorg.OU_OH = outRorg.OU_OH
										and OU_Relationship in ('BTH', 'OWN') and OU_OP in 
										(
											select OP_PK from dbo.OrgSupplierPart {0}
										)
										having count(1) > 1
									)
									and 
									OU_OH = @FromOrganisationPK and OU_Relationship in ('BTH', 'OWN') and OU_Relationship <> @ToRelationship and OU_OP in 
									(
										select OP_PK from dbo.OrgSupplierPart {0}
									);
								END

								-- Populate with those whose properties match
								insert into @relationshipsToUpdate (OU, OP, OH, hasErrors)							
								select OU_PK, OU_OP , OU_OH, 0 FROM dbo.OrgPartRelation
								where 
								OU_OH = @FromOrganisationPK and OU_Relationship = @FromRelationship 
								and OU_OP in 
								(
									select OP_PK from dbo.OrgSupplierPart  
									{0}
								);

								set @relationshipsDeleted = (select count(OU) from @relationshipsToDelete);
								delete from @relationshipsToUpdate where OU in (select OU from @relationshipsToDelete);
								delete from dbo.OrgPartRelation where OU_PK in (select OU from @relationshipsToDelete);

								IF @FromRelationship = @ToRelationship	
									BEGIN 
										-- No need to do anything the relationship type is not being changed
										set @relationshipsSkipped = (select count(OU) from @relationshipsToUpdate);
										set @productsSkipped = (select count(distinct OP) from @relationshipsToUpdate);
									END
								ELSE
									BEGIN
										-- Check to see if we should exclude any of relationships from the update because they are owner relationships with transactions that are being changed to a non owner type
										update @relationshipsToUpdate set hasErrors=1
										from @relationshipsToUpdate 
										join dbo.WhsDocket ON WD_OH_Client = OH
									where WD_PK IN 
									(
										SELECT WI_WD
											FROM dbo.WhsInventoryView
											WHERE WI_OP = OP AND (@FromRelationship IN ('OWN', 'BTH') AND @ToRelationship NOT IN ('OWN', 'BTH'))
										);

										-- Check to see if we should exclude any of relationships from the update because they will cause duplicate products
										IF (@ToRelationship IN ('OWN', 'BTH'))
										BEGIN
											update @relationshipsToUpdate set hasErrors=1											
											from @relationshipsToUpdate
											inner join dbo.OrgSupplierPart MP on MP.OP_PK=OP and MP.OP_IsActive=1
											inner join dbo.OrgSupplierPart SP on SP.OP_PartNum=MP.OP_PartNum and SP.OP_IsActive=1
											inner join dbo.OrgPartRelation SR on SR.OU_OP=SP.OP_PK and SR.OU_OH=OH and SR.OU_PK<>OU and SR.OU_Relationship in ('OWN', 'BTH', @FromRelationship);
										END

										-- Check to see if we should exclude any of relationships from the update because they will cause duplicate records
										update @relationshipsToUpdate set hasErrors=1											
										from @relationshipsToUpdate
										inner join dbo.OrgPartRelation SR on SR.OU_OP=OP and SR.OU_OH=OH and SR.OU_PK<>OU and SR.OU_Relationship= @ToRelationship;

										-- See how many OUs won't get changed because of current inventory or duplicate:
										set @relationshipsFailed = (select count(OU) from @relationshipsToUpdate where hasErrors = 1);

										-- Exclude them:
										delete from @relationshipsToUpdate where hasErrors = 1;

										-- Make the necessary changes
										UPDATE dbo.OrgPartRelation SET OU_Relationship= @ToRelationship, OU_SystemLastEditTimeUtc = SYSUTCDATETIME(), OU_SystemLastEditUser = @SystemLastEditUser	
										where OU_PK in (select OU from @relationshipsToUpdate);

										set @relationshipsUpdated = @@RowCount;
									END

								-- Finally return some counts about the work we've done
								select
									@relationshipsUpdated as updated,
									@relationshipsSkipped as relationshipsSkipped,
									@productsSkipped as productsSkipped,
									@relationshipsFailed as relationshipsFailed,
									@relationshipsDeleted as relationshipsDeleted
							", moduleWhereClause);
				using (var tx = Db.Connection.BeginTransactionWithManager()) // Code below is using direct SQL for performance reasons
				{
					using (var cmd = Db.Connection.Command(fastSql))  // "Please use the BusinessObjectFactory rather than hitting the DB directly.".... um, no thank you, I want SPEED not bureaucracy
					{
						cmd.AddParameters(productFilter.Params);
						cmd.AddParameter("@FromOrganisationPK", SqlDbType.UniqueIdentifier, FromOrganisationPK.ToGuid());
						cmd.AddParameter("@FromRelationship", SqlDbType.VarChar, FromRelationship.ToString());
						cmd.AddParameter("@ToRelationship", SqlDbType.VarChar, ToRelationship.ToString());
						cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());

						using (var reader = cmd.ExecuteReader())
						{
							reader.Read();
							RelationshipsUpdated = reader.GetInt32(0);
							ProductsSkipped = reader.GetInt32(2);
							RelationshipsThatFailedValidaton = reader.GetInt32(3);
							RelationshipsDeleted = reader.GetInt32(4);
						}
					}
					tx.CommitTransaction();
				}
			}
			else
			{
				ChangeRelatedOrganisationsSlow(productFilter);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Do shut up")]
		void ChangeRelatedOrganisationsSlow(ZQuery productFilter)
		{
			RelationshipsUpdated = 0;
			RelationshipsThatFailedValidaton = 0;
			RelationshipsAdded = 0;
			ProductsSkipped = 0;
			var productsProcessed = 0;

			FilteredBusinessObjectReader reader;
			if (FromOrganisation != null && !String.IsNullOrEmpty(FromRelationship))
			{
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				dbOnlyQuery.AddToFilter(productFilter);
				var dbOnlySubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				dbOnlySubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, FromOrganisationPK);
				dbOnlySubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, FromRelationship);
				dbOnlyQuery.AddSubQuery(OrgSupplierPartSchema.PK, dbOnlySubQuery, JoinCondition.And);
				reader = new FilteredBusinessObjectReader(dbOnlyQuery, typeof(OrgSupplierPart)) { BatchSize = 500, SaveBeforeLoadNextEnabled = true };
			}
			else
			{
				reader = new FilteredBusinessObjectReader(productFilter, typeof(OrgSupplierPart)) { BatchSize = 500, SaveBeforeLoadNextEnabled = true };
			}

			reader.Factory.RefreshEnabled = false;
			foreach (OrgSupplierPart orgSupplierPart in reader)
			{
				if (cancelled)
				{
					reader.Factory.Save();
					break;
				}

				if (FromOrganisation != null && ToOrganisation != null)
				{
					var orgRelationFrom = orgSupplierPart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(FromOrganisationPK, FromRelationship);
					var orgRelationTo = orgSupplierPart.RelatedOrganisations.FindFirstByOrganisationPK(ToOrganisationPK);
					if (orgRelationFrom != null &&
						orgRelationTo == null)
					{
						var originalOUOH = orgRelationFrom.OU_OH;
						var originalOURelationship = orgRelationFrom.OU_Relationship;
						if (ToOrganisationPK != originalOUOH || ToRelationship != originalOURelationship)
						{
							orgRelationFrom.OU_OH = ToOrganisationPK;
							orgRelationFrom.OU_Relationship = ToRelationship;
							if (orgSupplierPart.OP_PartNumInfo.HasErrors() || orgRelationFrom.OU_OHInfo.HasErrors())
							{
								RelationshipsThatFailedValidaton++;
								orgRelationFrom.OU_Relationship = originalOURelationship;
								orgRelationFrom.OU_OH = originalOUOH;
							}
							else
							{
								RelationshipsUpdated++;
							}
						}
						else
						{
							ProductsSkipped++;
						}
					}
					else if (orgRelationFrom != null)
					{
						orgSupplierPart.HasChanges = true; // to allow the next line to see that channges are present and to fully validate upon removal.
						orgSupplierPart.RelatedOrganisations.Remove(orgRelationFrom);
						var originalRelationship = orgRelationTo.OU_Relationship;
						orgRelationTo.OU_Relationship = ToRelationship;
						if (orgSupplierPart.OP_PartNumInfo.HasErrors())
						{
							orgRelationTo.OU_Relationship = originalRelationship;
							orgSupplierPart.RelatedOrganisations.Add(orgRelationFrom);  // restore
							RelationshipsThatFailedValidaton++;
						}
						else
						{
							// Simulate User Delete
							orgSupplierPart.RelatedOrganisations.Add(orgRelationFrom);
							if (orgRelationFrom.CanDelete)
							{
								orgSupplierPart.RelatedOrganisations.RemoveAndDelete(orgRelationFrom);
							}

							if (orgRelationFrom.IsDeleted)
							{
								RelationshipsUpdated++;
							}
							else
							{
								orgRelationTo.OU_Relationship = originalRelationship; // restore
								RelationshipsThatFailedValidaton++;
							}
						}
					}
					else
					{
						ProductsSkipped++;
					}
				}
				else if (FromOrganisation != null)
				{
					var orgRelation = orgSupplierPart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(FromOrganisationPK, FromRelationship);
					if (orgRelation != null)
					{
						var originalRelationship = orgRelation.OU_Relationship;
						if (ToRelationship != originalRelationship)
						{
							orgRelation.OU_Relationship = ToRelationship;
							if (orgSupplierPart.OP_PartNumInfo.HasErrors() || orgRelation.OU_RelationshipInfo.HasErrors())
							{
								orgRelation.OU_Relationship = originalRelationship;
								RelationshipsThatFailedValidaton++;
							}
							else
							{
								RelationshipsUpdated++;
							}
						}
						else
						{
							ProductsSkipped++;
						}
					}
					else
					{
						ProductsSkipped++;
					}
				}
				else
				{
					var bthRelation = orgSupplierPart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(ToOrganisation.PK, OrgPartRelation.RelationshipTypes.Both);
					var ownRelation = orgSupplierPart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(ToOrganisation.PK, OrgPartRelation.RelationshipTypes.Owner);
					if (bthRelation != null && ownRelation != null && (ToRelationship == OrgPartRelation.RelationshipTypes.Both || ToRelationship == OrgPartRelation.RelationshipTypes.Owner))
					{
						orgSupplierPart.RelatedOrganisations.RemoveAndDelete(ToRelationship == OrgPartRelation.RelationshipTypes.Both ? ownRelation : bthRelation);
						RelationshipsDeleted++;
					}
					else
					{
						var orgRelation = orgSupplierPart.RelatedOrganisations.FindFirstByOrganisationPK(ToOrganisationPK);
						if (orgRelation != null)
						{
							var originalRelationship = orgRelation.OU_Relationship;
							if (originalRelationship != ToRelationship)
							{
								orgRelation.OU_Relationship = ToRelationship;
								if (orgSupplierPart.OP_PartNumInfo.HasErrors() || orgRelation.OU_RelationshipInfo.HasErrors())
								{
									orgRelation.OU_Relationship = originalRelationship;
									RelationshipsThatFailedValidaton++;
								}
								else
								{
									RelationshipsUpdated++;
								}
							}
							else
							{
								ProductsSkipped++;
							}
						}
						else
						{
							var orgRelationNew = orgSupplierPart.RelatedOrganisations.AddNew();
							orgRelationNew.OU_OH = ToOrganisationPK;
							orgRelationNew.OU_Relationship = ToRelationship;
							if (orgSupplierPart.OP_PartNumInfo.HasErrors())
							{
								orgRelationNew.Delete();
								RelationshipsThatFailedValidaton++;
							}
							else
							{
								RelationshipsAdded++;
							}
						}
					}
				}

				OnProgressChanged(++productsProcessed);
			}
		}

		void OnProgressChanged(int count)
		{
			if (ProgressChanged != null && EstimatedProductCount > 0)
			{
				ProgressChanged((int)Math.Min(100, (decimal)count / EstimatedProductCount * 100));
			}
		}

		public void Cancel()
		{
			cancelled = true;
		}

		bool cancelled;

		public event Action<int> ProgressChanged;

		#endregion
	}
}
