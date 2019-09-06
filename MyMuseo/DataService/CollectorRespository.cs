using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;
using System;

namespace MyMuseo.DataService
{
	public class CollectorRespository
	{
		private readonly IDbConnection _db;

		public CollectorRespository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

		public List<Collector> GetCollectors(int amount, string sort)
		{
			return this._db.Query<Collector>("SELECT * FROM [Collectors] JOIN [AspNetUsers] ON Collectors.UserId = AspNetUsers.Id WHERE Collectors.IsAdmin = 0 AND AspNetUsers.EmailConfirmed = 'True' ORDER BY Collectors.CreatedDate " + sort).ToList();
		}

        public List<Collector> GetActiveCollectors(int amount, string sort)
        {
            return this._db.Query<Collector>("SELECT * FROM [Collectors] JOIN [AspNetUsers] ON Collectors.UserId = AspNetUsers.Id WHERE Collectors.IsAdmin = 0 AND AspNetUsers.EmailConfirmed = 'True' AND  CollectorId IN (SELECT DISTINCT CollectorId FROM Collectibles) ORDER BY Collectors.CreatedDate " + sort).ToList();
        }

        public List<Collector> GetProfileCollectors(int amount, string sort)
        {
            return this._db.Query<Collector>("SELECT * FROM [Collectors] JOIN [AspNetUsers] ON Collectors.UserId = AspNetUsers.Id WHERE Collectors.IsAdmin = 0 AND AspNetUsers.EmailConfirmed = 'True' AND  DisplayOrder = 0  ORDER BY Collectors.CreatedDate " + sort).ToList();
        }

        public Collector GetCollector(int collectorId)
		{
			return _db.Query<Collector>("SELECT * FROM [Collectors] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
		}

        public Collector GetCollector(string userId)
        {
            return _db.Query<Collector>("SELECT * FROM [Collectors] WHERE UserId =@UserId", new { UserId = userId }).SingleOrDefault();
        }

        public Collector GetCollectorByEmail(string email)
        {
            return _db.Query<Collector>("SELECT * FROM [Collectors] WHERE UserId IN (SELECT Id FROM [AspNetUsers] WHERE Email = @Email)", new { Email = email }).SingleOrDefault();
        }

        public string GetUserEmail(string userId)
        {
            return _db.Query<string>("SELECT Email FROM [AspNetUsers] WHERE Id =@Id", new { Id = userId }).SingleOrDefault();
        }

        public string GetCountryName(int countryId)
        {
            return _db.Query<string>("SELECT Name FROM [Country] WHERE CountryId =@CountryId", new { CountryId = countryId }).SingleOrDefault();
        }

        public bool InsertCollector(Collector ourCollector)
		{
			int rowsAffected = this._db.Execute(@"INSERT Collectors ([FirstName],[LastName],[AboutMe],
                        [PageImage],[ProfileImage],[DisplayOrder],[Active],[CreatedDate],[UserId],[AreasOfInterest],[WebSites], [TypeId] ) 
                        values (@FirstName, @LastName, @AboutMe, 
                                @PageImage, @ProfileImage, @DisplayOrder, @Active, @CreatedDate, @UserId, @AreasOfInterest, @WebSites, @TypeId )", ourCollector
				);

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool DeleteCollector(int collectorId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Collectors] WHERE CollectorId = @CollectorId",
				new { CollectorId = collectorId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}


        public bool CleanUpCollector()
        {
            int rowsAffected = this._db.Execute(@"
                delete follow where CollectorId not in (select CollectorId from Collectors);
                delete favorite where CollectorId not in (select CollectorId from Collectors);
                delete favorite where FavoriteCollectorId not in (select CollectorId from Collectors);
                delete follow where FollowCollectorId not in (select CollectorId from Collectors);
                delete collectibles where CollectorId not in (select CollectorId from Collectors);
                delete collections where CollectorId not in (select CollectorId from Collectors);
                delete groupmembers where CollectorId not in (select CollectorId from Collectors);
                delete comments where PostByCollectorId not in (select CollectorId from Collectors);
                delete discussions where PostByCollectorId not in (select CollectorId from Collectors);
                ",
                null);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateCollector(Collector ourCollector)
		{
			int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [FirstName] = @FirstName,
                            [LastName] = @LastName, 
                            [AboutMe] = @AboutMe,
                            [PageImage] = @PageImage,
                            [ProfileImage] = @ProfileImage, 
                            [DisplayOrder] = @DisplayOrder,
                            [AreasOfInterest] = @AreasOfInterest,
                            [WebSites] = @WebSites
                        WHERE CollectorId = " +
						ourCollector.CollectorId, ourCollector);
            
			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

        public bool UpdateCollectorAccountInfo(Collector ourCollector)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [FirstName] = @FirstName,
                            [LastName] = @LastName,
                            [TypeId] = @TypeId   
                        WHERE CollectorId = " +
                        ourCollector.CollectorId, ourCollector);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateCollectorProfileImage(int collectorId, string profileImage)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [ProfileImage] = @ProfileImage
                        WHERE CollectorId = " +
                        collectorId, new { ProfileImage = profileImage });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateCollectorFeaturedItemId(int collectorId, int featuredItemId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [FeaturedItemId] = @FeaturedItemId
                        WHERE CollectorId = " +
                        collectorId, new { FeaturedItemId = featuredItemId });

            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateCollectorFeaturedImage(int collectorId, string featuredImage)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [PageImage] = @FeaturedImage
                        WHERE CollectorId = " +
                        collectorId, new { FeaturedImage = featuredImage });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool SiteAdmin(int collectorId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [IsAdmin] = 1
                        WHERE CollectorId = " +
                        collectorId);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool HideProfile(int collectorId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [DisplayOrder] = -1
                        WHERE CollectorId = " +
                        collectorId);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool ShowProfile(int collectorId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectors] SET 
                            [DisplayOrder] = 0
                        WHERE CollectorId = " +
                        collectorId);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int GetCollectionsCount(int collectorId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [Collections] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
        }

        public int GetActiveCollectionsCount(int collectorId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [Collections] WHERE CollectorId =@CollectorId AND IsDraft = 'false'", new { CollectorId = collectorId }).SingleOrDefault();
        }

        public int GetCollectiblesCount(int collectorId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [Collectibles] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
        }
        public int GetFollowersCount(int collectorId)
        {
            return _db.Query<int>("SELECT COUNT(DISTINCT CollectorId) FROM [Follow] WHERE FollowCollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
        }

        public List<int>GetFollowerIds (int collectorId)
        {
            return _db.Query<int>("SELECT DISTINCT [CollectorId] FROM [Follow] WHERE FollowCollectorId = @CollectorId", new { CollectorId = collectorId }).ToList();
        }

        public bool DeleteFollower(int collectorId, int followerId)
        {
            int rowsAffected = this._db.Execute(@"DELETE Follow 
                                WHERE CollectorId = @CollectorId AND FollowCollectorId = @FollowCollectorId", new {CollectorId = followerId, FollowCollectorId = collectorId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int GetFollowingCount(int collectorId)
        {
            return _db.Query<int>("SELECT COUNT(DISTINCT FollowCollectorId) FROM [Follow] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
        }

        public bool DeleteFollowing(int collectorId, int followingId)
        {
            int rowsAffected = this._db.Execute(@"DELETE Follow 
                                WHERE CollectorId = @CollectorId AND FollowCollectorId = @FollowCollectorId", new {CollectorId = collectorId, FollowCollectorId = followingId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public List<int> GetFollowingIds(int collectorId)
        {
            return _db.Query<int>("SELECT DISTINCT [FollowCollectorId] FROM [Follow] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).ToList();
        }


        public int GetFavoritesCount(int collectorId)
        {
            return _db.Query<int>("SELECT COUNT(DISTINCT CollectorId) FROM [Favorite] WHERE FavoriteCollectorId =@FavoriteCollectorId", new { FavoriteCollectorId = collectorId }).SingleOrDefault();
        }

        public int GetFavoritesCountForACollectible(int collectibleId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [Favorite] WHERE FavoriteCollectibleId = @FavoriteCollectibleId", new { FavoriteCollectibleId = collectibleId }).SingleOrDefault();
        }

        public int GetFavoritesCountForACollection(int collectionId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [Favorite] WHERE FavoriteCollectionId = @FavoriteCollectionId", new { FavoriteCollectionId = collectionId }).SingleOrDefault();
        }

        public List<int> GetFavoriteCollectionIds(int collectorId)
        {
            return _db.Query<int>("SELECT DISTINCT [FavoriteCollectionId] FROM [Favorite] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).ToList();
        }

        public List<int> GetFavoriteCollectibleIds(int collectorId)
        {
            return _db.Query<int>("SELECT DISTINCT [FavoriteCollectibleId] FROM [Favorite] WHERE CollectorId =@CollectorId AND FavoriteCollectibleId != 0", new { CollectorId = collectorId }).ToList();
        }

        

        public List<int> GetFavoriteCollectorIds(int collectorId)
        {
            return _db.Query<int>("SELECT DISTINCT [CollectorId] FROM [Favorite] WHERE FavoriteCollectorId = @FavoriteCollectorId", new { FavoriteCollectorId = collectorId }).ToList();
        }

        public List<Collector> GetFavoriteCollectorsForACollectible(int collectibleId)
        {
            return _db.Query<Collector>("SELECT * FROM [Collectors] WHERE CollectorId IN (SELECT DISTINCT [CollectorId] FROM [Favorite] WHERE FavoriteCollectibleId = @FavoriteCollectibleId)", new { FavoriteCollectibleId = collectibleId }).ToList();
        }

        public List<Collector> GetFavoriteCollectorsForACollection(int collectionId)
        {
            return _db.Query<Collector>("SELECT * FROM [Collectors] WHERE CollectorId IN (SELECT DISTINCT [CollectorId] FROM [Favorite] WHERE FavoriteCollectionId = @FavoriteCollectionId)", new { FavoriteCollectionId = collectionId }).ToList();
        }

        public List<Favorite> GetAllFavorites()
        {
            return _db.Query<Favorite>("SELECT * FROM [Favorite] ORDER By CreatedDate DESC", null).ToList();
        }

        public bool FollowCollector(Follow followModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT Follow ([CollectorId],[FollowCollectorId], [CreatedDate])    
                        values (@CollectorId, @FollowCollectorId,
                                @CreatedDate)", followModel
                );

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool AddFavorite(Favorite favoriteModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT Favorite ([CollectorId],
                        [FavoriteCollectorId],
                        [FavoriteCollectionId],
                        [FavoriteCollectibleId],
                        [CreatedDate])    
                        values (@CollectorId, 
                                @FavoriteCollectorId,
                                @FavoriteCollectionId,
                                @FavoriteCollectibleId,
                                @CreatedDate)", favoriteModel
                );

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public List<Group> GetGroups()
        {
            return this._db.Query<Group>("SELECT * FROM [Groups] ORDER BY DisplayOrder").ToList();
        }

        public Group GetGroup(int groupId)
        {
            return this._db.Query<Group>("SELECT * FROM [Groups] WHERE GroupId =" + groupId).SingleOrDefault();
        }

        public bool UpdateGroupImage(int groupId, string groupImage)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Groups] SET 
                            [Image] = @Image
                        WHERE GroupId = " +
                        groupId, new { Image = groupImage });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public List<GroupMember> GetGroupMembers(int groupId)
        {
            return this._db.Query<GroupMember>("SELECT * FROM [GroupMembers] WHERE GroupId=" + groupId).ToList();
        }

        public int GetGroupMembersCount(int groupId)
        {
            return _db.Query<int>("SELECT COUNT(DISTINCT CollectorId) FROM [GroupMembers] WHERE GroupId = @GroupId", new { GroupId = groupId }).SingleOrDefault();
        }

        public bool JoinGroup(GroupMember memberModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT GroupMembers ([CollectorId],[GroupId], [GroupRoleId], [CreatedDate])    
                        values (@CollectorId, @GroupId, @GroupRoleId,
                                @CreatedDate)", memberModel
                );

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool LeaveGroup(GroupMember memberModel)
        {
            int rowsAffected = this._db.Execute(@"DELETE GroupMembers 
                                WHERE [GroupId] = @GroupId AND [CollectorId] = @CollectorId", memberModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }


        public bool UpdateGroupDescription(int groupId, string description)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Groups] SET 
                            [Description] = @Description
                        WHERE GroupId = " +
                        groupId, new { Description = description });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateGroupOrder(string groupName, int displayOrder)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Groups] SET 
                            [DisplayOrder] = @DisplayOrder
                        WHERE Name = '" + groupName + "'"
                        , new { DisplayOrder = displayOrder });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool InsertGroupInvitation(GroupInvitation invitationModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT GroupInvitation ([CollectorId],[GroupId], 
                        [ToCollectorId], [StatusId], [Message], [CreatedDate])    
                        values (@CollectorId, @GroupId, @ToCollectorId, @StatusId, @Message,
                                @CreatedDate)", invitationModel
                );

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteGroup(int groupId)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Groups] WHERE GroupId = @GroupId",
                new { GroupId = groupId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool InsertGroupPhoto(GroupPhoto photoModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT GroupPhotos ([CollectorId],[GroupId], [CollectibleId], [CreatedDate])    
                        values (@CollectorId, @GroupId, @CollectibleId,
                                @CreatedDate)", photoModel
                );

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public List<GroupPhoto> GetGroupPhotos(int groupId)
        {
            return this._db.Query<GroupPhoto>("SELECT * FROM [GroupPhotos] WHERE GroupId=" + groupId).ToList();
        }

        public List<Collectible> GetGroupImages(int groupId)
        {
            return this._db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CollectibleId IN (SELECT CollectibleId FROM [GroupPhotos] WHERE GroupId=" + groupId + ")").ToList();
        }

        public bool InsertMessage(Message messageModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT Messages (
                                [MessageTopic],
                                [MessageText],
                                [FromCollectorId],
                                [ToCollectorId],
                                [ParentId],
                                [CreatedDate] ) 
                        values (
                                @MessageTopic, 
                                @MessageText, 
                                @FromCollectorId, 
                                @ToCollectorId, 
                                @ParentId, 
                                @CreatedDate )",
                        messageModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteMessage(int messageId)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Messages] WHERE MessageId = @MessageId",
                new { MessageId = messageId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteAllMessages(int toCollectorId)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Messages] WHERE ToCollectorId = @ToCollectorId OR FromCollectorId = @ToCollectorId",
                new { ToCollectorId = toCollectorId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public List<Message> GetMessages(int collectorId)
        {
            return _db.Query<Message>("SELECT * FROM [Messages] WHERE ToCollectorId =@ToCollectorId OR FromCollectorId =@ToCollectorId ORDER BY CreatedDate DESC", new { ToCollectorId = collectorId }).ToList();
        }

        public bool InsertCollectibleView(int collectibleId)
        {
            int rowsAffected = this._db.Execute(@"INSERT [View] (
                                [ViewCollectionId],
                                [ViewCollectibleId],
                                [CreatedDate] ) 
                        values (
                                @ViewCollectionId, 
                                @ViewCollectibleId, 
                                @CreatedDate )",
                        new { ViewCollectionId = 0, ViewCollectibleId = collectibleId, CreatedDate = DateTime.Now });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool InsertViewLog(ViewLog model)
        {
            int rowsAffected = this._db.Execute(@"INSERT [ViewLog] (
                                [ViewCollectorId],
                                [ViewCollectionId],
                                [ViewCollectibleId],
                                [ViewByCollectorId],
                                [ViewPageName],
                                [CreatedDate] ) 
                        values (
                                @ViewCollectorId, 
                                @ViewCollectionId, 
                                @ViewCollectibleId, 
                                @ViewByCollectorId, 
                                @ViewPageName, 
                                @CreatedDate )",
                        model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int GetViewsCountForACollectible(int collectibleId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [ViewLog] WHERE ViewCollectibleId = @ViewCollectibleId", new { ViewCollectibleId = collectibleId }).SingleOrDefault();
        }

        public int GetViewsCountForACollection(int collectionId)
        {
            return _db.Query<int>("SELECT COUNT(*) FROM [ViewLog] WHERE ViewCollectionId = @ViewCollectionId", new { ViewCollectionId = collectionId }).SingleOrDefault();
        }

        public List<ViewLog> GetViewLogsForCollector(int collectorId)
        {
            return _db.Query<ViewLog>("SELECT * FROM [ViewLog] WHERE ViewByCollectorId =@ViewByCollectorId ORDER BY CreatedDate DESC", new { ViewByCollectorId = collectorId }).ToList();
        }

        public int InsertGroup(Group groupModel)
        {
            int id = this._db.Query<int>(@"INSERT Groups (
                                [Name],
                                [Description],
                                [Image],
                                [ImageBanner],
                                [DisplayOrder],
                                [CreatedOnUtc],
                                [UpdatedOnUtc] ) 
                        values (
                                @Name, 
                                @Description, 
                                @Image, 
                                @ImageBanner, 
                                @DisplayOrder,
                                @CreatedOnUtc, 
                                @UpdatedOnUtc ); SELECT CAST(SCOPE_IDENTITY() as int)",
                        groupModel).Single();
            return id;
        }

        public int InsertPurchase(Purchase purchaseModel)
        {
            int id = this._db.Query<int>(@"INSERT Purchase (
                                [CustomerId],
                                [CollectibleId],
                                [CollectorId],
                                [Count],
                                [Title],
                                [Image],
                                [Price],
                                [TypeId],
                                [StatusId],
                                [CreatedDate] ) 
                        values (
                                @CustomerId, 
                                @CollectibleId, 
                                @CollectorId, 
                                @Count, 
                                @Title, 
                                @Image,
                                @Price, 
                                @TypeId, 
                                @StatusId, 
                                @CreatedDate ); SELECT CAST(SCOPE_IDENTITY() as int)",
                        purchaseModel).Single();
            return id;
        }

        public Purchase GetPurchaseById(int purchaseId)
        {
            return _db.Query<Purchase>("SELECT * FROM [Purchase] WHERE PurchaseId =@PurchaseId", new { PurchaseId = purchaseId }).SingleOrDefault();
        }

        public Purchase GetPurchaseByCollectible (int collectibleId)
        {
            return _db.Query<Purchase>("SELECT * FROM [Purchase] WHERE CollectibleId =@CollectibleId", new { CollectibleId = collectibleId }).SingleOrDefault();
        }

        public List<Purchase> GetPurchasesByCollectible(int collectibleId)
        {
            return _db.Query<Purchase>("SELECT * FROM [Purchase] WHERE CollectibleId =@CollectibleId", new { CollectibleId = collectibleId }).ToList();
        }

        public List<Purchase> GetPurchasesByCollector(int collectorId)
        {
            return _db.Query<Purchase>("SELECT * FROM [Purchase] WHERE CustomerId =@CustomerId", new { CustomerId = collectorId }).ToList();
        }

        public List<Purchase> GetSalesByCollector(int collectorId)
        {
            return _db.Query<Purchase>("SELECT * FROM [Purchase] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).ToList();
        }


        public List<ContentModel> GetContents()
        {
            return this._db.Query<ContentModel>("SELECT * FROM [Contents] ORDER BY ContentName").ToList();
        }
        public ContentModel GetContent(int contentId)
        {
            return this._db.Query<ContentModel>("SELECT * FROM [Contents] WHERE ContentId =@ContentId", new { ContentId = contentId }).SingleOrDefault();
        }

        public ContentModel GetContentByName(string contentName)
        {
            return this._db.Query<ContentModel>("SELECT * FROM [Contents] WHERE ContentName =@ContentName", new { ContentName = contentName }).SingleOrDefault();
        }

        public int InsertContent(ContentModel contentModel)
        {
            int id = this._db.Query<int>(@"INSERT Contents (
                                [ContentName],
                                [ContentText],
                                [CreatedDate] ) 
                        values (
                                @ContentName, 
                                @ContentText, 
                                @CreatedDate ); SELECT CAST(SCOPE_IDENTITY() as int)",
                        contentModel).Single();
            return id;
        }

        public bool UpdateContent(ContentModel contentModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Contents] SET 
                            [ContentName] = @ContentName,
                            [ContentText] = @ContentText
                        WHERE ContentId = " +
                        contentModel.ContentId, contentModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public List<TemplateModel> GetTemplates()
        {
            return this._db.Query<TemplateModel>("SELECT * FROM [Templates] ORDER BY TemplateName").ToList();
        }

        public List<TemplateModel> GetEmailTemplates()
        {
            return this._db.Query<TemplateModel>("SELECT * FROM [Templates] WHERE TemplateTypeId = 1 ORDER BY TemplateName").ToList();
        }

        public TemplateModel GetTemplate(int templateId)
        {
            return this._db.Query<TemplateModel>("SELECT * FROM [Templates] WHERE TemplateId = @TemplateId", new { TemplateId = templateId }).SingleOrDefault();
        }

        public TemplateModel GetTemplateByName(string templateName)
        {
            return this._db.Query<TemplateModel>("SELECT * FROM [Templates] WHERE TemplateName = @TemplateName", new { TemplateName = templateName }).SingleOrDefault();
        }

        public bool UpdateTemplate(TemplateModel templateModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Templates] SET 
                            [TemplateName] = @TemplateName,
                            [TemplateText] = @TemplateText
                        WHERE TemplateId = " +
                        templateModel.TemplateId, templateModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int InsertTemplate(TemplateModel templateModel)
        {
            int id = this._db.Query<int>(@"INSERT Templates (
                                [TemplateTypeId],
                                [TemplateName],
                                [TemplateText],
                                [CreatedDate] ) 
                        values (
                                @TemplateTypeId, 
                                @TemplateName, 
                                @TemplateText, 
                                @CreatedDate ); SELECT CAST(SCOPE_IDENTITY() as int)",
                        templateModel).Single();
            return id;
        }

        public int InsertUserSettings(UserSettings settingsModel)
        {
            int id = this._db.Query<int>(@"INSERT UserSettings (
                                [CollectorId],
                                [AllowFollowId],
                                [AllowUnfollowId],
                                [FeaturePrivacyId],
                                [FollowPrivacyId],
                                [LocationPrivacyId],
                                [ProfilePrivacyId],
                                [ReceiveEmails],
                                [ReceiveLetters],
                                [ChangesToPolicies],
                                [NotifyWhenFollowed],
                                [NotifyWhenCommented],
                                [NotifyWhenReplied],
                                [NotifyWhenInvited],
                                [CreatedDate],
                                [NotifyWhenInvitedToAnEvent],
                                [NotifyWhenSomeoneLikes],
                                [NotifyWhenSomeoneAttendingEvent]
                                ) 
                        values (
                                @CollectorId, 
                                @AllowFollowId, 
                                @AllowUnfollowId, 
                                @FeaturePrivacyId, 
                                @FollowPrivacyId, 
                                @LocationPrivacyId,
                                @ProfilePrivacyId,
                                @ReceiveEmails, 
                                @ReceiveLetters, 
                                @ChangesToPolicies, 
                                @NotifyWhenFollowed, 
                                @NotifyWhenCommented, 
                                @NotifyWhenReplied, 
                                @NotifyWhenInvited,  
                                @CreatedDate,
                                @NotifyWhenInvitedToAnEvent,
                                @NotifyWhenSomeoneLikes,
                                @NotifyWhenSomeoneAttendingEvent
                        ); SELECT CAST(SCOPE_IDENTITY() as int)",
                        settingsModel).Single();
            return id;
        }

        public UserSettings GetUserSettings(int collectorId)
        {
            return this._db.Query<UserSettings>("SELECT * FROM [UserSettings] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
        }

        public bool UpdateAllowFollow(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                            [AllowFollowId] = @AllowFollowId
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateAllowUnfollow(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                            [AllowUnfollowId] = @AllowUnfollowId
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateFeaturePrivacy(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                            [FeaturePrivacyId] = @FeaturePrivacyId
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateFollowPrivacy(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                            [FollowPrivacyId] = @FollowPrivacyId
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateLocationPrivacy(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                            [LocationPrivacyId] = @LocationPrivacyId
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateProfilePrivacy(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                            [ProfilePrivacyId] = @ProfilePrivacyId
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateEmailPreferences(UserSettings settingsModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [UserSettings] SET 
                                [ReceiveEmails] = @ReceiveEmails,
                                [ReceiveLetters] = @ReceiveLetters,
                                [ChangesToPolicies] = @ChangesToPolicies,
                                [NotifyWhenFollowed] = @NotifyWhenFollowed,
                                [NotifyWhenCommented] = @NotifyWhenCommented,
                                [NotifyWhenReplied] = @NotifyWhenReplied,
                                [NotifyWhenInvited] = @NotifyWhenInvited,
                                [NotifyWhenInvitedToAnEvent] = @NotifyWhenInvitedToAnEvent,
                                [NotifyWhenSomeoneLikes] = @NotifyWhenSomeoneLikes,
                                [NotifyWhenSomeoneAttendingEvent] = @NotifyWhenSomeoneAttendingEvent
                        WHERE CollectorId = " +
                        settingsModel.CollectorId, settingsModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateShipping(int id, string trackingNumber, string shipper)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Purchase] SET 
                            [TrackingNumber] = @TrackingNumber,
                            [Shipper] = @Shipper
                        WHERE PurchaseId = " +
                        id, new { TrackingNumber = trackingNumber, Shipper = shipper});

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public Purchase GetPurchase(int purchaseId)
        {
            return this._db.Query<Purchase>("SELECT * FROM [Purchase] WHERE PurchaseId =@PurchaseId", new { PurchaseId = purchaseId }).SingleOrDefault();
        }

        public bool RemovePendingCollectible(int id)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Purchase] WHERE CollectibleId = @CollectibleId",
                new { CollectibleId = id });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeletePurchase(int id)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Purchase] WHERE PurchaseId = @PurchaseId",
                new { PurchaseId = id });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }


    }

}