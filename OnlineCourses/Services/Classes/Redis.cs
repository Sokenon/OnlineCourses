using Microsoft.Extensions.Caching.Distributed;

namespace OnlineCourses.Services.Classes
{
    public class Redis
    {
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "deleted_email:";

        public Redis(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task DeleteEmailAsync(string email, TimeSpan? banDuration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = banDuration ?? TimeSpan.FromDays(1)
            };

            await _cache.SetStringAsync($"{CacheKeyPrefix}{email.ToLower()}", "deleted", options);
        }
        public bool IsEmailDeleted(string email)
        {
            var value = _cache.GetStringAsync($"{CacheKeyPrefix}{email.ToLower()}");
            return value.Result != null;
        }
    }
}
