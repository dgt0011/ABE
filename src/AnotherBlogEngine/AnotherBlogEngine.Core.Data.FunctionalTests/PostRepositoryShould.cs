using AnotherBlogEngine.Core.Data.Dto;
using AnotherBlogEngine.Core.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Dapper;
using FluentAssertions;
using Xunit;

namespace AnotherBlogEngine.Core.Data.FunctionalTests
{
    public class PostRepositoryShould
    {
        private readonly ILogger _logger;

        // CA1859: Use concrete types when possible for improved performance
        //         Choosing to use the interface in the tests
#pragma warning disable CA1859
        private readonly IDbContext? _dbContext;
        private readonly IPostRepository<PostDetailsDto>? _repository;
#pragma warning restore CA1859

        public PostRepositoryShould()
        {
            _logger = new FakeLogger(LogLevel.Trace);

            var assName = Assembly.GetExecutingAssembly().GetName().Name;
            if (assName != null)
            {
                var appAssembly = Assembly.Load(new AssemblyName(assName));

                var config = new ConfigurationBuilder().AddUserSecrets(appAssembly, true).Build();

                _dbContext = new PostgresqlDbContext(config);

                _repository = new PostRepository(_dbContext, _logger);
            }
        }

        [Fact]
        public async Task Read_Non_Existent_Id_Without_Error_Or_Exception_Being_Logged()
        {
            // Arrange: Given the repository has already been created with a valid database connection

            // Act : When the repository Get is called with an Id that is known to not exist
            _ = await _repository!.Find(long.MaxValue);

            // Assert : Then no errors or exceptions have been logged
            LogHasErrorsOrExceptions((FakeLogger)_logger).Should().BeFalse();
        }

        [Fact]
        public async Task Save_New_Basic_PostDetails_Without_Error_Or_Exception_Being_Logged()
        {
            // Arrange: Given the repository has already been created with a valid database connection

            // Act : When a basic new PostDetails record is saved
            var postDetails = new PostDetailsDto
            {
                title = "Test Post 1",
                description = "The test post description"
            };
            var (_, savedDto) = await _repository!.UpsertPost(postDetails);

            // Assert : Then no errors or exceptions have been logged
            LogHasErrorsOrExceptions((FakeLogger)_logger).Should().BeFalse();

            Cleanup(savedDto?.id);
        }

        [Fact]
        public async Task Populate_Id_When_Saving_A_New_Basic_PostDetails()
        {
            // Arrange: Given the repository has already been created with a valid database connection

            // Act : When a basic new PostDetails record is saved
            var postDetails = new PostDetailsDto
            {
                title = "Test Post 2",
                description = "The test post description"
            };
            var (_, savedDto) = await _repository!.UpsertPost(postDetails);

            // Assert : Then the Id has been populated with a non zero value
            savedDto?.id.Should().BeGreaterThan(0);

            Cleanup(savedDto?.id);
        }

        [Fact]
        public async Task Returns_Expected_Result_When_Saving_A_New_Basic_PostDetails()
        {
            // Arrange: Given the repository has already been created with a valid database connection

            // Act : When a basic new PostDetails record is saved
            var postDetails = new PostDetailsDto
            {
                title = "Test Post 2",
                description = "The test post description"
            };
            var (result, savedDto) = await _repository!.UpsertPost(postDetails);

            // Assert : Then the expected result is returned
            result.Should().BeTrue();

            Cleanup(savedDto?.id);
        }

        [Fact]
        public async Task Read_Basic_PostDetails_Without_Error_Or_Exception_Being_Logged()
        {
            // Arrange: Given the repository has already been created with a valid database connection
            //          And a known Post Details record exists with minimal details.
            var postDetails = new PostDetailsDto
            {
                title = "Test Post 2",
                description = "The test post description"
            };
            var (_, savedDto) = await _repository!.UpsertPost(postDetails);

            // Act : When a basic new PostDetails record is read
            _ = await _repository!.Find(savedDto!.id);

            // Assert : Then no errors or exceptions have been logged
            LogHasErrorsOrExceptions((FakeLogger)_logger).Should().BeFalse();

            Cleanup(savedDto.id);
        }

        [Fact]
        public async Task Save_New_PostDetails_With_Topic_Without_Error_Or_Exception_Being_Logged()
        {
            // Arrange: Given the repository has already been created with a valid database connection
            //          And a known topic already exists
            var topicId = SetupTestTopic("TEST_TOPIC_1");

            // Act : When a basic new PostDetails record is saved
            var postDetails = new PostDetailsDto
            {
                title = "Test Post 3",
                description = "The test post description",
                Topics = new List<TopicDto>
                {
                    new() { id = topicId }
                }
            };
            var (_, savedDto) = await _repository!.UpsertPost(postDetails);

            // Assert : Then no errors or exceptions have been logged
            LogHasErrorsOrExceptions((FakeLogger)_logger).Should().BeFalse();

            Cleanup(savedDto?.id);
            CleanupTopic(topicId);
        }

        [Fact]
        public async Task Read_TopicDetails_When_Reading_Stored_PostDetails()
        {
            // Arrange: Given the repository has already been created with a valid database connection, and
            //          a known topic already exists, and
            //          a basic post details record exists with an associated TopicId
            var topicId = SetupTestTopic("TEST_TOPIC_1");

            var postDetails = new PostDetailsDto
            {
                title = "Test Post 3",
                description = "The test post description",
                Topics = new List<TopicDto>
                {
                    new() { id = topicId }
                }
            };
            var (_, savedDto) = await _repository!.UpsertPost(postDetails);

            if (savedDto != null)
            {
                // Act : When the PostDetails record is read
                var (_,storedDto) = await _repository!.GetPost(savedDto.id);

                // Assert : Then the Topic details are included in the Post Details Topic collection
                storedDto!.Topics.Should().NotBeNull();
                storedDto.Topics!.Count.Should().Be(1);

                storedDto.Topics.Single().id.Should().Be(topicId);
                storedDto.Topics.Single().title.Should().Be("TEST_TOPIC_1");
                storedDto.Topics.Single().description.Should().NotBeNullOrEmpty();

                Cleanup(savedDto.id);
            }
            else
            {
                savedDto.Should().NotBeNull();
            }

            CleanupTopic(topicId);
        }

        [Fact]
        public async Task Read_TagDetails_When_Reading_Stored_PostDetails()
        {
            // Arrange: Given the repository has already been created with a valid database connection, and
            //          a known topic already exists, and
            //          a known tag already exists, and
            //          a basic post details record exists with an associated TopicId
            var topicId = SetupTestTopic("TEST_TOPIC_1");
            var tagId = SetupTestTag("TEST_KEY_1");

            var postDetails = new PostDetailsDto
            {
                title = "Test Post 3",
                description = "The test post description",
                Topics = new List<TopicDto>
                {
                    new() { id = topicId }
                },
                Tags = new List<TagDto>
                {
                    new() { id = tagId }
                }
            };

            var (_, savedDto) = await _repository!.UpsertPost(postDetails);

            // Act : When the PostDetails record is read
            var (_, storedDto) = await _repository!.GetPost(savedDto!.id);

            // Assert : Then the Topic details are included in the Post Details Topic collection
            storedDto!.Topics.Should().NotBeNull();
            storedDto.Topics!.Count.Should().Be(1);
            storedDto.Topics.Single().id.Should().Be(topicId);
            storedDto.Topics.Single().title.Should().Be("TEST_TOPIC_1");
            storedDto.Topics.Single().description.Should().NotBeNullOrEmpty();

            storedDto.Tags.Should().NotBeNullOrEmpty();
            storedDto.Tags!.Count.Should().Be(1);
            storedDto.Tags.Single().id.Should().Be(tagId);
            storedDto.Tags.Single().title.Should().Be("TEST_KEY_1");
            storedDto.Tags.Single().description.Should().NotBeNullOrEmpty();

            Cleanup(savedDto.id);
            CleanupTopic(topicId);
            CleanupTag(topicId);
        }

        private static bool LogHasErrorsOrExceptions(FakeLogger logger)
        {
            var errorLogCounts = logger.LogEntries.Count(a => a.StartsWith("[ERROR]"));
            var exceptionLogCounts = logger.LogEntries.Count(a => a.Contains("exception", StringComparison.CurrentCultureIgnoreCase));

            return errorLogCounts > 0 || exceptionLogCounts > 0;
        }

        private long SetupTestTopic(string title)
        {
            try
            {
                using var connection = _dbContext!.CreateOpenConnection();
                var existingId =  connection!.ExecuteScalar("SELECT id FROM topic WHERE title = @Title", new { Title = title });
                if (existingId is null)
                {
                    return connection!.QuerySingle<long>("INSERT INTO topic (title, description, deleted_fg) VALUES (@Title,'Test Topic for Testing', false) RETURNING id;", new { Title = title });

                }

                return (long)existingId;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private long SetupTestTag(string title)
        {
            using var connection = _dbContext!.CreateOpenConnection();
            var existingId = connection!.ExecuteScalar("SELECT id FROM tag WHERE title = @Title", new { Title = title });
            if (existingId is null)
            {
                return connection!.QuerySingle<long>("INSERT INTO tag (title, description, deleted_fg) VALUES (@Title,'Test Tag for Testing', false) RETURNING id;", new { Title = title });
            }
            return (long)existingId;
        }

        private void Cleanup(long? id)
        {
            if (_dbContext is not null && id is not null)
            {
                using var connection = _dbContext.CreateOpenConnection();
                connection!.Execute("DELETE FROM blog_post WHERE id = @Id;", new { Id = id });
                connection!.Execute("DELETE FROM blog_post_tag WHERE post_id = @Id;", new { Id = id });
                connection!.Execute("DELETE FROM blog_post_topic WHERE post_id = @Id;", new { Id = id });
            }
        }

        private void CleanupTopic(long id)
        {
            using var connection = _dbContext!.CreateOpenConnection();
            connection!.Execute("DELETE FROM topic WHERE id = @Id;", new { Id = id });
        }

        private void CleanupTag(long id)
        {
            using var connection = _dbContext!.CreateOpenConnection();
            connection!.Execute("DELETE FROM tag WHERE id = @Id;", new { Id = id });
        }
    }
}