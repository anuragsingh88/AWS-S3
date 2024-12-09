namespace AWS_S3.Repository
{
    public class TrackUser
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        public static void Initialize(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static int AppUserID()
        {
            var appUserIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("AppUserID");
            if (appUserIdClaim != null && int.TryParse(appUserIdClaim.Value, out int appUserId))
            {
                return appUserId;
            }
            return 0;
        }
    }
}
