using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public static class SecureControlProcessor
{
    public static void Process(Control root)
    {
        if (root == null)
            return;

        ProcessControl(root);

        foreach (Control child in root.Controls)
        {
            Process(child);
        }
    }

    private static void ProcessControl(Control control)
    {
        HyperLink hyperLink =
            control as HyperLink;

        if (hyperLink != null)
        {
            if (!string.IsNullOrWhiteSpace(
                hyperLink.NavigateUrl))
            {
                hyperLink.NavigateUrl =
                    SecureUrl.Create(
                        hyperLink.NavigateUrl);
            }

            return;
        }

        HtmlAnchor htmlAnchor =
            control as HtmlAnchor;

        if (htmlAnchor != null)
        {
            if (!string.IsNullOrWhiteSpace(
                htmlAnchor.HRef))
            {
                htmlAnchor.HRef =
                    SecureUrl.Create(
                        htmlAnchor.HRef);
            }

            return;
        }
    }
}