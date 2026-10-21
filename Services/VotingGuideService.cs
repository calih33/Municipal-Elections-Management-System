// This file contains the VotingGuideService class for the Municipal Elections Management System.
namespace Municipal_Elections_Management_System.Services;

// saves picked candidate ids in a cookie, no login needed
public class VotingGuideService
{
    public const string CookieName = "VotingGuide";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public VotingGuideService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // read ids from the cookie
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
            // skip anything that isn't a number
            if (int.TryParse(part, out var id) && !ids.Contains(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    // add an id
    public void Add(int id)
    {
        var ids = GetIds();
        if (!ids.Contains(id))
        {
            ids.Add(id);
            Save(ids);
        }
    }

    // remove an id
    public void Remove(int id)
    {
        var ids = GetIds();
        if (ids.Remove(id))
        {
            Save(ids);
        }
    }

    // clear the guide
    public void Clear()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName);
    }

    // save ids as "4,12,17"
    private void Save(List<int> ids)
    {
        var options = new CookieOptions();
        options.Expires = DateTime.Now.AddDays(60); // lasts 60 days
        options.HttpOnly = true;                   // js can't read it

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieName, string.Join(",", ids), options);
    }
}
