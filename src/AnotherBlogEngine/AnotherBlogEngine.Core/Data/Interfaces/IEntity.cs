namespace AnotherBlogEngine.Core.Data.Interfaces;

public interface IEntity
{
#pragma warning disable IDE1006

    long id { get; set; }

    bool deleted_fg { get; set; }

#pragma warning restore IDE1006
}