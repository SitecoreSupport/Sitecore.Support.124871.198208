namespace Sitecore.Support.Analytics.Extensions
{
  using Diagnostics;
  using Sitecore.Analytics.Tracking;

  internal static class SessionExtensions
  {
    internal const string OriginalSessionKey = "SessionExtensions.OriginalSessionKey";

    public static Session GetOriginalSession(this Session session)
    {
      Assert.ArgumentNotNull(session, "session");
      if (session.CustomData.ContainsKey("SessionExtensions.OriginalSessionKey"))
        return session.CustomData["SessionExtensions.OriginalSessionKey"] as Session;
      return null;
    }

    public static void SetOriginalSession(this Session session, Session originalSession)
    {
      Assert.ArgumentNotNull(session, "session");
      session.CustomData["SessionExtensions.OriginalSessionKey"] = originalSession;
    }
  }
}