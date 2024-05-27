using AnotherBlogEngine.Core.Data.Interfaces;
using AnotherBlogEngine.Core.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AnotherBlogEngine.Core.Data
{
    public abstract class RepositoryBase<T> : IBaseRepository<T> where T : IEntity, new()
    {
        protected readonly IDbContext DbContext;

        private readonly string? _typeName;
        private readonly string _tableName = string.Empty;

        private readonly string _selectAll = string.Empty;
        private readonly string _selectItemById = string.Empty;
        private readonly string _deleteItemById = string.Empty;
        private readonly string _insertItemSql = string.Empty;
        private readonly string _countItemsSql = string.Empty;

        public ILogger Logger { get; set; }

        protected RepositoryBase(ILogger logger, IDbContext dbContext)
        {
            Logger = logger;

            DbContext = dbContext;

            var typeObj = typeof(T);
            _typeName = typeObj.FullName;

            TableAttribute? tableAttribute = null;

            var customAttributes = Attribute.GetCustomAttributes(typeof(T));
            foreach (var customAttribute in customAttributes)
            {
                if (customAttribute.GetType() == typeof(TableAttribute))
                {
                    tableAttribute = (TableAttribute)customAttribute;
                }
            }

            if (tableAttribute == null) return;

            _tableName = tableAttribute.Name;

            _selectAll = $"{BuildSelectStatement()} {_tableName} WHERE deleted_fg = false;";
            _selectItemById = $"{BuildSelectStatement()} {_tableName} WHERE Id = @Id;";
            _insertItemSql = BuildInsertStatement();
            _deleteItemById = $"UPDATE {_tableName} SET deleted_fg = true WHERE Id = @Id;";
            _countItemsSql = $"SELECT COUNT(Id) FROM {_tableName} WHERE deleted_fg = false;";
        }

        public async Task<(bool Result, T? Entity)> Upsert(T item, IDbTransaction? transaction = null)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({JsonSerializer.Serialize(item)})");

            var isInsert = true;

            T? existingItem = default;

            if (item.id != 0)
            {
                existingItem = await Find(item.id);

                if (existingItem != null)
                {
                    isInsert = false;
                }
            }

            if (existingItem != null && existingItem.Equals(item))
            {
                Logger.LogDebug("Passed value is the same as the stored value.  No update required.");
                Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({JsonSerializer.Serialize(item)})");
                return (true, item);
            }

            var retVal = false;

            try
            {
                IDbConnection? connection = null;

                connection = transaction != null ? transaction.Connection : DbContext.CreateOpenConnection();

                if (connection != null)
                {
                    if (isInsert)
                    {
                        // Note: QuerySingleAsync rather than ExecuteAsync to be able to retrieve the Id
                        var id = await connection.QuerySingleAsync<long>(_insertItemSql, BuildParameterObjectForInsert(item));
                        retVal = true;
                        existingItem = new T();
                        Mappings.Instance.Mapper.Map(item, existingItem);
                        existingItem.id = id;
                    }
                    else
                    {
                        // existing item is known to not be null here (isInsert is false from above logic)
                        var (updateStatement, updateParams) = BuildUpdateStatementAndParameters(existingItem!, item);

                        Logger.LogDebug(updateStatement);
                        Logger.LogDebug(JsonConvert.SerializeObject(updateParams));

                        await connection.ExecuteAsync(updateStatement, updateParams);

                        if (transaction == null)
                        {
                            connection.Close();
                            connection.Dispose();
                        }

                        retVal = true;
                    }
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({JsonSerializer.Serialize(item)})");

            return (retVal, existingItem);
        }

        public async Task<uint> Count()
        {
            Logger.TraceMethodEntry(prefix: nameof(Data));

            uint retVal = 0;

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var countVal  = await connection.ExecuteScalarAsync<int>(_countItemsSql);

                    retVal = (uint)countVal;

                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data));

            return retVal;
        }

        public async Task<IEnumerable<T>> FindAll()
        {
            Logger.TraceMethodEntry(prefix: nameof(Data));

            IEnumerable<T> retVal = new List<T>();

            if (string.IsNullOrEmpty(_selectAll))
            {
                Logger.LogError($"Failed to determine the repository table name for type '{_typeName}'.  Cannot read all records");
                Logger.TraceMethodExit(prefix: nameof(Data));
                return retVal;
            }

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    retVal = await connection.QueryAsync<T>(_selectAll);
                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data));

            return retVal;
        }

        public async Task<T?> Find(long id)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({id})");

            T? retVal = default;

            if (string.IsNullOrEmpty(_selectItemById))
            {
                Logger.LogError($"Failed to determine the repository table name for type '{_typeName}'.  Cannot read by Id for Id '{id}'");
                Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({id})");
                return retVal;
            }

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    retVal = await connection.QueryFirstOrDefaultAsync<T>(_selectItemById, new { Id = id });
                    connection.Close();
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({id})");

            return retVal;
        }

        public async Task<bool> Delete(long id)
        {
            Logger.TraceMethodEntry(prefix: nameof(Data), suffix: $"({id})");

            var retVal = false;

            if (string.IsNullOrEmpty(_deleteItemById))
            {
                Logger.LogError($"Failed to determine the repository table name for type '{_typeName}'.  Cannot delete by Id for Id '{id}'");
                Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({id})");
                return false;
            }

            try
            {
                using var connection = DbContext.CreateOpenConnection();
                if (connection != null)
                {
                    var result = await connection.ExecuteAsync(_deleteItemById, new { Id = id });
                    Logger.LogDebug($"{result} rows deleted from {_tableName}");
                    retVal = result != 0;
                }
                else
                {
                    Logger.LogError("No database connection exists.");
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            Logger.TraceMethodExit(prefix: nameof(Data), suffix: $"({id})");

            return retVal;
        }

        private string BuildInsertStatement()
        {
            var insertStatementBuilder = new StringBuilder($"INSERT INTO {_tableName} (");
            var parameterBuilder = new StringBuilder();

            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var computedAttributes = property.GetCustomAttributes(typeof(ComputedAttribute), true);
                if (computedAttributes.Any()) continue; //don't want to generate inserts for computed attributes

                var keyAttributes = property.GetCustomAttributes(typeof(KeyAttribute), true);
                if (keyAttributes.Any()) continue; //don't want to generate inserts for autogenerated key attributes

                insertStatementBuilder.Append($"{property.Name}, ");
                parameterBuilder.Append($"@{property.Name}, ");
            }

            parameterBuilder = parameterBuilder.Remove(parameterBuilder.Length - 2, 2);
            insertStatementBuilder = insertStatementBuilder.Remove(insertStatementBuilder.Length - 2, 2);
            insertStatementBuilder.Append($") VALUES ({parameterBuilder}) RETURNING id;");

            return insertStatementBuilder.ToString();
        }

        private static string BuildSelectStatement()
        {
            //potentially partial dtos may be passed that do not reflect the full table.
            // build the select so that only columns that map to the passed in dto fields are read

            var selectStatementBuilder = new StringBuilder("SELECT ");

            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var computedAttributes = property.GetCustomAttributes(typeof(ComputedAttribute), true);
                if (computedAttributes.Any()) continue; //don't want to generate select for computed attributes

                selectStatementBuilder.Append($"{property.Name}, ");
            }

            //trim the last comma and space
            selectStatementBuilder = selectStatementBuilder.Remove(selectStatementBuilder.Length - 2, 2);
            selectStatementBuilder.Append(" FROM ");
            return selectStatementBuilder.ToString();
        }

        private (string, object) BuildUpdateStatementAndParameters(T storedItem, T updatedItem)
        {
            if (storedItem == null || updatedItem == null)
            {
                throw new InvalidOperationException("Arg cannot be null");
            }

            var updateSqlBuilder = new StringBuilder();

            dynamic paramsObject = new ExpandoObject();
            var dict = paramsObject as IDictionary<string, object>;

            updateSqlBuilder.Append($" UPDATE {_tableName} SET ");

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var updateSet = false;
            foreach (var property in properties)
            {
                var computedAttributes = property.GetCustomAttributes(typeof(ComputedAttribute), true);
                if (computedAttributes.Any()) continue;  //don't want to generate updated for computed attributes

                if (property.Name == "Id")
                {
                    Logger.LogDebug(" ID property encountered.");

                    // we don't want the update to ever include the primary key, however the parameter 'bag' must contain it
                    if (!dict!.ContainsKey(property.Name))
                    {
                        dict.Add(property.Name, property.GetValue(storedItem)!); // storedItem is known to not be null
                    }
                    //TODO: Eventually, change this to look for a PrimaryKey attribute instead?
                    continue;
                }
                // if the values don't differ, skip these as well
                if (Equals(property.GetValue(updatedItem), property.GetValue(storedItem))) continue;

                // if the updatedItem Property is null, we've potentially only received a partial modify model.
                // skip the field if the updatedItem Property is null
                if (property.GetValue(updatedItem) == null) continue;

                updateSet = true;
                updateSqlBuilder.AppendFormat("\"{0}\" = @{0}, ", property.Name);

                if (!dict!.ContainsKey(property.Name))
                {
                    dict.Add(property.Name, property.GetValue(updatedItem)!);
                }
            }

            if (updateSet)
            {
                updateSqlBuilder = updateSqlBuilder.Remove(updateSqlBuilder.Length - 2, 2);
            }

            updateSqlBuilder.Append(" WHERE Id = @Id;");
            if (!dict!.ContainsKey("Id"))
            {
                dict.Add("Id", storedItem.id);
            }

            return (updateSqlBuilder.ToString(), paramsObject);
        }

        private static object BuildParameterObjectForInsert(T item)
        {
            dynamic retVal = new ExpandoObject();

            var properties = typeof(T).GetProperties();

            foreach (var propertyInfo in properties)
            {
                var computedAttributes = propertyInfo.GetCustomAttributes(typeof(ComputedAttribute), true);
                if (computedAttributes.Any()) continue; //we don't want to add params for a computed property

                if (retVal is not IDictionary<string, object> dict || dict.ContainsKey(propertyInfo.Name)) continue;

                if (propertyInfo.PropertyType == typeof(uint?) && propertyInfo.GetValue(item) != null)
                {
                    dict.Add(propertyInfo.Name, Convert.ToInt32(propertyInfo.GetValue(item)));
                }
                else
                {
                    dict.Add(propertyInfo.Name, propertyInfo.GetValue(item)!);
                }
            }
            return retVal;
        }
    }
}
