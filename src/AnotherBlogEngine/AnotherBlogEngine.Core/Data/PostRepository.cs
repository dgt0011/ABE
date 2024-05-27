using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Extensions;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace AnotherBlogEngine.Core.Data
{
    public class PostRepository : RepositoryBase<PostDetailsDto>, IPostRepository<PostDetailsDto>
    {
        public PostRepository(IDbContext context, ILogger logger) : base(logger, context)
        {
        }

        public async Task<int> GetPostCount(Constants.PostStatus postStatusFilter)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({postStatusFilter})");

            int retVal = 0;

            // if the requested status is draft, include published as well
            // this does feel more like a 'business logic' decision - probably needs to shift
            // from this level up to BLL
            var query = SelectPostCountByStatusSql;

            if (postStatusFilter == Constants.PostStatus.Draft)
            {
                query += " OR status = 1";  // include published, not just draft
            }

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var value = await connection.ExecuteScalarAsync(query, new { Status = postStatusFilter });
                    if (value != null)
                    {
                        retVal = Convert.ToInt32(value);
                    }

                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("{Exception}", ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({postStatusFilter})");
            return retVal;
        }

        public async Task<IReadOnlyCollection<PostSummaryDto>> GetPostSummaries(Constants.PostStatus postStatusFilter)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({postStatusFilter})");

            var retVal = new List<PostSummaryDto>();

            // if the requested status is draft, include published as well
            // this does feel more like a 'business logic' decision - probably needs to shift
            // from this level up to BLL
            var query = SelectPostsByPostStatus;
            if (postStatusFilter == Constants.PostStatus.Draft)
            {
                query += " OR status = 1";
            }
            query += ";";

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var dtos = (await connection.QueryAsync<PostSummaryDto>(
                        query, 
                        new { Status = postStatusFilter })).ToList();

                    retVal = dtos;
                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("{Exception}",ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({postStatusFilter})");
            return retVal;
        }

        public async Task<IReadOnlyCollection<PostSummaryDto>> GetPostSummaries(Constants.PostStatus postStatusFilter, long count, long startIndex)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({postStatusFilter},{count}, {startIndex})");

            var retVal = new List<PostSummaryDto>();

            // if the requested status is draft, include published as well
            // this does feel more like a 'business logic' decision - probably needs to shift
            // from this level up to BLL
            var query = SelectPostsByPostStatus;
            var orderByClause = " ORDER BY published_datetime DESC ";

            if (postStatusFilter == Constants.PostStatus.Draft)
            {
                query += " OR status = 1";  // include published, not just draft
                orderByClause = " ORDER BY created_datetime DESC ";
            }

            // limit-offset paging is acknowledged as a suboptimal approach, but for a single user blog engine, its
            // unlikely to hit inconsistency issues or inefficiency issues that a larger system is likely to experience
            query += $"{orderByClause} LIMIT {count} OFFSET {startIndex}";

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var dtos = (await connection.QueryAsync<PostSummaryDto>(
                        query,
                        new { Status = postStatusFilter })).ToList();

                    retVal = dtos;
                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("{Exception}", ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({postStatusFilter},{count}, {startIndex})");
            return retVal;
        }

        public async Task<PostDetailsDto?> GetPostBySlug(string slug)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({slug})");

            PostDetailsDto? retVal = null;

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var dtos = (await connection.QueryAsync<PostDetailsDto>(SelectPostBySlugQuery, new { Slug = slug} )).ToList();
                    if (dtos.Count > 1)
                    {
                        Logger.LogWarning("Multiple records found for slug '{Slug}' - returning the first instance", slug);
                    }
                    retVal = dtos.FirstOrDefault();
                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("{Exception}", ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({slug})");
            return retVal;
        }

        public async Task<(bool Result, PostDetailsDto? dto)> UpsertPost(PostDetailsDto dto)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data));

            var retVal = false;
            PostDetailsDto? savedDto = dto;

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var transaction = connection.BeginTransaction();

                     (var result, savedDto) = await Upsert(dto, transaction);

                    if (savedDto != null)
                    {
                        result &= await ManageTopics(dto, savedDto, transaction);
                        result &= await ManageTags(dto, savedDto, transaction);

                        if (result)
                        {
                            retVal = true;
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("{Exception}", ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data));

            return (retVal, savedDto);
        }

        public async Task<(bool Result, PostDetailsDto? dto)> GetPost(long id)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix:$"({id})");

            var retValResult = false;
            PostDetailsDto? retVal = null;

            var query = $"{SelectFullPostSql} WHERE bp.id = @Id";

            try
            {
                List<PostDetailsDto> storedRecords = new();

                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    await connection.QueryAsync<PostDetailsDto, TopicDto, TagDto, PostDetailsDto>(
                        query,
                        param: new { Id = id },
                        map: (post, topic, tag) =>
                        {
                            var dto = storedRecords.FirstOrDefault(a => a.id == post.id);
                            if (dto == null)
                            {
                                dto = new PostDetailsDto
                                {
                                    id = post.id,
                                    title = post.title,
                                    description = post.description,
                                    slug = post.slug,
                                    status = post.status,
                                    view_count = post.view_count,
                                    published_datetime = post.published_datetime,
                                    created_datetime = post.created_datetime,
                                    body = post.body,
                                    cover_img_path = post.cover_img_path,
                                    deleted_fg = post.deleted_fg
                                };
                                storedRecords.Add(dto);
                            }

                            var topicDto = new TopicDto
                            {
                                id = topic.id,
                                title = topic.title,
                                description = topic.description,
                                date_created = topic.date_created,
                                date_updated = topic.date_updated,
                                deleted_fg = topic.deleted_fg
                            };

                            dto.Topics ??= new List<TopicDto>();
                            dto.Topics.Add(topicDto);

                            var tagDto = new TagDto
                            {
                                id = tag.id,
                                title = tag.title,
                                description = tag.description,
                                date_created = tag.date_created,
                                date_updated = tag.date_updated,
                                deleted_fg = tag.deleted_fg
                            };

                            dto.Tags ??= new List<TagDto>();
                            dto.Tags.Add(tagDto);

                            return dto;
                        },
                        splitOn: "PostId, TopicId, TagId");

                    connection.Close();

                    retVal = storedRecords.FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                Logger.LogError("{Exception}", ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({id})");

            return (retValResult, retVal);
        }

        private static async Task<bool> ManageTopics(PostDetailsDto dto, PostDetailsDto storedDto, IDbTransaction transaction)
        {
            bool result = true;
            var toStoreTopicRelationshipIds = new List<long>();

            // does the post already have a relationship with the topic/s?
            if (dto.Topics != null && dto.Topics.Count != 0)
            {
                if (storedDto is not { Topics: { } } || storedDto.Topics.Count == 0)
                {
                    // we need to store all topic ids
                    toStoreTopicRelationshipIds.AddRange(dto.Topics.Select(topic => topic.id));
                }
                else
                {
                    foreach (var topic in dto.Topics)
                    {
                        var storedTopic = storedDto.Topics.SingleOrDefault(a => a.id == topic.id);
                        if (storedTopic == null)
                        {
                            // not currently stored
                            toStoreTopicRelationshipIds.Add(topic.id);
                        }
                    }
                }

                if (toStoreTopicRelationshipIds.Count != 0)
                {
                    foreach (var storeTopicRelationshipId in toStoreTopicRelationshipIds)
                    {
                        var insertCount =
                            await transaction.Connection!.ExecuteAsync(
                                InsertPostTopicRelationshipSql,
                                new { PostId = storedDto.id, TopicId = storeTopicRelationshipId },
                                transaction);

                        result &= insertCount > 0;
                    }
                }
            }

            var toRemoveTopicRelationshipIds = new List<long>();

            if (storedDto is { Topics: { } } && storedDto.Topics.Count != 0)
            {
                //remove any relationships that no longer exist
                if (dto.Topics is null || dto.Topics.Count == 0)
                {
                    toRemoveTopicRelationshipIds.AddRange(storedDto.Topics.Select(topic => topic.id));
                }
                else
                {
                    foreach (var storedDtoTopic in storedDto.Topics)
                    {
                        var topic = dto.Topics.SingleOrDefault(a => a.id == storedDtoTopic.id);
                        if (topic == null)
                        {
                            toRemoveTopicRelationshipIds.Add(storedDtoTopic.id);
                        }
                    }
                }

                if (toRemoveTopicRelationshipIds.Count != 0)
                {
                    foreach (var topicId in toRemoveTopicRelationshipIds)
                    {
                        var deleteCount =
                            await transaction.Connection!.ExecuteAsync(
                                DeletePostTopicRelationshipSql,
                                new { PostId = dto.id, TopicId = topicId },
                                transaction);

                        result &= deleteCount > 0;
                    }
                }
            }

            return result;
        }

        private static async Task<bool> ManageTags(PostDetailsDto dto, PostDetailsDto storedDto, IDbTransaction transaction)
        {
            bool result = true;
            var toStoreTagRelationshipIds = new List<long>();

            // does the post already have a relationship with the topic/s?
            if (dto.Tags != null && dto.Tags.Count != 0)
            {
                if (storedDto is not { Tags: { } } || storedDto.Tags.Count == 0)
                {
                    // we need to store all topic ids
                    toStoreTagRelationshipIds.AddRange(dto.Tags.Select(tag => tag.id));
                }
                else
                {
                    foreach (var tag in dto.Tags)
                    {
                        var storedTag = storedDto.Tags.SingleOrDefault(a => a.id == tag.id);
                        if (storedTag == null)
                        {
                            // not currently stored
                            toStoreTagRelationshipIds.Add(tag.id);
                        }
                    }
                }

                if (toStoreTagRelationshipIds.Count != 0)
                {
                    foreach (var storeTagRelationshipId in toStoreTagRelationshipIds)
                    {
                        var insertCount =
                            await transaction.Connection!.ExecuteAsync(
                                InsertPostTagRelationshipSql,
                                new { PostId = storedDto.id, TagId = storeTagRelationshipId },
                                transaction);

                        result &= insertCount > 0;
                    }
                }
            }

            var toRemoveTagRelationshipIds = new List<long>();

            if (storedDto is { Tags: { } } && storedDto.Tags.Count != 0)
            {
                //remove any relationships that no longer exist
                if (dto.Tags is null || dto.Tags.Count == 0)
                {
                    toRemoveTagRelationshipIds.AddRange(storedDto.Tags.Select(topic => topic.id));
                }
                else
                {
                    foreach (var storedDtoTopic in storedDto.Tags)
                    {
                        var topic = dto.Tags.SingleOrDefault(a => a.id == storedDtoTopic.id);
                        if (topic == null)
                        {
                            toRemoveTagRelationshipIds.Add(storedDtoTopic.id);
                        }
                    }
                }

                if (toRemoveTagRelationshipIds.Count != 0)
                {
                    foreach (var tagId in toRemoveTagRelationshipIds)
                    {
                        var deleteCount =
                            await transaction.Connection!.ExecuteAsync(
                                DeletePostTagRelationshipSql,
                                new { PostId = dto.id, TagId = tagId },
                                transaction);

                        result &= deleteCount > 0;
                    }
                }
            }

            return result;
        }


        private const string SelectPostBySlugQuery = "SELECT * FROM blog_post WHERE deleted_fg = false AND slug = @Slug;";

        private const string SelectPostCountByStatusSql = "SELECT COUNT(*) FROM blog_post WHERE deleted_fg = false AND status = @Status ";

        private const string SelectPostsByPostStatus = "SELECT * FROM blog_post WHERE deleted_fg = false AND status = @Status ";

        private const string InsertPostTopicRelationshipSql = "INSERT INTO blog_post_topic (post_id, topic_id) " +
                                                              "VALUES (@PostId, @TopicId);";

        private const string DeletePostTopicRelationshipSql = "DELETE FROM blog_post_topic " +
                                                              "WHERE post_id = @PostId "+
                                                              "AND topic_id = @TopicId;";

        private const string InsertPostTagRelationshipSql = "INSERT INTO blog_post_tag (post_id, tag_id) " +
                                                              "VALUES (@PostId, @TagId);";

        private const string DeletePostTagRelationshipSql = "DELETE FROM blog_post_tag " +
                                                              "WHERE post_id = @PostId " +
                                                              "AND tag_id = @TagId;";

        private const string SelectFullPostSql = "SELECT bp.id as PostId, bp.*," +
                                                 " bpt.post_id, bpt.topic_id," +
                                                 " t.id as TopicId, t.*," +
                                                 " bptag.post_id, bptag.tag_id," +
                                                 " tg.id as TagId, tg.*" +
                                                 " FROM blog_post bp" +
                                                 " LEFT OUTER JOIN blog_post_topic bpt" +
                                                 "      ON bp.id = bpt.post_id" +
                                                 " INNER JOIN topic t" +
                                                 "      ON bpt.topic_id = t.id" +
                                                 " LEFT OUTER JOIN blog_post_tag bptag" +
                                                 "      ON bp.id = bptag.post_id" + 
                                                 " INNER JOIN tag tg" +
                                                 "      ON bptag.tag_id = tg.id";

    }
}
