using System.Data;

namespace DapperWrapper
{
    public interface IDbContext
    {
        string ContextName { get; }
        IDbConnection GetConnection();
    }
}