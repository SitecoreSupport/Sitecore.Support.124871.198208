using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Lookups;
using Sitecore.Analytics.Pipelines.SubmitSessionContext;
using Sitecore.Diagnostics;
using Sitecore.Support.Analytics.Extensions;

namespace Sitecore.Support.Analytics.Pipelines.SubmitSessionContext
{
  public class SaveVisit : Sitecore.Analytics.Pipelines.SubmitSessionContext.SaveVisit
  {
    public override void Process(SubmitSessionContextArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      var originalSession = args.Session.GetOriginalSession();
      if (originalSession != null && originalSession.Interaction != null)
      {
        originalSession.Interaction.SaveDateTime = System.DateTime.UtcNow;
        if (AnalyticsSettings.RedactIpAddress)
        {
          originalSession.Interaction.Ip = IpHashProviderBase.EmptyIpAddress;
        }
      }

      base.Process(args);
    }
  }
}