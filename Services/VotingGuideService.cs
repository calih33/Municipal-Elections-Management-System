// This file contains the VotingGuideService class for the Municipal Elections Management System.
namespace Municipal_Elections_Management_System.Services;

// saves the candidates the user picked in a cookie, no login needed
// the cookie looks like "4,12,17"
public class VotingGuideService
{
    public const string CookieName = "VotingGuide";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public VotingGuideService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<int> GetIds()
    {
        var ids = new List<int>();
        var value = _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];

        if (string.IsNullOrEmpty(value))
        {
            return ids;
        }

        foreach (var part in value.Split(','))
        {
            // skip anything in the cookie that is not a number
            if (int.TryParse(part, out var id) && !ids.Contains(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    // add a candidate id to the cookie
    public void Add(int id)
    {
        var ids = GetIds();
        if (!ids.Contains(id))
        {
            ids.Add(id);
            Save(ids);
        }
    }

    // remove a candidate id from the cookie
    public void Remove(int id)
    {
        var ids = GetIds();
        if (ids.Remove(id))
        {
            Save(ids);
        }
    }

    // delete the whole cookie
    public void Clear()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName);
    }

    private void Save(List<int> ids)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        // cookie lasts 60 days
        httpContext.Response.Cookies.Append(CookieName, string.Join(",", ids), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(60)
        });
    }
}
