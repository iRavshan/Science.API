using MyCSharp.HttpUserAgentParser;

namespace Science.Domain.Models
{
    public class UserAgent
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public HttpUserAgentType Type { get; set; } = HttpUserAgentType.Unknown;
        public string? PlatformName { get; set; } = string.Empty;
        public HttpUserAgentPlatformType? PlatformType { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Version { get; set; } = string.Empty;
        public string? MobileDeviceType { get; set; } = string.Empty;
        public DateTime Added_At { get; set; }
    }
}
