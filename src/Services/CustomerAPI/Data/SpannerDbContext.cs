using Google.Cloud.Spanner.Data;
namespace CustomerAPI.Data
{
    public class SpannerDbContext
    {
        private readonly string _connectionString;
        public SpannerDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Spanner");
        }
        public SpannerConnection GetConnection()
        {
            return new SpannerConnection(_connectionString);
        }
    }
}